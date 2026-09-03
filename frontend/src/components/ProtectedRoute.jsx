import { Navigate } from 'react-router-dom'
import { useAuth } from './AuthContext'

// Protege una ruta requiriendo autenticación.
// Si se pasa allowedRoles, también verifica que el usuario tenga el rol correcto.
export default function ProtectedRoute({ children, allowedRoles }) {
  const { user, token } = useAuth()

  if (!token || !user) {
    return <Navigate to="/login" replace />
  }

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    // Autenticado pero sin el rol requerido → llevar a su pantalla
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/dashboard'} replace />
  }

  return children
}
