import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import type { Ticket } from "../types/ticket";


export function useTicketHub(onNewTicket: (ticket: Ticket) => void) {

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);


  useEffect(() => {
    const token = localStorage.getItem("token");
    if (!token) return;

    const hubBaseUrl = import.meta.env.VITE_API_URL.replace("/api", "");

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${hubBaseUrl}/hubs/ticket`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect() 
      .build();

    connection.on("ReceiveNewTicket", (ticket: Ticket) => {
      onNewTicket(ticket);
    });

    connection
      .start()
      .then(() => setIsConnected(true))
      .catch((err) => console.error("SignalR bağlantı hatası:", err));

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [onNewTicket]);

  return { isConnected };
}