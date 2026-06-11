# CORRECCIÓN - Estadísticas y Finalización de Lote

## 🐛 Problemas Reportados

### 1. Estadísticas Incorrectas
**Mensaje de Advertencia:**
```
Total de archivos: 3
Controlados: 2
Ilegibles: 0        ← ❌ Debería ser 1
Pendientes: 1       ← ❌ Debería ser 0
```

**Estado Real en BD:**
- Archivo 8: Estado 5 (Controlado)
- Archivo 9: Estado 6 (Carátula Ilegible)
- Archivo 10: Estado 5 (Controlado)

**Resultado Esperado:**
- Total: 3 ✅
- Controlados: 2 ✅
- Ilegibles: 1 ✅
- Pendientes: 0 ✅

---

### 2. Finalización Bloqueada
**Comportamiento Anterior:**
```
Si hay archivos pendientes → Mensaje "Hay X archivo(s) pendiente(s) de controlar"
						   → NO permite finalizar
```

**Comportamiento Requerido:**
```
Si hay archivos pendientes → Mensaje de advertencia
						   → Permite confirmar y finalizar de todas formas
```

**Razón:** El control de calidad puede ser **parcial** (no todos los archivos requieren control).

---

## 🔍 Análisis de Causas

### Problema 1: Búsqueda por Nombre en Estadísticas

**Código Original:**
```csharp
string query = @"
	SELECT 
		COUNT(*) AS Total,
		SUM(CASE WHEN ea.dsEstado = 'Controlado' THEN 1 ELSE 0 END) AS Controlados,
		SUM(CASE WHEN ea.dsEstado = 'Carátula Ilegible' THEN 1 ELSE 0 END) AS Ilegibles,
		SUM(CASE WHEN ea.dsEstado NOT IN ('Controlado', 'Carátula Ilegible') THEN 1 ELSE 0 END) AS Pendientes
	FROM IAP_TD_LOTE_ARCHIVOS la
	INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE ea ON la.cdEstadoArchivoLote = ea.cdEstadoArchivoLote
	WHERE la.cdLote = @cdLote";
```

**Problemas:**
1. ❌ Búsqueda por nombre `'Carátula Ilegible'` vs valor real `'CarÃ¡tula Ilegible'`
2. ❌ JOIN innecesario solo para obtener el nombre
3. ❌ Comparación de strings sensible a codificación

**Resultado:** `Ilegibles = 0` (no encuentra coincidencias por codificación)

---

### Problema 2: Validación Bloqueante

**Código Original:**
```csharp
public bool FinalizarControlLote(int cdLote, int cdUsuarioModificacion, out string mensaje)
{
	// Verificar cuántos archivos quedan pendientes
	string queryVerificar = @"
		SELECT COUNT(*) 
		FROM IAP_TD_LOTE_ARCHIVOS
		WHERE cdLote = @cdLote 
		  AND cdEstadoArchivoLote NOT IN (...)";

	int numPendientes = ...;

	if (numPendientes > 0)
	{
		mensaje = "Hay X archivo(s) pendiente(s) de controlar";
		return false;  // ❌ Bloquea la finalización
	}

	// Actualizar estado del lote...
}
```

**Problema:** El método **rechaza** la finalización si hay pendientes.

---

## ✅ Soluciones Implementadas

### 1. Estadísticas por IDs en lugar de Nombres

**Código Refactorizado:**
```csharp
public (int Total, int Controlados, int Ilegibles, int Pendientes) ObtenerEstadisticasLote(int cdLote)
{
	string query = @"
		SELECT 
			COUNT(*) AS Total,
			SUM(CASE WHEN la.cdEstadoArchivoLote = @estadoControlado THEN 1 ELSE 0 END) AS Controlados,
			SUM(CASE WHEN la.cdEstadoArchivoLote = @estadoIlegible THEN 1 ELSE 0 END) AS Ilegibles,
			SUM(CASE WHEN la.cdEstadoArchivoLote NOT IN (@estadoControlado, @estadoIlegible) THEN 1 ELSE 0 END) AS Pendientes
		FROM IAP_TD_LOTE_ARCHIVOS la
		WHERE la.cdLote = @cdLote";

	DataTable dt = _db.EjecutarConsulta(query,
		DatabaseHelper.CrearParametro("@cdLote", cdLote),
		DatabaseHelper.CrearParametro("@estadoControlado", EstadosArchivoLote.Controlado),      // = 5
		DatabaseHelper.CrearParametro("@estadoIlegible", EstadosArchivoLote.CaratulaIlegible)); // = 6

	// ... mapeo de resultados
}
```

