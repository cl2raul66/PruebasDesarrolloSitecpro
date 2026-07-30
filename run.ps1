#!/usr/bin/env pwsh
#Requires -Version 5.1

<#
.SYNOPSIS
    Inicia MesaSitec (backend + frontend) en un solo comando.
.DESCRIPTION
    1. Verifica prerequisitos (dotnet, node, npm)
    2. Restaura paquetes NuGet y módulos npm
    3. Genera frontend/.env desde .env.example y asigna env vars del backend
    4. Inicia backend (dotnet run) en background
    5. Inicia frontend (npm run dev) en background
    Q detiene todo y libera los puertos.
#>

$ErrorActionPreference = 'Stop'
$script:backendProcess = $null
$script:frontendProcess = $null


# ─── Funciones auxiliares ───

function Write-Timestamp {
    return "[$(Get-Date -Format 'HH:mm:ss')]"
}

function Write-OK($message) {
    Write-Host "$(Write-Timestamp) [OK] $message" -ForegroundColor Green
}

function Write-Error($message) {
    Write-Host "$(Write-Timestamp) [ERROR] $message" -ForegroundColor Red
}

function Write-Warn($message) {
    Write-Host "$(Write-Timestamp) [WARN] $message" -ForegroundColor Yellow
}

function Write-Info($message) {
    Write-Host "$(Write-Timestamp) [INFO] $message" -ForegroundColor Cyan
}

function Step($name, $scriptBlock) {
    Write-Host ""
    Write-Host "─── $name ───" -ForegroundColor Cyan
    try {
        & $scriptBlock
        Write-OK $name
    } catch {
        Write-Error "$name — $($_.Exception.Message)"
        throw
    }
}

function Cleanup {
    Write-Host ""
    Write-Info "Deteniendo procesos..."

    @(
        @{ Process = $script:backendProcess; Name = 'Backend' },
        @{ Process = $script:frontendProcess; Name = 'Frontend' }
    ) | ForEach-Object {
        $p = $_.Process
        if ($p -and -not $p.HasExited) {
            try {
                $p.Kill()
                $p.WaitForExit(3000)
                Write-OK "$($_.Name) detenido"
            } catch {
                Write-Warn "No se pudo detener $($_.Name): $_"
            }
        }
    }

    @(5080, 5173) | ForEach-Object {
        try {
            $conn = Get-NetTCPConnection -LocalPort $_ -ErrorAction Stop
            $conn | ForEach-Object {
                Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
            }
        } catch {
            # No hay proceso en ese puerto
        }
    }

    Write-OK "Limpieza completada — puertos liberados"
}

# ─── Main ───

