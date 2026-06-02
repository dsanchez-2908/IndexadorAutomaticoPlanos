-- ==================================================================
-- Script 05: Agregar campos faltantes a IAP_TD_ARCHIVOS
-- ==================================================================
-- Agrega campos necesarios para FASE 2: Indexación de Archivos
-- ==================================================================

USE Capturador;
GO

PRINT '===========================================';
PRINT 'Agregando campos a IAP_TD_ARCHIVOS...';
PRINT '===========================================';

-- Agregar campo para tamaño del archivo en bytes
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
			   WHERE TABLE_NAME = 'IAP_TD_ARCHIVOS' AND COLUMN_NAME = 'nuTamanoBytes')
BEGIN
	ALTER TABLE IAP_TD_ARCHIVOS
	ADD nuTamanoBytes BIGINT NOT NULL DEFAULT 0;

	PRINT 'Campo nuTamanoBytes agregado correctamente.';
END
ELSE
BEGIN
	PRINT 'Campo nuTamanoBytes ya existe.';
END

-- Agregar campo para fecha de modificación del archivo físico
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
			   WHERE TABLE_NAME = 'IAP_TD_ARCHIVOS' AND COLUMN_NAME = 'feModificacionArchivo')
BEGIN
	ALTER TABLE IAP_TD_ARCHIVOS
	ADD feModificacionArchivo DATETIME NOT NULL DEFAULT GETDATE();

	PRINT 'Campo feModificacionArchivo agregado correctamente.';
END
ELSE
BEGIN
	PRINT 'Campo feModificacionArchivo ya existe.';
END

PRINT '===========================================';
PRINT 'Campos agregados exitosamente.';
PRINT '===========================================';
GO
