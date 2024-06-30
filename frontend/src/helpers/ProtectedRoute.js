import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { getBoard } from '../utils/api/boards';
import NotFoundPage from '../pages/NotFoundPage';

export function ProtectedRoute({ children }) {
  const { token } = useAuth();
  const [finished, setFinished] = useState(false);
  const [elementsToRender, setElementsToRender] = useState();
  const navigate = useNavigate();

  const validateBoard = async () => {
    const currentUrl = window.location.pathname;
    const regex = /^\/board\/\d+$/;
    const boardId = currentUrl.substring(currentUrl.lastIndexOf('/') + 1);
    if (regex.test(currentUrl)) {
      const response = await getBoard(token, boardId);
      return Math.floor(response.status / 100) !== 4;
    }
    return true;
  };

  useEffect(() => {
    setElementsToRender(children);
    setFinished(false);
  }, [window.location.href]);

  useEffect(() => {
    if (!token) {
      navigate('/auth', { replace: true });
    }
    validateBoard().then((response) => {
      if (!response) {
        setElementsToRender(<NotFoundPage />);
      }
      setFinished(true);
    });
  }, [window.location.href]);

  if (!finished) {
    return <div />;
  }

  return elementsToRender;
}
