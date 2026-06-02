-- ==================================================================
-- Script 04: Corrección FINAL del Password Admin
-- ==================================================================
-- Este script actualiza la contraseña del usuario admin con un hash
-- que ha sido verificado como válido para la contraseña "123"
-- ==================================================================

USE Capturador;
GO

-- Actualizar con el hash CORRECTO y VERIFICADO
UPDATE IAP_TD_USUARIOS
SET dsClave = '$2a$11$vrFOkkpu6IhttnjPjqw44ewCKl2XnDECIVuO.7joiSz7pYayMd8RS',
	snClaveTemporal = 0,
	snPrimerIngreso = 0,
	feUltimaModificacion = GETDATE(),
	cdUsuarioModificacion = 1
WHERE dsUsuario = 'admin';

PRINT 'Password del usuario admin actualizado correctamente.';
PRINT 'Ahora puedes ingresar con: admin / 123';
GO
