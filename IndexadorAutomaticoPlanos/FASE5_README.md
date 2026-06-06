# FASE 5: Procesamiento por OpenAI

## Descripción General

La **Fase 5** implementa la extracción automática de datos estructurados de planos arquitectónicos mediante la API de OpenAI (modelo `gpt-4o-mini`). El sistema procesa los planos utilizando dos modalidades:

1. **Modalidad Híbrida (OCR + Imagen)**: Intenta primero extraer datos del texto OCR generado en la Fase 4. Si no se obtienen todos los campos obligatorios, procesa la imagen en base64.
2. **Solo Imagen**: Procesa directamente la imagen en base64 del plano.

Los datos extraídos incluyen:
- **Tipo de Plano**: Obra, Mensura o Instalaciones
- **Expediente**: Número de expediente (opcional)
- **Sección**, **Manzana**, **Parcela**: Datos catastrales
- **Dirección**: Ubicación del inmueble
- **Niveles de Confianza**: Para cada campo extraído (0.00 a 1.00)
- **Tokens Consumidos**: Seguimiento de uso de API

---

## Componentes Implementados

### 1. Base de Datos

**Script: `10_ExtenderProcesamientoIA.sql`**

#### Tabla Principal: `IAP_TD_RESULTADOS_IA`

Almacena los resultados del procesamiento por OpenAI:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `cdResultadoIA` | INT (PK) | ID único del resultado |
| `cdLoteArchivo` | INT (FK) | Referencia al archivo procesado |
| `txNombreArchivo` | NVARCHAR(500) | Nombre del archivo PDF |
| `dsTipoPlano` | NVARCHAR(50) | Tipo de plano extraído |
| `dsExpediente` | NVARCHAR(100) | Número de expediente |
| `dsSeccion` | NVARCHAR(50) | Sección catastral |
| `dsManzana` | NVARCHAR(50) | Manzana catastral |
| `dsParcela` | NVARCHAR(50) | Parcela catastral |
| `dsDireccion` | NVARCHAR(500) | Dirección del inmueble |
| `nuConfianzaTipoPlano` | DECIMAL(5,2) | Nivel de confianza (0-1) |
| `nuConfianzaSeccion` | DECIMAL(5,2) | Nivel de confianza |
| `nuConfianzaManzana` | DECIMAL(5,2) | Nivel de confianza |
| `nuConfianzaParcela` | DECIMAL(5,2) | Nivel de confianza |
| `nuConfianzaDireccion` | DECIMAL(5,2) | Nivel de confianza |
| `nuPromptTokens` | INT | Tokens del prompt |
| `nuCompletionTokens` | INT | Tokens de respuesta |
| `nuTotalTokens` | INT | Tokens totales consumidos |
| `dsModalidadProcesamiento` | NVARCHAR(50) | OCR / Imagen / Hibrido-OCR / Hibrido-Imagen |
| `nuIntentos` | INT | Número de intentos realizados |
| `txRespuestaCompleta` | NVARCHAR(MAX) | JSON completo de OpenAI |
| `txMensajeError` | NVARCHAR(MAX) | Mensaje de error (si aplica) |

#### Vista: `VW_IAP_RESULTADOS_IA`

Vista consolidada que muestra:
- Resultados de IA con información de lotes
- Estado de procesamiento
- Usuario que procesó
- Métricas de tokens

#### Parámetros de Configuración

Se insertan automáticamente en `IAP_TV_PARAMETROS`:

| Parámetro | Valor por Defecto | Descripción |
|-----------|-------------------|-------------|
| `OPENAI_API_KEY` | (API Key proporcionada) | Clave de autenticación de OpenAI |
| `OPENAI_API_URL` | `https://api.openai.com/v1` | URL base de la API |
| `OPENAI_MODELO` | `gpt-4o-mini` | Modelo a utilizar |
| `OPENAI_MAX_REINTENTOS` | `3` | Reintentos en caso de error |
| `OPENAI_API_PROMPT_OCR` | (Prompt predefinido) | Prompt para procesamiento OCR |
| `OPENAI_API_PROMPT_IMAGEN` | (Prompt predefinido) | Prompt para procesamiento imagen |

#### Estados de Lote

