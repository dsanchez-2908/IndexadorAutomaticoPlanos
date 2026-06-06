/*
==============================================================================
Script: 10_ExtenderProcesamientoIA.sql
Descripción: Extiende el esquema para soportar procesamiento por OpenAI
			 - Crea tabla IAP_TD_RESULTADOS_IA para almacenar respuestas estructuradas
			 - Inserta parámetros de configuración de OpenAI en IAP_TV_PARAMETROS
			 - Verifica estados de lote y archivo necesarios para el flujo
Fecha: 2026-06-03
Autor: Sistema Indexador Automático de Planos - FASE 5
==============================================================================
*/

USE Capturador;
GO

-- =============================================
-- 1. CREAR TABLA DE RESULTADOS DE IA
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_RESULTADOS_IA]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_RESULTADOS_IA]
	(
		cdResultadoIA INT IDENTITY(1,1) NOT NULL,
		cdLoteArchivo INT NOT NULL,

		-- Campos extraídos del plano
		txNombreArchivo NVARCHAR(500) NULL,
		dsTipoPlano NVARCHAR(50) NULL,
		dsExpediente NVARCHAR(100) NULL,
		dsSeccion NVARCHAR(50) NULL,
		dsManzana NVARCHAR(50) NULL,
		dsParcela NVARCHAR(50) NULL,
		dsDireccion NVARCHAR(500) NULL,

		-- Niveles de confianza (0.00 a 1.00)
		nuConfianzaTipoPlano DECIMAL(5,2) NULL,
		nuConfianzaExpediente DECIMAL(5,2) NULL,
		nuConfianzaSeccion DECIMAL(5,2) NULL,
		nuConfianzaManzana DECIMAL(5,2) NULL,
		nuConfianzaParcela DECIMAL(5,2) NULL,
		nuConfianzaDireccion DECIMAL(5,2) NULL,

		-- Tokens consumidos
		nuPromptTokens INT NULL,
		nuCompletionTokens INT NULL,
		nuTotalTokens INT NULL,

		-- Metadata de procesamiento
		dsModalidadProcesamiento NVARCHAR(50) NULL, -- 'OCR', 'Imagen', 'Hibrido'
		nuIntentos INT NOT NULL DEFAULT 1,
		txRespuestaCompleta NVARCHAR(MAX) NULL, -- JSON completo de respuesta
		txMensajeError NVARCHAR(MAX) NULL,

		-- Auditoría
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NOT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,

		CONSTRAINT PK_IAP_TD_RESULTADOS_IA PRIMARY KEY CLUSTERED (cdResultadoIA ASC),
		CONSTRAINT FK_IAP_TD_RESULTADOS_IA_LOTE_ARCHIVOS FOREIGN KEY (cdLoteArchivo)
			REFERENCES IAP_TD_LOTE_ARCHIVOS(cdLoteArchivo),
		CONSTRAINT FK_IAP_TD_RESULTADOS_IA_USUARIO_ALTA FOREIGN KEY (cdUsuarioAlta)
			REFERENCES IAP_TD_USUARIOS(cdUsuario)
	);

	CREATE INDEX IX_IAP_TD_RESULTADOS_IA_LOTE_ARCHIVO ON IAP_TD_RESULTADOS_IA(cdLoteArchivo);
	CREATE INDEX IX_IAP_TD_RESULTADOS_IA_FECHA ON IAP_TD_RESULTADOS_IA(feAlta);

	PRINT 'Tabla IAP_TD_RESULTADOS_IA creada correctamente';
END
ELSE
BEGIN
	PRINT 'Tabla IAP_TD_RESULTADOS_IA ya existe';
END
GO

-- =============================================
-- 2. INSERTAR PARÁMETROS DE OPENAI
-- =============================================

