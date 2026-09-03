import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

// TAREA: Dashboard usuario - Mi tarea
// Criterios que cumple: saldo, alias/cbu, botones, ultimos movimientos, colores
export default function Dashboard() {
  // Estados para guardar datos del backend
  const [account, setAccount] = useState(null); // Guarda saldo, alias, cbu
  const [transactions, setTransactions] = useState([]); // Guarda movimientos
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Al cargar la pantalla, pide datos al backend
  useEffect(() => {
    async function loadData() {
      try {
        const token = localStorage.getItem("token");
        const headers = { Authorization: `Bearer ${token}` };

        // Pido mi cuenta y mis movimientos
        const accRes = await fetch("http://localhost:3000/api/accounts/me", {
          headers,
        });
        const transRes = await fetch(
          "http://localhost:3000/api/transactions/me",
          { headers },
        );

        if (!accRes.ok || !transRes.ok) {
          throw new Error("No se pudo cargar la info");
        }

        const accData = await accRes.json();
        const transData = await transRes.json();

        setAccount(accData);
        setTransactions(transData.slice(0, 5)); // Solo los ultimos 5
      } catch (e) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, []);

  // Mensajes de carga y error
  if (loading) return <p>Cargando datos...</p>;
  if (error) return <p style={{ color: "red" }}>Error: {error}</p>;

  // --- RETURN: Lo que se ve en pantalla ---

  return (
    <div style={{ padding: 20, maxWidth: 600, margin: "auto" }}>
      {/* 1. CARD DE SALDO - Muestra saldo, alias y cbu */}
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

      {/* 2. ACCESOS DIRECTOS - Botones para depositar y transferir */}
      <div style={{ margin: "20px 0", display: "flex", gap: 10 }}>
        <Link to="/deposit">
          <button>Depositar</button>
        </Link>
        <Link to="/transfer">
          <button>Transferir</button>
        </Link>
      </div>

      {/* 3 y 4. ULTIMOS MOVIMIENTOS con colores */}
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
                // 4. Color verde si entra plata, rojo si sale
                color: t.amount > 0 ? "green" : "red",
                fontWeight: "bold",
              }}
            >
              <span>
                {t.type} - {t.counterparty || "sin detalle"}
              </span>
              <span>
                {t.amount} - {new Date(t.createdAt).toLocaleDateString()}
              </span>
            </li>
          ))
        )}
      </ul>
    </div>
  );
}
