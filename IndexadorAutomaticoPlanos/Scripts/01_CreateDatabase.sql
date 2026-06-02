-- =============================================
-- Script: 01_CreateDatabase.sql
-- Descripción: Crea la base de datos Capturador y todas las tablas necesarias
-- Proyecto: Indexador Automático de Planos
-- =============================================

USE master;
GO

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Capturador')
BEGIN
	CREATE DATABASE Capturador;
	PRINT 'Base de datos Capturador creada exitosamente.';
END
ELSE
BEGIN
	PRINT 'La base de datos Capturador ya existe.';
END
GO

USE Capturador;
GO

-- =============================================
-- TABLAS DE VALORES FIJOS (TV_)
-- =============================================

-- Tabla de Estados de Archivos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_ESTADOS_ARCHIVO]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TV_ESTADOS_ARCHIVO] (
		cdEstadoArchivo INT PRIMARY KEY IDENTITY(1,1),
		dsEstado NVARCHAR(100) NOT NULL,
		dsDescripcion NVARCHAR(500) NULL,
		snActivo BIT NOT NULL DEFAULT 1
	);
	PRINT 'Tabla IAP_TV_ESTADOS_ARCHIVO creada.';
END
GO

-- Tabla de Estados de Lotes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_ESTADOS_LOTE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TV_ESTADOS_LOTE] (
		cdEstadoLote INT PRIMARY KEY IDENTITY(1,1),
		dsEstado NVARCHAR(100) NOT NULL,
		dsDescripcion NVARCHAR(500) NULL,
		snActivo BIT NOT NULL DEFAULT 1
	);
	PRINT 'Tabla IAP_TV_ESTADOS_LOTE creada.';
END
GO

-- Tabla de Estados de Archivos en Lote
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_ESTADOS_ARCHIVO_LOTE]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TV_ESTADOS_ARCHIVO_LOTE] (
		cdEstadoArchivoLote INT PRIMARY KEY IDENTITY(1,1),
		dsEstado NVARCHAR(100) NOT NULL,
		dsDescripcion NVARCHAR(500) NULL,
		snActivo BIT NOT NULL DEFAULT 1
	);
	PRINT 'Tabla IAP_TV_ESTADOS_ARCHIVO_LOTE creada.';
END
GO

-- Tabla de Estados de Validación de Planos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_ESTADOS_VALIDACION]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TV_ESTADOS_VALIDACION] (
		cdEstadoValidacion INT PRIMARY KEY IDENTITY(1,1),
		dsEstado NVARCHAR(100) NOT NULL,
		dsDescripcion NVARCHAR(500) NULL,
		snActivo BIT NOT NULL DEFAULT 1
	);
	PRINT 'Tabla IAP_TV_ESTADOS_VALIDACION creada.';
END
GO

-- Tabla de Tipos de Plano
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TV_TIPOS_PLANO]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TV_TIPOS_PLANO] (
		cdTipoPlano INT PRIMARY KEY IDENTITY(1,1),
		dsTipoPlano NVARCHAR(50) NOT NULL,
		snActivo BIT NOT NULL DEFAULT 1
	);
	PRINT 'Tabla IAP_TV_TIPOS_PLANO creada.';
END
GO

-- =============================================
-- TABLAS DE DATOS (TD_)
-- =============================================

-- Tabla de Usuarios
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_USUARIOS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_USUARIOS] (
		cdUsuario INT PRIMARY KEY IDENTITY(1,1),
		dsUsuario NVARCHAR(50) NOT NULL UNIQUE,
		dsClave NVARCHAR(255) NOT NULL,
		dsNombreCompleto NVARCHAR(200) NOT NULL,
		snClaveTemporal BIT NOT NULL DEFAULT 0,
		snPrimerIngreso BIT NOT NULL DEFAULT 1,
		snActivo BIT NOT NULL DEFAULT 1,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,
		CONSTRAINT FK_IAP_TD_USUARIOS_UsuarioAlta FOREIGN KEY (cdUsuarioAlta) REFERENCES IAP_TD_USUARIOS(cdUsuario),
		CONSTRAINT FK_IAP_TD_USUARIOS_UsuarioMod FOREIGN KEY (cdUsuarioModificacion) REFERENCES IAP_TD_USUARIOS(cdUsuario)
	);

	CREATE INDEX IX_IAP_TD_USUARIOS_Usuario ON IAP_TD_USUARIOS(dsUsuario);
	PRINT 'Tabla IAP_TD_USUARIOS creada.';
