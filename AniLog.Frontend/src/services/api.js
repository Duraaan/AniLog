import axios from 'axios';

const API_URL = 'http://localhost:5224/api';

export const searchAnime = (query) =>
  axios.get(`${API_URL}/search?q=${encodeURIComponent(query)}`);

export const getAllAnime = (status, page = 1, pageSize = 20) =>
  axios.get(`${API_URL}/anime`, { params: { ...(status ? { status } : {}), page, pageSize } });

export const addAnime = (data) =>
  axios.post(`${API_URL}/anime`, data);

export const updateAnime = (id, data) =>
  axios.put(`${API_URL}/anime/${id}`, data);

export const deleteAnime = (id) =>
  axios.delete(`${API_URL}/anime/${id}`);
