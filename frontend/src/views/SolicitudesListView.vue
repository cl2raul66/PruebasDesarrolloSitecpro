<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { EstadoSolicitud, Prioridad } from '../types'
import { useSolicitudesStore } from '../stores/solicitudes'
import { etiquetaEstado, etiquetaPrioridad, formatFecha } from '../utils/formato'
import AppIcon from '../components/AppIcon.vue'

const router = useRouter()
const store = useSolicitudesStore()

const estados: EstadoSolicitud[] = ['Nueva', 'Asignada', 'EnProceso', 'Resuelta', 'Cerrada', 'Cancelada']
const prioridades: Prioridad[] = ['Baja', 'Media', 'Alta', 'Critica']

const filtrosRef = ref<HTMLElement | null>(null)
let observador: ResizeObserver | null = null
let temporizadorBusqueda: ReturnType<typeof setTimeout> | null = null

function sincronizarAlturaFiltros(): void {
  if (filtrosRef.value) {
    document.documentElement.style.setProperty('--altura-filtros', `${filtrosRef.value.offsetHeight}px`)
  }
}

function irANueva(): void {
  void router.push({ name: 'solicitud-nueva' })
}

function irADetalle(id: string): void {
  void router.push({ name: 'solicitud-detalle', params: { id } })
}

function onCambioBusqueda(): void {
  if (temporizadorBusqueda !== null) {
    clearTimeout(temporizadorBusqueda)
  }
  temporizadorBusqueda = setTimeout(() => {
    store.aplicarFiltros()
  }, 400)
}

function onCambioFiltro(): void {
  store.aplicarFiltros()
}

function textoPagina(): string {
  const paginas = Math.max(1, store.totalPaginas)
  return `Página ${store.page} de ${paginas} — ${store.total} resultados`
}

onMounted(() => {
  sincronizarAlturaFiltros()
  if (filtrosRef.value) {
    observador = new ResizeObserver(sincronizarAlturaFiltros)
    observador.observe(filtrosRef.value)
  }
  void store.cargarCategorias().catch(() => {
    store.error = 'No se pudieron cargar las categorías.'
  })
  void store.cargarListado()
})

onBeforeUnmount(() => {
  observador?.disconnect()
})
</script>

