-- =============================================
-- Script: 11_ExtenderControlCalidad.sql
-- Descripción: Extiende el esquema para FASE 6 - Control de Calidad
-- Autor: Sistema
-- Fecha: 2026-06-06
-- =============================================

USE Capturador;
GO

PRINT '==================================';
PRINT 'Iniciando extensión FASE 6: Control de Calidad';
PRINT '==================================';
GO

-- =============================================
-- 1. Verificar/Crear Estado de Lote 4: Pendiente de Finalizar
-- =============================================

IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote = 4)
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (cdEstadoLote, dsEstado, dsDescripcion)
	VALUES (4, 'Pendiente de Finalizar', 'Lote controlado y pendiente de finalización');
	PRINT 'Estado de lote 4 "Pendiente de Finalizar" creado';
END
ELSE
BEGIN
	PRINT 'Estado de lote 4 ya existe';
END
GO

-- =============================================
-- 2. Verificar/Crear Estados de Archivo en Lote
-- =============================================

-- Estado: Controlado
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Controlado')
BEGIN
	DECLARE @nextId INT;
	SELECT @nextId = ISNULL(MAX(cdEstadoArchivoLote), 0) + 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE;

	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (cdEstadoArchivoLote, dsEstado, dsDescripcion)
	VALUES (@nextId, 'Controlado', 'Archivo validado y controlado por usuario');
	PRINT 'Estado "Controlado" creado con ID: ' + CAST(@nextId AS VARCHAR);
END
ELSE
BEGIN
	PRINT 'Estado "Controlado" ya existe';
END
GO

-- Estado: Carátula Ilegible
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Carátula Ilegible')
BEGIN
	DECLARE @nextId INT;
	SELECT @nextId = ISNULL(MAX(cdEstadoArchivoLote), 0) + 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE;

	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (cdEstadoArchivoLote, dsEstado, dsDescripcion)
	VALUES (@nextId, 'Carátula Ilegible', 'Archivo con carátula ilegible o incompleta');
	PRINT 'Estado "Carátula Ilegible" creado con ID: ' + CAST(@nextId AS VARCHAR);
END
ELSE
BEGIN
	PRINT 'Estado "Carátula Ilegible" ya existe';
END
GO

-- =============================================
-- 3. Crear Tabla de Tipos de Plano (Valores Válidos)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_TIPOS_PLANO]') AND type in (N'U'))
BEGIN
	CREATE TABLE IAP_TV_TIPOS_PLANO (
		cdTipoPlano INT IDENTITY(1,1) NOT NULL,
		dsTipoPlano VARCHAR(100) NOT NULL,
		dsDescripcion VARCHAR(500) NULL,
		snActivo BIT NOT NULL DEFAULT 1,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		CONSTRAINT PK_IAP_TV_TIPOS_PLANO PRIMARY KEY CLUSTERED (cdTipoPlano ASC)
	);

	PRINT 'Tabla IAP_TV_TIPOS_PLANO creada exitosamente';
END
ELSE
BEGIN
	PRINT 'Tabla IAP_TV_TIPOS_PLANO ya existe';
END
GO

-- =============================================
-- 4. Poblar Tabla de Tipos de Plano con Valores Iniciales
-- =============================================

-- Limpiar datos existentes si la tabla está vacía o fue recién creada
IF NOT EXISTS (SELECT * FROM IAP_TV_TIPOS_PLANO)
BEGIN
	INSERT INTO IAP_TV_TIPOS_PLANO (dsTipoPlano, dsDescripcion) VALUES
	('Plano de Mensura', 'Plano que representa la medición de un terreno'),
	('Plano de Subdivisión', 'Plano que divide un terreno en parcelas'),
	('Plano de Unificación', 'Plano que unifica varias parcelas en una'),
	('Plano de Loteo', 'Plano de loteo urbano o rural'),
	('Plano de Propiedad Horizontal', 'Plano de división en propiedad horizontal'),
	('Plano de Deslinde', 'Plano que establece límites entre propiedades'),
	('Plano de Rectificación', 'Plano que corrige errores de planos anteriores'),
	('Plano de Obra', 'Plano de construcción de obra'),
	('Croquis', 'Representación esquemática del terreno'),
	('Otro', 'Otros tipos de planos no categorizados');

	PRINT 'Tipos de plano iniciales insertados';
END
ELSE
BEGIN
	PRINT 'Tipos de plano ya existen en la tabla';
END
GO

-- =============================================
-- 5. Crear Vista de Archivos con Resultados IA para Control
-- =============================================

IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_IAP_CONTROL_CALIDAD]'))
BEGIN
	DROP VIEW VW_IAP_CONTROL_CALIDAD;
	PRINT 'Vista VW_IAP_CONTROL_CALIDAD eliminada para recreación';
