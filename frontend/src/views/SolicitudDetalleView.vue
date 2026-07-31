<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { AccionTransicion, SolicitudDetalle, UsuarioResumen } from '../types'
import { obtenerSolicitud, ejecutarTransicion } from '../api/solicitudes'
import { obtenerAgentes } from '../api/usuarios'
import { ApiClientError } from '../api/http'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import { accionesPermitidas, puedeEditar } from '../utils/acciones'
import { etiquetaEstado, etiquetaPrioridad, formatFecha } from '../utils/formato'
import AccionModal from '../components/AccionModal.vue'

const props = defineProps<{ id: string }>()

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()

const solicitud = ref<SolicitudDetalle | null>(null)
const cargando = ref(true)
const error = ref<string | null>(null)

const accionActiva = ref<AccionTransicion | null>(null)
const agentes = ref<UsuarioResumen[]>([])
const cargandoAgentes = ref(false)
const modalError = ref<string | null>(null)

const userId = auth.usuario?.id ?? ''

const accionesVisibles = (): AccionTransicion[] => {
  if (!solicitud.value || !auth.rol) {
    return []
  }
  return accionesPermitidas(solicitud.value.estado, auth.rol, solicitud.value.solicitante.id, userId)
}

const puedeEditarVisible = (): boolean => {
  if (!solicitud.value || !auth.rol) {
    return false
  }
  return puedeEditar(solicitud.value.estado, auth.rol, solicitud.value.solicitante.id, userId)
}

async function cargarDetalle(): Promise<void> {
  cargando.value = true
  error.value = null
  try {
    solicitud.value = await obtenerSolicitud(props.id)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'No se pudo cargar la solicitud.'
  } finally {
    cargando.value = false
  }
}

async function abrirAccion(accion: AccionTransicion): Promise<void> {
  modalError.value = null
  if (accion === 'asignar') {
    cargandoAgentes.value = true
    try {
      agentes.value = await obtenerAgentes()
    } catch (e) {
      modalError.value = e instanceof Error ? e.message : 'No se pudieron cargar los agentes.'
      return
    } finally {
      cargandoAgentes.value = false
    }
  }
  accionActiva.value = accion
}

function cerrarModal(): void {
  accionActiva.value = null
  modalError.value = null
}

async function confirmarAccion(payload: { agenteId?: string; motivo?: string }): Promise<void> {
  if (!accionActiva.value || !solicitud.value) {
    return
  }
  cargandoAgentes.value = true
  modalError.value = null
  try {
    solicitud.value = await ejecutarTransicion(solicitud.value.id, {
      accion: accionActiva.value,
      agenteId: payload.agenteId,
      motivo: payload.motivo,
    })
    accionActiva.value = null
    toast.mostrar('Acción aplicada correctamente.', 'exito')
  } catch (e) {
    if (e instanceof ApiClientError && e.problem.errores) {
      modalError.value = e.problem.detail
    } else {
      modalError.value = e instanceof Error ? e.message : 'No se pudo ejecutar la acción.'
    }
  } finally {
    cargandoAgentes.value = false
  }
}

function editar(): void {
  if (solicitud.value) {
    void router.push({ name: 'solicitud-editar', params: { id: solicitud.value.id } })
  }
}

function motivoActual(): string | null {
  if (!solicitud.value) {
    return null
  }
  return solicitud.value.motivoResolucion ?? solicitud.value.motivoCancelacion
}

onMounted(() => {
  void cargarDetalle()
})
</script>

