import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5058/api',
  headers: { 'Content-Type': 'application/json' },
});

export default apiClient;

// CompanyId — gerçek projede auth'dan gelir, şimdilik sabit
export const COMPANY_ID = 'COMPANY_001';
