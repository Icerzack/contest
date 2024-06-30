import { client } from './api';

export const getCollections = (authToken) => {
  return client.get('/collections/all', {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const getCollectionById = (collectionId, authToken) => {
  return client.get(`/collections/${collectionId}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const postCollection = (data, authToken) => {
  return client.post('/collections', JSON.stringify(data), {
    headers: {
      'Content-Type': 'application/json',
      'X-Auth-Token': authToken,
    },
  });
};

export const putCollection = (collectionId, data, authToken) => {
  return client.put(`/collections/${collectionId}`, JSON.stringify(data), {
    headers: {
      'Content-Type': 'application/json',
      'X-Auth-Token': authToken,
    },
  });
};

export const deleteCollection = (collectionId, authToken) => {
  return client.delete(`/collections/${collectionId}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};
export const putBoardToCollection = (data, authToken) => {
  return client.put(`/collections/${data.collectionId}/${data.boardId}`, null, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const deleteBoardFromCollection = (data, authToken) => {
  return client.delete(`/collections/${data.collectionId}/${data.boardId}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const getSharedRoles = (collectionId, authToken) => {
  return client.get(`/collections/share/${collectionId}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
  });
};

export const putSharedRoles = (collectionId, data, authToken) => {
  return client.put(
    `/collections/share/${collectionId}`,
    JSON.stringify(data),
    {
      headers: {
        'Content-Type': 'application/json',
        'X-Auth-Token': authToken,
      },
    },
  );
};

export const deleteSharedRoles = (collectionId, data, authToken) => {
  return client.delete(`/collections/share/${collectionId}`, {
    headers: {
      'X-Auth-Token': authToken,
    },
    data: {
      reader: data.reader,
      editor: data.editor,
      moderator: data.moderator,
    },
  });
};
