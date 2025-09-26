import axios from 'axios';
import type { AxiosRequestConfig } from 'axios';
import { storeToRefs } from 'pinia';
import useUserStore from '@/stores/user.ts'
import type { UserState } from '@/types'

axios.defaults.withCredentials = false;

const axiosInstance = axios.create({
  // needed to prevent double json parsing (axios vs nswag)
  transformResponse: undefined,
});

axiosInstance.interceptors.request.use((config: AxiosRequestConfig) => {
  const userStore = useUserStore();
  const user: UserState = storeToRefs(userStore);
  const modifiedConfig = config;

  if (user.accessToken.value && modifiedConfig.headers) {
    modifiedConfig.headers['X-Hackathon-Token'] = user.accessToken.value;
    // modifiedConfig.headers.Authorization = `Bearer ${user.accessToken}`;
  }

  return modifiedConfig;
});

export default axiosInstance;
