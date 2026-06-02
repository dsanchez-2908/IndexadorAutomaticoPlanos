# Scripts de Base de Datos - Indexador Automático de Planos

## Instrucciones de Ejecución

### Primera Instalación

Ejecutar los scripts en el siguiente orden:

1. **01_CreateDatabase.sql** - Crea la base de datos y todas las tablas
2. **02_InsertInitialData.sql** - Inserta datos iniciales y usuario administrador

### Si no puede iniciar sesión con admin/123

Si después de ejecutar los scripts no puede iniciar sesión:

3. **03_FixAdminPassword.sql** - Corrige la contraseña del usuario admin

### Resetear Base de Datos (DESARROLLO ÚNICAMENTE)

**⚠️ ADVERTENCIA: Esto eliminará TODOS los datos**

1. **00_ResetDatabase.sql** - Elimina completamente la base de datos
2. Luego ejecutar los scripts de primera instalación

## Conexión a SQL Server

- **Servidor**: localhost\SQLEXPRESS
- **Usuario**: sa
- **Contraseña**: 123
- **Base de Datos**: Capturador

## Usuario Administrador Inicial

- **Usuario**: admin
- **Contraseña**: 123

## Estructura de Nomenclatura

### Prefijo de Tablas
- `IAP_` - Indexador Automático de Planos (todas las tablas del proyecto)

### Tipo de Tabla
- `TD_` - Tablas de datos (ej: IAP_TD_USUARIOS)
- `TR_` - Tablas de relación (ej: IAP_TR_LOTE_ARCHIVO)
- `TV_` - Tablas de valores fijos (ej: IAP_TV_ESTADOS_LOTE)

### Prefijo de Campos
- `cd` - Campos ID (ej: cdUsuario)
- `fe` - Campos de fecha (ej: feAlta)
- `nu` - Campos numéricos (ej: nuCantidadPaginas)
- `ds` - Campos de texto (ej: dsNombreCompleto)
- `sn` - Campos SI/NO booleanos (ej: snActivo)

## Tablas Creadas

### Valores Fijos (TV_)
- IAP_TV_ESTADOS_ARCHIVO
- IAP_TV_ESTADOS_LOTE
- IAP_TV_ESTADOS_ARCHIVO_LOTE
- IAP_TV_ESTADOS_VALIDACION
- IAP_TV_TIPOS_PLANO

### Datos (TD_)
- IAP_TD_USUARIOS
- IAP_TD_PARAMETROS
- IAP_TD_ARCHIVOS
- IAP_TD_LOTES
- IAP_TD_PLANOS_PROCESADOS
- IAP_TD_LOGS

### Relación (TR_)
- IAP_TR_LOTE_ARCHIVO

## Estados del Sistema

### Estados de Lotes
1. Pendiente de Preparar Imágenes
2. Pendiente de Procesar por IA
3. Pendiente de Control de Calidad
4. Pendiente de Finalizar
5. Finalizado
6. Con Error

### Estados de Archivos en Lote
1. Pendiente de Preparar
2. Imagen Preparada
3. Procesado por IA
4. Pendiente de Controlar
5. Controlado
6. Carátula Ilegible
7. Con Error

## Parámetros Configurables

Los siguientes parámetros se pueden modificar desde la pantalla de configuración:

- OPENAI_API_KEY
- OPENAI_API_URL
- OPENAI_API_IMAGEN_PROMPT
- OPENAI_API_OCR_PROMPT
- PATH_REPOSITORIO
- UMBRAL_CONFIANZA
- DPI_IMAGEN
- MAX_REINTENTOS_API
- DELAY_REINTENTOS_MS
- OPENAI_MODEL
- MAX_PROCESAMIENTO_PARALELO
