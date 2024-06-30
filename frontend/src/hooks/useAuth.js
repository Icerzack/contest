import { createContext, useContext, useMemo } from 'react';
import { useCookies } from 'react-cookie';
import { postLogin } from '../utils/api/users';

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [cookie, setCookie, removeCookie] = useCookies(['X-Auth-Token']);
  const maxAge = 3600;
  const login = async (data) => {
    const response = await postLogin(data);
    if (response.status === 200) {
      setCookie('X-Auth-Token', response.headers['x-auth-token'], {
        path: '/',
        maxAge,
      });
      return true;
    }
    return false;
  };

  const logout = () => {
    removeCookie('X-Auth-Token');
  };
  const token = cookie['X-Auth-Token'];

  const value = useMemo(
    () => ({
      token,
      maxAge,
      login,
      logout,
    }),
    [cookie['X-Auth-Token']],
  );
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => {
  return useContext(AuthContext);
};
