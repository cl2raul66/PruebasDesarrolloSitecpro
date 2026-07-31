import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: { public: true },
  },
  {
    path: '/',
    redirect: '/solicitudes',
  },
  {
    path: '/solicitudes',
    name: 'solicitudes',
    component: () => import('../views/SolicitudesListView.vue'),
  },
  {
    path: '/solicitudes/nueva',
    name: 'solicitud-nueva',
    component: () => import('../views/SolicitudFormView.vue'),
  },
  {
    path: '/solicitudes/:id',
    name: 'solicitud-detalle',
    component: () => import('../views/SolicitudDetalleView.vue'),
    props: true,
  },
  {
    path: '/solicitudes/:id/editar',
    name: 'solicitud-editar',
    component: () => import('../views/SolicitudFormView.vue'),
    props: true,
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()

  if (to.meta.public) {
    if (auth.autenticado) {
      return { name: 'solicitudes' }
    }
    return true
  }

  if (auth.token && !auth.usuario) {
    const restaurada = await auth.restaurarSesion()
    if (!restaurada) {
      return { name: 'login' }
    }
    return true
  }

  if (!auth.autenticado) {
    return { name: 'login' }
  }

  return true
})

export default router
