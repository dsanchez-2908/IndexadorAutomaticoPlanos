-- =============================================
-- Script: 12_FinalizarLotes.sql
-- Descripción: Extiende el esquema para Finalización de Lotes
-- Autor: Sistema
-- Fecha: 2026-06-10
-- =============================================

USE Capturador;
GO

PRINT '==================================';
PRINT 'Iniciando extensión: Finalización de Lotes';
PRINT '==================================';
GO

-- =============================================
-- 1. Agregar columnas faltantes a IAP_TD_LOTES
-- =============================================

-- Agregar dsCarpetaOrigen si no existe
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
			   WHERE TABLE_NAME = 'IAP_TD_LOTES' AND COLUMN_NAME = 'dsCarpetaOrigen')
BEGIN
	ALTER TABLE IAP_TD_LOTES ADD dsCarpetaOrigen NVARCHAR(500) NULL;
	PRINT 'Columna dsCarpetaOrigen agregada a IAP_TD_LOTES';
END
ELSE
BEGIN
	PRINT 'Columna dsCarpetaOrigen ya existe en IAP_TD_LOTES';
END
GO

-- Agregar snActivo si no existe
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
			   WHERE TABLE_NAME = 'IAP_TD_LOTES' AND COLUMN_NAME = 'snActivo')
BEGIN
	ALTER TABLE IAP_TD_LOTES ADD snActivo BIT NOT NULL DEFAULT 1;
	PRINT 'Columna snActivo agregada a IAP_TD_LOTES';
END
ELSE
BEGIN
	PRINT 'Columna snActivo ya existe en IAP_TD_LOTES';
END
GO

-- =============================================
-- 2. Verificar/Crear Estado de Lote 4: Pendiente de Finalizar
-- =============================================

IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote = 4)
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (cdEstadoLote, dsEstado, dsDescripcion)
	VALUES (4, 'Pendiente de Finalizar', 'Lote controlado y pendiente de finalización');
	PRINT 'Estado de lote 4 "Pendiente de Finalizar" creado';
END
ELSE
BEGIN
	-- Actualizar descripción si ya existe
	UPDATE IAP_TV_ESTADOS_LOTE 
	SET dsEstado = 'Pendiente de Finalizar',
		dsDescripcion = 'Lote controlado y pendiente de finalización'
	WHERE cdEstadoLote = 4;
	PRINT 'Estado de lote 4 "Pendiente de Finalizar" actualizado';
END
GO

-- =============================================
-- 3. Verificar/Crear Estado de Lote 5: Finalizado (NUEVO)
-- =============================================

IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote = 5)
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (cdEstadoLote, dsEstado, dsDescripcion)
	VALUES (5, 'Finalizado', 'Lote finalizado con archivos renombrados e índice generado');
	PRINT 'Estado de lote 5 "Finalizado" creado';
END
ELSE
BEGIN
	PRINT 'Estado de lote 5 "Finalizado" ya existe';
END
GO

-- =============================================
-- 4. Verificar índice en cdEstadoLote (si no existe)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IAP_TD_LOTES_cdEstadoLote' AND object_id = OBJECT_ID('IAP_TD_LOTES'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_IAP_TD_LOTES_cdEstadoLote
		ON IAP_TD_LOTES (cdEstadoLote)
		INCLUDE (dsNombreLote, feAlta);
	PRINT 'Índice IX_IAP_TD_LOTES_cdEstadoLote creado';
END
ELSE
BEGIN
	PRINT 'Índice IX_IAP_TD_LOTES_cdEstadoLote ya existe';
END
GO

-- =============================================
-- 5. Crear vista de lotes para finalización
-- =============================================

IF OBJECT_ID('dbo.IAP_VW_LOTES_PARA_FINALIZACION', 'V') IS NOT NULL
	DROP VIEW dbo.IAP_VW_LOTES_PARA_FINALIZACION;
GO

CREATE VIEW dbo.IAP_VW_LOTES_PARA_FINALIZACION
AS
SELECT 
	l.cdLote,
	l.dsNombreLote,
	l.dsCarpetaOrigen,
	l.cdEstadoLote,
	el.dsEstado AS dsEstadoLote,
	l.feAlta,
	l.feUltimaModificacion,
	(SELECT COUNT(*) 
	 FROM IAP_TD_LOTE_ARCHIVOS la 
	 WHERE la.cdLote = l.cdLote) AS CantidadArchivos
FROM IAP_TD_LOTES l
INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
WHERE l.cdEstadoLote = 4  -- Pendiente de Finalizar
  AND l.snActivo = 1;
GO

PRINT 'Vista IAP_VW_LOTES_PARA_FINALIZACION creada';
GO

-- =============================================
-- 6. Verificar configuración PATH_REPOSITORIO
-- =============================================

PRINT '==================================';
PRINT 'NOTA: Verificar que App.config contenga:';
PRINT '<add key="PATH_REPOSITORIO" value="C:\Repositorio\Planos" />';
PRINT '==================================';
GO

PRINT '==================================';
PRINT 'Script 12_FinalizarLotes.sql completado exitosamente';
PRINT '==================================';
GO
