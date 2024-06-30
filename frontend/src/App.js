import { useEffect } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import { useRecoilState } from 'recoil';
// eslint-disable-next-line import/no-extraneous-dependencies
import { Toaster } from 'react-hot-toast';
import { useAuth } from './hooks/useAuth';
import TopScreenAlert from './components/TopScreenAlert';
import { topBarHeight } from './state/atoms';
import { getCurrentPageName } from './utils/url';

function App() {
  const navigate = useNavigate();
  const { token, maxAge } = useAuth();
  const [, setBarHeight] = useRecoilState(topBarHeight);

  const validateTokenSet = () => {
    return !!token;
  };

  useEffect(() => {
    if (validateTokenSet()) {
      const tokenTimeSet = localStorage.getItem('currentTimestamp');
      if (Math.abs(Date.now() - Number(tokenTimeSet)) > (maxAge - 300) * 1000) {
        setBarHeight(35);
      } else {
        setTimeout(
          () => {
            setBarHeight(35);
          },
          Number(tokenTimeSet) + (maxAge - 300) * 1000 - Date.now(),
        );
      }
      navigate(getCurrentPageName());
    } else {
      navigate('/auth', { replace: true });
    }
  }, []);

  return (
    <div>
      <Toaster position="top-center" reverseOrder={false} />
      <TopScreenAlert />
      <Outlet />
    </div>
  );
}

export default App;
