import { defineStore } from 'pinia'
import type { Categoria, EstadoSolicitud, Prioridad, SolicitudPaginada } from '../types'
import { obtenerCategorias } from '../api/categorias'
import { listarSolicitudes } from '../api/solicitudes'

export interface FiltrosListado {
  estado: EstadoSolicitud | ''
  prioridad: Prioridad | ''
  categoriaId: string
  vencidas: '' | 'true' | 'false'
  busqueda: string
}

export function filtrosIniciales(): FiltrosListado {
  return {
    estado: '',
    prioridad: '',
    categoriaId: '',
    vencidas: '',
    busqueda: '',
  }
}

export const useSolicitudesStore = defineStore('solicitudes', {
  state: () => ({
    filtros: filtrosIniciales(),
    page: 1,
    pageSize: 20,
    resultado: null as SolicitudPaginada | null,
    categorias: [] as Categoria[],
    cargando: false,
    error: null as string | null,
  }),

  getters: {
    totalPaginas: (state): number => state.resultado?.totalPaginas ?? 0,
    total: (state): number => state.resultado?.total ?? 0,
  },

  actions: {
    async cargarCategorias(): Promise<void> {
      if (this.categorias.length > 0) {
        return
      }
      this.categorias = await obtenerCategorias()
    },

    async cargarListado(): Promise<void> {
      this.cargando = true
      this.error = null
      try {
        const vencidas = this.filtros.vencidas === '' ? undefined : this.filtros.vencidas === 'true'
        this.resultado = await listarSolicitudes({
          estado: this.filtros.estado === '' ? undefined : this.filtros.estado,
          prioridad: this.filtros.prioridad === '' ? undefined : this.filtros.prioridad,
          categoriaId: this.filtros.categoriaId === '' ? undefined : this.filtros.categoriaId,
          vencidas,
          q: this.filtros.busqueda.trim() === '' ? undefined : this.filtros.busqueda.trim(),
          page: this.page,
          pageSize: this.pageSize,
        })
      } catch (e) {
        this.error = e instanceof Error ? e.message : 'No se pudo cargar el listado.'
      } finally {
        this.cargando = false
      }
    },

    cambiarPagina(pagina: number): void {
      if (pagina < 1 || (this.totalPaginas > 0 && pagina > this.totalPaginas)) {
        return
      }
      this.page = pagina
      void this.cargarListado()
    },

    aplicarFiltros(): void {
      this.page = 1
      void this.cargarListado()
    },

    limpiarFiltros(): void {
      this.filtros = filtrosIniciales()
      this.page = 1
      void this.cargarListado()
    },
  },
})