END
GO

-- Tabla de Parámetros
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_PARAMETROS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_PARAMETROS] (
		cdParametro INT PRIMARY KEY IDENTITY(1,1),
		dsClaveParametro NVARCHAR(100) NOT NULL UNIQUE,
		dsValorParametro NVARCHAR(MAX) NULL,
		dsDescripcion NVARCHAR(500) NULL,
		feUltimaModificacion DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioModificacion INT NULL,
		CONSTRAINT FK_IAP_TD_PARAMETROS_Usuario FOREIGN KEY (cdUsuarioModificacion) REFERENCES IAP_TD_USUARIOS(cdUsuario)
	);

	CREATE INDEX IX_IAP_TD_PARAMETROS_Clave ON IAP_TD_PARAMETROS(dsClaveParametro);
	PRINT 'Tabla IAP_TD_PARAMETROS creada.';
END
GO

-- Tabla de Archivos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_ARCHIVOS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_ARCHIVOS] (
		cdArchivo INT PRIMARY KEY IDENTITY(1,1),
		dsNombreArchivo NVARCHAR(255) NOT NULL,
		dsRutaCompleta NVARCHAR(1000) NOT NULL,
		dsNombreUltimaCarpeta NVARCHAR(255) NULL,
		nuCantidadPaginas INT NOT NULL DEFAULT 1,
		cdEstadoArchivo INT NOT NULL,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NOT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,
		CONSTRAINT FK_IAP_TD_ARCHIVOS_Estado FOREIGN KEY (cdEstadoArchivo) REFERENCES IAP_TV_ESTADOS_ARCHIVO(cdEstadoArchivo),
		CONSTRAINT FK_IAP_TD_ARCHIVOS_UsuarioAlta FOREIGN KEY (cdUsuarioAlta) REFERENCES IAP_TD_USUARIOS(cdUsuario),
		CONSTRAINT FK_IAP_TD_ARCHIVOS_UsuarioMod FOREIGN KEY (cdUsuarioModificacion) REFERENCES IAP_TD_USUARIOS(cdUsuario),
		CONSTRAINT UQ_IAP_TD_ARCHIVOS_RutaNombre UNIQUE (dsRutaCompleta, dsNombreArchivo)
	);

	CREATE INDEX IX_IAP_TD_ARCHIVOS_Estado ON IAP_TD_ARCHIVOS(cdEstadoArchivo);
	CREATE INDEX IX_IAP_TD_ARCHIVOS_Carpeta ON IAP_TD_ARCHIVOS(dsNombreUltimaCarpeta);
	PRINT 'Tabla IAP_TD_ARCHIVOS creada.';
END
GO

-- Tabla de Lotes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_LOTES]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_LOTES] (
		cdLote INT PRIMARY KEY IDENTITY(1,1),
		dsNombreLote NVARCHAR(50) NOT NULL UNIQUE,
		nuCantidadArchivos INT NOT NULL DEFAULT 0,
		cdEstadoLote INT NOT NULL,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NOT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,
		CONSTRAINT FK_IAP_TD_LOTES_Estado FOREIGN KEY (cdEstadoLote) REFERENCES IAP_TV_ESTADOS_LOTE(cdEstadoLote),
		CONSTRAINT FK_IAP_TD_LOTES_UsuarioAlta FOREIGN KEY (cdUsuarioAlta) REFERENCES IAP_TD_USUARIOS(cdUsuario),
		CONSTRAINT FK_IAP_TD_LOTES_UsuarioMod FOREIGN KEY (cdUsuarioModificacion) REFERENCES IAP_TD_USUARIOS(cdUsuario)
	);

	CREATE INDEX IX_IAP_TD_LOTES_Estado ON IAP_TD_LOTES(cdEstadoLote);
	CREATE INDEX IX_IAP_TD_LOTES_Nombre ON IAP_TD_LOTES(dsNombreLote);
	PRINT 'Tabla IAP_TD_LOTES creada.';