<template>
  <main class="pagina">
    <div class="pagina__cabecera">
      <h1 class="pagina__titulo">Solicitudes</h1>
      <button type="button" class="btn btn--primario" data-testid="btn-nueva-solicitud" @click="irANueva">
        <AppIcon name="plus" :size="16" />
        Nueva solicitud
      </button>
    </div>

    <section ref="filtrosRef" class="filtros" aria-label="Filtros">
      <div class="campo">
        <label class="campo__etiqueta" for="filtro-estado">Estado</label>
        <select
          id="filtro-estado"
          v-model="store.filtros.estado"
          class="campo__control"
          data-testid="filtro-estado"
          @change="onCambioFiltro"
        >
          <option value="">Todos</option>
          <option v-for="estado in estados" :key="estado" :value="estado">
            {{ etiquetaEstado[estado] }}
          </option>
        </select>
      </div>

      <div class="campo">
        <label class="campo__etiqueta" for="filtro-prioridad">Prioridad</label>
        <select
          id="filtro-prioridad"
          v-model="store.filtros.prioridad"
          class="campo__control"
          data-testid="filtro-prioridad"
          @change="onCambioFiltro"
        >
          <option value="">Todas</option>
          <option v-for="prioridad in prioridades" :key="prioridad" :value="prioridad">
            {{ etiquetaPrioridad[prioridad] }}
          </option>
        </select>
      </div>

      <div class="campo">
        <label class="campo__etiqueta" for="filtro-categoria">Categoría</label>
        <select
          id="filtro-categoria"
          v-model="store.filtros.categoriaId"
          class="campo__control"
          data-testid="filtro-categoria"
          @change="onCambioFiltro"
        >
          <option value="">Todas</option>
          <option v-for="categoria in store.categorias" :key="categoria.id" :value="categoria.id">
            {{ categoria.nombre }}
          </option>
        </select>
      </div>

      <div class="campo">
        <label class="campo__etiqueta" for="filtro-vencidas">Vencidas</label>
        <select
          id="filtro-vencidas"
          v-model="store.filtros.vencidas"
          class="campo__control"
          data-testid="filtro-vencidas"
          @change="onCambioFiltro"
        >
          <option value="">Todas</option>
          <option value="true">Solo vencidas</option>
          <option value="false">Sin vencidas</option>
        </select>
      </div>

      <div class="campo campo--busqueda">
        <label class="campo__etiqueta" for="filtro-busqueda">Buscar</label>
        <input
          id="filtro-busqueda"
          v-model="store.filtros.busqueda"
          class="campo__control"
          type="search"
          placeholder="Código, título o descripción"
          data-testid="filtro-busqueda"
          @input="onCambioBusqueda"
        />
      </div>

      <button
        type="button"
        class="btn btn--secundario filtros__limpiar"
        data-testid="btn-limpiar-filtros"
        @click="store.limpiarFiltros()"
      >
        Limpiar filtros
      </button>
    </section>

    <div v-if="store.cargando" class="estado" data-testid="listado-cargando">
      Cargando solicitudes…
    </div>

    <div
      v-else-if="store.error"
      class="estado estado--error"
      role="alert"
      data-testid="listado-error"
    >
      {{ store.error }}
      <button type="button" class="btn btn--secundario" @click="store.cargarListado()">Reintentar</button>
    </div>

    <div v-else-if="store.resultado && store.resultado.items.length === 0" class="estado" data-testid="listado-vacio">
      No hay solicitudes que coincidan con los filtros.
    </div>

    <template v-else-if="store.resultado && store.resultado.items.length > 0">
      <div class="tabla-envoltorio">
        <table class="tabla" data-testid="tabla-solicitudes">
          <thead>
            <tr>
              <th>Código</th>
              <th>Título</th>
              <th>Estado</th>
              <th>Prioridad</th>
              <th>Categoría</th>
              <th>SLA</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="solicitud in store.resultado.items"
              :key="solicitud.id"
              class="tabla__fila"
              :data-codigo="solicitud.codigo"
              data-testid="fila-solicitud"
              @click="irADetalle(solicitud.id)"
            >
              <td class="tabla__celda" data-testid="celda-codigo">{{ solicitud.codigo }}</td>
              <td class="tabla__celda tabla__celda--titulo">{{ solicitud.titulo }}</td>
              <td class="tabla__celda" data-testid="celda-estado">
                {{ etiquetaEstado[solicitud.estado] }}
              </td>
              <td class="tabla__celda" data-testid="celda-prioridad">
                {{ etiquetaPrioridad[solicitud.prioridad] }}
              </td>
              <td class="tabla__celda">{{ solicitud.categoria.nombre }}</td>
              <td class="tabla__celda" data-testid="celda-sla">
                <span>{{ formatFecha(solicitud.fechaLimiteSla) }}</span>
                <span v-if="solicitud.vencida" class="badge badge--vencida" data-testid="badge-vencida">
                  Vencida
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <nav class="paginacion" aria-label="Paginación">
        <button
          type="button"
          class="btn btn--secundario"
          data-testid="paginacion-anterior"
          :disabled="store.page <= 1 || store.cargando"
          @click="store.cambiarPagina(store.page - 1)"
        >
          ← Anterior
        </button>
        <span class="paginacion__info" data-testid="paginacion-info">{{ textoPagina() }}</span>
        <button
          type="button"
          class="btn btn--secundario"
          data-testid="paginacion-siguiente"
          :disabled="store.page >= store.totalPaginas || store.cargando"
          @click="store.cambiarPagina(store.page + 1)"
        >
          Siguiente →
        </button>
      </nav>
    </template>
  </main>
</template>

<style scoped>
.pagina__cabecera {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 20px;
}

.filtros {
  position: sticky;
  top: var(--altura-nav, 0px);
  z-index: 40;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 12px;
  align-items: end;
  margin-bottom: 20px;
  padding: 16px;
  border: 1px solid var(--border);
  border-radius: 10px;
  background: var(--bg-suave);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.filtros__limpiar {
  align-self: end;
  justify-self: start;
}

.tabla-envoltorio {
  overflow: visible;
}

.tabla {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.tabla th,
.tabla td {
  padding: 10px 12px;
  text-align: left;
  border-bottom: 1px solid var(--border);
}

.tabla th {
  position: sticky;
  top: calc(var(--altura-nav, 0px) + var(--altura-filtros, 0px));
  z-index: 30;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--text);
  background: var(--bg);
}

.tabla__fila {
  cursor: pointer;
}

.tabla__fila:hover {
  background: var(--accent-bg);
}

.tabla__celda--titulo {
  font-weight: 500;
  color: var(--text-h);
}

.paginacion {
  position: sticky;
  bottom: 0;
  z-index: 40;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 20px;
  padding: 10px 0 0;
  background: var(--bg-suave);
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.05);
}

.paginacion__info {
  font-size: 14px;
  color: var(--text);
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
