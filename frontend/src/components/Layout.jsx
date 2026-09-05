import { Link, Outlet } from 'react-router-dom'
import { useAuth } from './AuthContext'

export default function Layout() {
  const { user, logout } = useAuth()

  return (
    <>
      <nav>
        <Link to="/dashboard">Dashboard</Link>

        {user?.role === 'Admin' && (
          <Link to="/admin">Admin</Link>
        )}

        <button onClick={logout}>Cerrar sesión</button>
      </nav>

      <main>
  <Outlet />
</main>
    </>
  )
}