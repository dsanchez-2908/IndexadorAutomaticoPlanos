-- =============================================
-- Script: 09_ActualizarPorcentajesRecorte.sql
-- Descripción: Actualiza tabla IAP_TD_LOTE_ARCHIVOS 
--              para manejar porcentajes de recorte horizontal y vertical separados
-- Fase: 4 - Preparación de Imágenes (corrección)
-- =============================================

USE Capturador
GO

-- Renombrar columna nuPorcentajeRecorte a nuPorcentajeRecorteHorizontal
IF EXISTS (SELECT * FROM sys.columns 
		   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
		   AND name = 'nuPorcentajeRecorte')
AND NOT EXISTS (SELECT * FROM sys.columns 
				WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
				AND name = 'nuPorcentajeRecorteHorizontal')
BEGIN
	EXEC sp_rename 'IAP_TD_LOTE_ARCHIVOS.nuPorcentajeRecorte', 
				   'nuPorcentajeRecorteHorizontal', 'COLUMN'
	PRINT 'Columna nuPorcentajeRecorte renombrada a nuPorcentajeRecorteHorizontal'
END
GO

-- Agregar columna nuPorcentajeRecorteVertical
IF NOT EXISTS (SELECT * FROM sys.columns 
			   WHERE object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS') 
			   AND name = 'nuPorcentajeRecorteVertical')
BEGIN
	ALTER TABLE IAP_TD_LOTE_ARCHIVOS
	ADD nuPorcentajeRecorteVertical DECIMAL(5,2) NULL
	PRINT 'Columna nuPorcentajeRecorteVertical agregada'
END
GO

-- Verificar columnas
SELECT 
	c.name AS NombreColumna,
	t.name AS TipoDato,
	c.max_length AS LongitudMaxima,
	c.is_nullable AS Nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('IAP_TD_LOTE_ARCHIVOS')
	AND c.name IN ('nuPorcentajeRecorteHorizontal', 'nuPorcentajeRecorteVertical')
ORDER BY c.column_id
GO

PRINT 'Script 09_ActualizarPorcentajesRecorte.sql ejecutado correctamente'
GO
