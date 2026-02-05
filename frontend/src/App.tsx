import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ProjectProvider } from './context/ProjectContext';
import { NotificationProvider } from './context/NotificationContext';
import Layout from './components/Layout';

// Public pages
import Home from './pages/Home';
import About from './pages/About';
import Contact from './pages/Contact';
import Login from './pages/Login';
import Register from './pages/Register';

// Protected pages
import Projects from './pages/Projects';
import Dashboard from './pages/Dashboard';
import Board from './pages/Board';
import Backlog from './pages/Backlog';
import Sprints from './pages/Sprints';
import FileTickets from './pages/FileTickets';
import Activity from './pages/Activity';
import Team from './pages/Team';

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return user ? <>{children}</> : <Navigate to="/login" />;
}

function PublicOnlyRoute({ children }: { children: React.ReactNode }) {
  const { user, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return user ? <Navigate to="/projects" /> : <>{children}</>;
}

function AppRoutes() {
  return (
    <Routes>
      {/* Public pages - accessible to everyone */}
      <Route path="/" element={<Home />} />
      <Route path="/about" element={<About />} />
      <Route path="/contact" element={<Contact />} />
      
      {/* Auth pages - redirect to projects if logged in */}
      <Route path="/login" element={<PublicOnlyRoute><Login /></PublicOnlyRoute>} />
      <Route path="/register" element={<PublicOnlyRoute><Register /></PublicOnlyRoute>} />
      
      {/* Protected pages */}
      <Route path="/projects" element={<PrivateRoute><Projects /></PrivateRoute>} />
      <Route path="/dashboard" element={<PrivateRoute><Layout><Dashboard /></Layout></PrivateRoute>} />
      <Route path="/board" element={<PrivateRoute><Layout><Board /></Layout></PrivateRoute>} />
      <Route path="/backlog" element={<PrivateRoute><Layout><Backlog /></Layout></PrivateRoute>} />
      <Route path="/sprints" element={<PrivateRoute><Layout><Sprints /></Layout></PrivateRoute>} />
      <Route path="/file-tickets" element={<PrivateRoute><Layout><FileTickets /></Layout></PrivateRoute>} />
      <Route path="/activity" element={<PrivateRoute><Layout><Activity /></Layout></PrivateRoute>} />
      <Route path="/team" element={<PrivateRoute><Layout><Team /></Layout></PrivateRoute>} />
      
      {/* Catch-all redirect */}
      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <ProjectProvider>
          <NotificationProvider>
            <AppRoutes />
            <Toaster position="top-right" />
          </NotificationProvider>
        </ProjectProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
