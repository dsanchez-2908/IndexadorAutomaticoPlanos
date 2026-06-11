-- ================================================================================
-- Script: Corrección de codificación de caracteres en estados
-- Versión: 1.0
-- Fecha: 2024-06-10
-- Descripción: Corrige la codificación del estado "Carátula Ilegible"
--              que aparece como "CarÃ¡tula Ilegible" debido a problemas de encoding
-- ================================================================================

USE Capturador;
GO

PRINT '======================================';
PRINT 'Corrección de Codificación de Estados';
PRINT '======================================';
PRINT '';

-- ================================================================================
-- 1. Verificar estado actual
-- ================================================================================

PRINT '1. Estado actual de "Carátula Ilegible":';
SELECT 
	cdEstadoArchivoLote,
	dsEstado,
	LEN(dsEstado) AS Longitud,
	CAST(dsEstado AS VARBINARY(100)) AS CodigoBinario
FROM IAP_TV_ESTADOS_ARCHIVO_LOTE
WHERE dsEstado LIKE '%legible%';
PRINT '';

-- ================================================================================
-- 2. Actualizar el estado con codificación correcta
-- ================================================================================

PRINT '2. Corrigiendo codificación...';

-- Actualizar si existe el estado mal codificado
UPDATE IAP_TV_ESTADOS_ARCHIVO_LOTE
SET dsEstado = N'Carátula Ilegible'
WHERE cdEstadoArchivoLote = 6;

IF @@ROWCOUNT > 0
	PRINT '✓ Estado actualizado correctamente';
ELSE
	PRINT '⚠ No se encontró el estado con ID 6';

PRINT '';

-- ================================================================================
-- 3. Verificar resultado
-- ================================================================================

PRINT '3. Verificación final:';
SELECT 
	cdEstadoArchivoLote,
	dsEstado,
	LEN(dsEstado) AS Longitud
FROM IAP_TV_ESTADOS_ARCHIVO_LOTE
WHERE cdEstadoArchivoLote = 6;

-- Verificar que la comparación funcione
IF EXISTS (SELECT 1 FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = N'Carátula Ilegible')
	PRINT '✓ El estado puede ser encontrado con búsqueda exacta';
ELSE
	PRINT '✗ ERROR: El estado NO puede ser encontrado con búsqueda exacta';

PRINT '';

-- ================================================================================
-- 4. Verificar otros estados con posibles problemas de codificación
-- ================================================================================

PRINT '4. Otros estados con caracteres especiales:';
SELECT 
	cdEstadoArchivoLote,
	dsEstado,
	CASE 
		WHEN dsEstado LIKE '%Ã%' THEN '⚠ Posible problema de codificación'
		ELSE '✓ OK'
	END AS Estado
FROM IAP_TV_ESTADOS_ARCHIVO_LOTE
WHERE dsEstado LIKE '%Ã%';

IF @@ROWCOUNT = 0
	PRINT '✓ No se encontraron otros estados con problemas de codificación';

PRINT '';
PRINT '======================================';
PRINT 'Script completado';
PRINT '======================================';
GO
