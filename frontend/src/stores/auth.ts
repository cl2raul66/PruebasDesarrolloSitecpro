import { defineStore } from 'pinia'
import type { LoginRequest, Usuario } from '../types'
import { login as apiLogin, obtenerMiPerfil } from '../api/auth'
import { guardarSesion, limpiarSesion, leerToken } from '../api/http'

const USUARIO_KEY = 'mesasitec_usuario'

function usuarioGuardado(): Usuario | null {
  const raw = localStorage.getItem(USUARIO_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as Usuario
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: leerToken(),
    usuario: usuarioGuardado(),
  }),

  getters: {
    autenticado: (state): boolean => state.token !== null && state.usuario !== null,
    rol: (state): Usuario['rol'] | null => state.usuario?.rol ?? null,
  },

  actions: {
    async login(credenciales: LoginRequest): Promise<Usuario> {
      const respuesta = await apiLogin(credenciales)
      guardarSesion(respuesta.accessToken, respuesta.usuario)
      this.token = respuesta.accessToken
      this.usuario = respuesta.usuario
      return respuesta.usuario
    },

    async restaurarSesion(): Promise<boolean> {
      if (this.token && !this.usuario) {
        try {
          this.usuario = await obtenerMiPerfil()
          localStorage.setItem(USUARIO_KEY, JSON.stringify(this.usuario))
          return true
        } catch {
          this.cerrarSesion()
          return false
        }
      }
      return this.autenticado
    },

    cerrarSesion(): void {
      limpiarSesion()
      this.token = null
      this.usuario = null
    },
  },
})
