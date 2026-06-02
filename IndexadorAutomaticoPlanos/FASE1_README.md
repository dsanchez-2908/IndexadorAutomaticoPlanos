# FASE 1 - Base de Datos, Arquitectura y Sistema de Login

## ✅ Completado

La Fase 1 del proyecto Indexador Automático de Planos ha sido completada exitosamente.

## 📁 Estructura del Proyecto

```
IndexadorAutomaticoPlanos/
├── Business/           (Capa de negocios - para futuras reglas)
├── DataAccess/         (Capa de acceso a datos)
│   ├── DatabaseHelper.cs
│   ├── UsuarioRepository.cs
│   └── ParametroRepository.cs
├── Entities/           (Clases de entidades/POCOs)
│   ├── Usuario.cs
│   ├── Parametro.cs
│   ├── Archivo.cs
│   ├── Lote.cs
│   ├── LoteArchivo.cs
│   ├── PlanoProcesado.cs
│   ├── Estado.cs
│   ├── TipoPlano.cs
│   ├── LogEntry.cs
│   └── SesionActual.cs
├── Scripts/            (Scripts SQL)
│   ├── 00_ResetDatabase.sql
│   ├── 01_CreateDatabase.sql
│   ├── 02_InsertInitialData.sql
│   └── README.md
├── Security/           (Capa de seguridad)
│   ├── Encriptacion.cs
│   └── ConfigManager.cs
├── UI/                 (Interfaz de usuario)
│   ├── FrmLogin.cs
│   ├── FrmCambiarClave.cs
│   └── FrmPrincipal.cs
├── Utils/              (Utilidades)
│   └── Logger.cs
├── App.config          (Configuración de la aplicación)
└── Program.cs          (Punto de entrada)
```

## 🗄️ Base de Datos

### Tablas Creadas

**Valores Fijos (TV_)**
- `IAP_TV_ESTADOS_ARCHIVO` - Estados de archivos
- `IAP_TV_ESTADOS_LOTE` - Estados de lotes
- `IAP_TV_ESTADOS_ARCHIVO_LOTE` - Estados de archivos en lotes
- `IAP_TV_ESTADOS_VALIDACION` - Estados de validación de planos
- `IAP_TV_TIPOS_PLANO` - Tipos de plano (Obra, Mensura, Instalaciones)

**Datos (TD_)**
- `IAP_TD_USUARIOS` - Usuarios del sistema
- `IAP_TD_PARAMETROS` - Parámetros de configuración
- `IAP_TD_ARCHIVOS` - Archivos PDF ingresados
- `IAP_TD_LOTES` - Lotes de procesamiento
- `IAP_TD_PLANOS_PROCESADOS` - Planos procesados por OpenAI
- `IAP_TD_LOGS` - Logs del sistema

**Relación (TR_)**
- `IAP_TR_LOTE_ARCHIVO` - Relación entre lotes y archivos

### Usuario Administrador

- **Usuario**: `admin`
- **Contraseña**: `123`

⚠️ **Importante**: Cambiar la contraseña después del primer ingreso en producción.

## 🔐 Seguridad

### Encriptación de Contraseñas
- Se utiliza **BCrypt** con salt de 11 rondas
- Las contraseñas nunca se almacenan en texto plano
- Validación de fortaleza: mínimo 6 caracteres

### Encriptación de Cadena de Conexión
- Se utiliza **DPAPI** (Data Protection API de Windows)
- La encriptación es específica del usuario de Windows
- Al cambiar de usuario, se debe reconfigurar

### Gestión de Sesión
- Clase `SesionActual` mantiene el usuario logueado
- Cierre de sesión automático al salir
- Opción de cerrar sesión y volver al login

## 📝 Sistema de Logging

### Características
- **Doble escritura**: Archivo y Base de Datos
- **Niveles**: Info, Warning, Error, Debug
- **Archivos**: Rotación diaria (`Log_YYYYMMDD.txt`)
- **Ubicación**: Carpeta `Logs` en el directorio de la aplicación
- **Asíncrono**: Escritura en BD no bloquea la aplicación

### Ejemplo de Uso

```csharp
Logger.Info("Mensaje informativo", "NombreModulo");
Logger.Warning("Advertencia");
Logger.Error("Error crítico", exception, "ModuloError");
Logger.Debug("Debug de variable");
```

## 🖥️ Interfaz de Usuario

### FrmLogin
- Validación de usuario/contraseña
- Verificación de conexión a BD al iniciar
- Detección de clave temporal y primer ingreso
- Redirección automática a cambio de clave

