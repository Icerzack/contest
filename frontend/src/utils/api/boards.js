import { client } from './api';

export const getBoard = (authToken, boardId) => {
  return client.get(`/boards/${boardId}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const postBoard = (data, authToken) => {
  return client.post('/boards', JSON.stringify(data), {
    headers: {
      'Content-Type': 'application/json',
      'X-Auth-Token': authToken,
    },
  });
};

export const putBoard = (boardId, data, authToken) => {
  return client.put(`/boards/${boardId}`, JSON.stringify(data), {
    headers: {
      'Content-Type': 'application/json',
      'X-Auth-Token': authToken,
    },
  });
};

export const deleteBoard = (boardId, authToken) => {
  return client.delete(`/boards/${boardId}`, {
    headers: {
      'Content-Type': 'application/json',
      'X-Auth-Token': authToken,
    },
  });
};
