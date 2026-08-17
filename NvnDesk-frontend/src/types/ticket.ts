// Mevcut Ticket interface'i AYNEN kalıyor, sadece status'a tip güvenliği ekliyoruz.
// Backend'den string olarak geldiği için literal union type kullanıyoruz —
// runtime'da hâlâ string ama TypeScript artık yazım hatalarını yakalar.
export type TicketStatus = "Open" | "InProgress" | "Resolved" | "Closed";

export interface Ticket {
  id: string;
  title: string;
  description: string;
  status: TicketStatus;       // string yerine TicketStatus
  priority: string;
  createdByName: string;
  assignedToName: string | null;
  createdAt: string;
  category: string | null;
  summary: string | null;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;
}

// Sıradaki durumu bulmak için kullandığımız sabit dizi (bir önceki mesajdaki gibi)
export const STATUS_FLOW: TicketStatus[] = ["Open", "InProgress", "Resolved", "Closed"];