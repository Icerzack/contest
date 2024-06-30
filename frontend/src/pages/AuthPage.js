import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { useRecoilState } from 'recoil';
import { useAuth } from '../hooks/useAuth';
import Alert from '../components/Alert';
import { topBarHeight } from '../state/atoms';
import ProfileSVG from '../svg/ProfileSVG';

export default function AuthPage() {
  const [enteredUsername, setEnteredUsername] = useState(' ');
  const [enteredPassword, setEnteredPassword] = useState(' ');
  const [alertVisible, setAlertVisible] = useState(false);
  const navigate = useNavigate();
  const { token, login, maxAge } = useAuth();
  const [barHeight, setBarHeight] = useRecoilState(topBarHeight);

  const auth = () => {
    setAlertVisible(false);
    const data = {
      username: enteredUsername,
      password: enteredPassword,
    };
    login(data).then((status) => {
      if (!status) {
        setAlertVisible(true);
        return;
      }
      localStorage.setItem('currentTimestamp', Date.now());
      setTimeout(() => {
        setBarHeight(35);
      }, maxAge * 1000);
      navigate('/personal');
    });
  };

  const updateUsername = (event) => {
    setEnteredUsername(event.target.value);
  };

  const updatePassword = (event) => {
    setEnteredPassword(event.target.value);
  };

  useEffect(() => {
    if (token) {
      const finalPage = '/personal';
      navigate(finalPage);
    }
  }, []);

  const h = window.innerHeight;
  const contentHeight = h - barHeight;

  return (
    <div
      className="flex flex-col justify-center px-6 py-12 lg:px-8"
      style={{ height: contentHeight }}
    >
      <div className="flex flex-col justify-center items-center">
        <ProfileSVG width="50" height="50" />
        <h2 className="mt-5 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
          Sign in to your account
        </h2>
      </div>

      <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-sm">
        <div className="space-y-6">
          <div>
            <div className="block text-sm font-medium leading-6 text-gray-900">
              Username
            </div>
            <div className="mt-2">
              <input
                id="email"
                className="block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                onChange={updateUsername}
              />
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between">
              <div className="block text-sm font-medium leading-6 text-gray-900">
                Password
              </div>
            </div>
            <div className="mt-2">
              <input
                id="password"
                type="password"
                className="block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                onChange={updatePassword}
              />
            </div>
          </div>
          <div>
            <button
              type="submit"
              className="flex w-full justify-center rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
              onClick={auth}
            >
              Sign in
            </button>
          </div>
        </div>
        <div
          className={`mt-3 mb-4 transition-opacity duration-300 h-8${alertVisible ? ' opacity-100' : ' opacity-0'}`}
        >
          <Alert
            title="Error!"
            description="Oops, wrong username or password!"
            padding="p-3"
          />
        </div>
        <p className="text-center text-sm text-gray-500">
          Not a member? Contact admin to request a new account.
        </p>
      </div>
    </div>
  );
}
