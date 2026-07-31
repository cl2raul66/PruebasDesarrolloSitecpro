<script setup lang="ts">
import { useToastStore } from '../stores/toast'

const toast = useToastStore()
</script>

<template>
  <Transition name="toast">
    <div
      v-if="toast.visibilidad && toast.mensaje"
      class="toast"
      :class="`toast--${toast.tipo}`"
      role="status"
      data-testid="toast-mensaje"
    >
      <span>{{ toast.mensaje }}</span>
      <button type="button" class="toast__cerrar" aria-label="Cerrar" @click="toast.ocultar()">
        ×
      </button>
    </div>
  </Transition>
</template>

<style scoped>
.toast {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: 1000;
  display: flex;
  align-items: center;
  gap: 12px;
  max-width: 420px;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
  box-shadow: var(--shadow);
  color: #fff;
}

.toast--exito {
  background: #1f9d55;
}

.toast--error {
  background: #c0392b;
}

.toast--info {
  background: #2c3e50;
}

.toast__cerrar {
  background: transparent;
  border: none;
  color: inherit;
  font-size: 18px;
  line-height: 1;
  cursor: pointer;
}

.toast-enter-active,
.toast-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
