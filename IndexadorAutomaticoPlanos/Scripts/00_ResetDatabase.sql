-- =============================================
-- Script: 00_ResetDatabase.sql
-- Descripción: ELIMINA Y RECREA la base de datos (USO SOLO EN DESARROLLO)
-- ADVERTENCIA: Este script BORRA TODOS LOS DATOS
-- =============================================

USE master;
GO

-- Cerrar todas las conexiones activas a la base de datos
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Capturador')
BEGIN
	ALTER DATABASE Capturador SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE Capturador;
	PRINT 'Base de datos Capturador eliminada.';
END
GO

PRINT '===========================================';
PRINT 'Ejecutar ahora los siguientes scripts en orden:';
PRINT '1. 01_CreateDatabase.sql';
PRINT '2. 02_InsertInitialData.sql';
PRINT '===========================================';