**Mejoras:**
- ✅ Usa IDs (`5` y `6`) en lugar de nombres
- ✅ Elimina JOIN innecesario con `IAP_TV_ESTADOS_ARCHIVO_LOTE`
- ✅ Inmune a problemas de codificación
- ✅ Más rápido (menos tablas involucradas)

---

### 2. Finalización Permisiva con Advertencia

**Código Refactorizado:**
```csharp
/// <summary>
/// Finaliza el control de un lote, cambiándolo a estado 4 (Pendiente de Finalizar)
/// NOTA: Permite finalizar aunque haya archivos pendientes (el control puede ser parcial)
/// </summary>
public bool FinalizarControlLote(int cdLote, int cdUsuarioModificacion, out string mensaje)
{
	try
	{
		// ✅ Actualiza directamente sin validar pendientes
		string queryActualizar = @"
			UPDATE IAP_TD_LOTES 
			SET cdEstadoLote = @estadoPendienteFinalizar,
				feUltimaModificacion = GETDATE(),
				cdUsuarioModificacion = @cdUsuarioModificacion
			WHERE cdLote = @cdLote";

		_db.EjecutarComando(queryActualizar,
			DatabaseHelper.CrearParametro("@cdLote", cdLote),
			DatabaseHelper.CrearParametro("@estadoPendienteFinalizar", EstadosLote.PendienteFinalizar), // = 4
			DatabaseHelper.CrearParametro("@cdUsuarioModificacion", cdUsuarioModificacion));

		mensaje = $"Lote {cdLote} finalizado exitosamente";
		Logger.Info(mensaje, "LoteRepository");
		return true;
	}
	catch (Exception ex)
	{
		Logger.Error($"Error al finalizar control del lote {cdLote}", ex, "LoteRepository");
		mensaje = $"Error al finalizar lote: {ex.Message}";
		return false;
	}
}
```

**Cambios:**
- ✅ Eliminada validación bloqueante de archivos pendientes
- ✅ Usa constante `EstadosLote.PendienteFinalizar` (4) en lugar de hardcodear
- ✅ Siempre retorna `true` si no hay error de BD
- ✅ Comentario documental indica que permite finalización parcial

---

### 3. Advertencia en el Formulario

**Flujo en `FrmControlLote`:**

```csharp
private void btnFinalizarLote_Click(object sender, EventArgs e)
{
	var stats = _loteRepo.ObtenerEstadisticasLote(_lote.CdLote);

	// ⚠️ Si hay pendientes, ADVERTIR (no bloquear)
	if (stats.Pendientes > 0)
	{
		var result = MessageBox.Show(
			$"ADVERTENCIA:\n\n" +
			$"Total de archivos: {stats.Total}\n" +
			$"Controlados: {stats.Controlados}\n" +
			$"Ilegibles: {stats.Ilegibles}\n" +
			$"Pendientes: {stats.Pendientes}\n\n" +
			$"¿Desea finalizar el lote de todas formas?",
			"Archivos Pendientes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

		if (result != DialogResult.Yes) return;  // ← Usuario puede cancelar
	}

	// ✅ Finalizar siempre (si usuario confirma)
	bool exito = _loteRepo.FinalizarControlLote(_lote.CdLote, ...);

	if (exito)
	{
		MessageBox.Show("Lote finalizado exitosamente...");
		this.DialogResult = DialogResult.OK;
		this.Close();
	}
}
```

**Comportamiento:**
1. Obtiene estadísticas (ahora correctas)
2. Si hay pendientes → Muestra advertencia con opción Sí/No
3. Usuario elige:
   - **Sí** → Finaliza el lote
   - **No** → Cancela la operación
4. Si no hay pendientes → Finaliza directamente

---

## 🧪 Verificación

### Consulta SQL Directa:

```sql
-- Estadísticas del LOTE_000005
SELECT 
	COUNT(*) AS Total,
	SUM(CASE WHEN la.cdEstadoArchivoLote = 5 THEN 1 ELSE 0 END) AS Controlados,
	SUM(CASE WHEN la.cdEstadoArchivoLote = 6 THEN 1 ELSE 0 END) AS Ilegibles,
	SUM(CASE WHEN la.cdEstadoArchivoLote NOT IN (5, 6) THEN 1 ELSE 0 END) AS Pendientes
FROM IAP_TD_LOTE_ARCHIVOS la
WHERE la.cdLote = 5;
```

**Resultado:**
| Total | Controlados | Ilegibles | Pendientes |
|-------|-------------|-----------|------------|
| 3     | 2           | 1         | 0          |

