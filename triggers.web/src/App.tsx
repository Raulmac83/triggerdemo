import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { theme } from './theme';
import DashboardLayout from './layout/DashboardLayout';
import DashboardPage from './pages/DashboardPage';
import TriggersPage from './pages/TriggersPage';
import TriggerNewPage from './pages/TriggerNewPage';
import TriggerEditPage from './pages/TriggerEditPage';
import NotificationsPage from './pages/NotificationsPage';
import TriggerMethodsPage from './pages/TriggerMethodsPage';
import DocsPage from './pages/DocsPage';
import LoginPage from './pages/LoginPage';
import { AuthProvider } from './auth/AuthContext';
import RequireAuth from './auth/RequireAuth';

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<RequireAuth />}>
              <Route element={<DashboardLayout />}>
                <Route index element={<DashboardPage />} />
                <Route path="triggers" element={<TriggersPage />} />
                <Route path="triggers/new" element={<TriggerNewPage />} />
                <Route path="triggers/:id/edit" element={<TriggerEditPage />} />
                <Route path="notifications" element={<NotificationsPage />} />
                <Route path="methods" element={<TriggerMethodsPage />} />
                <Route path="docs" element={<DocsPage />} />
                <Route path="docs/:id" element={<DocsPage />} />
              </Route>
            </Route>
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  );
}