- **Estado 2**: "Pendiente de Procesamiento por IA" (entrada a esta fase)
- **Estado 3**: "Pendiente de Control de Calidad" (salida exitosa)

#### Estados de Archivo en Lote

- **Enviado a IA**: Archivo enviado a OpenAI
- **Procesado**: Procesamiento exitoso
- **Pendiente de Controlar**: Listo para control de calidad
- **Error**: Falló el procesamiento

---

### 2. Entidades

**`ResultadoIA.cs`**

Mapea la tabla `IAP_TD_RESULTADOS_IA` con todas sus propiedades.

**Clases Auxiliares:**
- `RespuestaOpenAI`: Deserializa el JSON de respuesta de OpenAI
- `ConfianzaRespuesta`: Niveles de confianza por campo
- `UsageTokens`: Métricas de consumo de tokens

---

### 3. Lógica de Negocio

**`OpenAIProcesador.cs`** (en `Utils/`)

Procesador principal para interactuar con OpenAI:

#### Métodos Principales:

- **`ProcesarHibridoAsync()`**: Intenta primero OCR, luego imagen
- **`ProcesarPorOcrAsync()`**: Procesa solo texto OCR
- **`ProcesarPorImagenAsync()`**: Procesa imagen en base64
- **`LlamarAPIConReintentosAsync()`**: Llamada a API con backoff exponencial
- **`ParsearRespuesta()`**: Deserializa JSON con Newtonsoft.Json
- **`TieneCamposObligatorios()`**: Valida campos obligatorios (TipoPlano, Seccion, Manzana, Parcela)

#### Características:

- ✅ Reintentos automáticos con backoff exponencial (2s, 4s, 8s...)
- ✅ Manejo de rate limits y errores de API
- ✅ Temperatura baja (0.1) para respuestas deterministas
- ✅ JSON response format forzado
- ✅ Logging detallado de todas las operaciones

---

### 4. Acceso a Datos

**`LoteRepository.cs` - Métodos Extendidos:**

- **`ObtenerLotesPendientesProcesarIA()`**: Lista lotes en estado 2
- **`ObtenerArchivosParaProcesar(cdLote)`**: Archivos con imagen/OCR del lote
- **`GuardarResultadoIA(resultado)`**: Persiste resultado en DB
- **`ActualizarEstadoProcesamiento()`**: Actualiza estado de archivo
- **`MarcarLoteComoProcesado()`**: Cambia lote a estado 3 si todos sus archivos están OK
- **`ObtenerIdEstadoArchivoLotePorNombre()`**: Helper para obtener IDs de estados

---

### 5. Interfaz de Usuario

**`FrmProcesamientoIA.cs`**

#### Layout:

```
┌─────────────────────────────────────────────────────────────────┐
│ PROCESAMIENTO POR OPENAI                                         │
├──────────────┬──────────────────────────────────────────────────┤
│ Lotes        │ Configuración de Prompts                          │
│ Pendientes   │                                                   │
│              │ Prompt para Procesamiento OCR:                    │
│ [x] LOTE_001 │ ┌──────────────────────────────────────────────┐ │
│ [ ] LOTE_002 │ │ Eres un asistente experto...                 │ │
│ [x] LOTE_003 │ │ Extraer los siguientes campos...             │ │
│              │ │ ...                                          │ │
│              │ └──────────────────────────────────────────────┘ │
│              │                                                   │
│              │ Prompt para Procesamiento Imagen:                │
│              │ ┌──────────────────────────────────────────────┐ │
│              │ │ Analiza la imagen del plano...               │ │
│              │ │ ...                                          │ │
│              │ └──────────────────────────────────────────────┘ │
│              │                                                   │
│              │ [x] Usar Modalidad Híbrida (OCR + Imagen)        │
│              │                      [Guardar Prompts]            │
├──────────────┴──────────────────────────────────────────────────┤
│ Estado: Procesando lote 2/3 - Archivo 5/12: plano_001.pdf       │
│ [████████████████████░░░░░░░░░░░] 65%                           │
│                           [Refrescar] [Procesar Seleccionados]  │
└──────────────────────────────────────────────────────────────────┘
```

#### Funcionalidades:

1. **Grid de Lotes**: 
   - Muestra lotes en estado "Pendiente de Procesamiento por IA"
   - Selección múltiple con checkboxes
   - Información: Nombre, cantidad de archivos, fecha, estado

