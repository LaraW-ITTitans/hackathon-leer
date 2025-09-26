import {
  createRouter,
  createWebHistory
} from 'vue-router'
import { authGuard } from './navigation-guard'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'landing-page',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/landing/LandingView.vue'),
      meta: { public: true },
    },
    {
      path: '/auth',
      name: 'login',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/auth/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/home',
      name: 'home',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/home/HomeView.vue'),
      meta: { public: true },
    },

    // last but not least
    {
      path: '/:catchAll(.*)*',
      component: () => import('../views/NotFoundView.vue'),
      meta: { public: true },
    }
  ],
})

router.beforeEach(authGuard);

export default router
