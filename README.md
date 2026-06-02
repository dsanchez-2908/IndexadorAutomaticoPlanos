# Indexador Automático de Planos

Sistema desarrollado en **C# .NET 10 Windows Forms** para la indexación masiva de planos que están en archivos PDF mediante **OpenAI**.

## 📋 Descripción

Este sistema permite gestionar y procesar grandes volúmenes de planos técnicos en formato PDF, preparándolos para su análisis mediante inteligencia artificial (OpenAI). El proyecto está estructurado en fases modulares que van desde la autenticación de usuarios hasta el procesamiento con IA.

## 🚀 Características Principales

### ✅ FASE 1 - Fundamentos (Completada)
- **Autenticación de usuarios** con BCrypt
- **Gestión de sesiones** con `SesionActual`
- **Base de datos SQL Server** con encriptación de cadena de conexión
- **Sistema de logging dual**: archivo + base de datos (Info, Warning, Error, Debug)
- **Cambio de contraseña** integrado
- **Interfaz MDI** (Multiple Document Interface)

### ✅ FASE 2 - Ingreso de Planos (Completada)
- **Escaneo de carpetas** para localizar archivos PDF
- **Detección de duplicados** antes de insertar en BD
- **Conteo preciso de páginas** usando librería PDFsharp
- **Metadata completa**: tamaño, fecha modificación, cantidad de páginas
- **Inserción batch** con feedback visual de progreso
- **Estados de archivo**: Ingresado, Preparado, Procesado, etc.

### 🔄 FASE 3 - Preparación de Lotes (Pendiente)
- Agrupación de archivos en lotes
- Gestión de estado de lotes
- Asignación de prioridades

### 🔄 FASE 4 - Preparación de Imágenes (Pendiente)
- Extracción de imágenes desde PDFs
- Conversión y optimización
- OCR opcional con Tesseract

### 🔄 FASE 5 - Procesamiento OpenAI (Pendiente)
- Integración con **Batch API de OpenAI**
- Envío masivo de imágenes/texto
- Procesamiento asíncrono y seguimiento

### 🔄 FASE 6 - Validación y Finalización (Pendiente)
- Revisión de resultados
- Corrección manual si es necesario
- Exportación de datos indexados

## 🛠️ Tecnologías Utilizadas

- **Framework**: .NET 10 (Windows Forms)
- **Base de Datos**: SQL Server (localhost\\SQLEXPRESS)
- **Seguridad**: BCrypt.Net-Next para hashing de contraseñas
- **PDF Processing**: PDFsharp 6.2.4 (MIT License)
- **Logging**: Sistema personalizado dual (archivo + BD)
- **Encriptación**: AES para cadenas de conexión

## 📦 Estructura del Proyecto

```
IndexadorAutomaticoPlanos/
├── DataAccess/          # Repositorios y acceso a datos
│   ├── ArchivoRepository.cs
│   ├── DatabaseHelper.cs
│   └── UsuarioRepository.cs
├── Entities/            # Modelos de dominio
│   ├── Archivo.cs
│   ├── Usuario.cs
│   ├── SesionActual.cs
│   └── ...
├── Security/            # Encriptación y configuración segura
│   ├── ConfigManager.cs
│   └── Encriptacion.cs
├── UI/                  # Formularios Windows Forms
│   ├── FrmLogin.cs
│   ├── FrmPrincipal.cs
│   └── FrmIngresoPlanos.cs
├── Utils/               # Utilidades y helpers
│   ├── Logger.cs
│   ├── PdfHelper.cs
│   └── PasswordHashGenerator.cs
├── Scripts/             # Scripts SQL para BD
│   ├── 01_CreateDatabase.sql
│   ├── 02_InsertInitialData.sql
│   └── ...
└── App.config           # Configuración (connection string encriptada)
```

## 🔧 Requisitos Previos

- **Visual Studio 2026** (o posterior) Community/Professional/Enterprise
- **.NET 10 SDK**
- **SQL Server** (Express o superior)
- **Windows 10/11**

## 📥 Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone https://github.com/dsanchez-2908/IndexadorAutomaticoPlanos.git
cd IndexadorAutomaticoPlanos
```

### 2. Configurar SQL Server
Ejecutar los scripts en orden desde la carpeta `Scripts/`:

```sql
-- 1. Crear la base de datos
sqlcmd -S localhost\SQLEXPRESS -U sa -P tu_password -i Scripts/01_CreateDatabase.sql

