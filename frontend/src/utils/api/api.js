import axios from 'axios';

const BACKEND_API_URL = 'https://.../';

export const client = axios.create({
  baseURL: BACKEND_API_URL,
  validateStatus: () => true,
});
