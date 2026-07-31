# MesaSitec — Prueba Técnica Desarrollador Junior

Mesa de servicio SaaS **multi-tenant** construida para la prueba técnica de Sitecpro.

- **Backend:** .NET 10 (Web API) · EF Core · SQLite · JWT (HS256) · BCrypt · Swagger
- **Frontend:** Vue 3 `<script setup>` · TypeScript `strict` · Vite (puerto 5173) · Vue Router · Pinia
- **Tests:** xUnit (56 pruebas) — `dotnet test`

---

## Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0** |
| [Node.js](https://nodejs.org/) | **18** o superior |
| npm | incluido con Node |
| PowerShell | 5.1+ (incluido en Windows 10/11) |

No se necesita nada más: SQLite es un archivo local y las dependencias se restauran automáticamente.

---

## Cómo levantar el proyecto (4 comandos, < 5 minutos)

Desde la raíz del repositorio:

```powershell
dotnet run --project backend\src\Api\Api.csproj
```

```powershell
cd frontend
npm install
npm run dev
```

Con eso queda:

- API en **http://localhost:5080** — `/health` responde `{"estado":"ok"}` y `/swagger` es accesible
- Frontend en **http://localhost:5173**
- Base de datos migrada y sembrada automáticamente al primer arranque (sin pasos manuales)

### Alternativa en un solo comando

```powershell
.\run.ps1
```

El script verifica prerequisitos, restaura dependencias, carga `.env.example`, inicia backend y frontend, espera a que respondan y muestra las URLs. Pulsa **Q** para detener todo y liberar los puertos 5080 y 5173.

> Variables de entorno: si no defines ninguna, el proyecto usa valores por defecto seguros para desarrollo (secreto JWT de ejemplo, `SEED_FECHA_BASE=2026-01-15T08:00:00Z`). Para personalizarlas, copia `.env.example` a `.env` y ajusta los valores.

---

## Credenciales de prueba

Contraseña de **todos** los usuarios semilla: `Sitec.2026`

**Cooperativa Norte**
| Rol | Email |
|---|---|
| Admin | `admin@norte.test` |
| Agente | `agente1@norte.test` · `agente2@norte.test` |
| Solicitante | `user1@norte.test` · `user2@norte.test` |

**Bufete Sur**
| Rol | Email |
|---|---|
| Admin | `admin@sur.test` |
| Solicitante | `user1@sur.test` |

---

## Estado de la implementación

### Backend — implementado

- [x] Modelo de datos (Tenant, Usuario, Categoria, Solicitud) y EF Core con SQLite
- [x] Migraciones automáticas al arrancar + datos semilla deterministas (respecto a `SEED_FECHA_BASE`, nunca `UtcNow`)
- [x] Autenticación JWT (HS256, expiración 8 h, claims `sub`, `tenantId`, `rol`, `email`) y contraseñas BCrypt
- [x] Swagger en `/swagger` con esquema de seguridad Bearer
- [x] CORS habilitado para `http://localhost:5173`
- [x] Aislamiento multi-tenant **RN-01** (recurso de otra organización → 404)
- [x] Máquina de estados **RN-02**, permisos por rol **RN-03**, SLA server-side **RN-04**, asignación válida **RN-05**, cierre con justificación **RN-06**, código correlativo por organización/año **RN-07**
- [x] Manejador global de excepciones (`application/problem+json` con `codigo` en todos los errores)
- [x] Los 9 endpoints del contrato, con paginación/filtrado/búsqueda/orden **server-side**
- [x] `fechaLimiteSla` enviada por el cliente se **ignora en silencio** (RN-04)
- [x] 56 pruebas unitarias xUnit (estados, SLA, permisos, código) — `dotnet test` en verde

### Frontend — implementado

- [x] Login con manejo de credenciales inválidas
- [x] Listado con filtros (estado, prioridad, categoría, vencidas), búsqueda y paginación, todo server-side
- [x] Creación y edición de solicitudes con validación en el cliente
- [x] Detalle con botones de acción según **estado + rol** (RN-02 + RN-03 combinadas)
- [x] Los botones de acción no permitidos **no se renderizan en el DOM** (regla 7.5)
- [x] Modal de acciones (asignar con selector de agentes; resolver/cancelar con motivo)
- [x] Estados de cada vista: cargando, vacío y error
- [x] `tsc --noEmit` sin errores, **sin `any` explícito**
- [x] Único módulo HTTP centralizado que inyecta el token y redirige a `/login` ante 401
- [x] Todos los `data-testid` de la sección 7.4 escritos literalmente

### Adiciones declaradas (fuera del contrato)

- [x] `GET /api/v1/usuarios/agentes` — agentes activos de la organización, necesario para el selector de asignación (RN-05). Justificado en `DECISIONES.md`.
- [x] `toast-mensaje` global para retroalimentación (está en la lista de `data-testid` globales).

### No implementado (a propósito, declarado honestamente)

- **Concurrencia en el correlativo RN-07:** el enunciado dice explícitamente que queda fuera de alcance; el correlativo se calcula con `MAX(correlativo)+1`.
- **Concurrencia optimista (RowVersion):** dos agentes podrían pisar el estado de una solicitud simultáneamente. Es deuda técnica conocida, descrita en `DECISIONES.md`.
- **Pruebas E2E automatizadas:** el frontend expone todos los `data-testid` para que las pruebas de interfaz del evaluador funcionen, pero no hay suite E2E propia.

---

## Estructura del repositorio

```
/
├─ README.md            ← este archivo
├─ DECISIONES.md        ← decisiones técnicas y trampas del enunciado
├─ .env.example         ← variables de entorno de ejemplo
├─ run.ps1              ← arranque en un comando
├─ backend/
│  ├─ src/
│  │  ├─ Dominio/       ← entidades, enums, máquina de estados, SLA, código
│  │  ├─ Aplicacion/    ← servicios, DTOs, validaciones, permisos
│  │  ├─ Infraestructura/ ← DbContext, migraciones, repositorios, semilla
│  │  └─ Api/           ← controllers, JWT, middleware de errores
│  └─ tests/            ← 56 pruebas xUnit
└─ frontend/
   └─ src/
      ├─ api/           ← cliente HTTP centralizado + módulos por recurso
      ├─ components/    ← nav, toast, formulario, modal de acciones
      ├─ stores/        ← Pinia: auth, solicitudes, toast
      ├─ types/         ← DTOs tipados de la API
      ├─ utils/         ← formato de fechas y lógica de botones
      ├─ views/         ← login, listado, detalle, formulario
      └─ router/        ← rutas y guardas
```

---

## Comandos útiles

```powershell
# Pruebas del backend
dotnet test backend\MesaSitec.slnx

# Type-check del frontend
cd frontend; npx tsc --noEmit

# Build de producción del frontend
cd frontend; npm run build
```
