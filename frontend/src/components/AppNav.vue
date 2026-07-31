<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import AppIcon from './AppIcon.vue'

const router = useRouter()
const auth = useAuthStore()
const navRef = ref<HTMLElement | null>(null)
let observador: ResizeObserver | null = null

function sincronizarAlturaNav(): void {
  if (navRef.value) {
    document.documentElement.style.setProperty('--altura-nav', `${navRef.value.offsetHeight}px`)
  }
}

onMounted(() => {
  sincronizarAlturaNav()
  if (navRef.value) {
    observador = new ResizeObserver(sincronizarAlturaNav)
    observador.observe(navRef.value)
  }
})

onBeforeUnmount(() => {
  observador?.disconnect()
})

function cerrarSesion(): void {
  auth.cerrarSesion()
  void router.push({ name: 'login' })
}
</script>

<template>
  <nav ref="navRef" class="app-nav" data-testid="app-nav">
    <div class="app-nav__marca">
      <span class="app-nav__logo">MesaSitec</span>
      <span v-if="auth.usuario" class="app-nav__org">{{ auth.usuario.tenantNombre }}</span>
    </div>
    <div v-if="auth.usuario" class="app-nav__usuario">
      <span data-testid="nav-usuario-nombre">{{ auth.usuario.nombre }}</span>
      <span class="app-nav__rol" data-testid="nav-usuario-rol">{{ auth.usuario.rol }}</span>
      <button type="button" class="btn btn--secundario" data-testid="btn-logout" @click="cerrarSesion">
        <AppIcon name="salir" :size="16" />
        Salir
      </button>
    </div>
  </nav>
</template>

<style scoped>
.app-nav {
  position: sticky;
  top: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 24px;
  border-bottom: 1px solid var(--border);
  background: var(--nav-bg, #ffffff);
}

.app-nav__marca {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
}

.app-nav__logo {
  font-weight: 600;
  font-size: 18px;
  color: var(--text-h);
}

.app-nav__org {
  font-size: 12px;
  color: var(--text);
}

.app-nav__usuario {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 14px;
}

.app-nav__rol {
  padding: 2px 8px;
  border-radius: 999px;
  background: var(--accent-bg);
  color: var(--accent);
  font-size: 12px;
  font-weight: 600;
  text-transform: capitalize;
}
</style>
