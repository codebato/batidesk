
import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getTicketById, updateTicketStatus } from "../api/tickets";
import { STATUS_FLOW, type Ticket, type TicketStatus } from "../types/ticket";

export default function TicketDetail() {
  // URL'den :id parametresini alıyoruz (App.tsx'te route'u tanımlarken kullanacağız)
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(false); // buton spam'ini önlemek için
  const [error, setError] = useState<string | null>(null);

  // Sayfa açılınca ticket'ı çek
  useEffect(() => {
    if (!id) return;
    getTicketById(id)
      .then(setTicket)
      .catch(() => setError("Ticket yüklenemedi."))
      .finally(() => setLoading(false));
  }, [id]);

  // STATUS_FLOW dizisinde mevcut durumun bir sonrasını buluyoruz.
  // Closed'daysa artık ilerleyecek durum yok, null dönüyoruz -> buton gizlenecek.
  const getNextStatus = (current: TicketStatus): TicketStatus | null => {
    const currentIndex = STATUS_FLOW.indexOf(current);
    if (currentIndex === -1 || currentIndex === STATUS_FLOW.length - 1) return null;
    return STATUS_FLOW[currentIndex + 1];
  };

  const handleStatusChange = async (newStatus: TicketStatus) => {
    if (!ticket) return;
    setUpdating(true);
    try {
      // UpdateTicketRequest'teki her alan nullable olduğu için sadece status gönderiyoruz
      const updated = await updateTicketStatus(ticket.id, newStatus);
      setTicket(updated);
    } catch {
      setError("Durum güncellenemedi.");
    } finally {
      setUpdating(false);
    }
  };

  if (loading) return <p>Yükleniyor...</p>;
  if (error) return <p>{error}</p>;
  if (!ticket) return <p>Ticket bulunamadı.</p>;

  const nextStatus = getNextStatus(ticket.status);

  return (
    <div style={{ maxWidth: 700, margin: "0 auto", padding: "1.5rem" }}>
      <button onClick={() => navigate("/tickets")}>&larr; Listeye dön</button>

      <h1>{ticket.title}</h1>
      <p>{ticket.description}</p>

      <div style={{ display: "flex", gap: "1rem", margin: "1rem 0" }}>
        <span><strong>Durum:</strong> {ticket.status}</span>
        <span><strong>Öncelik:</strong> {ticket.priority}</span>
        <span><strong>Oluşturan:</strong> {ticket.createdByName}</span>
        {ticket.assignedToName && <span><strong>Atanan:</strong> {ticket.assignedToName}</span>}
      </div>

      {ticket.category && <p><strong>AI Kategori:</strong> {ticket.category}</p>}
      {ticket.summary && <p><strong>AI Özet:</strong> {ticket.summary}</p>}

      {nextStatus ? (
        <button disabled={updating} onClick={() => handleStatusChange(nextStatus)}>
          {updating ? "Güncelleniyor..." : `Durumu "${nextStatus}" yap`}
        </button>
      ) : (
        <p><em>Bu ticket kapatılmış, ilerletilecek durum yok.</em></p>
      )}
    </div>
  );
}