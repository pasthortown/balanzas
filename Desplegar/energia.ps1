#Requires -RunAsAdministrator

# Configuracion de energia para equipos de monitoreo
# Requiere ejecutar como administrador

# Verificar privilegios de administrador
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Este script requiere privilegios de administrador." -ForegroundColor Red
    Write-Host "Reiniciando como administrador..." -ForegroundColor Yellow

    Start-Process powershell.exe -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

Write-Host "=== Configuracion de energia ===" -ForegroundColor Cyan

# Desactivar hibernacion
Write-Host "Desactivando hibernacion..." -ForegroundColor Yellow
powercfg -h off

# Activar esquema de alto rendimiento (minimo ahorro de energia)
Write-Host "Activando esquema de alto rendimiento..." -ForegroundColor Yellow
powercfg -setactive SCHEME_MIN

# Desactivar Modern Standby (Connected Standby)
Write-Host "Desactivando Modern Standby..." -ForegroundColor Yellow
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power" /v PlatformAoAcOverride /t REG_DWORD /d 0 /f | Out-Null

# Mostrar estados de suspension disponibles
Write-Host "`nEstados de suspension disponibles:" -ForegroundColor Cyan
powercfg /a

Write-Host "`n=== Configuracion completada ===" -ForegroundColor Green
Write-Host "Se recomienda reiniciar el equipo para aplicar todos los cambios." -ForegroundColor Yellow

Read-Host "`nPresione Enter para salir"
