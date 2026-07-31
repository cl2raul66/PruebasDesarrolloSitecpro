<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import type { Categoria, Prioridad, SolicitudDetalle, SolicitudRequest } from '../types'
import AppIcon from './AppIcon.vue'

const props = defineProps<{
  modo: 'crear' | 'editar'
  categorias: Categoria[]
  inicial?: SolicitudDetalle | null
  enviando: boolean
}>()

const emit = defineEmits<{
  enviar: [payload: SolicitudRequest]
  cancelar: []
}>()

const prioridades: Prioridad[] = ['Baja', 'Media', 'Alta', 'Critica']

const titulo = ref('')
const descripcion = ref('')
const categoriaId = ref('')
const prioridad = ref<Prioridad>('Media')

const intentado = ref(false)

const errores = reactive({
  titulo: null as string | null,
  descripcion: null as string | null,
  categoria: null as string | null,
})

watch(
  () => props.inicial,
  (solicitud) => {
    if (solicitud) {
      titulo.value = solicitud.titulo
      descripcion.value = solicitud.descripcion
      categoriaId.value = solicitud.categoria.id
      prioridad.value = solicitud.prioridad
    }
  },
  { immediate: true },
)

const esValido = computed(() => {
  errores.titulo = titulo.value.trim().length < 5 || titulo.value.trim().length > 120
    ? 'El título debe tener entre 5 y 120 caracteres.'
    : null
  errores.descripcion = descripcion.value.trim().length < 10 || descripcion.value.trim().length > 4000
    ? 'La descripción debe tener entre 10 y 4000 caracteres.'
    : null
  errores.categoria = categoriaId.value === '' ? 'Debes seleccionar una categoría.' : null
  return errores.titulo === null && errores.descripcion === null && errores.categoria === null
})

function enviar(): void {
  intentado.value = true
  if (!esValido.value || props.enviando) {
    return
  }
  emit('enviar', {
    titulo: titulo.value.trim(),
    descripcion: descripcion.value.trim(),
    categoriaId: categoriaId.value,
    prioridad: prioridad.value,
  })
}

function cancelar(): void {
  emit('cancelar')
}
</script>

<template>
  <form class="form" novalidate @submit.prevent="enviar">
    <div class="campo">
      <label class="campo__etiqueta" for="form-titulo">Título</label>
      <input
        id="form-titulo"
        v-model="titulo"
        class="campo__control"
        type="text"
        maxlength="120"
        data-testid="form-titulo"
      />
      <p v-if="intentado && errores.titulo" class="campo__error" data-testid="error-titulo">
        {{ errores.titulo }}
      </p>
    </div>

    <div class="campo">
      <label class="campo__etiqueta" for="form-descripcion">Descripción</label>
      <textarea
        id="form-descripcion"
        v-model="descripcion"
        class="campo__control"
        rows="5"
        maxlength="4000"
        data-testid="form-descripcion"
      ></textarea>
      <p v-if="intentado && errores.descripcion" class="campo__error" data-testid="error-descripcion">
        {{ errores.descripcion }}
      </p>
    </div>

    <div class="campo">
      <label class="campo__etiqueta" for="form-categoria">Categoría</label>
      <select
        id="form-categoria"
        v-model="categoriaId"
        class="campo__control"
        data-testid="form-categoria"
      >
        <option value="" disabled>Selecciona una categoría</option>
        <option v-for="categoria in categorias" :key="categoria.id" :value="categoria.id">
          {{ categoria.nombre }}
        </option>
      </select>
      <p v-if="intentado && errores.categoria" class="campo__error" data-testid="error-categoria">
        {{ errores.categoria }}
      </p>
    </div>

    <div class="campo">
      <label class="campo__etiqueta" for="form-prioridad">Prioridad</label>
      <select
        id="form-prioridad"
        v-model="prioridad"
        class="campo__control"
        data-testid="form-prioridad"
      >
        <option v-for="p in prioridades" :key="p" :value="p">{{ p }}</option>
      </select>
    </div>

    <div class="form__acciones">
      <button type="button" class="btn btn--secundario" data-testid="form-cancelar" @click="cancelar">
        <AppIcon name="back" :size="16" />
        Regresar
      </button>
      <button type="submit" class="btn btn--primario" data-testid="form-submit" :disabled="enviando">
        <AppIcon name="check" :size="16" />
        {{ enviando ? 'Guardando…' : modo === 'crear' ? 'Crear solicitud' : 'Guardar cambios' }}
      </button>
    </div>
  </form>
</template>

<style scoped>
.form {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-width: 560px;
  margin: 0 auto;
}

.form__acciones {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 8px;
}
</style>
