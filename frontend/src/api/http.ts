import type { ApiError } from '../types'

const BASE_URL: string = import.meta.env.VITE_API_URL ?? '/api/v1'

const TOKEN_KEY = 'mesasitec_token'
const USUARIO_KEY = 'mesasitec_usuario'

export class ApiClientError extends Error {
  readonly problem: ApiError

  constructor(problem: ApiError) {
    super(problem.detail || problem.title)
    this.name = 'ApiClientError'
    this.problem = problem
  }
}

export function guardarSesion(token: string, usuario: unknown): void {
  localStorage.setItem(TOKEN_KEY, token)
  localStorage.setItem(USUARIO_KEY, JSON.stringify(usuario))
}

export function leerToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function limpiarSesion(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USUARIO_KEY)
}

function irAlLogin(): void {
  limpiarSesion()
  if (window.location.pathname !== '/login') {
    window.location.assign('/login')
  }
}

function errorPorDefecto(status: number): ApiError {
  return {
    type: 'https://mesasitec.local/errores/desconocido',
    title: 'Error inesperado',
    status,
    detail: 'Ocurrió un error inesperado. Inténtalo de nuevo.',
    codigo: status === 500 ? 'ERROR_INTERNO' : 'ERROR_INESPERADO',
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT'
  body?: unknown
}

async function peticion<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = {
    Accept: 'application/json',
  }

  const token = leerToken()
  if (token) {
    headers.Authorization = `Bearer ${token}`
  }
  if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }

  const response = await fetch(`${BASE_URL}${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  })

  if (response.status === 401) {
    irAlLogin()
    throw new ApiClientError(errorPorDefecto(401))
  }

  if (!response.ok) {
    let problem: ApiError | null = null
    try {
      problem = (await response.json()) as ApiError
    } catch {
      problem = null
    }
    throw new ApiClientError(problem ?? errorPorDefecto(response.status))
  }

  return (await response.json()) as T
}

export const http = {
  get<T>(path: string): Promise<T> {
    return peticion<T>(path, { method: 'GET' })
  },
  post<T>(path: string, body: unknown): Promise<T> {
    return peticion<T>(path, { method: 'POST', body })
  },
  put<T>(path: string, body: unknown): Promise<T> {
    return peticion<T>(path, { method: 'PUT', body })
  },
}
