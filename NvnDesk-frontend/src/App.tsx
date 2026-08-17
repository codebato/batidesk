import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { Login } from "./pages/Login";
import { TicketList } from "./pages/TicketList";
<<<<<<< HEAD
import TicketDetail from "./pages/TicketDetail";

=======
import { Register } from "./pages/Register";
>>>>>>> 27aefe967f9bd2582460f8b46bbb563f535ec676

function RequireAuth({ children }: { children: React.ReactNode }) {
  const token = localStorage.getItem("token");
  return token ? <>{children}</> : <Navigate to="/login" replace />;
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/tickets"
          element={
            <RequireAuth>
              <TicketList />
            </RequireAuth>
          }
        />
        {}
        <Route
          path="/tickets/:id"
          element={
            <RequireAuth>
              <TicketDetail />
            </RequireAuth>
          }
        />
        <Route path="/" element={<Navigate to="/tickets" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;