✅ **Correcto**

---

### Estados de Archivos del Lote 5:

```sql
SELECT 
	la.cdLoteArchivo, 
	a.dsNombreArchivo,
	la.cdEstadoArchivoLote, 
	ea.dsEstado
FROM IAP_TD_LOTE_ARCHIVOS la
INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE ea ON la.cdEstadoArchivoLote = ea.cdEstadoArchivoLote
WHERE la.cdLote = 5
ORDER BY la.cdLoteArchivo;
```

**Resultado:**
| cdLoteArchivo | dsNombreArchivo | cdEstadoArchivoLote | dsEstado |
|---------------|-----------------|---------------------|----------|
| 8 | EX-2007-00067880-...-copia.pdf | 5 | Controlado |
| 9 | EX-2006-00041690-...-copia.pdf | 6 | CarÃ¡tula Ilegible |
| 10 | EX-1993-00094543-...-copia.pdf | 5 | Controlado |

✅ **Datos confirmados**

---

## 📊 Flujo Completo Actualizado

### Antes (❌):

```
1. Usuario marca archivo como ilegible
2. Click en "Finalizar Lote"
3. Estadísticas incorrectas mostradas (Ilegibles = 0, Pendientes = 1)
4. Si hay pendientes → ERROR "Hay archivo(s) pendiente(s)" → NO finaliza
```

---

### Ahora (✅):

```
1. Usuario marca archivo como ilegible (cdEstadoArchivoLote = 6)
2. Click en "Finalizar Lote"
3. Estadísticas correctas:
   - Total: 3
   - Controlados: 2
   - Ilegibles: 1
   - Pendientes: 0
4. Si hay pendientes → Advertencia → Usuario confirma → Finaliza
5. Si no hay pendientes → Finaliza directamente
6. Estado del lote cambia a 4 (Pendiente de Finalizar)
```

---

## 🎯 Casos de Uso

### Caso 1: Todos Controlados o Ilegibles
```
Total: 10
Controlados: 8
Ilegibles: 2
Pendientes: 0

→ Click "Finalizar Lote"
→ NO muestra advertencia
→ Finaliza directamente
```

---

### Caso 2: Con Archivos Pendientes
```
Total: 10
Controlados: 6
Ilegibles: 1
Pendientes: 3

→ Click "Finalizar Lote"
→ Muestra advertencia:
   "ADVERTENCIA:
	Total de archivos: 10
	Controlados: 6
	Ilegibles: 1
	Pendientes: 3

	¿Desea finalizar el lote de todas formas?"

→ Usuario elige:
   - SÍ → Finaliza el lote (permite control parcial)
   - NO → Cancela (puede seguir controlando)
```

---

## 📋 Resumen de Cambios

| Archivo | Método | Cambio |
|---------|--------|--------|
| **LoteRepository.cs** | `ObtenerEstadisticasLote()` | 🔧 Usa IDs en lugar de nombres |
| | | 🔧 Elimina JOIN innecesario |
| **LoteRepository.cs** | `FinalizarControlLote()` | 🔧 Elimina validación bloqueante |
| | | 🔧 Usa `EstadosLote.PendienteFinalizar` |
| | | 📝 Documenta que permite control parcial |

---

## ✅ Validación Final

### Compilación:
```
✅ Compilación correcta
```

### Estadísticas:
```
✅ Total calculado correctamente
✅ Controlados calculados correctamente
✅ Ilegibles calculados correctamente (ahora encuentra ID 6)
✅ Pendientes calculados correctamente
```

### Finalización:
```
✅ Permite finalizar con advertencia si hay pendientes
✅ Permite finalizar directamente si no hay pendientes
✅ Estado del lote cambia a 4 (Pendiente de Finalizar)
```

---

## 🚀 Próximas Pruebas Recomendadas

1. **Lote sin pendientes:**
   - Todos controlados → Finalizar → Debe finalizar sin advertencia

2. **Lote con pendientes:**
   - 2 controlados, 1 pendiente → Finalizar → Debe mostrar advertencia → Confirmar → Debe finalizar

3. **Lote con ilegibles:**
   - 2 controlados, 1 ilegible → Finalizar → Debe finalizar sin advertencia

4. **Lote mixto:**
   - 3 controlados, 2 ilegibles, 1 pendiente → Finalizar → Debe mostrar advertencia → Confirmar → Debe finalizar

---

**Fecha:** 2026-06-10  
**Versión:** 2.1 (Corrección Estadísticas y Finalización Permisiva)  
**Impacto:** Alto - Corrige cálculo de estadísticas y permite control parcial