2. **Configuración de Prompts**:
   - Dos TextBox multilínea editables
   - Prompts se cargan desde `IAP_TV_PARAMETROS`
   - Botón para guardar cambios en DB

3. **Modalidad de Procesamiento**:
   - Checkbox "Usar Modalidad Híbrida (OCR + Imagen)"
   - Si está marcado: intenta OCR primero, luego imagen
   - Si está desmarcado: solo imagen

4. **Procesamiento Asíncrono**:
   - No bloquea la UI
   - Barra de progreso en tiempo real
   - Label de estado con archivo actual
   - Deshabilita controles durante procesamiento

5. **Manejo de Errores**:
   - Lotes con errores no avanzan de estado
   - Resumen final con estadísticas
   - Logging completo en `Logger`

---

## Flujo de Procesamiento

### Secuencia Normal:

```
1. Usuario selecciona uno o más lotes en estado 2
2. Configura modalidad (Híbrida o Solo Imagen)
3. Clic en "Procesar Seleccionados"
4. Sistema confirma la operación
5. Para cada lote:
   a. Obtiene archivos con imagen/OCR
   b. Para cada archivo:
	  - Actualiza estado a "Enviado a IA"
	  - Llama a OpenAI (con reintentos)
	  - Si exitoso:
		* Guarda resultado en IAP_TD_RESULTADOS_IA
		* Actualiza estado a "Pendiente de Controlar"
	  - Si falla:
		* Guarda error
		* Actualiza estado a "Error"
   c. Si todos los archivos OK:
	  - Marca lote como estado 3 (Pendiente de Control de Calidad)
   d. Si hay errores:
	  - Lote permanece en estado 2 para reintento posterior
6. Muestra resumen con estadísticas
7. Refresca grid de lotes
```

### Modalidad Híbrida:

```
1. Si archivo tiene OCR (snTieneOcr = 1):
   a. Procesa con texto OCR
   b. Si obtiene campos obligatorios (TipoPlano, Seccion, Manzana, Parcela):
	  - Guarda como "Hibrido-OCR"
	  - FIN
   c. Si faltan campos:
	  - Continúa con imagen
2. Procesa con imagen base64
3. Si exitoso:
   - Guarda como "Hibrido-Imagen" (si hubo intento OCR) o "Imagen"
4. Si falla:
   - Guarda como "Hibrido-Error" o "Imagen-Error"
```

---

## Instalación y Configuración

### 1. Ejecutar Script SQL

```sql
USE Capturador;
GO

-- Ejecutar el script completo
-- Este script es idempotente (se puede ejecutar múltiples veces)
```

```bash
sqlcmd -S localhost\SQLEXPRESS -d Capturador -U sa -P 123 -i "Scripts\10_ExtenderProcesamientoIA.sql"
```

### 2. Verificar Parámetros

Desde SQL Server Management Studio:

```sql
SELECT * FROM IAP_TV_PARAMETROS
WHERE dsParametro LIKE 'OPENAI%';
```

Deberías ver:
- `OPENAI_API_KEY` con tu clave
- `OPENAI_API_URL`
- `OPENAI_MODELO`
- `OPENAI_MAX_REINTENTOS`
- `OPENAI_API_PROMPT_OCR`
- `OPENAI_API_PROMPT_IMAGEN`

### 3. Verificar Estados

```sql
-- Estados de lote
SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote IN (2, 3);

-- Estados de archivo
SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE
WHERE dsEstado IN ('Enviado a IA', 'Procesado', 'Pendiente de Controlar', 'Error');
```

### 4. Preparar Lotes para Procesamiento

Los lotes deben haber completado la Fase 4 (Preparación de Imágenes). Para mover un lote al estado 2:

```sql
UPDATE IAP_TD_LOTES
SET cdEstadoLote = 2
WHERE cdLote = <ID_DEL_LOTE>;
```

### 5. Compilar y Ejecutar

```bash
dotnet build
dotnet run
```

---

## Uso del Sistema

### 1. Acceder a la Pantalla

Menú → **Procesos** → **4. Procesamiento OpenAI**

### 2. Configurar Prompts (Opcional)

Los prompts vienen preconfigurados, pero puedes editarlos según tus necesidades:

