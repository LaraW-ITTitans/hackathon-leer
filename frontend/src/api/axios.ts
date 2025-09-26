import axios from 'axios';
import type { AxiosRequestConfig } from 'axios';
import { storeToRefs } from 'pinia';
import useUserStore from '@/stores/user.ts'

axios.defaults.withCredentials = false;

const axiosInstance = axios.create({
  // needed to prevent double json parsing (axios vs nswag)
  transformResponse: undefined,
});

axiosInstance.interceptors.request.use((config: AxiosRequestConfig) => {
  const userStore = useUserStore();
  const user = storeToRefs(userStore);
  const modifiedConfig = config;

  if (user && modifiedConfig.headers) {
    modifiedConfig.headers.Authorization = `Bearer ${user.accessToken}`;
  }

  return modifiedConfig;
});

export default axiosInstance;
