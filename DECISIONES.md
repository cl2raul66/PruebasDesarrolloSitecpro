# Decisiones Técnicas y Reflexiones

## 1. Decisiones Técnicas Tomadas

1. **Script de arranque automatizado (run.ps1)**
   - **Decisión:** Creé un script `run.ps1` que verifica prerequisitos, restaura dependencias, configura env vars, e inicia backend y frontend en un solo comando con limpieza automática al salir.
   - **Alternativa descartada:** Usar `docker-compose.yml` o dejar los 4 comandos manuales indicados en el README.
   - **Por qué:** Docker Compose introduce dependencia de Docker Desktop sin beneficio real para un proyecto SQLite local. Los comandos manuales son frágiles (el usuario puede saltarse pasos). Un solo script garantiza consistencia, detecta faltantes antes de ejecutar y libera procesos/puertos al presionar Ctrl+C.

## 2. Uso de Inteligencia Artificial

- **Qué hice con IA:** Usé asistentes de IA para generar el script `run.ps1`, la estructura inicial de componentes Vue, crear datos de prueba para tests unitarios y redactar expresiones regulares para validaciones.
- **Qué escribí a mano:** Toda la lógica de negocio en el Dominio (Cálculo de SLA, Máquina de Estados), la configuración del DbContext, los interceptores de Axios en el frontend, la configuración de Pinia y la lógica de control secuencial del `run.ps1`.

## 3. ¿Qué haría distinto con una semana más?

- Implementaría un sistema de concurrencia optimista (RowVersion) para evitar que dos agentes modifiquen la misma solicitud al mismo tiempo.
- Mejoraría la generación del código de solicitud (RN-07) usando una tabla de secuencias en base de datos con bloqueos transaccionales para garantizar que no haya colisiones bajo alta concurrencia.
- Añadiría pruebas E2E con Playwright o Cypress aprovechando los `data-testid` implementados.

## 4. ¿Dónde me atasqué y cómo lo resolví?

- **El problema 1 — Limpieza de procesos en run.ps1:** Al diseñar el script de arranque, me costó lograr que Ctrl+C detuviera tanto el backend como el frontend y liberara los puertos 5080 y 5173. En PowerShell, el comportamiento de Ctrl+C cambia si hay procesos hijo en la misma ventana (`-NoNewWindow`) y el bloque `finally` no siempre se ejecuta si se presiona Ctrl+C dos veces.
- **La solución:** Combiné dos mecanismos: un bloque `try/finally` para el caso normal (un Ctrl+C), más la búsqueda explícita de procesos huérfanos por puerto usando `Get-NetTCPConnection` como respaldo. Así, aunque el finally no se ejecute, la limpieza por puertos remata cualquier proceso residual.
