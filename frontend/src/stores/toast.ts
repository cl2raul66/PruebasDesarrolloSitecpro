import { defineStore } from 'pinia'

export type TipoToast = 'exito' | 'error' | 'info'

export const useToastStore = defineStore('toast', {
  state: () => ({
    mensaje: null as string | null,
    tipo: 'info' as TipoToast,
    visibilidad: false,
    _temporizador: null as ReturnType<typeof setTimeout> | null,
  }),

  actions: {
    mostrar(mensaje: string, tipo: TipoToast = 'info'): void {
      this.mensaje = mensaje
      this.tipo = tipo
      this.visibilidad = true
      if (this._temporizador !== null) {
        clearTimeout(this._temporizador)
      }
      this._temporizador = setTimeout(() => {
        this.visibilidad = false
        this.mensaje = null
      }, 4000)
    },

    ocultar(): void {
      if (this._temporizador !== null) {
        clearTimeout(this._temporizador)
        this._temporizador = null
      }
      this.visibilidad = false
    },
  },
})
