import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./components/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import LoginForm from "./components/LoginForm";
import Dashboard from "./pages/Dashboard";
import Deposit from "./pages/Deposit";
import Transfer from "./pages/Transfer";

function AdminPanel() {
  return <div>Panel admin</div>;
}

function App() {
  return (
    // Provee la autenticacion a toda la app
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Ruta pública - Login */}
          <Route path="/login" element={<LoginForm />} />

          {/* Ruta para usuarios comunes - Usa MI Dashboard */}
          <Route
            path="/dashboard"
            element={
              // Solo deja entrar si tiene rol User o Admin
              <ProtectedRoute allowedRoles={["User", "Admin"]}>
                <Dashboard />
              </ProtectedRoute>
            }
          />

          {/* TAREA DE HOY - Ruta de Deposito */}
          <Route
            path="/deposit"
            element={
              <ProtectedRoute allowedRoles={["User", "Admin"]}>
                <Deposit />
              </ProtectedRoute>
            }
          />

          {/* Ruta de Transferencia */}
          <Route
            path="/transfer"
            element={
              <ProtectedRoute allowedRoles={["User", "Admin"]}>
                <Transfer />
              </ProtectedRoute>
            }
          />

          {/* Ruta exclusiva para admins */}
          <Route
            path="/admin"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <AdminPanel />
              </ProtectedRoute>
            }
          />

          {/* Si entra a "/" o a una ruta que no existe, lo mando al login */}
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