### FrmCambiarClave
- Validación de clave actual
- Validación de fortaleza de nueva clave
- Confirmación de nueva clave
- Modo obligatorio para primer ingreso

### FrmPrincipal (MDI)
- Menú estructurado por fases del proyecto
- Barra de estado con usuario y fecha/hora
- Opción de cerrar sesión
- Opción de cambiar contraseña
- Confirmación al salir

## ⚙️ Configuración

### App.config

```xml
<appSettings>
	<!-- Cadena de conexión por defecto -->
	<add key="ConexionBD" value="Server=localhost\SQLEXPRESS;Database=Capturador;User Id=sa;Password=123;TrustServerCertificate=True;" />

	<!-- Cadena encriptada (se genera automáticamente) -->
	<!-- <add key="ConexionBDEncriptada" value="" /> -->
</appSettings>
```

### Parámetros en Base de Datos

Los siguientes parámetros están configurados en `IAP_TD_PARAMETROS`:

- `OPENAI_API_KEY` - API Key de OpenAI
- `OPENAI_API_URL` - URL de la API
- `OPENAI_API_IMAGEN_PROMPT` - Prompt para imágenes
- `OPENAI_API_OCR_PROMPT` - Prompt para OCR
- `PATH_REPOSITORIO` - Ruta de imágenes JPG
- `UMBRAL_CONFIANZA` - Umbral de confianza (0.85)
- `DPI_IMAGEN` - DPI para conversión (300)
- `MAX_REINTENTOS_API` - Reintentos de API (3)
- `DELAY_REINTENTOS_MS` - Delay entre reintentos (5000ms)
- `OPENAI_MODEL` - Modelo de OpenAI (gpt-4o-mini)
- `MAX_PROCESAMIENTO_PARALELO` - PDFs en paralelo (4)

## 🧪 Pruebas Realizadas

### ✅ Compilación
- El proyecto compila sin errores
- Todas las dependencias están correctamente instaladas

### ✅ Base de Datos
- Scripts SQL ejecutados correctamente
- Todas las tablas creadas con índices
- Datos iniciales insertados
- Usuario admin creado

### ✅ Próximas Pruebas Manuales

1. **Login**
   - [ ] Iniciar aplicación y verificar login con admin/123
   - [ ] Probar usuario incorrecto
   - [ ] Probar contraseña incorrecta

2. **Cambio de Clave**
   - [ ] Cambiar contraseña del usuario admin
   - [ ] Verificar que no acepta clave actual incorrecta
   - [ ] Verificar validación de contraseñas no coinciden
   - [ ] Verificar nueva clave funciona

3. **Sesión**
   - [ ] Verificar que muestra usuario en barra de estado
   - [ ] Probar cerrar sesión y volver al login
   - [ ] Probar salir de la aplicación

4. **Logs**
   - [ ] Verificar que se crean archivos en carpeta Logs
   - [ ] Verificar registros en tabla IAP_TD_LOGS

## 📦 Paquetes NuGet Instalados

- `System.Data.SqlClient` 4.9.1
- `BCrypt.Net-Next` 4.2.0
- `System.Configuration.ConfigurationManager` 10.0.8

## 🔄 Próximos Pasos - FASE 2

1. CRUD de Usuarios
2. Pantalla de Configuración de Parámetros
3. Dashboard en el formulario principal

## 📋 Notas Importantes

### Para Desarrollo
- La cadena de conexión está en texto plano en `App.config`
- Los logs se generan en la carpeta `Logs` del ejecutable
- El usuario admin tiene clave "123" (cambiar en producción)

### Para Producción
- Encriptar la cadena de conexión
- Cambiar contraseña del usuario admin
- Configurar limpieza automática de logs antiguos
- Revisar permisos de carpeta de logs

## 🐛 Solución de Problemas

### Error de conexión a SQL Server
1. Verificar que SQL Server Express está en ejecución
2. Verificar usuario y contraseña en App.config
3. Ejecutar scripts de base de datos si es primera vez

### Error "La cadena de certificación..."
- El parámetro `TrustServerCertificate=True` ya está configurado

### No aparecen logs en BD
- Verificar conexión a base de datos
- Revisar archivos de log en carpeta Logs
- Verificar permisos de escritura

## 📞 Soporte

Para consultas o problemas, verificar los logs en:
- **Archivos**: `Logs/Log_YYYYMMDD.txt`
- **Base de Datos**: Tabla `IAP_TD_LOGS`
- **Emergencia**: `Logs/Emergency.txt`
