import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./components/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import LoginForm from "./components/LoginForm";

// Placeholders hasta que se implementen las pantallas reales
function Dashboard() {
  return <div>Dashboard usuario</div>;
}
function AdminPanel() {
  return <div>Panel admin</div>;
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Ruta pública */}
          <Route path="/login" element={<LoginForm />} />

          {/* Ruta para usuarios comunes */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute allowedRoles={["User", "Admin"]}>
                <Dashboard />
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

          {/* Redirigir raíz al login */}
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
