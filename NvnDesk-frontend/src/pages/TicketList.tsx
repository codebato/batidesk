import { useCallback, useEffect, useState } from "react";
import { getAllTickets, createTicket } from "../api/tickets";
import { useTicketHub } from "../hooks/useTicketHub";
import type { Ticket } from "../types/ticket";
import { useNavigate } from "react-router-dom";

export function TicketList() {

  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    getAllTickets().then(setTickets);
  }, []);

  const handleNewTicket = useCallback((newTicket: Ticket) => {
    setTickets((prev) => [newTicket, ...prev]);
  }, []);

  const { isConnected } = useTicketHub(handleNewTicket);

  async function handleCreateTicket(e: React.FormEvent) {
    e.preventDefault();
    await createTicket({ title, description, priority: "Medium" });

    setTitle("");
    setDescription("");
  }

  return (
    <div>
      <h2>Ticket'lar {isConnected ? "🟢 Canlı" : "🔴 Bağlanıyor..."}</h2>

      <form onSubmit={handleCreateTicket}>
        <input
          placeholder="Başlık"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
        />
        <input
          placeholder="Açıklama"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          required
        />
        <button type="submit">Ticket Oluştur</button>
      </form>

      <ul>
        {tickets.map((ticket) => (
          // onClick + cursor:pointer eklendi -> satıra tıklayınca detay sayfasına gidiyor
          <li
            key={ticket.id}
            onClick={() => navigate(`/tickets/${ticket.id}`)}
            style={{ cursor: "pointer" }}
          >
            <strong>{ticket.title}</strong> — {ticket.status} — {ticket.priority}
            {ticket.category && <span> [{ticket.category}]</span>}
            {ticket.summary && <p>{ticket.summary}</p>}
          </li>
        ))}
      </ul>
    </div>
  );
}