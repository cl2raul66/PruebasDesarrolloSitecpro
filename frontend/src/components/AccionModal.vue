<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { AccionTransicion, UsuarioResumen } from '../types'

const props = defineProps<{
  accion: AccionTransicion | null
  agentes: UsuarioResumen[]
  cargando: boolean
  nombreSolicitud: string
}>()

const emit = defineEmits<{
  confirmar: [payload: { agenteId?: string; motivo?: string }]
  cerrar: []
}>()

const agenteId = ref<string>('')
const motivo = ref<string>('')
const error = ref<string>('')

const titulo = computed(() => {
  const etiquetas: Record<AccionTransicion, string> = {
    asignar: 'Asignar solicitud',
    iniciar: 'Iniciar atención',
    resolver: 'Resolver solicitud',
    cerrar: 'Cerrar solicitud',
    reabrir: 'Reabrir solicitud',
    cancelar: 'Cancelar solicitud',
  }
  return props.accion ? etiquetas[props.accion] : ''
})

const requiereAgente = computed(() => props.accion === 'asignar')
const requiereMotivo = computed(() => props.accion === 'resolver' || props.accion === 'cancelar')

const longitudMinimaMotivo = computed(() => (props.accion === 'resolver' ? 20 : 10))

const puedeConfirmar = computed(() => {
  if (props.cargando) {
    return false
  }
  if (requiereAgente.value) {
    return agenteId.value !== ''
  }
  if (requiereMotivo.value) {
    return motivo.value.trim().length >= longitudMinimaMotivo.value
  }
  return true
})

watch(
  () => props.accion,
  (nuevo) => {
    agenteId.value = ''
    motivo.value = ''
    error.value = ''
    if (nuevo === 'asignar' && props.agentes.length > 0) {
      agenteId.value = props.agentes[0].id
    }
  },
)

function confirmar(): void {
  if (!puedeConfirmar.value) {
    error.value = 'Revisa la información antes de confirmar.'
    return
  }
  const payload: { agenteId?: string; motivo?: string } = {}
  if (requiereAgente.value) {
    payload.agenteId = agenteId.value
  }
  if (requiereMotivo.value) {
    payload.motivo = motivo.value.trim()
  }
  emit('confirmar', payload)
}

function cerrar(): void {
  emit('cerrar')
}
</script>

<template>
  <div v-if="accion" class="modal-backdrop" data-testid="modal-accion">
    <div class="modal" role="dialog" aria-modal="true">
      <h2 class="modal__titulo">{{ titulo }}</h2>
      <p class="modal__descripcion">{{ nombreSolicitud }}</p>

      <div v-if="requiereAgente" class="campo">
        <label class="campo__etiqueta" for="modal-agente">Agente</label>
        <select
          id="modal-agente"
          class="campo__control"
          v-model="agenteId"
          data-testid="modal-select-agente"
        >
          <option value="" disabled>Selecciona un agente</option>
          <option v-for="agente in agentes" :key="agente.id" :value="agente.id">
            {{ agente.nombre }}
          </option>
        </select>
      </div>

      <div v-if="requiereMotivo" class="campo">
        <label class="campo__etiqueta" for="modal-motivo">
          Motivo (mínimo {{ longitudMinimaMotivo }} caracteres)
        </label>
        <textarea
          id="modal-motivo"
          class="campo__control"
          rows="4"
          v-model="motivo"
          data-testid="modal-motivo"
        ></textarea>
      </div>

      <p v-if="error" class="modal__error" data-testid="modal-error">{{ error }}</p>

      <div class="modal__acciones">
        <button type="button" class="btn btn--secundario" data-testid="modal-cancelar" @click="cerrar">
          Cancelar
        </button>
        <button
          type="button"
          class="btn btn--primario"
          data-testid="modal-confirmar"
          :disabled="!puedeConfirmar"
          @click="confirmar"
        >
          {{ cargando ? 'Guardando…' : 'Confirmar' }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 900;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  background: rgba(0, 0, 0, 0.45);
}

.modal {
  width: 100%;
  max-width: 460px;
  background: var(--bg);
  border-radius: 10px;
  padding: 24px;
  box-shadow: var(--shadow);
}

.modal__titulo {
  margin: 0 0 4px;
  font-size: 20px;
}

.modal__descripcion {
  margin: 0 0 16px;
  color: var(--text);
  font-size: 14px;
}

.modal__error {
  color: var(--error);
  font-size: 14px;
  margin: 8px 0 0;
}

.modal__acciones {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 20px;
}
</style>
