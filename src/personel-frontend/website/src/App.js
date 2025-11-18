import {BrowserRouter, Route, Routes, Navigate} from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import PersonelPage from "./pages/PersonelPage";

function ProtectedRoute({ children }) {
  const token = localStorage.getItem('token');
  return token ? children : <Navigate to="/login" />;
}


export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/personeller"
          element={
            <ProtectedRoute>
              <PersonelPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}