END
GO

CREATE VIEW VW_IAP_CONTROL_CALIDAD
AS
SELECT 
	-- Datos del lote
	l.cdLote,
	l.dsNombreLote,
	l.cdEstadoLote,
	el.dsEstado AS DsEstadoLote,
	l.nuCantidadArchivos,

	-- Datos del archivo en lote
	la.cdLoteArchivo,
	la.cdArchivo,
	la.cdEstadoArchivoLote,
	ea.dsEstado AS DsEstadoArchivoLote,
	la.nuOrden,
	la.dsRutaImagenJpg,

	-- Datos del archivo original
	a.dsNombreArchivo,
	a.dsRutaCompleta,

	-- Resultados de IA
	r.cdResultadoIA,
	r.dsTipoPlano,
	r.dsExpediente,
	r.dsSeccion,
	r.dsManzana,
	r.dsParcela,
	r.dsDireccion,
	r.nuConfianzaTipoPlano,
	r.nuConfianzaExpediente,
	r.nuConfianzaSeccion,
	r.nuConfianzaManzana,
	r.nuConfianzaParcela,
	r.nuConfianzaDireccion,
	r.dsModalidadProcesamiento,
	r.nuIntentos,
	r.feAlta AS FeProcesamientoIA,

	-- Auditoría
	l.feAlta AS FeAltaLote,
	l.cdUsuarioAlta,
	u.dsNombreUsuario AS DsUsuarioAlta
FROM IAP_TD_LOTES l
INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
INNER JOIN IAP_TD_LOTE_ARCHIVOS la ON l.cdLote = la.cdLote
INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE ea ON la.cdEstadoArchivoLote = ea.cdEstadoArchivoLote
LEFT JOIN IAP_TD_RESULTADOS_IA r ON la.cdLoteArchivo = r.cdLoteArchivo
INNER JOIN SEGURIDAD.SEG_TD_USUARIOS u ON l.cdUsuarioAlta = u.cdUsuario
WHERE l.cdEstadoLote IN (3, 4); -- Pendiente de Control de Calidad o Pendiente de Finalizar
GO

PRINT 'Vista VW_IAP_CONTROL_CALIDAD creada exitosamente';
GO

-- =============================================
-- 6. Crear Índices para Mejorar Performance en Control de Calidad
-- =============================================

-- Índice en cdEstadoLote para filtrado rápido
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IAP_TD_LOTES_EstadoLote' AND object_id = OBJECT_ID('IAP_TD_LOTES'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_IAP_TD_LOTES_EstadoLote
	ON IAP_TD_LOTES (cdEstadoLote)
	INCLUDE (dsNombreLote, nuCantidadArchivos, feAlta);
	PRINT 'Índice IX_IAP_TD_LOTES_EstadoLote creado';
END
GO

-- Índice en cdLote de IAP_TD_RESULTADOS_IA para joins más rápidos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_IAP_TD_RESULTADOS_IA_Lote' AND object_id = OBJECT_ID('IAP_TD_RESULTADOS_IA'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_IAP_TD_RESULTADOS_IA_Lote
	ON IAP_TD_RESULTADOS_IA (cdLoteArchivo)
	INCLUDE (dsTipoPlano, dsSeccion, dsManzana, dsParcela, nuConfianzaTipoPlano);
	PRINT 'Índice IX_IAP_TD_RESULTADOS_IA_Lote creado';
END
GO

-- =============================================
-- 7. Verificación Final
-- =============================================

PRINT '';
PRINT '==================================';
PRINT 'Verificación de objetos creados:';
PRINT '==================================';

-- Verificar estado lote 4
IF EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote = 4)
	PRINT '✓ Estado de lote 4: Pendiente de Finalizar';

-- Verificar estados de archivo
IF EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Controlado')
	PRINT '✓ Estado de archivo: Controlado';

IF EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Carátula Ilegible')
	PRINT '✓ Estado de archivo: Carátula Ilegible';

-- Verificar tabla de tipos de plano
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_TIPOS_PLANO]'))
BEGIN
	DECLARE @countTipos INT;
	SELECT @countTipos = COUNT(*) FROM IAP_TV_TIPOS_PLANO;
	PRINT '✓ Tabla IAP_TV_TIPOS_PLANO con ' + CAST(@countTipos AS VARCHAR) + ' tipos';
END

-- Verificar vista
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VW_IAP_CONTROL_CALIDAD]'))
	PRINT '✓ Vista VW_IAP_CONTROL_CALIDAD';

PRINT '';
PRINT '==================================';
PRINT 'Script completado exitosamente';
PRINT '==================================';
GO