- **Prompt OCR**: Especifica cómo interpretar el texto OCR
- **Prompt Imagen**: Especifica cómo analizar la imagen visualmente

**Recomendaciones:**
- Mantén las instrucciones claras y específicas
- Especifica el formato JSON de respuesta
- Incluye las variaciones de términos (ej: "Secc, Sección, S")
- Usa temperatura baja (ya configurado en código: 0.1)

Clic en **[Guardar Prompts]** para persistir cambios.

### 3. Seleccionar Lotes

- Marca los checkboxes de los lotes a procesar
- Puedes seleccionar múltiples lotes
- La cantidad de archivos se muestra en la columna "Archivos"

### 4. Configurar Modalidad

- **[x] Usar Modalidad Híbrida (OCR + Imagen)**: 
  - Intenta primero con OCR
  - Si falla o faltan campos, usa imagen
  - Recomendado para mejor precisión

- **[ ] (Desmarcado)**: 
  - Solo procesa con imagen
  - Útil si el OCR es de baja calidad

### 5. Iniciar Procesamiento

1. Clic en **[Procesar Seleccionados]**
2. Confirma la operación en el diálogo
3. Observa el progreso en tiempo real:
   - Label de estado muestra lote y archivo actual
   - Barra de progreso muestra avance
4. Al finalizar:
   - Resumen con estadísticas
   - Grid se refresca automáticamente
   - Lotes exitosos desaparecen (pasan a estado 3)
   - Lotes con errores permanecen para reintento

---

## Seguimiento de Consumo de Tokens

### Consultar Tokens por Lote

```sql
SELECT 
	l.dsNombreLote,
	COUNT(*) AS nuArchivos,
	SUM(r.nuPromptTokens) AS nuTotalPromptTokens,
	SUM(r.nuCompletionTokens) AS nuTotalCompletionTokens,
	SUM(r.nuTotalTokens) AS nuTotalTokens
FROM IAP_TD_RESULTADOS_IA r
INNER JOIN IAP_TD_LOTE_ARCHIVOS la ON r.cdLoteArchivo = la.cdLoteArchivo
INNER JOIN IAP_TD_LOTES l ON la.cdLote = l.cdLote
GROUP BY l.dsNombreLote
ORDER BY l.dsNombreLote;
```

### Consultar Tokens Totales

```sql
SELECT 
	COUNT(*) AS nuArchivosProcesados,
	SUM(nuPromptTokens) AS nuTotalPromptTokens,
	SUM(nuCompletionTokens) AS nuTotalCompletionTokens,
	SUM(nuTotalTokens) AS nuTotalTokens,
	AVG(nuTotalTokens) AS nuPromedioTokensPorArchivo
FROM IAP_TD_RESULTADOS_IA
WHERE txMensajeError IS NULL;
```

### Consultar Modalidades Utilizadas

```sql
SELECT 
	dsModalidadProcesamiento,
	COUNT(*) AS nuCantidad,
	AVG(nuTotalTokens) AS nuPromedioTokens
FROM IAP_TD_RESULTADOS_IA
GROUP BY dsModalidadProcesamiento
ORDER BY nuCantidad DESC;
```

---

## Troubleshooting

### Problema: "No se encontró la API key de OpenAI"

**Causa**: Parámetro `OPENAI_API_KEY` no existe o está vacío.

**Solución**:
```sql
UPDATE IAP_TV_PARAMETROS
SET dsValor = 'sk-proj-...'
WHERE dsParametro = 'OPENAI_API_KEY';
```

### Problema: "Rate limit exceeded"

**Causa**: Demasiadas llamadas simultáneas a OpenAI.

**Solución**:
- El sistema reintenta automáticamente con backoff exponencial
- Si persiste, espera unos minutos y reintenta el lote
- Considera procesar lotes más pequeños

### Problema: "Error al parsear respuesta de OpenAI"

**Causa**: El modelo devolvió JSON malformado o texto adicional.

**Solución**:
- Revisa el campo `txRespuestaCompleta` en `IAP_TD_RESULTADOS_IA`
- Ajusta el prompt para ser más específico: "Responder SOLO JSON"
- Verifica que el modelo sea `gpt-4o-mini` (soporta JSON mode)

### Problema: Lote no avanza a estado 3

**Causa**: Al menos un archivo tiene error.