END
GO

-- =============================================
-- TABLAS DE RELACIÓN (TR_)
-- =============================================

-- Tabla de Relación Lote-Archivo
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TR_LOTE_ARCHIVO]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TR_LOTE_ARCHIVO] (
		cdLoteArchivo INT PRIMARY KEY IDENTITY(1,1),
		cdLote INT NOT NULL,
		cdArchivo INT NOT NULL,
		cdEstadoArchivoLote INT NOT NULL,
		dsRutaImagenJPG NVARCHAR(1000) NULL,
		dsImagenBase64 NVARCHAR(MAX) NULL,
		dsTextoOCR NVARCHAR(MAX) NULL,
		snTieneOCR BIT NOT NULL DEFAULT 0,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NOT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,
		CONSTRAINT FK_IAP_TR_LOTE_ARCHIVO_Lote FOREIGN KEY (cdLote) REFERENCES IAP_TD_LOTES(cdLote),
		CONSTRAINT FK_IAP_TR_LOTE_ARCHIVO_Archivo FOREIGN KEY (cdArchivo) REFERENCES IAP_TD_ARCHIVOS(cdArchivo),
		CONSTRAINT FK_IAP_TR_LOTE_ARCHIVO_Estado FOREIGN KEY (cdEstadoArchivoLote) REFERENCES IAP_TV_ESTADOS_ARCHIVO_LOTE(cdEstadoArchivoLote),
		CONSTRAINT FK_IAP_TR_LOTE_ARCHIVO_UsuarioAlta FOREIGN KEY (cdUsuarioAlta) REFERENCES IAP_TD_USUARIOS(cdUsuario),
		CONSTRAINT FK_IAP_TR_LOTE_ARCHIVO_UsuarioMod FOREIGN KEY (cdUsuarioModificacion) REFERENCES IAP_TD_USUARIOS(cdUsuario),
		CONSTRAINT UQ_IAP_TR_LOTE_ARCHIVO UNIQUE (cdLote, cdArchivo)
	);

	CREATE INDEX IX_IAP_TR_LOTE_ARCHIVO_Lote ON IAP_TR_LOTE_ARCHIVO(cdLote);
	CREATE INDEX IX_IAP_TR_LOTE_ARCHIVO_Archivo ON IAP_TR_LOTE_ARCHIVO(cdArchivo);
	CREATE INDEX IX_IAP_TR_LOTE_ARCHIVO_Estado ON IAP_TR_LOTE_ARCHIVO(cdEstadoArchivoLote);
	PRINT 'Tabla IAP_TR_LOTE_ARCHIVO creada.';
END
GO

