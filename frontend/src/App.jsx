import { BrowserRouter, Routes, Route } from 'react-router-dom'


function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<div>Inicio</div>} />
        <Route path="/login" element={<div>Login</div>} />
        <Route path="/register" element={<div>Registro</div>} />
      </Routes>
    </BrowserRouter>
  )
}

export default App