<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { SolicitudDetalle, SolicitudRequest } from '../types'
import { obtenerCategorias } from '../api/categorias'
import { crearSolicitud, editarSolicitud, obtenerSolicitud } from '../api/solicitudes'
import { ApiClientError } from '../api/http'
import { useToastStore } from '../stores/toast'
import SolicitudForm from '../components/SolicitudForm.vue'

const route = useRoute()
const router = useRouter()
const toast = useToastStore()

const esEdicion = computed(() => route.name === 'solicitud-editar')

const solicitudInicial = ref<SolicitudDetalle | null>(null)
const categorias = ref([] as Awaited<ReturnType<typeof obtenerCategorias>>)
const cargando = ref(true)
const error = ref<string | null>(null)
const enviando = ref(false)

async function cargarDatos(): Promise<void> {
  cargando.value = true
  error.value = null
  try {
    const [cats, detalle] = await Promise.all([
      obtenerCategorias(),
      esEdicion.value ? obtenerSolicitud(String(route.params.id)) : Promise.resolve(null),
    ])
    categorias.value = cats
    solicitudInicial.value = detalle
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudieron cargar los datos del formulario.'
  } finally {
    cargando.value = false
  }
}

async function enviar(payload: SolicitudRequest): Promise<void> {
  enviando.value = true
  try {
    const guardada = esEdicion.value
      ? await editarSolicitud(String(route.params.id), payload)
      : await crearSolicitud(payload)
    toast.mostrar(esEdicion.value ? 'Solicitud actualizada.' : 'Solicitud creada.', 'exito')
    void router.push({ name: 'solicitud-detalle', params: { id: guardada.id } })
  } catch (e) {
    const detalle = e instanceof ApiClientError ? e.problem.detail : e instanceof Error ? e.message : ''
    toast.mostrar(detalle || 'No se pudo guardar la solicitud.', 'error')
  } finally {
    enviando.value = false
  }
}

function cancelar(): void {
  if (esEdicion.value && solicitudInicial.value) {
    void router.push({ name: 'solicitud-detalle', params: { id: solicitudInicial.value.id } })
  } else {
    void router.push({ name: 'solicitudes' })
  }
}

onMounted(() => {
  void cargarDatos()
})
</script>

<template>
  <main class="pagina">
    <h1 class="pagina__titulo">{{ esEdicion ? 'Editar solicitud' : 'Nueva solicitud' }}</h1>

    <div v-if="cargando" class="estado" data-testid="form-cargando">Cargando formulario…</div>

    <div v-else-if="error" class="estado estado--error" role="alert">
      {{ error }}
      <button type="button" class="btn btn--secundario" @click="cargarDatos()">Reintentar</button>
    </div>

    <SolicitudForm
      v-else
      :modo="esEdicion ? 'editar' : 'crear'"
      :categorias="categorias"
      :inicial="solicitudInicial"
      :enviando="enviando"
      @enviar="enviar"
      @cancelar="cancelar"
    />
  </main>
</template>

<style scoped>
.estado {
  padding: 40px 16px;
  text-align: center;
  color: var(--text);
  border: 1px dashed var(--border);
  border-radius: 10px;
}

.estado--error {
  color: var(--error);
  border-color: var(--error);
}
</style>
