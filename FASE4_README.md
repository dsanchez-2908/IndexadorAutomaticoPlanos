# FASE 4 - Preparación de Imágenes

## Requisitos Previos

### Tesseract OCR (Opcional - solo si se va a usar OCR)

La aplicación requiere Tesseract OCR para extraer texto de las imágenes. Si no planea usar la función de OCR, puede omitir esta instalación.

#### Instalación de Tesseract:

1. **Descargar Tesseract para Windows:**
   - Visite: https://github.com/UB-Mannheim/tesseract/wiki
   - Descargue el instalador más reciente (ej: `tesseract-ocr-w64-setup-5.3.3.20231005.exe`)

2. **Ejecutar el instalador:**
   - Durante la instalación, asegúrese de seleccionar:
	 - ✅ Spanish language data (`spa.traineddata`)
	 - ✅ English language data (`eng.traineddata`)
   - Ubicación de instalación recomendada: `C:\Program Files\Tesseract-OCR\`

3. **Configurar ubicación de tessdata:**
   La aplicación busca `tessdata` en las siguientes ubicaciones (en orden):
   - `[Carpeta de la aplicación]\tessdata`
   - `[Directorio actual]\tessdata`
   - `C:\Program Files\Tesseract-OCR\tessdata`
   - `C:\tessdata`

#### Opción alternativa: Copiar tessdata a la aplicación

Si prefiere no instalar Tesseract globalmente:

1. Cree una carpeta `tessdata` junto al ejecutable de la aplicación
2. Copie los archivos `spa.traineddata` y `eng.traineddata` dentro
3. Estos archivos se pueden descargar desde:
   https://github.com/tesseract-ocr/tessdata/tree/main

### Configuración del Repositorio de Imágenes

Edite el archivo `App.config` y ajuste la ruta del repositorio de imágenes:

```xml
<add key="PATH_REPOSITORIO" value="C:\Repositorio_Imagenes_Planos" />
```

La aplicación creará esta carpeta automáticamente si no existe.

## Uso del Módulo de Preparación de Imágenes

### 1. Acceder al módulo

**Menú:** Procesos → **3. Preparación de Imágenes**

### 2. Configurar parámetros

- **Esquina a recortar:** Seleccione dónde está la carátula del plano
  - Inferior Derecha (predeterminado - más común)
  - Inferior Izquierda
  - Superior Derecha
  - Superior Izquierda

- **DPI:** Resolución de la imagen (predeterminado: 300)
  - Mayor DPI = mejor calidad, mayor tamaño de archivo
  - Rango recomendado: 200-400 DPI

- **Porcentaje de recorte:** % del plano original a conservar (predeterminado: 30%)
  - Ajuste según el tamaño de la carátula en sus planos

- **Ejecutar OCR:** Active solo si desea extraer texto con Tesseract
  - ⚠️ Aumenta significativamente el tiempo de procesamiento
  - Útil para el método híbrido con OpenAI (más económico)

### 3. Previsualizar el recorte

1. Haga clic en **"Cargar PDF para Preview"**
2. Seleccione un PDF de ejemplo de su lote
3. La imagen se cargará mostrando un **rectángulo rojo** que indica la zona a recortar
4. Ajuste el **porcentaje** o la **esquina** y observe cómo cambia el rectángulo en tiempo real

### 4. Seleccionar lotes

- La grilla muestra todos los lotes en estado **"Pendiente de Preparar Imágenes"**
- Puede seleccionar **uno o múltiples lotes** (Ctrl+Click o Shift+Click)

### 5. Procesar

1. Haga clic en **"Procesar Lotes Seleccionados"**
2. Confirme los parámetros en el diálogo
3. El sistema procesará los PDFs:
   - Extrae la primera página de cada PDF
   - Recorta según los parámetros configurados
   - Convierte a JPG con alta calidad (95%)
   - Opcionalmente ejecuta OCR
   - Convierte a Base64 para OpenAI
   - Guarda imagen física en `PATH_REPOSITORIO\[NombreLote]\[NombreArchivo].jpg`
   - Actualiza la base de datos con todos los datos
4. Al finalizar, el estado del lote cambia a **"Pendiente de Procesar por IA"**

## Procesamiento Paralelo y Control de Memoria

⚠️ **Importante:** Los PDFs pueden ser archivos grandes (15-60 MB)

- El sistema procesa un **máximo de 4 PDFs simultáneamente** para evitar sobrecarga de memoria
- Después de procesar cada archivo, libera explícitamente la memoria
- Si experimenta errores de memoria (`OutOfMemoryException`):
  1. Cierre otros programas que consuman RAM
  2. Procese lotes más pequeños
  3. Reinicie la aplicación entre procesamiento de lotes grandes

## Estructura de Archivos Generados

```
C:\Repositorio_Imagenes_Planos\
  ├── LOTE_000001\
  │     ├── Plano001.jpg
  │     ├── Plano002.jpg
  │     └── ...
  ├── LOTE_000002\
  │     ├── Plano010.jpg
  │     └── ...
  └── ...
```

## Datos Almacenados en Base de Datos

Para cada archivo procesado, se guarda:

- **dsRutaImagenJpg:** Ruta física de la imagen JPG recortada
- **txImagenBase64:** Imagen en Base64 (para envío a OpenAI)
- **txResultadoOcr:** Texto extraído por OCR (si está habilitado)
- **snTieneOcr:** Flag indicando si se ejecutó OCR exitosamente
- **nuDpiProcesamiento:** DPI usado
- **dsEsquinaRecorte:** Esquina seleccionada para recorte
- **nuPorcentajeRecorte:** Porcentaje aplicado

## Solución de Problemas

### Error: "No se encontró la carpeta tessdata"

- Instale Tesseract OCR siguiendo las instrucciones de este documento
- O copie manualmente `spa.traineddata` y `eng.traineddata` a `[App]\tessdata\`
- O desactive el checkbox **"Ejecutar OCR"** si no lo necesita

### Error: "Error al extraer primera página del PDF"

- Verifique que el archivo PDF no esté corrupto
- Intente abrir el PDF manualmente en Adobe Reader
- Algunos PDFs protegidos por contraseña pueden fallar

### La imagen de preview no se actualiza

- Asegúrese de haber cargado un PDF primero con **"Cargar PDF para Preview"**
- Pruebe con un PDF diferente

### El procesamiento es muy lento

- **Desactive OCR** si no lo necesita (es el paso más lento)
- Reduzca el **DPI** a 200 o 250
- Procese lotes más pequeños

## Próximos Pasos

Una vez completada la preparación de imágenes, los lotes estarán listos para:

**FASE 5:** Procesamiento por OpenAI (siguiente módulo)
