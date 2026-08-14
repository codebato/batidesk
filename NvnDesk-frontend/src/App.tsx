import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Login } from "./pages/Login";
import { TicketList } from "./pages/TicketList";


function RequireAuth({ children }: { children: React.ReactNode }) {
  const token = localStorage.getItem("token");
  return token ? <>{children}</> : <Navigate to="/login" replace />;
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route
          path="/tickets"
          element={
            <RequireAuth>
              <TicketList />
            </RequireAuth>
          }
        />
        {}
        <Route path="/" element={<Navigate to="/tickets" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;