-- API Key de OpenAI
IF NOT EXISTS (SELECT * FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_KEY')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion)
	VALUES ('OPENAI_API_KEY', 
			'TU_API_KEY_AQUI',
			'Clave de API de OpenAI para procesamiento de imágenes');
	PRINT 'Parámetro OPENAI_API_KEY insertado';
END
ELSE
BEGIN
	-- IMPORTANTE: No sobrescribir la API key existente
	-- UPDATE IAP_TD_PARAMETROS 
	-- SET dsValorParametro = 'TU_API_KEY_AQUI'
	-- WHERE dsClaveParametro = 'OPENAI_API_KEY';
	PRINT 'Parámetro OPENAI_API_KEY ya existe (no se modifica)';
END
GO

-- URL base de la API de OpenAI
IF NOT EXISTS (SELECT * FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_URL')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion)
	VALUES ('OPENAI_API_URL', 'https://api.openai.com/v1', 'URL base de la API de OpenAI');
	PRINT 'Parámetro OPENAI_API_URL insertado';
END
GO

-- Modelo de OpenAI a utilizar
IF NOT EXISTS (SELECT * FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_MODELO')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion)
	VALUES ('OPENAI_MODELO', 'gpt-4o-mini', 'Modelo de OpenAI para procesamiento de planos');
	PRINT 'Parámetro OPENAI_MODELO insertado';
END
GO

-- Máximo de reintentos en caso de error
IF NOT EXISTS (SELECT * FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_MAX_REINTENTOS')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion)
	VALUES ('OPENAI_MAX_REINTENTOS', '3', 'Número máximo de reintentos en caso de error de API');
	PRINT 'Parámetro OPENAI_MAX_REINTENTOS insertado';
END
GO

-- Prompt para procesamiento por OCR
IF NOT EXISTS (SELECT * FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_PROMPT_OCR')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion)
	VALUES ('OPENAI_API_PROMPT_OCR',
'Eres un asistente experto en extracción de datos de planos arquitectónicos de Buenos Aires.

Extraer los siguientes campos del texto OCR del plano:

Reglas de extracción:
- **TipoPlano** solo puede ser: "Obra", "Mensura" o "Instalaciones"
- **Expediente** puede ser null si no está presente
- **Seccion** puede aparecer como: Secc, Sección, S, Secc:, Sección:
- **Manzana** puede aparecer como: Manz, M, Manz:
- **Parcela** puede aparecer como: Par, P, Parc, Por, Parcela, Par:, P:
- **Direccion** es la calle y altura (ej: "AV. DEL LIBERTADOR 6755/57")

Para cada campo extraído, proporciona un nivel de confianza entre 0.00 y 1.00.

Responder SOLO con JSON en el siguiente formato exacto:
{
  "archivo": "nombre_del_archivo.pdf",
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
}',
			'Prompt para procesamiento de planos mediante OCR');
	PRINT 'Parámetro OPENAI_API_PROMPT_OCR insertado';
END
GO

-- Prompt para procesamiento por imagen (base64)
IF NOT EXISTS (SELECT * FROM IAP_TD_PARAMETROS WHERE dsClaveParametro = 'OPENAI_API_PROMPT_IMAGEN')
BEGIN
	INSERT INTO IAP_TD_PARAMETROS (dsClaveParametro, dsValorParametro, dsDescripcion)
	VALUES ('OPENAI_API_PROMPT_IMAGEN', 
'Eres un asistente experto en extracción de datos de planos arquitectónicos de Buenos Aires.

Analiza la imagen del plano y extraer los siguientes campos:

Reglas de extracción:
- **TipoPlano** solo puede ser: "Obra", "Mensura" o "Instalaciones"
- **Expediente** puede ser null si no está presente en el plano
- **Seccion** puede aparecer como: Secc, Sección, S, Secc:, Sección:
- **Manzana** puede aparecer como: Manz, M, Manz:
- **Parcela** puede aparecer como: Par, P, Parc, Por, Parcela, Par:, P:
- **Direccion** es la calle y altura (ej: "AV. DEL LIBERTADOR 6755/57")

Estos datos generalmente se encuentran en un recuadro o cartucho en la esquina del plano.

Para cada campo extraído, proporciona un nivel de confianza entre 0.00 y 1.00.

Responder SOLO con JSON en el siguiente formato exacto:
{
  "archivo": "nombre_del_archivo.pdf",
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
}',
			'Prompt para procesamiento de planos mediante visión (imagen base64)');
	PRINT 'Parámetro OPENAI_API_PROMPT_IMAGEN insertado';
END
GO

-- =============================================
-- 3. VERIFICAR ESTADOS DE LOTE NECESARIOS
-- =============================================
PRINT '';
PRINT 'Verificando estados de lote...';

-- Estado 2: Pendiente de Procesamiento por IA
DECLARE @estadoIA INT;
SELECT @estadoIA = cdEstadoLote FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Pendiente de Procesamiento por IA';

IF @estadoIA IS NULL
BEGIN
	-- Verificar si existe estado 2
	IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote = 2)
	BEGIN
		SET IDENTITY_INSERT IAP_TV_ESTADOS_LOTE ON;
		INSERT INTO IAP_TV_ESTADOS_LOTE (cdEstadoLote, dsEstado, dsDescripcion)
		VALUES (2, 'Pendiente de Procesamiento por IA', 'Lote listo para envío a OpenAI');
		SET IDENTITY_INSERT IAP_TV_ESTADOS_LOTE OFF;
		PRINT '  - Estado "Pendiente de Procesamiento por IA" creado con ID 2';
	END
	ELSE
	BEGIN
		UPDATE IAP_TV_ESTADOS_LOTE 
		SET dsEstado = 'Pendiente de Procesamiento por IA',
			dsDescripcion = 'Lote listo para envío a OpenAI'
		WHERE cdEstadoLote = 2;
		PRINT '  - Estado ID 2 actualizado a "Pendiente de Procesamiento por IA"';
	END
END
ELSE
BEGIN
	PRINT '  - Estado "Pendiente de Procesamiento por IA" ya existe (ID ' + CAST(@estadoIA AS VARCHAR) + ')';
END
GO

-- Estado 3: Pendiente de Control de Calidad
DECLARE @estadoQC INT;
SELECT @estadoQC = cdEstadoLote FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Pendiente de Control de Calidad';

IF @estadoQC IS NULL
BEGIN
	-- Verificar si existe estado 3
	IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote = 3)
	BEGIN
		SET IDENTITY_INSERT IAP_TV_ESTADOS_LOTE ON;
		INSERT INTO IAP_TV_ESTADOS_LOTE (cdEstadoLote, dsEstado, dsDescripcion)
		VALUES (3, 'Pendiente de Control de Calidad', 'Procesado por IA, pendiente de validación');
		SET IDENTITY_INSERT IAP_TV_ESTADOS_LOTE OFF;
		PRINT '  - Estado "Pendiente de Control de Calidad" creado con ID 3';
	END
	ELSE
	BEGIN
		UPDATE IAP_TV_ESTADOS_LOTE 
		SET dsEstado = 'Pendiente de Control de Calidad',
			dsDescripcion = 'Procesado por IA, pendiente de validación'
		WHERE cdEstadoLote = 3;
		PRINT '  - Estado ID 3 actualizado a "Pendiente de Control de Calidad"';
	END
END
ELSE
BEGIN
	PRINT '  - Estado "Pendiente de Control de Calidad" ya existe (ID ' + CAST(@estadoQC AS VARCHAR) + ')';
END
GO

-- =============================================
-- 4. VERIFICAR ESTADOS DE ARCHIVO EN LOTE
-- =============================================
PRINT '';
PRINT 'Verificando estados de archivo en lote...';

-- Estado: Enviado a IA
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Enviado a IA')
BEGIN
	PRINT '  - ADVERTENCIA: Estado "Enviado a IA" no existe. Debería haber sido creado en script 07.';
	PRINT '    Verificar IAP_TV_ESTADOS_ARCHIVO_LOTE manualmente.';
END
ELSE
BEGIN
	PRINT '  - Estado "Enviado a IA" existe correctamente';
END

-- Estado: Procesado
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Procesado')
BEGIN
	PRINT '  - ADVERTENCIA: Estado "Procesado" no existe. Debería haber sido creado en script 07.';
END
ELSE
BEGIN
	PRINT '  - Estado "Procesado" existe correctamente';
END

-- Estado: Pendiente de Controlar
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Pendiente de Controlar')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion)
	VALUES ('Pendiente de Controlar', 'Archivo procesado por IA, pendiente de control de calidad');
	PRINT '  - Estado "Pendiente de Controlar" creado';
END
ELSE
BEGIN
	PRINT '  - Estado "Pendiente de Controlar" existe correctamente';
END
GO

-- =============================================
-- 5. VISTA PARA CONSULTA DE RESULTADOS IA
-- =============================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_IAP_RESULTADOS_IA')
	DROP VIEW VW_IAP_RESULTADOS_IA;
GO

CREATE VIEW VW_IAP_RESULTADOS_IA
AS
SELECT 
	r.cdResultadoIA,
	r.cdLoteArchivo,
	l.cdLote,
	l.dsNombreLote,
	r.txNombreArchivo,
	r.dsTipoPlano,
	r.dsExpediente,
	r.dsSeccion,
	r.dsManzana,
	r.dsParcela,
	r.dsDireccion,
	r.nuConfianzaTipoPlano,
	r.nuConfianzaSeccion,
	r.nuConfianzaManzana,
	r.nuConfianzaParcela,
	r.nuConfianzaDireccion,
	r.nuPromptTokens,
	r.nuCompletionTokens,
	r.nuTotalTokens,
	r.dsModalidadProcesamiento,
	r.nuIntentos,
	r.txMensajeError,
	r.feAlta AS feProcesamientoIA,
	u.dsUsuario AS dsUsuarioProceso,
	eal.dsEstado AS dsEstadoArchivoLote
FROM IAP_TD_RESULTADOS_IA r
INNER JOIN IAP_TD_LOTE_ARCHIVOS la ON r.cdLoteArchivo = la.cdLoteArchivo
INNER JOIN IAP_TD_LOTES l ON la.cdLote = l.cdLote
INNER JOIN IAP_TD_USUARIOS u ON r.cdUsuarioAlta = u.cdUsuario
LEFT JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE eal ON la.cdEstadoArchivoLote = eal.cdEstadoArchivoLote;
GO

PRINT '';
PRINT '========================================';
PRINT 'Script 10_ExtenderProcesamientoIA.sql ejecutado correctamente';
PRINT '';
PRINT 'Componentes creados:';
PRINT '  - Tabla IAP_TD_RESULTADOS_IA';
PRINT '  - Vista VW_IAP_RESULTADOS_IA';
PRINT '';
PRINT 'Parámetros insertados:';
PRINT '  - OPENAI_API_KEY';
PRINT '  - OPENAI_API_URL';
PRINT '  - OPENAI_MODELO';
PRINT '  - OPENAI_MAX_REINTENTOS';
PRINT '  - OPENAI_API_PROMPT_OCR';
PRINT '  - OPENAI_API_PROMPT_IMAGEN';
PRINT '';
PRINT 'Estados verificados:';
PRINT '  - Lote: Pendiente de Procesamiento por IA (ID 2)';
PRINT '  - Lote: Pendiente de Control de Calidad (ID 3)';
PRINT '  - Archivo: Enviado a IA, Procesado, Pendiente de Controlar';
PRINT '========================================';
GO
