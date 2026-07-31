import { http } from './http'
import type { Categoria } from '../types'

export function obtenerCategorias(): Promise<Categoria[]> {
  return http.get<Categoria[]>('/categorias')
}
