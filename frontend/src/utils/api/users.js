import { client } from './api';

export const postLogin = (credentials) => {
  return client.post('/users/login', JSON.stringify(credentials), {
    headers: {
      'Content-Type': 'application/json',
    },
  });
};

export const getAboutMe = (authToken) => {
  return client.get('/users/about/me', {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const getAboutId = (userId, authToken) => {
  return client.get(`/users/about/id/${userId}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const getAboutName = (userName, authToken) => {
  return client.get(`/users/about/name/${userName}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};
