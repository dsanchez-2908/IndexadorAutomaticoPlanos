-- =============================================
-- Script: 02_InsertInitialData.sql
-- Descripción: Inserta datos iniciales y usuario administrador
-- Proyecto: Indexador Automático de Planos
-- =============================================

USE Capturador;
GO

PRINT '===========================================';
PRINT 'Insertando datos iniciales...';
PRINT '===========================================';

-- =============================================
-- ESTADOS DE ARCHIVOS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO WHERE dsEstado = 'Ingresado')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Ingresado', 'Archivo ingresado al sistema, pendiente de asignar a lote', 1);
	PRINT 'Estado Archivo: Ingresado';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO WHERE dsEstado = 'Asignado a Lote')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Asignado a Lote', 'Archivo asignado a un lote', 1);
	PRINT 'Estado Archivo: Asignado a Lote';
END

-- =============================================
-- ESTADOS DE LOTES
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Pendiente de Preparar Imágenes')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Pendiente de Preparar Imágenes', 'Lote creado, pendiente de preparar las imágenes', 1);
	PRINT 'Estado Lote: Pendiente de Preparar Imágenes';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Pendiente de Procesar por IA')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Pendiente de Procesar por IA', 'Imágenes preparadas, pendiente de procesar por OpenAI', 1);
	PRINT 'Estado Lote: Pendiente de Procesar por IA';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Pendiente de Control de Calidad')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Pendiente de Control de Calidad', 'Procesado por IA, pendiente de control de calidad', 1);
	PRINT 'Estado Lote: Pendiente de Control de Calidad';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Pendiente de Finalizar')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Pendiente de Finalizar', 'Control de calidad completado, pendiente de finalizar', 1);
	PRINT 'Estado Lote: Pendiente de Finalizar';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Finalizado')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Finalizado', 'Lote finalizado y archivos renombrados', 1);
	PRINT 'Estado Lote: Finalizado';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Con Error')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Con Error', 'Lote con errores durante el procesamiento', 1);
	PRINT 'Estado Lote: Con Error';
END

-- =============================================
-- ESTADOS DE ARCHIVOS EN LOTE
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Pendiente de Preparar')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Pendiente de Preparar', 'Archivo en lote, pendiente de preparar imagen', 1);
	PRINT 'Estado Archivo Lote: Pendiente de Preparar';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Imagen Preparada')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Imagen Preparada', 'Imagen preparada, pendiente de procesar por IA', 1);
	PRINT 'Estado Archivo Lote: Imagen Preparada';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Procesado por IA')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Procesado por IA', 'Procesado por OpenAI, pendiente de controlar', 1);
	PRINT 'Estado Archivo Lote: Procesado por IA';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Pendiente de Controlar')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Pendiente de Controlar', 'Esperando control de calidad manual', 1);
	PRINT 'Estado Archivo Lote: Pendiente de Controlar';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Controlado')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Controlado', 'Control de calidad completado', 1);
	PRINT 'Estado Archivo Lote: Controlado';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Carátula Ilegible')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Carátula Ilegible', 'Plano con carátula ilegible', 1);
	PRINT 'Estado Archivo Lote: Carátula Ilegible';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Con Error')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Con Error', 'Error durante el procesamiento', 1);
	PRINT 'Estado Archivo Lote: Con Error';
END

-- =============================================
-- ESTADOS DE VALIDACIÓN DE PLANOS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_VALIDACION WHERE dsEstado = 'Pendiente de Validar')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_VALIDACION (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Pendiente de Validar', 'Plano pendiente de validación manual', 1);
	PRINT 'Estado Validación: Pendiente de Validar';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_VALIDACION WHERE dsEstado = 'Validado')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_VALIDACION (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Validado', 'Plano validado correctamente', 1);
	PRINT 'Estado Validación: Validado';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_VALIDACION WHERE dsEstado = 'Campos Incompletos')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_VALIDACION (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Campos Incompletos', 'Plano con campos obligatorios faltantes', 1);
	PRINT 'Estado Validación: Campos Incompletos';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_VALIDACION WHERE dsEstado = 'Baja Confianza')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_VALIDACION (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Baja Confianza', 'Plano con porcentaje de confianza bajo', 1);
	PRINT 'Estado Validación: Baja Confianza';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_VALIDACION WHERE dsEstado = 'Carátula Ilegible')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_VALIDACION (dsEstado, dsDescripcion, snActivo) 
	VALUES ('Carátula Ilegible', 'Carátula del plano ilegible', 1);
	PRINT 'Estado Validación: Carátula Ilegible';
END

-- =============================================
-- TIPOS DE PLANO
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IAP_TV_TIPOS_PLANO WHERE dsTipoPlano = 'Obra')
BEGIN
	INSERT INTO IAP_TV_TIPOS_PLANO (dsTipoPlano, snActivo) VALUES ('Obra', 1);
	PRINT 'Tipo de Plano: Obra';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_TIPOS_PLANO WHERE dsTipoPlano = 'Mensura')
BEGIN
	INSERT INTO IAP_TV_TIPOS_PLANO (dsTipoPlano, snActivo) VALUES ('Mensura', 1);
	PRINT 'Tipo de Plano: Mensura';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TV_TIPOS_PLANO WHERE dsTipoPlano = 'Instalaciones')
BEGIN
	INSERT INTO IAP_TV_TIPOS_PLANO (dsTipoPlano, snActivo) VALUES ('Instalaciones', 1);
	PRINT 'Tipo de Plano: Instalaciones';
END

