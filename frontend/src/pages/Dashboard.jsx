import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

export default function Dashboard() {
  const [account, setAccount] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadData() {
      try {
        const token = localStorage.getItem("token");
        const headers = { Authorization: `Bearer ${token}` };
        const API = import.meta.env.VITE_API_URL;

        const accRes = await fetch(`${API}/accounts/me`, { headers });
        const transRes = await fetch(`${API}/transactions/me`, { headers });

        if (!accRes.ok || !transRes.ok) {
          throw new Error("No se pudo cargar la info");
        }

        const accData = await accRes.json();
        const transData = await transRes.json();

        setAccount(accData);
        const lista = Array.isArray(transData)
          ? transData
          : transData.transactions || transData.data || [];
        setTransactions(lista.slice(0, 5));
      } catch (e) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, []);

  if (loading) return <p>Cargando datos...</p>;
  if (error) return <p style={{ color: "red" }}>Error: {error}</p>;

  return (
    <div style={{ padding: 20, maxWidth: 600, margin: "auto" }}>
      <div
        style={{
          border: "1px solid #ccc",
          borderRadius: 12,
          padding: 20,
          background: "#f9f9f9",
        }}
      >
        <h3 style={{ margin: 0 }}>Mi saldo</h3>
        <h1 style={{ margin: "10px 0" }}>
          {new Intl.NumberFormat("es-AR", {
            style: "currency",
            currency: "ARS",
          }).format(account?.balance || 0)}
        </h1>
        <small>
          Alias: {account?.alias} | CBU: {account?.cbu}
        </small>
      </div>
      <div style={{ margin: "20px 0", display: "flex", gap: 10 }}>
        <Link to="/deposit">
          <button>Depositar</button>
        </Link>
        <Link to="/transfer">
          <button>Transferir</button>
        </Link>
      </div>
      <h3>Últimos movimientos</h3>
      <ul style={{ listStyle: "none", padding: 0 }}>
        {transactions.length === 0 ? (
          <li>No hay movimientos aún</li>
        ) : (
          transactions.map((t) => (
            <li
              key={t.id}
              style={{
                display: "flex",
                justifyContent: "space-between",
                padding: "10px 0",
                borderBottom: "1px solid #eee",
                color: t.amount > 0 ? "green" : "red",
                fontWeight: "bold",
              }}
            >
              <span>
                {t.type} - {t.counterparty || "sin detalle"}
              </span>
              <span>
                {t.amount} - {new Date(t.date).toLocaleDateString()}
              </span>
            </li>
          ))
        )}
      </ul>
    </div>
  );
}
