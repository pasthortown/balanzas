# Proceso de Compilación - BalanzaService

## Requisitos Previos (Ubuntu)

### Instalar .NET SDK 8.0
```bash
sudo apt install dotnet-sdk-8.0
```

### Instalar wixl (generador de MSI para Linux)
```bash
sudo apt install msitools wixl
```

## Proceso de Compilación

### 1. Compilar el proyecto para Windows x64

```bash
cd /home/lsalazar/Proyectos/balanzas/solution

dotnet publish BalanzaService/BalanzaService.csproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    -o Installer/publish
```

### 2. Copiar appsettings.json al directorio publish

```bash
cp BalanzaService/appsettings.json Installer/publish/
```

### 3. Generar el MSI con wixl

```bash
cd Installer
wixl -v -o BalanzaService.msi BalanzaServiceLinux.wxs
```

## Archivos Generados

| Archivo | Ubicación | Descripción |
|---------|-----------|-------------|
| `BalanzaService.msi` | `Installer/` | Instalador MSI (~42MB) |
| `BalanzaService.exe` | `Installer/publish/` | Ejecutable del servicio |
| `appsettings.json` | `Installer/publish/` | Configuración del servicio |

## Copiar a USB

```bash
# Identificar punto de montaje de la USB
lsblk -o NAME,SIZE,TYPE,MOUNTPOINT | grep media

# Copiar MSI a la USB (ajustar ruta según punto de montaje)
cp Installer/BalanzaService.msi /media/lsalazar/LUCHOX/
```

## Archivos WXS

- `BalanzaServiceLinux.wxs` - Formato WiX v3, compatible con `wixl` (Linux)
- `Package.wxs` - Formato WiX v5, compatible con `wix` (Windows)
- `BalanzaService.wxs` - Formato WiX v3 antiguo (no usar)

## Configuración del Servicio

El archivo `appsettings.json` contiene:

```json
{
  "Serial": {
    "Puerto": "COM2",
    "BaudRate": 9600
  },
  "Sap": {
    "Url": "http://sazsappodesa.kfc.com.ec:50000/RESTAdapter/SAPIntegration/SendBalanzaMeasures",
    "Username": "USER_IF_Bala",
    "Password": "inicio2020"
  },
  "Worker": {
    "IntervaloMs": 3000
  }
}
```

Para producción, usar `appsettings.Production.json` con las credenciales de producción.

## Script Completo

```bash
#!/bin/bash
# compilar.sh - Script de compilación completo

cd /home/lsalazar/Proyectos/balanzas/solution

# Compilar
dotnet publish BalanzaService/BalanzaService.csproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    -o Installer/publish

# Copiar configuración
cp BalanzaService/appsettings.json Installer/publish/

# Generar MSI
cd Installer
wixl -v -o BalanzaService.msi BalanzaServiceLinux.wxs

echo "MSI generado: $(ls -lh BalanzaService.msi)"
```

## Notas

- El MSI instala el servicio en `C:\Program Files\BalanzaService\`
- El servicio se registra como "BalanzaService" con inicio automático
- Después de instalar, el servicio inicia automáticamente
- Para cambiar el puerto COM, editar `appsettings.json` en la carpeta de instalación y reiniciar el servicio
