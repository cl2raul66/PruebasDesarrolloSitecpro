import { http } from './http'
import type { LoginRequest, LoginResponse, Usuario } from '../types'

export function login(credenciales: LoginRequest): Promise<LoginResponse> {
  return http.post<LoginResponse>('/auth/login', credenciales)
}

export function obtenerMiPerfil(): Promise<Usuario> {
  return http.get<Usuario>('/me')
}
