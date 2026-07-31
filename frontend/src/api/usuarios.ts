import { http } from './http'
import type { UsuarioResumen } from '../types'

export function obtenerAgentes(): Promise<UsuarioResumen[]> {
  return http.get<UsuarioResumen[]>('/usuarios/agentes')
}
