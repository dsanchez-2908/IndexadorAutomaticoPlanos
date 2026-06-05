/*
==============================================================================
Script: 07_CrearTablasLotes.sql
Descripción: Crea las tablas necesarias para la gestión de lotes de procesamiento
			 - IAP_TD_LOTES: tabla principal de lotes
			 - IAP_TD_LOTE_ARCHIVOS: relación muchos a muchos entre lotes y archivos
			 - IAP_TV_ESTADOS_LOTE: catálogo de estados de lote
			 - IAP_TV_ESTADOS_ARCHIVO_LOTE: catálogo de estados de archivo dentro de lote
			 - Actualiza IAP_TV_ESTADOS_ARCHIVO para incluir "Asociado a Lote"
Fecha: 2026-06-02
Autor: Sistema Indexador Automático de Planos
==============================================================================
*/

USE Capturador;
GO

-- =============================================
-- 1. CREAR TABLA DE ESTADOS DE LOTE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_ESTADOS_LOTE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TV_ESTADOS_LOTE]
	(
		cdEstadoLote INT IDENTITY(1,1) NOT NULL,
		dsEstado NVARCHAR(100) NOT NULL,
		dsDescripcion NVARCHAR(500) NULL,
		CONSTRAINT PK_IAP_TV_ESTADOS_LOTE PRIMARY KEY CLUSTERED (cdEstadoLote ASC)
	);
	PRINT 'Tabla IAP_TV_ESTADOS_LOTE creada correctamente';
END
ELSE
BEGIN
	PRINT 'Tabla IAP_TV_ESTADOS_LOTE ya existe';
END
GO

-- =============================================
-- 2. CREAR TABLA DE ESTADOS DE ARCHIVO EN LOTE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_ESTADOS_ARCHIVO_LOTE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TV_ESTADOS_ARCHIVO_LOTE]
	(
		cdEstadoArchivoLote INT IDENTITY(1,1) NOT NULL,
		dsEstado NVARCHAR(100) NOT NULL,
		dsDescripcion NVARCHAR(500) NULL,
		CONSTRAINT PK_IAP_TV_ESTADOS_ARCHIVO_LOTE PRIMARY KEY CLUSTERED (cdEstadoArchivoLote ASC)
	);
	PRINT 'Tabla IAP_TV_ESTADOS_ARCHIVO_LOTE creada correctamente';
END
ELSE
BEGIN
	PRINT 'Tabla IAP_TV_ESTADOS_ARCHIVO_LOTE ya existe';
END
GO

-- =============================================
-- 3. VERIFICAR/ACTUALIZAR TABLA IAP_TD_LOTES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_LOTES]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_LOTES]
	(
		cdLote INT IDENTITY(1,1) NOT NULL,
		dsNombreLote NVARCHAR(50) NOT NULL,
		cdEstadoLote INT NOT NULL,
		nuCantidadArchivos INT NOT NULL DEFAULT 0,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NOT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,
		CONSTRAINT PK_IAP_TD_LOTES PRIMARY KEY CLUSTERED (cdLote ASC),
		CONSTRAINT FK_IAP_TD_LOTES_ESTADOS FOREIGN KEY (cdEstadoLote)
			REFERENCES IAP_TV_ESTADOS_LOTE(cdEstadoLote),
		CONSTRAINT FK_IAP_TD_LOTES_USUARIO_ALTA FOREIGN KEY (cdUsuarioAlta)
			REFERENCES IAP_TD_USUARIOS(cdUsuario)
	);

	CREATE UNIQUE INDEX IX_IAP_TD_LOTES_NOMBRE ON IAP_TD_LOTES(dsNombreLote);

	PRINT 'Tabla IAP_TD_LOTES creada correctamente';
END
ELSE
BEGIN
	PRINT 'Tabla IAP_TD_LOTES ya existe';

	-- Agregar columnas si no existen
	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
				   WHERE TABLE_NAME = 'IAP_TD_LOTES' AND COLUMN_NAME = 'nuCantidadArchivos')
	BEGIN
		ALTER TABLE IAP_TD_LOTES ADD nuCantidadArchivos INT NOT NULL DEFAULT 0;
		PRINT 'Columna nuCantidadArchivos agregada a IAP_TD_LOTES';
	END
END
GO

-- =============================================
-- 4. CREAR TABLA DE RELACIÓN LOTE-ARCHIVOS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_LOTE_ARCHIVOS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_LOTE_ARCHIVOS]
	(
		cdLoteArchivo INT IDENTITY(1,1) NOT NULL,
		cdLote INT NOT NULL,
		cdArchivo INT NOT NULL,
		cdEstadoArchivoLote INT NOT NULL,
		nuOrden INT NULL,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NOT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,
		CONSTRAINT PK_IAP_TD_LOTE_ARCHIVOS PRIMARY KEY CLUSTERED (cdLoteArchivo ASC),
		CONSTRAINT FK_IAP_TD_LOTE_ARCHIVOS_LOTE FOREIGN KEY (cdLote)
			REFERENCES IAP_TD_LOTES(cdLote),
		CONSTRAINT FK_IAP_TD_LOTE_ARCHIVOS_ARCHIVO FOREIGN KEY (cdArchivo)
			REFERENCES IAP_TD_ARCHIVOS(cdArchivo),
		CONSTRAINT FK_IAP_TD_LOTE_ARCHIVOS_ESTADO FOREIGN KEY (cdEstadoArchivoLote)
			REFERENCES IAP_TV_ESTADOS_ARCHIVO_LOTE(cdEstadoArchivoLote),
		CONSTRAINT UQ_IAP_TD_LOTE_ARCHIVOS UNIQUE (cdLote, cdArchivo)
	);

	CREATE INDEX IX_IAP_TD_LOTE_ARCHIVOS_LOTE ON IAP_TD_LOTE_ARCHIVOS(cdLote);
	CREATE INDEX IX_IAP_TD_LOTE_ARCHIVOS_ARCHIVO ON IAP_TD_LOTE_ARCHIVOS(cdArchivo);

	PRINT 'Tabla IAP_TD_LOTE_ARCHIVOS creada correctamente';
