import { apiClient } from "./client";
import type { Ticket, CreateTicketRequest } from "../types/ticket";

export async function getAllTickets(): Promise<Ticket[]> {
  const response = await apiClient.get<Ticket[]>("/ticket");
  return response.data;
}

export async function createTicket(request: CreateTicketRequest): Promise<Ticket> {
  const response = await apiClient.post<Ticket>("/ticket", request);
  return response.data;
}