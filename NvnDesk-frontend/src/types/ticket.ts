export interface Ticket {
  id: string;
  title: string;
  description: string;
  status: string;
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