-- Tabla de Planos Procesados
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_PLANOS_PROCESADOS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_PLANOS_PROCESADOS] (
		cdPlano INT PRIMARY KEY IDENTITY(1,1),
		cdLoteArchivo INT NOT NULL,
		cdTipoPlano INT NULL,
		dsExpediente NVARCHAR(100) NULL,
		dsSeccion NVARCHAR(50) NULL,
		dsManzana NVARCHAR(50) NULL,
		dsParcela NVARCHAR(50) NULL,
		dsDireccion NVARCHAR(500) NULL,
		nuConfianzaTipoPlano DECIMAL(5,4) NULL,
		nuConfianzaExpediente DECIMAL(5,4) NULL,
		nuConfianzaSeccion DECIMAL(5,4) NULL,
		nuConfianzaManzana DECIMAL(5,4) NULL,
		nuConfianzaParcela DECIMAL(5,4) NULL,
		nuConfianzaDireccion DECIMAL(5,4) NULL,
		nuTokensPrompt INT NULL,
		nuTokensCompletion INT NULL,
		nuTokensTotal INT NULL,
		cdEstadoValidacion INT NOT NULL,
		dsObservaciones NVARCHAR(MAX) NULL,
		feAlta DATETIME NOT NULL DEFAULT GETDATE(),
		cdUsuarioAlta INT NOT NULL,
		feUltimaModificacion DATETIME NULL,
		cdUsuarioModificacion INT NULL,
		CONSTRAINT FK_IAP_TD_PLANOS_LoteArchivo FOREIGN KEY (cdLoteArchivo) REFERENCES IAP_TR_LOTE_ARCHIVO(cdLoteArchivo),
		CONSTRAINT FK_IAP_TD_PLANOS_TipoPlano FOREIGN KEY (cdTipoPlano) REFERENCES IAP_TV_TIPOS_PLANO(cdTipoPlano),
		CONSTRAINT FK_IAP_TD_PLANOS_EstadoValidacion FOREIGN KEY (cdEstadoValidacion) REFERENCES IAP_TV_ESTADOS_VALIDACION(cdEstadoValidacion),
		CONSTRAINT FK_IAP_TD_PLANOS_UsuarioAlta FOREIGN KEY (cdUsuarioAlta) REFERENCES IAP_TD_USUARIOS(cdUsuario),
		CONSTRAINT FK_IAP_TD_PLANOS_UsuarioMod FOREIGN KEY (cdUsuarioModificacion) REFERENCES IAP_TD_USUARIOS(cdUsuario)
	);

	CREATE INDEX IX_IAP_TD_PLANOS_LoteArchivo ON IAP_TD_PLANOS_PROCESADOS(cdLoteArchivo);
	CREATE INDEX IX_IAP_TD_PLANOS_TipoPlano ON IAP_TD_PLANOS_PROCESADOS(cdTipoPlano);
	CREATE INDEX IX_IAP_TD_PLANOS_EstadoValidacion ON IAP_TD_PLANOS_PROCESADOS(cdEstadoValidacion);
	CREATE INDEX IX_IAP_TD_PLANOS_Nomenclatura ON IAP_TD_PLANOS_PROCESADOS(dsSeccion, dsManzana, dsParcela);
	PRINT 'Tabla IAP_TD_PLANOS_PROCESADOS creada.';
END
GO

-- =============================================
-- TABLAS DE LOG Y AUDITORÍA
-- =============================================

-- Tabla de Logs
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IAP_TD_LOGS]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[IAP_TD_LOGS] (
		cdLog BIGINT PRIMARY KEY IDENTITY(1,1),
		dsNivel NVARCHAR(20) NOT NULL, -- Info, Warning, Error, Debug
		dsModulo NVARCHAR(100) NULL,
		dsMensaje NVARCHAR(MAX) NOT NULL,
		dsExcepcion NVARCHAR(MAX) NULL,
		cdUsuario INT NULL,
		dsUsuario NVARCHAR(50) NULL,
		feRegistro DATETIME NOT NULL DEFAULT GETDATE(),
		CONSTRAINT FK_IAP_TD_LOGS_Usuario FOREIGN KEY (cdUsuario) REFERENCES IAP_TD_USUARIOS(cdUsuario)
	);

	CREATE INDEX IX_IAP_TD_LOGS_Nivel ON IAP_TD_LOGS(dsNivel);
	CREATE INDEX IX_IAP_TD_LOGS_Fecha ON IAP_TD_LOGS(feRegistro DESC);
	CREATE INDEX IX_IAP_TD_LOGS_Modulo ON IAP_TD_LOGS(dsModulo);
	PRINT 'Tabla IAP_TD_LOGS creada.';
END
GO

PRINT '===========================================';
PRINT 'Script 01_CreateDatabase.sql ejecutado exitosamente.';
PRINT 'Base de datos y tablas creadas.';
PRINT '===========================================';
GO
