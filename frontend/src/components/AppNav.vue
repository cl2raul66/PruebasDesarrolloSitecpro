<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

function cerrarSesion(): void {
  auth.cerrarSesion()
  void router.push({ name: 'login' })
}
</script>

<template>
  <nav class="app-nav" data-testid="app-nav">
    <div class="app-nav__marca">
      <span class="app-nav__logo">MesaSitec</span>
    </div>
    <div v-if="auth.usuario" class="app-nav__usuario">
      <span data-testid="nav-usuario-nombre">{{ auth.usuario.nombre }}</span>
      <span class="app-nav__rol" data-testid="nav-usuario-rol">{{ auth.usuario.rol }}</span>
      <button type="button" class="btn btn--secundario" data-testid="btn-logout" @click="cerrarSesion">
        Salir
      </button>
    </div>
  </nav>
</template>

<style scoped>
.app-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 24px;
  border-bottom: 1px solid var(--border);
  background: var(--nav-bg, #ffffff);
}

.app-nav__logo {
  font-weight: 600;
  font-size: 18px;
  color: var(--text-h);
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
