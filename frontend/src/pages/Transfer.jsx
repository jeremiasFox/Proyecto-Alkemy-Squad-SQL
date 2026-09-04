import { useState } from "react";
import { useNavigate } from "react-router-dom";

export default function Transfer() {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    destinationAccountId: "",
    amount: "",
    description: "",
  });
  const [errors, setErrors] = useState({});
  const [showConfirm, setShowConfirm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [msg, setMsg] = useState({ text: "", ok: true });

  // ── Validación ─────────────────────────────────────────────
  const validate = () => {
    const e = {};
    if (!form.destinationAccountId) {
      e.destinationAccountId = "El ID de cuenta destino es requerido.";
    } else if (!/^\d+$/.test(form.destinationAccountId)) {
      e.destinationAccountId = "El ID debe ser un número entero.";
    }
    if (!form.amount) {
      e.amount = "El monto es requerido.";
    } else if (isNaN(form.amount) || Number(form.amount) <= 0) {
      e.amount = "El monto debe ser mayor a 0.";
    }
    return e;
  };

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
    setErrors({ ...errors, [e.target.name]: "" });
    setMsg({ text: "", ok: true });
  };

  // Abre el modal solo si el form es válido
  const handleSubmit = (e) => {
    e.preventDefault();
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    setShowConfirm(true);
  };

  // ── Confirmar y enviar ──────────────────────────────────────
  const handleConfirm = async () => {
    setShowConfirm(false);
    setLoading(true);
    setMsg({ text: "", ok: true });

    try {
      const token = localStorage.getItem("token");
      const API = import.meta.env.VITE_API_URL;

      const res = await fetch(`${API}/transactions/transfer`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          destinationAccountId: Number(form.destinationAccountId),
          amount: Number(form.amount),
        }),
      });

      const data = await res.json();

      if (!res.ok) {
        // Mensajes de error del backend en lenguaje claro
        const backendMsg = data?.message || "";
        if (backendMsg.includes("Saldo insuficiente")) {
          throw new Error("No tenés saldo suficiente para realizar esta transferencia.");
        } else if (backendMsg.includes("misma cuenta") || backendMsg.includes("autotransferencia")) {
          throw new Error("No podés transferirte dinero a vos mismo.");
        } else if (backendMsg.includes("no existe") || res.status === 404) {
          throw new Error("La cuenta destino no existe. Verificá el ID ingresado.");
        } else {
          throw new Error(backendMsg || "Ocurrió un error al procesar la transferencia.");
        }
      }

      setMsg({ text: `¡Transferencia exitosa! Nuevo saldo: $${data.newBalance?.toFixed(2)}`, ok: true });
      setForm({ destinationAccountId: "", amount: "", description: "" });
    } catch (error) {
      setMsg({ text: error.message, ok: false });
    } finally {
      setLoading(false);
    }
  };

  // ── Estilos inline simples (igual que Deposit.jsx) ──────────
  const inputStyle = {
    width: "100%",
    padding: 8,
    marginBottom: 4,
    boxSizing: "border-box",
    fontSize: 14,
  };
  const errorStyle = { color: "red", fontSize: 12, marginBottom: 10 };

  return (
    <div style={{ padding: 20, maxWidth: 420, margin: "auto" }}>
      <button onClick={() => navigate("/dashboard")} style={{ marginBottom: 16, cursor: "pointer" }}>
        ← Volver
      </button>

      <h2>Transferir dinero</h2>

      <form onSubmit={handleSubmit} noValidate>
        {/* Cuenta destino */}
        <label style={{ fontSize: 13, fontWeight: "bold" }}>ID de cuenta destino</label>
        <input
          type="number"
          name="destinationAccountId"
          placeholder="Ej: 2"
          value={form.destinationAccountId}
          onChange={handleChange}
          style={inputStyle}
        />
        {errors.destinationAccountId && (
          <p style={errorStyle}>{errors.destinationAccountId}</p>
        )}

        {/* Monto */}
        <label style={{ fontSize: 13, fontWeight: "bold" }}>Monto</label>
        <input
          type="number"
          name="amount"
          placeholder="Ej: 500"
          min="0.01"
          step="0.01"
          value={form.amount}
          onChange={handleChange}
          style={inputStyle}
        />
        {errors.amount && <p style={errorStyle}>{errors.amount}</p>}

        {/* Descripción (solo visual, no se envía al backend) */}
        <label style={{ fontSize: 13, fontWeight: "bold" }}>
          Descripción <span style={{ fontWeight: "normal", color: "#888" }}>(opcional)</span>
        </label>
        <input
          type="text"
          name="description"
          placeholder="Ej: Pago alquiler"
          value={form.description}
          onChange={handleChange}
          style={inputStyle}
        />

        <button
          type="submit"
          disabled={loading}
          style={{
            width: "100%",
            padding: 12,
            marginTop: 12,
            background: loading ? "#aaa" : "#1976d2",
            color: "#fff",
            border: "none",
            borderRadius: 4,
            fontSize: 15,
            cursor: loading ? "not-allowed" : "pointer",
          }}
        >
          {loading ? "Procesando..." : "Continuar"}
        </button>
      </form>

      {/* Mensaje de resultado */}
      {msg.text && (
        <p style={{ marginTop: 16, color: msg.ok ? "green" : "red", fontWeight: "bold" }}>
          {msg.text}
        </p>
      )}

      {/* ── Modal de confirmación ────────────────────────────── */}
      {showConfirm && (
        <div
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.5)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 1000,
          }}
        >
          <div
            style={{
              background: "#fff",
              borderRadius: 8,
              padding: 28,
              maxWidth: 360,
              width: "90%",
              boxShadow: "0 4px 20px rgba(0,0,0,0.2)",
            }}
          >
            <h3 style={{ marginTop: 0 }}>Confirmá la transferencia</h3>
            <p>
              <strong>Cuenta destino:</strong> #{form.destinationAccountId}
            </p>
            <p>
              <strong>Monto:</strong> ${Number(form.amount).toFixed(2)}
            </p>
            {form.description && (
              <p>
                <strong>Descripción:</strong> {form.description}
              </p>
            )}
            <p style={{ color: "#888", fontSize: 13 }}>
              Esta acción no se puede deshacer.
            </p>
            <div style={{ display: "flex", gap: 10, marginTop: 16 }}>
              <button
                onClick={() => setShowConfirm(false)}
                style={{
                  flex: 1,
                  padding: 10,
                  background: "#f5f5f5",
                  border: "1px solid #ccc",
                  borderRadius: 4,
                  cursor: "pointer",
                }}
              >
                Cancelar
              </button>
              <button
                onClick={handleConfirm}
                style={{
                  flex: 1,
                  padding: 10,
                  background: "#1976d2",
                  color: "#fff",
                  border: "none",
                  borderRadius: 4,
                  cursor: "pointer",
                }}
              >
                Confirmar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
