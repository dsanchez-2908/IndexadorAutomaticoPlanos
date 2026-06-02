-- =============================================
-- Script: 03_FixAdminPassword.sql
-- Descripción: Corrige la contraseña del usuario admin
-- Este script debe ejecutarse si no puede iniciar sesión con admin/123
-- =============================================

USE Capturador;
GO

PRINT '===========================================';
PRINT 'Actualizando contraseña del usuario admin...';
PRINT '===========================================';

-- Actualizar contraseña del admin
-- Contraseña: 123
-- Hash generado y verificado con BCrypt (11 rounds)
-- Hash: $2a$11$vrFOkkpu6IhttnjPjqw44ewCKl2XnDECIVuO.7joiSz7pYayMd8RS
UPDATE IAP_TD_USUARIOS 
SET dsClave = '$2a$11$vrFOkkpu6IhttnjPjqw44ewCKl2XnDECIVuO.7joiSz7pYayMd8RS',
	snClaveTemporal = 0,
	snPrimerIngreso = 0,
	feUltimaModificacion = GETDATE()
WHERE dsUsuario = 'admin';

IF @@ROWCOUNT > 0
BEGIN
	PRINT 'Contraseña del usuario admin actualizada correctamente.';
	PRINT 'Usuario: admin';
	PRINT 'Contraseña: 123';
	PRINT '';
	PRINT 'IMPORTANTE: Cambie esta contraseña después del primer ingreso en producción.';
END
ELSE
BEGIN
	PRINT 'ERROR: No se encontró el usuario admin.';
	PRINT 'Ejecute primero el script 02_InsertInitialData.sql';
END

PRINT '===========================================';
GO