-- =============================================
-- USUARIO ADMINISTRADOR
-- Clave: 123 (encriptada con BCrypt)
-- Hash: $2a$11$vrFOkkpu6IhttnjPjqw44ewCKl2XnDECIVuO.7joiSz7pYayMd8RS
-- NOTA: Este hash fue generado y verificado por la aplicación
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IAP_TD_USUARIOS WHERE dsUsuario = 'admin')
BEGIN
	INSERT INTO IAP_TD_USUARIOS 
	(dsUsuario, dsClave, dsNombreCompleto, snClaveTemporal, snPrimerIngreso, snActivo, feAlta, cdUsuarioAlta)
	VALUES 
	('admin', '$2a$11$vrFOkkpu6IhttnjPjqw44ewCKl2XnDECIVuO.7joiSz7pYayMd8RS', 'Administrador del Sistema', 0, 0, 1, GETDATE(), NULL);

	PRINT 'Usuario admin creado exitosamente.';
	PRINT 'Usuario: admin';
	PRINT 'Clave: 123';
END
ELSE
BEGIN
	PRINT 'El usuario admin ya existe.';
END

-- =============================================
-- PARÁMETROS INICIALES
-- =============================================
IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_KEY')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('OPENAI_API_KEY', '', 'API Key de OpenAI para procesamiento de imágenes', GETDATE());
	PRINT 'Parámetro: OPENAI_API_KEY';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_URL')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('OPENAI_API_URL', 'https://api.openai.com/v1/chat/completions', 'URL de la API de OpenAI', GETDATE());
	PRINT 'Parámetro: OPENAI_API_URL';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_IMAGEN_PROMPT')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('OPENAI_API_IMAGEN_PROMPT', 
	'Extraer los siguientes campos del plano en la imagen:

Reglas:
- TipoPlano solo puede ser: Obra, Mensura, Instalaciones
- Expediente puede ser null
- Sección puede aparecer como: Secc, Sección, S
- Manzana puede aparecer como: Manz, M
- Parcela puede aparecer como: Par, P
- Dirección debe incluir calle y altura

Responder SOLO JSON con esta estructura:
{
  "tipoPlano": "...",
  "expediente": "...",
  "seccion": "...",
  "manzana": "...",
  "parcela": "...",
  "direccion": "...",
  "confianza": {
	"tipoPlano": 0.00,
	"expediente": 0.00,
	"seccion": 0.00,
	"manzana": 0.00,
	"parcela": 0.00,
	"direccion": 0.00
  }
}', 
	'Prompt para procesar imágenes con OpenAI', GETDATE());
	PRINT 'Parámetro: OPENAI_API_IMAGEN_PROMPT';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_OCR_PROMPT')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('OPENAI_API_OCR_PROMPT', 
	'Extraer los siguientes campos del plano a partir del texto OCR proporcionado:

Reglas:
- TipoPlano solo puede ser: Obra, Mensura, Instalaciones
- Expediente puede ser null
- Sección puede aparecer como: Secc, Sección, S
- Manzana puede aparecer como: Manz, M
- Parcela puede aparecer como: Par, P
- Dirección debe incluir calle y altura

Responder SOLO JSON con esta estructura:
{
  "tipoPlano": "...",
  "expediente": "...",
  "seccion": "...",
  "manzana": "...",
  "parcela": "...",
  "direccion": "...",
  "confianza": {
	"tipoPlano": 0.00,
	"expediente": 0.00,
	"seccion": 0.00,
	"manzana": 0.00,
	"parcela": 0.00,
	"direccion": 0.00
  }
}

OCR detectado:
[TEXTO_OCR]', 
	'Prompt para procesar texto OCR con OpenAI', GETDATE());
	PRINT 'Parámetro: OPENAI_API_OCR_PROMPT';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'PATH_REPOSITORIO')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('PATH_REPOSITORIO', 'C:\RepositorioPlanos', 'Ruta base para guardar las imágenes JPG procesadas', GETDATE());
	PRINT 'Parámetro: PATH_REPOSITORIO';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'UMBRAL_CONFIANZA')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('UMBRAL_CONFIANZA', '0.85', 'Umbral de confianza mínimo para considerar un campo válido (0.00 - 1.00)', GETDATE());
	PRINT 'Parámetro: UMBRAL_CONFIANZA';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'DPI_IMAGEN')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('DPI_IMAGEN', '300', 'DPI para la conversión de PDF a imagen', GETDATE());
	PRINT 'Parámetro: DPI_IMAGEN';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'MAX_REINTENTOS_API')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('MAX_REINTENTOS_API', '3', 'Cantidad máxima de reintentos al llamar a la API de OpenAI', GETDATE());
	PRINT 'Parámetro: MAX_REINTENTOS_API';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'DELAY_REINTENTOS_MS')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('DELAY_REINTENTOS_MS', '5000', 'Delay en milisegundos entre reintentos (exponencial: 5s, 15s, 30s)', GETDATE());
	PRINT 'Parámetro: DELAY_REINTENTOS_MS';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_MODEL')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('OPENAI_MODEL', 'gpt-4o-mini', 'Modelo de OpenAI a utilizar', GETDATE());
	PRINT 'Parámetro: OPENAI_MODEL';
END

IF NOT EXISTS (SELECT 1 FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'MAX_PROCESAMIENTO_PARALELO')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion, feUltimaModificacion)
	VALUES ('MAX_PROCESAMIENTO_PARALELO', '4', 'Cantidad máxima de PDFs a procesar en paralelo', GETDATE());
	PRINT 'Parámetro: MAX_PROCESAMIENTO_PARALELO';
END

PRINT '===========================================';
PRINT 'Script 02_InsertInitialData.sql ejecutado exitosamente.';
PRINT 'Datos iniciales y usuario admin creados.';
PRINT '===========================================';
GO
