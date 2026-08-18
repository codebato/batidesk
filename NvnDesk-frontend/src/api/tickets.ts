import { apiClient } from "./client";
import type { Ticket, CreateTicketRequest, TicketStatus } from "../types/ticket";

export const getAllTickets = async (): Promise<Ticket[]> => {
  const response = await apiClient.get<Ticket[]>("/ticket");
  return response.data;
};

export const createTicket = async (data: CreateTicketRequest): Promise<Ticket> => {
  const response = await apiClient.post<Ticket>("/ticket", data);
  return response.data;
};

export const getTicketById = async (id: string): Promise<Ticket> => {
  const response = await apiClient.get<Ticket>(`/ticket/${id}`);
  return response.data;
};

export const updateTicketStatus = async (
  id: string,
  status: TicketStatus
): Promise<Ticket> => {
  const response = await apiClient.put<Ticket>(`/ticket/${id}`, { status });
  return response.data;
};