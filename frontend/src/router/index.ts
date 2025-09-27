import {
  createRouter,
  createWebHistory
} from 'vue-router'

import { authGuard } from './navigation-guard'
import { afterEach } from './page-title.ts'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [

    // Publicly available pages
    {
      path: '/',
      name: 'landing-page',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/landing/LandingView.vue'),
      meta: { public: true, title: 'Landing Page' },
    },
    {
      path: '/auth',
      name: 'login',
      component: () => import('../views/auth/LoginView.vue'),
      meta: { public: true, title: 'Login' },
    },
    {
      path: '/home',
      name: 'home',
      component: () => import('../views/home/HomeView.vue'),
      meta: { public: true, title: 'Home' },
    },

    // Admin Pages
    {
      path: '/admin/roles',
      name: 'role-management',
      component: () => import('../views/roles/RoleOverviewView.vue'),
      meta: { public: false, title: 'Administration - Rollen' },
    },

    // User Pages
    {
      path: '/users/me',
      name: 'user-profile',
      component: () => import('../views/user/profile/UserProfileView.vue'),
      meta: { public: false, title: 'Eigenes Profil' },
    },

    // last but not least
    {
      path: '/:catchAll(.*)*',
      component: () => import('../views/NotFoundView.vue'),
      meta: { public: true, },
    }
  ],
})

router.beforeEach(authGuard)
router.afterEach(afterEach)

export default router
