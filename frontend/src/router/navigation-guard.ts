import type {
  RouteLocationNormalized
} from 'vue-router'

export const authGuard = async (to: RouteLocationNormalized) => {
  // TODO
  const isAuthenticated = true;

  if (!to.meta.public) {
    if (!isAuthenticated && to.name !== 'login') {
      return { name: 'login' };
    }
  }
}
