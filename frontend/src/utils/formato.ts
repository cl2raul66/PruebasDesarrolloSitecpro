import type { EstadoSolicitud, Prioridad } from '../types'

export function formatFecha(iso: string | null | undefined): string {
  if (!iso) {
    return '—'
  }
  const fecha = new Date(iso)
  if (Number.isNaN(fecha.getTime())) {
    return iso
  }
  return new Intl.DateTimeFormat('es-ES', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(fecha)
}

export const etiquetaPrioridad: Record<Prioridad, string> = {
  Baja: 'Baja',
  Media: 'Media',
  Alta: 'Alta',
  Critica: 'Crítica',
}

export const etiquetaEstado: Record<EstadoSolicitud, string> = {
  Nueva: 'Nueva',
  Asignada: 'Asignada',
  EnProceso: 'En proceso',
  Resuelta: 'Resuelta',
  Cerrada: 'Cerrada',
  Cancelada: 'Cancelada',
}
