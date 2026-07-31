<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { ApiClientError } from '../api/http'

const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const enviando = ref(false)
const error = ref<string | null>(null)

async function iniciarSesion(): Promise<void> {
  if (enviando.value) {
    return
  }
  enviando.value = true
  error.value = null
  try {
    await auth.login({ email: email.value.trim(), password: password.value })
    void router.push({ name: 'solicitudes' })
  } catch (e) {
    if (e instanceof ApiClientError && e.problem.codigo === 'NO_AUTENTICADO') {
      error.value = 'Credenciales inválidas. Revisa tu correo y contraseña.'
    } else {
      error.value = e instanceof Error ? e.message : 'No se pudo iniciar sesión.'
    }
  } finally {
    enviando.value = false
  }
}
</script>

<template>
  <main class="login">
    <form class="login__caja" novalidate @submit.prevent="iniciarSesion">
      <h1 class="login__titulo">MesaSitec</h1>
      <p class="login__subtitulo">Inicia sesión para acceder a tu mesa de servicio</p>

      <div class="campo">
        <label class="campo__etiqueta" for="login-email">Correo</label>
        <input
          id="login-email"
          v-model="email"
          class="campo__control"
          type="email"
          autocomplete="username"
          placeholder="admin@norte.test"
          data-testid="login-email"
        />
      </div>

      <div class="campo">
        <label class="campo__etiqueta" for="login-password">Contraseña</label>
        <input
          id="login-password"
          v-model="password"
          class="campo__control"
          type="password"
          autocomplete="current-password"
          data-testid="login-password"
        />
      </div>

      <p v-if="error" class="login__error" data-testid="login-error">{{ error }}</p>

      <button
        type="submit"
        class="btn btn--primario login__enviar"
        data-testid="login-submit"
        :disabled="enviando || email.trim() === '' || password === ''"
      >
        {{ enviando ? 'Entrando…' : 'Iniciar sesión' }}
      </button>
    </form>
  </main>
</template>

<style scoped>
.login {
  min-height: calc(100svh - 100px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.login__caja {
  width: 100%;
  max-width: 380px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 32px;
  border: 1px solid var(--border);
  border-radius: 12px;
  background: var(--bg);
  box-shadow: var(--shadow);
}

.login__titulo {
  margin: 0;
  font-size: 26px;
  text-align: center;
}

.login__subtitulo {
  margin: 0 0 8px;
  text-align: center;
  font-size: 14px;
}

.login__error {
  color: var(--error);
  font-size: 14px;
  margin: 0;
}

.login__enviar {
  margin-top: 8px;
}
</style>
