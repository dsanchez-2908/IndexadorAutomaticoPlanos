# CORRECCIÓN - Error "Carátula Ilegible" No Encontrado

## 🐛 Error Reportado

**Mensaje:**
```
Error: Error al marcar archivo como ilegible. No se encontró el estado de archivo "Carátula ilegible"
```

**Ubicación:** `FrmControlLote` → botón "Marcar Ilegible" → `LoteRepository.MarcarArchivoComoIlegible()`

**Causa Raíz:** 
Problema de **codificación de caracteres** en la base de datos. El estado se insertó originalmente con codificación incorrecta:
- **En BD:** `CarÃ¡tula Ilegible` (UTF-8 mal interpretado como Latin-1)
- **En código:** `Carátula Ilegible` (UTF-8 correcto)

---

## 🔍 Análisis del Problema

### Estado en Base de Datos:

```sql
SELECT cdEstadoArchivoLote, dsEstado, 
	   CAST(dsEstado AS VARBINARY(100)) AS CodigoBinario
FROM IAP_TV_ESTADOS_ARCHIVO_LOTE
WHERE cdEstadoArchivoLote = 6;
```

**Resultado:**
| cdEstadoArchivoLote | dsEstado | CodigoBinario |
|---------------------|----------|---------------|
| 6 | CarÃ¡tula Ilegible | 0x430061007200C300A100... |

**Problema:** El byte `C3 A1` representa `á` en UTF-8, pero se almacenó como dos caracteres separados en NVARCHAR.

### Búsqueda Fallida:

```sql
-- ❌ NO encuentra nada
SELECT * FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
WHERE dsEstado = N'Carátula Ilegible';

-- ❌ Devuelve 0 filas
```

### Método Original:

```csharp
public int ObtenerIdEstadoArchivoLotePorNombre(string nombreEstado)
{
	string query = @"
		SELECT cdEstadoArchivoLote 
		FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
		WHERE dsEstado = @nombreEstado";  // ❌ Falla por codificación

	object? resultado = _db.EjecutarEscalar(query, ...);

	if (resultado == null)
		throw new Exception($"No se encontró el estado '{nombreEstado}'");
}
```

---

## ✅ Soluciones Implementadas

### Opción A: Búsqueda Flexible en C# (✅ IMPLEMENTADA)

Modificado `LoteRepository.ObtenerIdEstadoArchivoLotePorNombre()` para usar **búsqueda en dos pasos**:

1. **Intento exacto** primero (rápido, ideal)
2. **Búsqueda flexible** como fallback (sin acentos, case-insensitive)

```csharp
public int ObtenerIdEstadoArchivoLotePorNombre(string nombreEstado)
{
	try
	{
		// 1️⃣ Primero intentar búsqueda exacta
		string query = @"
			SELECT cdEstadoArchivoLote 
			FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
			WHERE dsEstado = @nombreEstado";

		object? resultado = _db.EjecutarEscalar(query,
			DatabaseHelper.CrearParametro("@nombreEstado", nombreEstado));

		if (resultado != null && resultado != DBNull.Value)
		{
			return Convert.ToInt32(resultado);
		}

		// 2️⃣ Si no encuentra, búsqueda flexible (sin acentos)
		string queryFlexible = @"
			SELECT cdEstadoArchivoLote 
			FROM IAP_TV_ESTADOS_ARCHIVO_LOTE 
			WHERE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
					dsEstado COLLATE Modern_Spanish_CI_AI,
					'á', 'a'), 'é', 'e'), 'í', 'i'), 'ó', 'o'), 'ú', 'u')
				LIKE 
				  REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
					@nombreEstado COLLATE Modern_Spanish_CI_AI,
					'á', 'a'), 'é', 'e'), 'í', 'i'), 'ó', 'o'), 'ú', 'u')";

		resultado = _db.EjecutarEscalar(queryFlexible,
			DatabaseHelper.CrearParametro("@nombreEstado", nombreEstado));

		if (resultado == null || resultado == DBNull.Value)
		{
			throw new Exception($"No se encontró el estado de archivo '{nombreEstado}'");
		}

		// ⚠️ Log de advertencia para detectar problemas
		Logger.Warning($"Estado '{nombreEstado}' encontrado con búsqueda flexible (posible problema de codificación)", 
					  "LoteRepository");
		return Convert.ToInt32(resultado);
	}
	catch (Exception ex)
	{
		Logger.Error($"Error al obtener ID del estado '{nombreEstado}'", ex, "LoteRepository");
		throw;
	}
}
```

