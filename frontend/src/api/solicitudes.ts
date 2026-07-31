import { http } from './http'
import type {
  SolicitudDetalle,
  SolicitudListaParams,
  SolicitudPaginada,
  SolicitudRequest,
  TransicionRequest,
} from '../types'

function aQueryString(params: Record<string, string | number | boolean | undefined>): string {
  const partes = Object.entries(params)
    .filter(([, valor]) => valor !== undefined && valor !== null && valor !== '')
    .map(([clave, valor]) => `${encodeURIComponent(clave)}=${encodeURIComponent(String(valor))}`)
  return partes.length > 0 ? `?${partes.join('&')}` : ''
}

export function listarSolicitudes(params: SolicitudListaParams): Promise<SolicitudPaginada> {
  const query = aQueryString({
    estado: params.estado,
    prioridad: params.prioridad,
    categoriaId: params.categoriaId,
    agenteId: params.agenteId,
    q: params.q,
    vencidas: params.vencidas,
    page: params.page,
    pageSize: params.pageSize,
    sort: params.sort,
  })
  return http.get<SolicitudPaginada>(`/solicitudes${query}`)
}

export function crearSolicitud(body: SolicitudRequest): Promise<SolicitudDetalle> {
  return http.post<SolicitudDetalle>('/solicitudes', body)
}

export function obtenerSolicitud(id: string): Promise<SolicitudDetalle> {
  return http.get<SolicitudDetalle>(`/solicitudes/${id}`)
}

export function editarSolicitud(id: string, body: SolicitudRequest): Promise<SolicitudDetalle> {
  return http.put<SolicitudDetalle>(`/solicitudes/${id}`, body)
}

export function ejecutarTransicion(id: string, body: TransicionRequest): Promise<SolicitudDetalle> {
  return http.post<SolicitudDetalle>(`/solicitudes/${id}/transiciones`, body)
}
