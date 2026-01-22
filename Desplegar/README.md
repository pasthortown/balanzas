# Instalación BalanzaService

## Archivos Incluidos

- **BalanzaService.msi** - Instalador del servicio (42 MB)
- **appsettings.json** - Configuración de ejemplo (para referencia)
- **appsettings.Production.json** - Configuración de producción (para referencia)
- **comunicacion.md** - Documentación de comunicación con la balanza LP7516
- **Balanza.pdf** - Manual del fabricante
- **README.md** - Este archivo

> **Nota**: Los archivos `appsettings.json` están incluidos como referencia. El instalador ya incluye estos archivos, pero puedes usarlos para verificar o modificar la configuración después de la instalación.

## Instrucciones de Instalación

### 1. Requisitos Previos
- Windows 10 o superior
- Permisos de Administrador
- Puerto serial COM disponible (default: COM2)

### 2. Instalación

1. **Ejecutar el instalador**:
   - Hacer clic derecho en `BalanzaService.msi`
   - Seleccionar "Ejecutar como administrador"
   - Seguir el asistente de instalación

2. **Ubicación de instalación**:
   - El servicio se instalará en: `C:\Program Files (x86)\BalanzaService\`

3. **Inicio automático**:
   - El servicio se registrará como servicio de Windows
   - Iniciará automáticamente después de la instalación
   - Se configurará para inicio automático con Windows

### 3. Configuración

#### Configurar Puerto Serial

Si necesitas cambiar el puerto COM (default: COM2):

1. Abrir el archivo de configuración:
   ```
   C:\Program Files (x86)\BalanzaService\appsettings.json
   ```

2. Modificar el puerto:
   ```json
   {
     "Serial": {
       "Puerto": "COM2",     ← Cambiar aquí
       "BaudRate": 9600
     }
   }
   ```

3. Reiniciar el servicio:
   - Abrir "Servicios" (services.msc)
   - Buscar "BalanzaService"
   - Clic derecho → Reiniciar

### 4. Verificación

#### Verificar que el servicio está corriendo:

1. **Por Servicios de Windows**:
   - Presionar `Win + R`
   - Escribir `services.msc` y Enter
   - Buscar "BalanzaService"
   - Verificar que el estado sea "En ejecución"

2. **Por API Web**:
   - Abrir navegador
   - Ir a: `http://localhost/balanza`
   - Debería mostrar el último peso leído

#### Ver logs:

Los logs se almacenan en:
```
C:\Program Files (x86)\BalanzaService\logs\balanza-YYYYMMDD.log
```

### 5. Configuración de la Balanza

La balanza LP7516 debe estar configurada para enviar datos por serial:

**Modo recomendado**:
- **C18 = 4** (Modo Continuo) - Envía datos automáticamente
- **C19 = 3** (9600 bps)

**Modo alternativo**:
- **C18 = 3** (Modo Comando) - Responde a comandos
- **C19 = 3** (9600 bps)

Ver `comunicacion.md` para más detalles sobre la configuración.

### 6. Solución de Problemas

#### El servicio no inicia:
- Verificar que el puerto COM exista y esté disponible
- Revisar logs en `C:\Program Files (x86)\BalanzaService\logs\`

#### No lee pesos:
- Verificar conexión serial (cable RS232/USB)
- Verificar configuración C18 de la balanza (debe ser 3 o 4)
- Verificar puerto COM en `appsettings.json`
- Revisar logs para ver mensajes de error

#### Permisos del puerto serial:
- El servicio corre como sistema, debería tener acceso a puertos COM
- Si hay problemas, verificar en servicios.msc que "Inicio de sesión" sea "Sistema local"

### 7. Desinstalación

1. Ir a "Configuración" → "Aplicaciones"
2. Buscar "BalanzaService"
3. Clic en "Desinstalar"

O bien ejecutar:
```
C:\Program Files (x86)\BalanzaService\uninstall-service.cmd
```

### 8. Soporte Técnico

Para más información consultar:
- `comunicacion.md` - Documentación técnica de comunicación
- `Balanza.pdf` - Manual del fabricante
- Logs del servicio para diagnóstico

---

**Versión**: 1.0  
**Fecha**: Enero 2026  
**Desarrollado para**: KFC Ecuador
