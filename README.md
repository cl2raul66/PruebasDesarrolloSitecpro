# MesaSitec — Prueba Técnica

Mesa de servicio SaaS multi-tenant desarrollada para la prueba técnica de Sitecpro.

## 🚀 Requisitos Previos

Para levantar este proyecto necesitas tener instalado:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18 o superior)
- [PowerShell 5.1+](https://learn.microsoft.com/en-us/powershell/) (viene incluido en Windows 10/11)

## 🛠️ Cómo levantar el proyecto (en menos de 5 minutos)

**1. Clonar**
```powershell
git clone https://github.com/cl2raul66/PruebasDesarrolloSitecpro.git
cd PruebasDesarrolloSitecpro
```

**2. Ejecutar**
```powershell
.\run.ps1
```
> Nota:
>  - El script `run.ps1` se encarga de todo automáticamente.
>  - La base de datos SQLite se crea y siembra automáticamente al iniciar el backend.

## 🔐 Credenciales de Prueba (Datos Semilla)

La base de datos se inicializa con las siguientes credenciales. **La contraseña para todos los usuarios es:** `Sitec.2026`

**Cooperativa Norte:**
- Admin: `admin@norte.test`
- Agente: `agente1@norte.test` | `agente2@norte.test`
- Solicitante: `user1@norte.test` | `user2@norte.test`

**Bufete Sur:**
- Admin: `admin@sur.test`
- Solicitante: `user1@sur.test`

## 📋 Estado de la Implementación

**Backend:**
- [x] Modelo de datos y EF Core (SQLite)
- [x] Migraciones automáticas y Seed Data (con `SEED_FECHA_BASE`)
- [x] Autenticación JWT
- [x] Aislamiento por Tenant (RN-01)
- [x] Máquina de estados (RN-02)
- [x] Permisos por rol (RN-03)
- [x] Cálculo de SLA en servidor (RN-04)
- [x] Endpoints documentados en Swagger
- [x] Pruebas unitarias (xUnit) - *[Indicar cantidad]*

**Frontend:**
- [x] Login y manejo de sesión (Pinia + Vue Router)
- [x] Listado de solicitudes (Filtros, búsqueda y paginación server-side)
- [x] Creación y edición de solicitudes
- [x] Detalle de solicitud y ejecución de transiciones
- [x] Atributos `data-testid` implementados
- [x] Ocultamiento estricto de botones no permitidos (DOM)

**Faltantes / Deuda Técnica (Sé honesto aquí):**
- *Ejemplo: No me dio tiempo de implementar la paginación en el frontend, actualmente solo muestra la primera página.*
- *Ejemplo: El endpoint de edición funciona, pero la vista en Vue no maneja correctamente los errores 422.*
