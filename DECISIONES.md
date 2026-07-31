# Decisiones Técnicas

## 1. Tres decisiones técnicas

**1. Separación por capas `Api / Aplicacion / Dominio / Infraestructura`, con la lógica de negocio en clases puras.**
Descarté la Minimal API con toda la lógica en un solo proyecto porque el enunciado pide explícitamente que la máquina de estados, el SLA y los permisos se prueben sin levantar la aplicación. Puse `StateMachineService`, `SlaCalculator` y `CodigoFormateador` en `Dominio/Servicios` como clases puras (sin EF ni HTTP), y `PermissionService`/`SolicitudService` en `Aplicacion`. Los controllers quedan delgados: extraen claims, llaman al servicio y mapean.

**2. Un único endpoint extra declarado: `GET /api/v1/usuarios/agentes`.**
El contrato no define cómo el frontend obtiene la lista de agentes para `asignar` (RN-05 exige que el agente exista, esté activo y tenga rol Agente/Admin, y un selector a mano con emails fijos no lo garantiza). Descarté hardcodear la lista en el frontend porque se rompería con datos reales. Añadí el endpoint bajo `/usuarios/agentes`, filtrando por tenant y roles `Agente`/`Admin`, y lo declaro aquí como adición fuera del contrato.

**3. Semilla determinista con `SEED_FECHA_BASE` y GUIDs fijos.**
Todas las fechas del seed son desplazamientos fijos desde `SEED_FECHA_BASE` y los IDs son GUIDs constantes. Descarté usar `DateTime.UtcNow` y GUIDs aleatorios: el enunciado exige datos idénticos sin importar cuándo se ejecute, y los IDs fijos permiten verificar el 404 cross-tenant (RN-01) con URLs que se pueden comprobar desde Swagger sin depender de valores variables.

## 2. Trampas y ambigüedades del enunciado que encontré

- **RN-03 "cerrar solo las propias" vs RN-02:** un Solicitante solo puede cerrar lo propio *y* la transición `cerrar` solo existe desde `Resuelta`. Como un Solicitante no puede `resolver`, la única vía es que un Admin/Agente resuelva y el Solicitante cierre después. El ejemplo de la sección 7.5 lo confirma: en `Nueva` el Solicitante ve únicamente `btn-editar`. La visibilidad de botones se calcula con **RN-02 ∧ RN-03** (transición permitida *y* rol permitido), nunca por separado.
- **RN-04 `fechaLimiteSla` del cliente:** se ignora en silencio en `Crear` y `Editar`; al cambiar prioridad o categoría se recalcula sin tocar `fechaCreacion`, y solo si la solicitud no está en estado final. Si ya es `Resuelta/Cerrada/Cancelada`, el SLA no se recalcula (la fecha no debería "mover" sobre algo terminado).
- **Orden semántico por `prioridad`:** el requisito es `Critica > Alta > Media > Baja`, no alfabético. La solución fue declarar el enum en ese orden exacto y delegar en `OrderBy` de EF sobre el valor entero.
- **404 vs 403 (RN-01):** el repositorio filtra siempre por `tenantId`, así que un recurso de otra organización simplemente "no existe" → 404 `RECURSO_NO_ENCONTRADO`. Los 403 quedan reservados para infracciones de rol (RN-03).

## 3. Uso de IA vs. lo escrito a mano

- **Con ayuda de IA:** el esqueleto inicial del proyecto, los DTOs de TypeScript mapeados del contrato, la plantilla de `run.ps1` y las clases base de las pruebas.
- **Escrito a mano y revisado línea por línea:** toda la lógica de negocio del Dominio (máquina de estados, SLA, correlativo), el `DbContext` y los repositorios con el filtrado server-side, el middleware de errores con los códigos literales, la lógica de visibilidad de botones del frontend, los `data-testid` y el cliente HTTP centralizado. Entiendo y puedo explicar cada parte.

## 4. Qué haría distinto con una semana más

- **Concurrencia del correlativo RN-07:** cambiar `MAX(correlativo)+1` por una tabla de secuencias con transacción (`INSERT ... RETURNING`) para eliminar colisiones bajo carga.
- **Concurrencia optimista (RowVersion):** impedir que dos agentes sobrescriban el estado de la misma solicitud.
- **Pruebas E2E** con Playwright usando los `data-testid` ya presentes, y **validación de OpenAPI→TypeScript** generada en vez de escrita a mano (suma puntos en el enunciado).

## 5. Dónde me atasqué y cómo lo resolví

- **Generación del `OpenAPI 2.x` para Swagger:** `Swashbuckle` en su versión moderna (Microsoft.OpenApi 2.x) ya no usa `OpenApiReference` con un nombre de esquema; el requisito de seguridad "Bearer" dejaba de emitirse y las pruebas automáticas del Swagger fallarían. Lo resolví investigando la API nueva y usando `new OpenApiSecuritySchemeReference("Bearer", document)` en `AddSecurityRequirement`.
- **Fecha UTC vs SQLite:** SQLite devuelve `DateTime` con `Kind.Unspecified`, por lo que `System.Text.Json` no emitía la `Z` final y el contrato ISO-8601 se rompía. Lo resolví con un `UtcDateTimeJsonConverter` que normaliza a `Utc` y serializa siempre con `Z`.
- **Errores en el expression tree de EF:** intenté compilar filtros con el patrón `is` y EF Core no podía traducirlo a SQL. Lo resolví reemplazando los operadores `is`/`is not` por comparaciones `!=`/`==` que EF traduce correctamente.
