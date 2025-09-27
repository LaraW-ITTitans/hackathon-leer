import { defineStore } from 'pinia'
import type {
  AuthResult,
  User,
  UserState
} from '@/types'

const useUserStore = defineStore('user', {
  state: (): UserState => ({
    isAuthenticated: false,
    user: undefined,
    accessToken: undefined,
    tokenExpiresAt: 0,
  }),
  actions: {
    setUser(user: User) {
      this.user = user;
    },
    setToken(token: string) {
      this.accessToken = token;
    },
    setAuthenticated(flag: boolean) {
      this.isAuthenticated = flag;
    },
    login(user: AuthResult) {
      this.user = user;
      this.accessToken = user.token;
      this.isAuthenticated = true;
      this.tokenExpiresAt = user.expiration;
    },
    logout() {
      this.user = undefined;
      this.isAuthenticated = false;
    },
  },
  persist: true,
});

export default useUserStore;
