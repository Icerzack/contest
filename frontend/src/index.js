import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { RecoilRoot } from 'recoil';
import BoardPage from './pages/BoardPage';
import App from './App';
import AuthPage from './pages/AuthPage';
import PersonalPage from './pages/PersonalPage';
import SharedPage from './pages/SharedPage';
import { ProtectedRoute } from './helpers/ProtectedRoute';
import { AuthProvider } from './hooks/useAuth';
import NotFoundPage from './pages/NotFoundPage';
import AnonymousPage from './pages/AnonymousPage';

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      {
        path: '/personal',
        element: (
          <ProtectedRoute>
            <PersonalPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/board',
        element: (
          <ProtectedRoute>
            <BoardPage editMode={false} />
          </ProtectedRoute>
        ),
      },
      {
        path: '/board/:id',
        element: (
          <ProtectedRoute>
            <BoardPage editMode />
          </ProtectedRoute>
        ),
      },
      {
        path: '/shared',
        element: (
          <ProtectedRoute>
            <SharedPage />
          </ProtectedRoute>
        ),
      },
      {
        path: '/auth',
        element: <AuthPage />,
      },
      {
        path: '/view/:id',
        element: <AnonymousPage />,
      },
      {
        path: '*',
        element: <NotFoundPage />,
      },
    ],
  },
]);

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <AuthProvider>
      <RecoilRoot>
        <RouterProvider router={router} />
      </RecoilRoot>
    </AuthProvider>
  </React.StrictMode>,
);