<template>
  <main class="pagina">
    <div v-if="cargando" class="estado" data-testid="detalle-cargando">Cargando solicitud…</div>

    <div v-else-if="error" class="estado estado--error" role="alert" data-testid="detalle-error">
      {{ error }}
      <button type="button" class="btn btn--secundario" @click="cargarDetalle()">Reintentar</button>
    </div>

    <template v-else-if="solicitud">
      <div class="pagina__cabecera">
        <div>
          <p class="detalle__codigo" data-testid="detalle-codigo">{{ solicitud.codigo }}</p>
          <h1 class="pagina__titulo" data-testid="detalle-titulo">{{ solicitud.titulo }}</h1>
        </div>
        <div v-if="puedeEditarVisible()" class="detalle__acciones">
          <button type="button" class="btn btn--primario" data-testid="btn-editar" @click="editar">
            Editar
          </button>
          <button
            v-if="accionesVisibles().includes('asignar')"
            type="button"
            class="btn btn--accion"
            data-testid="btn-accion-asignar"
            @click="abrirAccion('asignar')"
          >
            Asignar
          </button>
          <button
            v-if="accionesVisibles().includes('iniciar')"
            type="button"
            class="btn btn--accion"
            data-testid="btn-accion-iniciar"
            @click="abrirAccion('iniciar')"
          >
            Iniciar
          </button>
          <button
            v-if="accionesVisibles().includes('resolver')"
            type="button"
            class="btn btn--accion"
            data-testid="btn-accion-resolver"
            @click="abrirAccion('resolver')"
          >
            Resolver
          </button>
          <button
            v-if="accionesVisibles().includes('cerrar')"
            type="button"
            class="btn btn--accion"
            data-testid="btn-accion-cerrar"
            @click="abrirAccion('cerrar')"
          >
            Cerrar
          </button>
          <button
            v-if="accionesVisibles().includes('reabrir')"
            type="button"
            class="btn btn--accion"
            data-testid="btn-accion-reabrir"
            @click="abrirAccion('reabrir')"
          >
            Reabrir
          </button>
          <button
            v-if="accionesVisibles().includes('cancelar')"
            type="button"
            class="btn btn--peligro"
            data-testid="btn-accion-cancelar"
            @click="abrirAccion('cancelar')"
          >
            Cancelar
          </button>
        </div>
      </div>

      <dl class="detalle__grid">
        <div class="detalle__campo">
          <dt>Estado</dt>
          <dd data-testid="detalle-estado">
            {{ etiquetaEstado[solicitud.estado] }}
            <span v-if="solicitud.vencida" class="badge badge--vencida" data-testid="detalle-vencida">
              Vencida
            </span>
          </dd>
        </div>
        <div class="detalle__campo">
          <dt>Prioridad</dt>
          <dd data-testid="detalle-prioridad">{{ etiquetaPrioridad[solicitud.prioridad] }}</dd>
        </div>
        <div class="detalle__campo">
          <dt>Categoría</dt>
          <dd data-testid="detalle-categoria">{{ solicitud.categoria.nombre }}</dd>
        </div>
        <div class="detalle__campo">
          <dt>Agente</dt>
          <dd data-testid="detalle-agente">{{ solicitud.agente?.nombre ?? 'Sin asignar' }}</dd>
        </div>
        <div class="detalle__campo">
          <dt>Solicitante</dt>
          <dd>{{ solicitud.solicitante.nombre }}</dd>
        </div>
        <div class="detalle__campo">
          <dt>Fecha de creación</dt>
          <dd data-testid="detalle-fecha-creacion">{{ formatFecha(solicitud.fechaCreacion) }}</dd>
        </div>
        <div class="detalle__campo">
          <dt>Fecha límite SLA</dt>
          <dd data-testid="detalle-fecha-limite">{{ formatFecha(solicitud.fechaLimiteSla) }}</dd>
        </div>
        <div class="detalle__campo">
          <dt>Fecha de resolución</dt>
          <dd>{{ formatFecha(solicitud.fechaResolucion) }}</dd>
        </div>
      </dl>

      <section class="detalle__seccion">
        <h2 class="detalle__subtitulo">Descripción</h2>
        <p class="detalle__descripcion" data-testid="detalle-descripcion">{{ solicitud.descripcion }}</p>
      </section>

      <section v-if="motivoActual()" class="detalle__seccion">
        <h2 class="detalle__subtitulo">Motivo</h2>
        <p class="detalle__descripcion" data-testid="detalle-motivo">{{ motivoActual() }}</p>
      </section>

      <AccionModal
        :accion="accionActiva"
        :agentes="agentes"
        :cargando="cargandoAgentes"
        :nombre-solicitud="solicitud.codigo"
        @confirmar="confirmarAccion"
        @cerrar="cerrarModal"
      />
    </template>
  </main>
</template>

<style scoped>
.detalle__codigo {
  margin: 0 0 4px;
  font-family: var(--mono);
  font-size: 13px;
  color: var(--text);
}

.detalle__acciones {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: flex-end;
}

.detalle__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
  margin: 0 0 24px;
  padding: 16px;
  border: 1px solid var(--border);
  border-radius: 10px;
}

.detalle__campo {
  margin: 0;
}

.detalle__campo dt {
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--text);
}

.detalle__campo dd {
  margin: 4px 0 0;
  color: var(--text-h);
}

.detalle__seccion {
  margin-bottom: 24px;
}

.detalle__subtitulo {
  font-size: 18px;
  margin: 0 0 8px;
}

.detalle__descripcion {
  margin: 0;
  color: var(--text-h);
  white-space: pre-wrap;
}

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
