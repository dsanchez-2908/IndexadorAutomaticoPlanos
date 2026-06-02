/*
==============================================================================
Script: 06_LimpiarDatosPrueba.sql
Descripción: Limpia todos los registros de prueba en IAP_TD_ARCHIVOS
			 Estos registros contienen información incorrecta (1 página para 
			 todos los PDFs cuando en realidad tienen múltiples páginas)
Fecha: 2026-06-02
==============================================================================
*/

USE Capturador;
GO

-- Eliminar todos los registros de la tabla IAP_TD_ARCHIVOS
DELETE FROM IAP_TD_ARCHIVOS;
GO

-- Reiniciar el identity (opcional, si deseas que los IDs vuelvan a empezar en 1)
DBCC CHECKIDENT ('IAP_TD_ARCHIVOS', RESEED, 0);
GO

PRINT 'Datos de prueba eliminados correctamente de IAP_TD_ARCHIVOS';
GO