**Solución**:
```sql
-- Ver archivos con error del lote
SELECT 
	la.cdLoteArchivo,
	a.dsNombreArchivo,
	e.dsEstado,
	r.txMensajeError
FROM IAP_TD_LOTE_ARCHIVOS la
INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE e ON la.cdEstadoArchivoLote = e.cdEstadoArchivoLote
LEFT JOIN IAP_TD_RESULTADOS_IA r ON la.cdLoteArchivo = r.cdLoteArchivo
WHERE la.cdLote = <ID_LOTE>
  AND e.dsEstado = 'Error';
```

- Revisa el error específico
- Corrige el prompt o la imagen
- Reprocesa el lote

### Problema: Campos extraídos con baja confianza

**Causa**: Imagen de baja calidad o texto OCR con errores.

**Solución**:
- Revisa la calidad de la imagen generada en Fase 4
- Ajusta parámetros de DPI (usar 300 o más)
- Refina el prompt con ejemplos específicos
- Considera modalidad "Solo Imagen" si OCR es muy malo

---

## Estructura de JSON de Respuesta

OpenAI devuelve (en `txRespuestaCompleta`):

```json
{
  "archivo": "EX-2006-00041690-MGEYA-DGROC-PLANOS.pdf",
  "tipoPlano": "Obra",
  "expediente": "EX-2006-00041690-MGEYA-DGROC",
  "seccion": "27",
  "manzana": "101",
  "parcela": "006 A",
  "direccion": "AV. DEL LIBERTADOR 6755/57",
  "confianza": {
	"tipoPlano": 0.99,
	"expediente": 0.95,
	"seccion": 0.98,
	"manzana": 0.98,
	"parcela": 0.97,
	"direccion": 0.96
  }
}
```

Adicionalmente, se captura el `usage` de la respuesta de OpenAI (no en el JSON, sino en metadata):

```json
{
  "prompt_tokens": 5230,
  "completion_tokens": 180,
  "total_tokens": 5410
}
```

---

## Mejoras Futuras

- ✨ Cancelación de procesamiento en curso
- ✨ Procesamiento paralelo con semáforo (actualmente secuencial)
- ✨ Validación de campos con reglas de negocio (ej: formato de expediente)
- ✨ Reentrenamiento del prompt con ejemplos específicos (few-shot learning)
- ✨ Métricas de costo en tiempo real (tokens × precio por token)
- ✨ Dashboard de análisis de confianza y precisión
- ✨ Integración con GPT-4 Vision para casos complejos

---

## Logs y Auditoría

Todas las operaciones se registran en:

- **Logger del sistema**: `Utils/Logger.cs`
- **Base de datos**: `IAP_TD_RESULTADOS_IA` (con `txMensajeError`, `nuIntentos`, `txRespuestaCompleta`)

### Ver Logs de Procesamiento

```sql
SELECT 
	r.cdResultadoIA,
	r.txNombreArchivo,
	r.dsModalidadProcesamiento,
	r.nuIntentos,
	r.nuTotalTokens,
	r.txMensajeError,
	r.feAlta
FROM IAP_TD_RESULTADOS_IA r
ORDER BY r.feAlta DESC;
```

---

## Contacto y Soporte

Para dudas o problemas:
- Revisar logs en `Logger`
- Consultar tabla `IAP_TD_RESULTADOS_IA` para errores específicos
- Verificar parámetros en `IAP_TV_PARAMETROS`

---

## Resumen de Estados del Sistema

| Estado | Lote | Archivo | Significado |
|--------|------|---------|-------------|
| 0 | Pendiente de Preparación de Lotes | - | Archivos ingresados, sin agrupar |
| 1 | Pendiente de Preparación de Imágenes | Asociado | Lote creado, esperando Fase 4 |
| 1 | - | Imagen Extraída | Fase 4 en progreso |
| 2 | **Pendiente de Procesamiento por IA** | **Enviado a IA** | **Fase 5 entrada** |
| 2 | - | Procesado | Archivo procesado exitosamente |
| 2 | - | Pendiente de Controlar | Listo para QC |
| 2 | - | Error | Falló procesamiento |
| 3 | **Pendiente de Control de Calidad** | - | **Fase 5 salida** (próxima fase) |

---

**Fin de FASE5_README.md**
