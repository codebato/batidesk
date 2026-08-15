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
      .catch((err) => {
        console.error("SignalR bağlantı hatası:", err);
        const isUnauthorized =
          err?.message?.includes("401") || err?.statusCode === 401;
        if (isUnauthorized) {
          localStorage.removeItem("token");
          window.location.href = "/login";
        }
        // diğer hatalarda (timeout, sunucu uykuda vs.) sessizce yeniden dener, kullanıcıyı atmaz
      });

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [onNewTicket]);

  return { isConnected };
}