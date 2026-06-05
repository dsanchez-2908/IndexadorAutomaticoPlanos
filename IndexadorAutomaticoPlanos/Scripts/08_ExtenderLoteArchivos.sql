-- =============================================
-- Script: 08_ExtenderLoteArchivos.sql
-- Descripción: Extiende la tabla IAP_TD_LOTE_ARCHIVOS 
--              con columnas para procesamiento de imágenes
-- Fase: 4 - Preparación de Imágenes
-- =============================================

USE Capturador
GO

-- Agregar columnas para almacenar datos de procesamiento de imágenes
IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'dsRutaImagenJpg')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD dsRutaImagenJpg NVARCHAR(500) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'txImagenBase64')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD txImagenBase64 NVARCHAR(MAX) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'txResultadoOcr')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD txResultadoOcr NVARCHAR(MAX) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'snTieneOcr')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD snTieneOcr BIT NOT NULL DEFAULT 0
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'nuDpiProcesamiento')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD nuDpiProcesamiento INT NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'dsEsquinaRecorte')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD dsEsquinaRecorte NVARCHAR(50) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'nuPorcentajeRecorte')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD nuPorcentajeRecorte DECIMAL(5,2) NULL
END
GO

-- Verificar columnas agregadas
SELECT 
	c.name AS NombreColumna,
	t.name AS TipoDato,
	c.max_length AS LongitudMaxima,
	c.is_nullable AS Nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS')
	AND c.name IN ('dsRutaImagenJpg', 'txImagenBase64', 'txResultadoOcr', 
				   'snTieneOcr', 'nuDpiProcesamiento', 'dsEsquinaRecorte', 
				   'nuPorcentajeRecorte')
ORDER BY c.column_id
GO

PRINT 'Script 08_ExtenderLoteArchivos.sql ejecutado correctamente'
GO
