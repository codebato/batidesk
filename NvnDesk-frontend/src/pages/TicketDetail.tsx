import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getTicketById, updateTicketStatus } from "../api/tickets";
import { STATUS_FLOW, type Ticket, type TicketStatus } from "../types/ticket";


const statusBadgeClass: Record<TicketStatus, string> = {
  Open: "badge-status-open",
  InProgress: "badge-status-inprogress",
  Resolved: "badge-status-resolved",
  Closed: "badge-status-closed",
};

export default function TicketDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getTicketById(id)
      .then(setTicket)
      .catch(() => setError("Ticket yüklenemedi."))
      .finally(() => setLoading(false));
  }, [id]);

  const getNextStatus = (current: TicketStatus): TicketStatus | null => {
    const currentIndex = STATUS_FLOW.indexOf(current);
    if (currentIndex === -1 || currentIndex === STATUS_FLOW.length - 1) return null;
    return STATUS_FLOW[currentIndex + 1];
  };

  const handleStatusChange = async (newStatus: TicketStatus) => {
    if (!ticket) return;
    setUpdating(true);
    try {
      const updated = await updateTicketStatus(ticket.id, newStatus);
      setTicket(updated);
    } catch {
      setError("Durum güncellenemedi.");
    } finally {
      setUpdating(false);
    }
  };

  // Yükleniyor / hata / bulunamadı durumlarında da aynı sayfa iskeletini
  // (app-shell + page-content) kullanıyoruz ki geçiş anında sayfa "zıplamasın".
  if (loading) {
    return (
      <div className="app-shell">
        <div className="page-content">
          <p>Yükleniyor...</p>
        </div>
      </div>
    );
  }

  if (error || !ticket) {
    return (
      <div className="app-shell">
        <div className="page-content">
          <p>{error ?? "Ticket bulunamadı."}</p>
        </div>
      </div>
    );
  }

  const nextStatus = getNextStatus(ticket.status);

  return (
    <div className="app-shell">
      <div className="page-content" style={{ maxWidth: 700 }}>
        <button className="detail-back" onClick={() => navigate("/tickets")}>
          ← Listeye dön
        </button>

        <div className="detail-card">
          <h1>{ticket.title}</h1>
          <p className="detail-description">{ticket.description}</p>

          {/* Ticket'a dair temel bilgiler — grid şeklinde 4 kutu */}
          <div className="detail-meta">
            <div className="detail-meta-item">
              <span className="detail-meta-label">Durum</span>
              <span className={`badge ${statusBadgeClass[ticket.status]}`}>
                {ticket.status}
              </span>
            </div>
            <div className="detail-meta-item">
              <span className="detail-meta-label">Öncelik</span>
              <span className="detail-meta-value">{ticket.priority}</span>
            </div>
            <div className="detail-meta-item">
              <span className="detail-meta-label">Oluşturan</span>
              <span className="detail-meta-value">{ticket.createdByName}</span>
            </div>
            {ticket.assignedToName && (
              <div className="detail-meta-item">
                <span className="detail-meta-label">Atanan</span>
                <span className="detail-meta-value">{ticket.assignedToName}</span>
              </div>
            )}
          </div>

          {/* AI kategori/özet varsa vurgulu kutucuklarda göster */}
          {ticket.category && (
            <div className="ai-insight">
              <div className="ai-insight-label">AI Kategori</div>
              <p>{ticket.category}</p>
            </div>
          )}
          {ticket.summary && (
            <div className="ai-insight">
              <div className="ai-insight-label">AI Özet</div>
              <p>{ticket.summary}</p>
            </div>
          )}

          <div className="detail-actions">
            {nextStatus ? (
              <button
                className="btn btn-primary"
                disabled={updating}
                onClick={() => handleStatusChange(nextStatus)}
              >
                {updating ? "Güncelleniyor..." : `Durumu "${nextStatus}" yap`}
              </button>
            ) : (
              <p className="closed-note">Bu ticket kapatılmış, ilerletilecek durum yok.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}