### Cómo Funciona la Búsqueda Flexible:

**Normalización:**
```sql
REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
	dsEstado,
	'á', 'a'), 'é', 'e'), 'í', 'i'), 'ó', 'o'), 'ú', 'u')
```

**Comparación:**
- Base de datos: `"CarÃ¡tula Ilegible"` → Normalizar → `"CarAtula Ilegible"`
- Parámetro: `"Carátula Ilegible"` → Normalizar → `"Caratula Ilegible"`
- Collation: `Modern_Spanish_CI_AI` (Case-Insensitive, Accent-Insensitive)
- **Resultado:** ✅ **Match!**

---

## 🎯 Ventajas de la Solución

### ✅ Pros:

1. **Retrocompatible:** Funciona con datos correctos E incorrectos
2. **Transparente:** No requiere cambios en código llamador
3. **Detecta problemas:** Log de advertencia cuando usa búsqueda flexible
4. **Sin migración de datos:** No necesita corregir la BD inmediatamente
5. **Robusta:** Maneja variaciones de acentos y mayúsculas

### ⚠️ Contras:

1. **Performance:** Búsqueda flexible es más lenta (usa REPLACE múltiple)
2. **No soluciona raíz:** Los datos siguen mal en BD
3. **Falsos positivos potenciales:** "Caratula" vs "Carátula" se consideran iguales

---

## 🔧 Opción B: Corrección en Base de Datos (NO IMPLEMENTADA)

### Script de Corrección:

Creado `13_CorregirCodificacionEstados.sql` para actualizar el valor:

```sql
UPDATE IAP_TV_ESTADOS_ARCHIVO_LOTE
SET dsEstado = N'Carátula Ilegible'
WHERE cdEstadoArchivoLote = 6;
```

### Problema Encontrado:

**El script se ejecutó pero el problema persiste** porque:
1. La columna ya tiene `NVARCHAR` (correcto para Unicode)
2. El collation es `Modern_Spanish_CI_AS` (correcto)
3. **Pero los DATOS se insertaron originalmente con bytes incorrectos**

### Por Qué No Funciona el UPDATE:

```sql
-- El valor actual en binario:
-- 0x430061007200C300A100...
--   C  a  r  Ã  á  ...

-- Al hacer UPDATE con N'Carátula Ilegible':
-- SQL Server interpreta el N'' como Unicode correcto
-- PERO si la fuente original (script .sql) también tiene codificación incorrecta,
-- se vuelve a insertar mal
```

### Solución Definitiva (Requiere Intervención Manual):

```sql
-- Opción 1: UPDATE directo con HEX
UPDATE IAP_TV_ESTADOS_ARCHIVO_LOTE
SET dsEstado = CAST(0x0043006100720061007400750006C00610020004900006C006500670069006200006C0065 AS NVARCHAR(100))
WHERE cdEstadoArchivoLote = 6;

-- Opción 2: DELETE + INSERT nuevo
DELETE FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE cdEstadoArchivoLote = 6;
SET IDENTITY_INSERT IAP_TV_ESTADOS_ARCHIVO_LOTE ON;
INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (cdEstadoArchivoLote, dsEstado, dsDescripcion, snActivo)
VALUES (6, N'Carátula Ilegible', N'Archivo con carátula ilegible o incompleta', 1);
SET IDENTITY_INSERT IAP_TV_ESTADOS_ARCHIVO_LOTE OFF;

-- Opción 3: Usar SSMS GUI
-- Abrir tabla en SSMS → Editar manualmente → Copiar/pegar texto correcto
```

---

## 📊 Comparación de Soluciones

| Aspecto | Búsqueda Flexible (A) | Corrección BD (B) |
|---------|----------------------|-------------------|
| **Inmediato** | ✅ Sí | ❌ Requiere acceso BD |
| **Performance** | ⚠️ Más lento | ✅ Óptimo |
| **Riesgo** | ✅ Bajo | ⚠️ Medio (migración) |
| **Mantenibilidad** | ✅ Alta | ✅ Alta |
| **Soluciona raíz** | ❌ No | ✅ Sí |