try {
    # ── Banner ──
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════╗" -ForegroundColor Magenta
    Write-Host "║        MesaSitec — Inicio rápido         ║" -ForegroundColor Magenta
    Write-Host "╚══════════════════════════════════════════╝" -ForegroundColor Magenta
    Write-Host ""

    # ── Sección 1: Prerequisitos ──
    Step "Prerequisitos" {
        $dotnetVer = dotnet --version
        if ($LASTEXITCODE -ne 0) { throw "No se encontró .NET SDK. Descárgalo de https://dotnet.microsoft.com/download" }
        Write-Host "  dotnet $dotnetVer"

        $nodeVer = node --version
        if ($LASTEXITCODE -ne 0) { throw "No se encontró Node.js. Descárgalo de https://nodejs.org" }
        Write-Host "  node $nodeVer"

        $npmVer = npm --version
        if ($LASTEXITCODE -ne 0) { throw "No se encontró npm." }
        Write-Host "  npm v$npmVer"

        try { $gitVer = git --version; Write-Host "  $gitVer" } catch { Write-Warn "git no instalado (opcional)" }
    }

    # ── Sección 2: Dependencias ──
    Step "Dependencias" {
        $csproj = "backend\src\Api\Api.csproj"
        if (-not (Test-Path $csproj)) { throw "No se encontró $csproj" }

        Write-Host "  Restaurando paquetes NuGet..."
        dotnet restore $csproj
        if ($LASTEXITCODE -ne 0) { throw "dotnet restore falló" }

        if (-not (Test-Path "frontend\node_modules")) {
            Write-Host "  Instalando módulos de Node..."
            Push-Location "frontend"
            try {
                npm install
                if ($LASTEXITCODE -ne 0) { throw "npm install falló" }
            } finally {
                Pop-Location
            }
        } else {
            Write-Host "  node_modules ya existe"
        }
    }

    # ── Sección 3: Configuración ──
    Step "Configuración" {
        $envExample = ".env.example"
        if (-not (Test-Path $envExample)) { throw "No se encontró $envExample en la raíz del proyecto" }

        $frontendLines = @()
        $backendVars = @{}

        foreach ($line in (Get-Content $envExample)) {
            $trimmed = $line.Trim()
            if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }
            $eqIndex = $trimmed.IndexOf('=')
            if ($eqIndex -lt 1) { continue }
            $key = $trimmed.Substring(0, $eqIndex).Trim()
            $value = $trimmed.Substring($eqIndex + 1).Trim()
            if ($value.Length -gt 1 -and (($value[0] -eq '"' -and $value[-1] -eq '"') -or ($value[0] -eq "'" -and $value[-1] -eq "'"))) {
                $value = $value.Substring(1, $value.Length - 2)
            }
            if ($key -like 'VITE_*') {
                $frontendLines += "$key=$value"
            } else {
                $backendVars[$key] = $value
            }
        }

        if ($frontendLines.Count -gt 0) {
            $frontendLines | Set-Content -Path "frontend\.env" -Encoding ASCII -Force
            Write-Host "  frontend\.env creado ($($frontendLines.Count) variables)"
        }

        $count = 0
        foreach ($kv in $backendVars.GetEnumerator()) {
            [System.Environment]::SetEnvironmentVariable($kv.Key, $kv.Value, 'Process')
            $count++
        }
        Write-Host "  $count variables de backend asignadas como env vars"
    }

    # ── Sección 4: Iniciar Backend ──
    Step "Backend" {
        $csproj = "backend\src\Api\Api.csproj"
        $healthUrl = "http://localhost:5080/health"
        Write-Host "  dotnet run --project $csproj"
        $script:backendProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$csproj`"" -NoNewWindow -PassThru

        Write-Host "  Esperando a que el backend responda en $healthUrl ..."
        $timeout = 30
        $elapsed = 0
        $ready = $false
        while ($elapsed -lt $timeout) {
            if ($script:backendProcess.HasExited) {
                throw "El backend falló al iniciar. Revisa la salida arriba."
            }
            try {
                $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 2
                if ($response.StatusCode -eq 200) {
                    $ready = $true
                    break
                }
            } catch { }
            Start-Sleep 1
            $elapsed++
        }
        if (-not $ready) {
            throw "El backend no respondió en $timeout segundos. Revisa la salida arriba."
        }
        Write-Host "  Backend corriendo en http://localhost:5080"
    }

    # ── Sección 5: Iniciar Frontend ──
    Step "Frontend" {
        $frontendUrl = "http://localhost:5173/"
        Write-Host "  npm run dev (frontend)"
        $script:frontendProcess = Start-Process -FilePath "npm.cmd" -ArgumentList "run dev" -WorkingDirectory "frontend" -NoNewWindow -PassThru

        Write-Host "  Esperando a que el frontend responda en $frontendUrl ..."
        $timeout = 30
        $elapsed = 0
        $ready = $false
        while ($elapsed -lt $timeout) {
            if ($script:frontendProcess.HasExited) {
                throw "El frontend falló al iniciar. Revisa la salida arriba."
            }
            try {
                $response = Invoke-WebRequest -Uri $frontendUrl -UseBasicParsing -TimeoutSec 2
                if ($response.StatusCode -eq 200) {
                    $ready = $true
                    break
                }
            } catch { }
            Start-Sleep 1
            $elapsed++
        }
        if (-not $ready) {
            throw "El frontend no respondió en $timeout segundos. Revisa la salida arriba."
        }
        Write-Host "  Frontend corriendo en http://localhost:5173"
    }

    # ── Mostrar URLs ──
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  API        -> http://localhost:5080" -ForegroundColor White
    Write-Host "  Swagger    -> http://localhost:5080/swagger" -ForegroundColor White
    Write-Host "  Health     -> http://localhost:5080/health" -ForegroundColor White
    Write-Host "  Frontend   -> http://localhost:5173" -ForegroundColor White
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Presiona Q para detener todo." -ForegroundColor Yellow
    Write-Host "  (Si alguien insiste con Ctrl+C, el script limpia los puertos 5080 y 5173)"
    Write-Host ""

    do {
        $key = [Console]::ReadKey($true)
    } while ($key.Key -ne [ConsoleKey]::Q)
} catch {
    Write-Info "El script se interrumpió por Ctrl+C o error."
} finally {
    Cleanup
}
