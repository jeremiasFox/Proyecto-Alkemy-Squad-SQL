import { createContext, useContext, useState, useEffect } from 'react'
import { jwtDecode } from 'jwt-decode'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('token'))
  const [user, setUser] = useState(() => {
    const saved = localStorage.getItem('token')
    if (!saved) return null
    try {
      const decoded = jwtDecode(saved)
      return {
        id: decoded.sub,
        email: decoded.email,
        role: decoded.role,
      }
    } catch {
      return null
    }
  })

  const login = (newToken) => {
    localStorage.setItem('token', newToken)
    setToken(newToken)
    try {
      const decoded = jwtDecode(newToken)
      setUser({
        id: decoded.sub,
        email: decoded.email,
        role: decoded.role,
      })
    } catch {
      setUser(null)
    }
  }

  const logout = () => {
    localStorage.removeItem('token')
    setToken(null)
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, token, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