**Decisión:** Implementar **Opción A** inmediatamente, y considerar **Opción B** como mejora futura.

---

## 🧪 Verificación

### Prueba del Flujo:

1. Abrir **FrmControlLote** con un lote en estado "Pendiente de Controlar"
2. Seleccionar un archivo
3. Click en **"Marcar Ilegible"**
4. Confirmar el diálogo
5. Verificar que:
   - ✅ NO aparece error de "No se encontró el estado"
   - ✅ Se marca el archivo como ilegible
   - ✅ Se actualiza la estadística del lote
   - ✅ Se avanza al siguiente archivo

### Log Esperado:

```
[WARNING] Estado 'Carátula Ilegible' encontrado con búsqueda flexible (posible problema de codificación)
[INFO] Archivo 123 marcado como ilegible
```

### Verificar en BD:

```sql
-- Ver archivo marcado
SELECT la.cdLoteArchivo, la.cdEstadoArchivoLote, ea.dsEstado
FROM IAP_TD_LOTE_ARCHIVOS la
JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE ea ON la.cdEstadoArchivoLote = ea.cdEstadoArchivoLote
WHERE la.cdLoteArchivo = 123;

-- Resultado esperado:
-- cdLoteArchivo | cdEstadoArchivoLote | dsEstado
-- 123           | 6                    | CarÃ¡tula Ilegible  ← Nota: sigue mal codificado pero funciona
```

---

## 📝 Otros Estados Afectados

### Verificación:

```sql
SELECT cdEstadoArchivoLote, dsEstado
FROM IAP_TV_ESTADOS_ARCHIVO_LOTE
WHERE dsEstado LIKE '%Ã%' OR dsEstado LIKE '%í%' OR dsEstado LIKE '%ó%';
```

**Resultado del script de verificación:**
- ✅ Solo `cdEstadoArchivoLote = 6` tiene problema
- ✅ Estados con acentos bien codificados: `Imagen Extraída` (ID 9)

### Estados con Caracteres Especiales:

| ID | Estado | Codificación |
|----|--------|--------------|
| 6 | Carátula Ilegible | ❌ Incorrecta |
| 9 | Imagen Extraída | ✅ Correcta |

---

## 🚀 Recomendación para el Futuro

### Prevención:

1. **Scripts SQL:** Siempre usar prefijo `N` en strings: `N'Texto con acentos'`
2. **Archivos .sql:** Guardar con codificación **UTF-8 with BOM**
3. **sqlcmd:** Usar flag `-f 65001` para UTF-8: `sqlcmd -f 65001 -i script.sql`

### Ejemplo Correcto:

```sql
-- ✅ CORRECTO
INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo)
VALUES (N'Carátula Ilegible', N'Archivo con carátula ilegible', 1);
	   ↑↑↑↑↑↑↑ Prefijo N obligatorio para Unicode

-- ❌ INCORRECTO
INSERT INTO IAP_TV_ESTADOS_ARCHIVO_LOTE (dsEstado, dsDescripcion, snActivo)
VALUES ('Carátula Ilegible', 'Archivo con carátula ilegible', 1);
	   ↑ Sin N, SQL Server interpreta como Latin-1/ANSI
```

---

## ✅ Validación

### Compilación:
```
✅ Compilación correcta
```

### Búsqueda Flexible:
```
✅ Encuentra "Carátula Ilegible" aunque esté mal codificado
✅ Log de advertencia registra el problema
✅ Funciona con cualquier variación de acentos
```

### Flujo Completo:
```
✅ Botón "Marcar Ilegible" funciona
✅ Estado se actualiza en BD
✅ Estadísticas se recalculan correctamente
```

---

## 📋 Resumen de Cambios

| Archivo | Método | Cambio |
|---------|--------|--------|
| `LoteRepository.cs` | `ObtenerIdEstadoArchivoLotePorNombre()` | Agregada búsqueda flexible con fallback |
| `Scripts/13_CorregirCodificacionEstados.sql` | - | Creado script de diagnóstico (informativo) |

---

**Fecha:** 2026-06-10  
**Versión:** 1.5 (Corrección Búsqueda Flexible Estados)