-- 2. Insertar datos iniciales
sqlcmd -S localhost\SQLEXPRESS -U sa -P tu_password -d Capturador -i Scripts/02_InsertInitialData.sql

-- 3. Agregar campos de archivos
sqlcmd -S localhost\SQLEXPRESS -U sa -P tu_password -d Capturador -i Scripts/05_AgregarCamposArchivos.sql
```

### 3. Configurar cadena de conexión
Editar `App.config` y actualizar con tus credenciales:

```xml
<connectionStrings>
  <add name="ConexionDB" 
	   connectionString="[cadena encriptada]" 
	   providerName="System.Data.SqlClient" />
</connectionStrings>
```

> **Nota**: La aplicación encriptará automáticamente la cadena de conexión al iniciar.

### 4. Compilar y ejecutar
```bash
dotnet build
dotnet run --project IndexadorAutomaticoPlanos/IndexadorAutomaticoPlanos.csproj
```

## 🔐 Credenciales Iniciales

- **Usuario**: `admin`
- **Contraseña**: `123`

> ⚠️ **IMPORTANTE**: Cambiar la contraseña después del primer inicio de sesión.

## 📊 Base de Datos

### Tablas Principales

- `IAP_TD_USUARIOS` - Usuarios del sistema
- `IAP_TD_ARCHIVOS` - Archivos PDF ingresados
- `IAP_TD_LOTES` - Lotes de procesamiento
- `IAP_TV_ESTADOS_ARCHIVO` - Estados de archivo
- `IAP_TD_LOGS` - Logs de la aplicación

### Estados de Archivo

1. **Ingresado** - PDF recién cargado
2. **Preparado** - Listo para procesamiento
3. **Procesado** - Enviado a OpenAI
4. **Validado** - Revisado y aprobado
5. **Finalizado** - Proceso completo
6. **Error** - Falló el procesamiento
7. **Anulado** - Cancelado manualmente

## 📝 Uso Básico

### 1. Iniciar Sesión
- Ejecutar la aplicación
- Ingresar usuario y contraseña
- El sistema registra el inicio de sesión en logs

### 2. Ingreso de Planos
1. Ir a menú **Procesos → 1. Ingreso de Planos**
2. Hacer clic en **Seleccionar Carpeta**
3. Elegir carpeta con archivos PDF
4. Hacer clic en **Escanear**
5. Revisar lista de archivos detectados
6. Seleccionar archivos a ingresar
7. Hacer clic en **Guardar** para insertar en BD

### 3. Consultar Logs
Los logs se encuentran en:
- **Archivo**: `Logs/IndexadorPlanos_YYYY-MM-DD.log`
- **Base de datos**: Tabla `IAP_TD_LOGS`

## 🐛 Solución de Problemas

### Error de conexión a SQL Server
```
SSL Provider: La cadena de certificación fue emitida por una entidad en la que no se confía
```
**Solución**: Agregar `TrustServerCertificate=True` a la cadena de conexión.

### Conteo de páginas incorrecto (siempre 1 página)
**Solución**: Verificar que PDFsharp esté instalado correctamente:
```bash
dotnet add package PDFsharp --version 6.2.4
```

### Login falla con contraseña correcta
**Solución**: Ejecutar script de corrección de hash:
```sql
sqlcmd -S localhost\SQLEXPRESS -U sa -P password -d Capturador -i Scripts/03_FixAdminPassword.sql
```

## 🗺️ Roadmap

- [x] FASE 1: Autenticación y Base de Datos
- [x] FASE 2: Ingreso de Planos
- [ ] FASE 3: Preparación de Lotes
- [ ] FASE 4: Preparación de Imágenes (OCR opcional)
- [ ] FASE 5: Procesamiento OpenAI (Batch API)
- [ ] FASE 6: Validación y Finalización

## 📄 Licencia

Este proyecto usa las siguientes librerías de código abierto:
- **PDFsharp** - MIT License
- **BCrypt.Net-Next** - MIT License

## 👤 Autor

**dsanchez-2908**

---

**Versión**: 1.0.0  
**Última actualización**: 2 de junio de 2026
