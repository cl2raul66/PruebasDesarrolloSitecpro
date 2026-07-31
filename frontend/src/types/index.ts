export type Rol = 'Admin' | 'Agente' | 'Solicitante'

export type Prioridad = 'Baja' | 'Media' | 'Alta' | 'Critica'

export type EstadoSolicitud =
  | 'Nueva'
  | 'Asignada'
  | 'EnProceso'
  | 'Resuelta'
  | 'Cerrada'
  | 'Cancelada'

export type AccionTransicion =
  | 'asignar'
  | 'iniciar'
  | 'resolver'
  | 'cerrar'
  | 'reabrir'
  | 'cancelar'

export interface Usuario {
  id: string
  nombre: string
  email: string
  rol: Rol
  tenantId: string
  tenantNombre: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: Usuario
}

export interface LoginRequest {
  email: string
  password: string
}

export interface Categoria {
  id: string
  nombre: string
  slaHoras: number
}

export interface CategoriaResumen {
  id: string
  nombre: string
}

export interface UsuarioResumen {
  id: string
  nombre: string
}

export interface SolicitudListItem {
  id: string
  codigo: string
  titulo: string
  estado: EstadoSolicitud
  prioridad: Prioridad
  categoria: CategoriaResumen
  agente: UsuarioResumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudPaginada {
  items: SolicitudListItem[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface SolicitudDetalle {
  id: string
  codigo: string
  titulo: string
  descripcion: string
  estado: EstadoSolicitud
  prioridad: Prioridad
  categoria: CategoriaResumen
  solicitante: UsuarioResumen
  agente: UsuarioResumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
  vencida: boolean
}

export interface SolicitudRequest {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: Prioridad
}

export interface TransicionRequest {
  accion: AccionTransicion
  agenteId?: string
  motivo?: string
}

export interface SolicitudListaParams {
  estado?: EstadoSolicitud
  prioridad?: Prioridad
  categoriaId?: string
  agenteId?: string
  q?: string
  vencidas?: boolean
  page: number
  pageSize: number
  sort?: string
}

export interface ApiError {
  type: string
  title: string
  status: number
  detail: string
  codigo: string
  errores?: Record<string, string[]>
}
