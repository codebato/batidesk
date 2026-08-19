import { useCallback, useEffect, useState } from "react";
import { getAllTickets, createTicket } from "../api/tickets";
import { useTicketHub } from "../hooks/useTicketHub";
import type { Ticket, TicketStatus } from "../types/ticket";
import { useNavigate } from "react-router-dom";

// Backend'deki TicketStatus değerlerini (PascalCase) CSS class isimlerine
// (lowercase) çevirmek için küçük bir yardımcı harita.
const statusBadgeClass: Record<TicketStatus, string> = {
  Open: "badge-status-open",
  InProgress: "badge-status-inprogress",
  Resolved: "badge-status-resolved",
  Closed: "badge-status-closed",
};

export function TicketList() {
  const navigate = useNavigate();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    getAllTickets().then(setTickets);
  }, []);

  const handleNewTicket = useCallback((newTicket: Ticket) => {
    setTickets((prev) => [newTicket, ...prev]);
  }, []);

  const { isConnected } = useTicketHub(handleNewTicket);

  async function handleCreateTicket(e: React.FormEvent) {
    e.preventDefault();
    setCreating(true);
    try {
      await createTicket({ title, description, priority: "Medium" });
      setTitle("");
      setDescription("");
    } finally {
      setCreating(false);
    }
  }

  function handleLogout() {
    localStorage.removeItem("token");
    localStorage.removeItem("userEmail");
    navigate("/login");
  }

  return (
    <div className="app-shell">
      {/* Üst bar: marka + canlı bağlantı durumu + çıkış butonu */}
      <div className="topbar">
        <div className="topbar-brand">
          <h2>NvnDesk</h2>
          <span className="connection-label">
            <span className={`connection-dot ${isConnected ? "online" : "offline"}`} />
            {isConnected ? "Canlı" : "Bağlanıyor..."}
          </span>
        </div>
        <button className="btn btn-secondary" onClick={handleLogout}>
          Çıkış Yap
        </button>
      </div>

      <div className="page-content">
        {/* Ticket oluşturma kartı */}
        <div className="create-ticket-card">
          <h3>Yeni Ticket Oluştur</h3>
          <form className="create-ticket-form" onSubmit={handleCreateTicket}>
            <input
              className="input-field"
              placeholder="Başlık"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
            <input
              className="input-field"
              placeholder="Açıklama"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
            />
            <button type="submit" className="btn btn-primary" disabled={creating}>
              {creating ? "Oluşturuluyor..." : "Ticket Oluştur"}
            </button>
          </form>
        </div>

        {/* Ticket listesi */}
        {tickets.length === 0 ? (
          <div className="empty-state">
            <p>Henüz hiç ticket yok. Yukarıdan ilkini oluşturabilirsin.</p>
          </div>
        ) : (
          <div className="ticket-list">
            {tickets.map((ticket) => (
              <button
                key={ticket.id}
                className="ticket-row"
                onClick={() => navigate(`/tickets/${ticket.id}`)}
              >
                <div className="ticket-row-main">
                  <div className="ticket-row-title">{ticket.title}</div>
                  {/* AI özeti varsa göster, yoksa açıklamanın kendisini kısaca göster */}
                  <div className="ticket-row-summary">
                    {ticket.summary ?? ticket.description}
                  </div>
                </div>
                <div className="ticket-row-badges">
                  {ticket.category && (
                    <span className="badge badge-priority">{ticket.category}</span>
                  )}
                  <span className="badge badge-priority">{ticket.priority}</span>
                  <span className={`badge ${statusBadgeClass[ticket.status]}`}>
                    {ticket.status}
                  </span>
                </div>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}