END
ELSE
BEGIN
	PRINT 'Tabla IAP_TD_LOTE_ARCHIVOS ya existe';
END
GO

-- =============================================
-- 5. INSERTAR ESTADOS DE LOTE
-- =============================================
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE dsEstado = 'Pendiente de Preparar Imágenes')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (dsEstado, dsDescripcion) VALUES 
	('Pendiente de Preparar Imágenes', 'Lote creado, pendiente de extracción de imágenes desde PDFs'),
	('Pendiente de Procesar por IA', 'Imágenes preparadas, listo para enviar a OpenAI Batch API'),
	('Pendiente de Control de Calidad', 'Procesado por IA, pendiente de validación humana'),
	('Pendiente de Finalizar', 'Validado, pendiente de cierre y exportación'),
	('Finalizado', 'Lote completamente procesado y cerrado');

	PRINT 'Estados de lote insertados correctamente';
END
ELSE
BEGIN
	PRINT 'Estados de lote ya existen';
END
GO

-- =============================================
-- 6. INSERTAR ESTADOS DE ARCHIVO EN LOTE
-- =============================================
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Asociado')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion) VALUES 
	('Asociado', 'Archivo asociado al lote'),
	('Imagen Extraída', 'Imagen extraída del PDF'),
	('Enviado a IA', 'Imagen enviada a OpenAI para procesamiento'),
	('Procesado', 'Imagen procesada por OpenAI con éxito'),
	('Error', 'Error en el procesamiento del archivo'),
	('Validado', 'Resultado validado por usuario'),
	('Finalizado', 'Procesamiento del archivo completado');

	PRINT 'Estados de archivo en lote insertados correctamente';
END
ELSE
BEGIN
	PRINT 'Estados de archivo en lote ya existen';
END
GO

-- =============================================
-- 7. AGREGAR ESTADO "Asociado a Lote" EN ESTADOS DE ARCHIVO
-- =============================================
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_ARCHIVO WHERE dsEstado = 'Asociado a Lote')
BEGIN
	INSERT INTO IAP_TV_ESTADOS_ARCHIVO (dsEstado, dsDescripcion) 
	VALUES ('Asociado a Lote', 'Archivo asociado a un lote de procesamiento');

	PRINT 'Estado "Asociado a Lote" agregado a IAP_TV_ESTADOS_ARCHIVO';
END
ELSE
BEGIN
	PRINT 'Estado "Asociado a Lote" ya existe en IAP_TV_ESTADOS_ARCHIVO';
END
GO

-- =============================================
-- 8. VISTA PARA CONSULTA RÁPIDA DE LOTES CON DETALLES
-- =============================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'VW_IAP_LOTES_DETALLE')
	DROP VIEW VW_IAP_LOTES_DETALLE;
GO

CREATE VIEW VW_IAP_LOTES_DETALLE
AS
SELECT 
	l.cdLote,
	l.dsNombreLote,
	l.cdEstadoLote,
	el.dsEstado AS dsEstadoLote,
	l.nuCantidadArchivos,
	l.feAlta,
	u.dsUsuario AS dsUsuarioAlta,
	l.feUltimaModificacion,
	COUNT(DISTINCT la.cdArchivo) AS nuArchivosActuales,
	SUM(a.nuCantidadPaginas) AS nuTotalPaginas
FROM IAP_TD_LOTES l
INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
INNER JOIN IAP_TD_USUARIOS u ON l.cdUsuarioAlta = u.cdUsuario
LEFT JOIN IAP_TD_LOTE_ARCHIVOS la ON l.cdLote = la.cdLote
LEFT JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
GROUP BY 
	l.cdLote, l.dsNombreLote, l.cdEstadoLote, el.dsEstado,
	l.nuCantidadArchivos, l.feAlta, u.dsUsuario, l.feUltimaModificacion;
GO

PRINT '========================================';
PRINT 'Script 07_CrearTablasLotes.sql ejecutado correctamente';
PRINT 'Tablas creadas:';
PRINT '  - IAP_TV_ESTADOS_LOTE';
PRINT '  - IAP_TV_ESTADOS_ARCHIVO_LOTE';
PRINT '  - IAP_TD_LOTES (verificada/actualizada)';
PRINT '  - IAP_TD_LOTE_ARCHIVOS';
PRINT 'Vista creada:';
PRINT '  - VW_IAP_LOTES_DETALLE';
PRINT '========================================';
GO
