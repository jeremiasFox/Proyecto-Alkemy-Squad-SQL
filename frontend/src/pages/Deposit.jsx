import { useState } from "react";
export default function Deposit() {
  const [amount, setAmount] = useState("");
  const [msg, setMsg] = useState("");
  async function handleDeposit(e) {
    e.preventDefault();
    try {
      const token = localStorage.getItem("token");
      const API = import.meta.env.VITE_API_URL;
      const res = await fetch(`${API}/accounts/deposit`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ amount: Number(amount) }),
      });
      if (!res.ok) throw new Error((await res.text()) || "Error al depositar");
      setMsg("¡Depósito exitoso!");
      setAmount("");
    } catch (error) {
      setMsg("Error: " + error.message);
    }
  }
  return (
    <div style={{ padding: 20, maxWidth: 400, margin: "auto" }}>
      <h2>Depositar</h2>
      <form onSubmit={handleDeposit}>
        <input
          type="number"
          placeholder="Monto"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          style={{ width: "100%", padding: 8, marginBottom: 10 }}
        />
        <button type="submit" style={{ width: "100%", padding: 10 }}>
          Depositar
        </button>
      </form>
      {msg && <p>{msg}</p>}
    </div>
  );
}
