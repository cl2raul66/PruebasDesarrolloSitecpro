import type {
  AccionTransicion,
  EstadoSolicitud,
  Rol,
  SolicitudDetalle,
} from '../types'

const transicionesPorEstado: Record<EstadoSolicitud, AccionTransicion[]> = {
  Nueva: ['asignar', 'cancelar'],
  Asignada: ['iniciar', 'asignar', 'cancelar'],
  EnProceso: ['resolver', 'asignar', 'cancelar'],
  Resuelta: ['cerrar', 'reabrir'],
  Cerrada: [],
  Cancelada: [],
}

export function accionesPermitidas(
  estado: EstadoSolicitud,
  rol: Rol,
  solicitanteId: string,
  userId: string,
): AccionTransicion[] {
  return transicionesPorEstado[estado].filter((accion) => {
    if (rol === 'Admin') {
      return true
    }
    if (rol === 'Agente') {
      return accion !== 'cancelar'
    }
    return accion === 'cerrar' && solicitanteId === userId
  })
}

export function puedeEditar(
  estado: EstadoSolicitud,
  rol: Rol,
  solicitanteId: string,
  userId: string,
): boolean {
  if (rol === 'Admin' || rol === 'Agente') {
    return true
  }
  return solicitanteId === userId && estado === 'Nueva'
}

export function esSolicitudPropia(solicitud: SolicitudDetalle, userId: string): boolean {
  return solicitud.solicitante.id === userId
}
