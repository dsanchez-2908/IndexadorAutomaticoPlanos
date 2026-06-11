# CORRECCIÓN - Error JOIN cdResultadoIA

## 🐛 Error Reportado

**Mensaje:**
```
Error al ejecutar consulta: Invalid column name 'cdResultadoIA'.
```

**Ubicación:** `LoteRepository.ObtenerArchivosParaFinalizacion()`

**Causa Raíz:** 
El JOIN estaba intentando usar `la.cdResultadoIA` (columna que **NO existe** en `IAP_TD_LOTE_ARCHIVOS`)

---

## 🔍 Análisis de Esquema

### Estructura Real de Tablas:

**IAP_TD_LOTE_ARCHIVOS:**
```
cdLoteArchivo (PK)
cdLote (FK)
cdArchivo (FK)
cdEstadoArchivoLote (FK)
nuOrden
feAlta
cdUsuarioAlta
...
❌ NO tiene cdResultadoIA
```

**IAP_TD_RESULTADOS_IA:**
```
cdResultadoIA (PK)
cdLoteArchivo (FK) ← Referencia a IAP_TD_LOTE_ARCHIVOS
dsTipoPlano
dsExpediente
dsSeccion
dsManzana
dsParcela
dsDireccion
...
```

### Relación Correcta:

```
IAP_TD_LOTE_ARCHIVOS (1) ←--→ (0..1) IAP_TD_RESULTADOS_IA
						 cdLoteArchivo
```

**La FK está en `IAP_TD_RESULTADOS_IA`, NO en `IAP_TD_LOTE_ARCHIVOS`**

---

## ✅ Solución Implementada

### JOIN Corregido:

**ANTES (❌ Incorrecto):**
```sql
LEFT JOIN IAP_TD_RESULTADOS_IA ri ON la.cdResultadoIA = ri.cdResultadoIA
									  ^^^^^^^^^^^^^^^^
									  Columna inexistente
```

**AHORA (✅ Correcto):**
```sql
LEFT JOIN IAP_TD_RESULTADOS_IA ri ON la.cdLoteArchivo = ri.cdLoteArchivo
									  ^^^^^^^^^^^^^^^
									  Columna correcta
```

---

### Query Completa Corregida:

```sql
SELECT 
	la.cdLoteArchivo,
	la.cdArchivo,
	la.cdEstadoArchivoLote,
	eal.dsEstado AS DsEstadoArchivoLote,
	a.dsNombreArchivo,
	a.dsRutaCompleta,
	a.dsNombreUltimaCarpeta,
	ri.cdResultadoIA,          -- ✅ Viene de IAP_TD_RESULTADOS_IA
	ri.dsTipoPlano,
	ri.dsExpediente,
	ri.dsSeccion,
	ri.dsManzana,
	ri.dsParcela,
	ri.dsDireccion,
	l.dsNombreLote
FROM IAP_TD_LOTE_ARCHIVOS la
INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE eal ON la.cdEstadoArchivoLote = eal.cdEstadoArchivoLote
LEFT JOIN IAP_TD_RESULTADOS_IA ri ON la.cdLoteArchivo = ri.cdLoteArchivo  -- ✅ CORREGIDO
INNER JOIN IAP_TD_LOTES l ON la.cdLote = l.cdLote
WHERE la.cdLote = @cdLote
ORDER BY a.dsNombreArchivo
```

---

### Manejo de DBNull Mejorado:

**ANTES:**
```csharp
DsTipoPlano = row["dsTipoPlano"]?.ToString(),
```

**AHORA:**
```csharp
DsTipoPlano = row["dsTipoPlano"] != DBNull.Value ? row["dsTipoPlano"].ToString() : null,
```

**Razón:** El operador `?.` no funciona con `DataRow`, siempre devuelve objeto (nunca null), solo puede ser `DBNull.Value`.

---

## 🧪 Verificación SQL

### Consulta de prueba ejecutada:

```sql
SELECT 
	la.cdLoteArchivo, 
	a.dsNombreArchivo, 
	ri.cdResultadoIA, 
	ri.dsTipoPlano, 
	ri.dsSeccion, 
	ri.dsManzana, 
	ri.dsParcela 
FROM IAP_TD_LOTE_ARCHIVOS la
INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
LEFT JOIN IAP_TD_RESULTADOS_IA ri ON la.cdLoteArchivo = ri.cdLoteArchivo
WHERE la.cdLote = 1
```

### Resultado:

| cdLoteArchivo | dsNombreArchivo | cdResultadoIA | dsTipoPlano | dsSeccion | dsManzana | dsParcela |
|---------------|-----------------|---------------|-------------|-----------|-----------|-----------|
| 1 | EX-2006-00041690-   -MGEYA-DGROC-PLANOS.pdf | 3 | Obra | 27 | 101 | 006 A |

✅ **Query funciona correctamente**

---

## 📊 Resumen de Cambios

| Archivo | Línea | Cambio |
|---------|-------|--------|
| `LoteRepository.cs` | 1303 | `LEFT JOIN ... ON la.cdLoteArchivo = ri.cdLoteArchivo` |
| `LoteRepository.cs` | 1328-1333 | Uso explícito de `!= DBNull.Value` en lugar de `?.` |

---

## ✅ Validación

### Compilación:
```
✅ Compilación correcta
```

### Query SQL:
```
✅ Ejecutada exitosamente
✅ Devuelve datos de ResultadoIA correctamente
```

---

## 🚀 Próxima Prueba

Ahora que la query está corregida, probar nuevamente el proceso de finalización:

1. Abrir aplicación
2. Menú → Procesos → "6. Finalizar Lotes"
3. Seleccionar **LOTE_000001**
4. Click en "Finalizar Lotes Seleccionados"
5. Verificar que **NO** aparezca el error de `cdResultadoIA`

---

## 📝 Datos de Ejemplo Encontrados

Del lote 1:
- **Archivo:** `EX-2006-00041690-   -MGEYA-DGROC-PLANOS.pdf`
- **Tipo Plano:** Obra
- **Nomenclatura:** 27 / 101 / 006 A
- **cdResultadoIA:** 3 (existe resultado de IA)

**Importante:** Este archivo tiene:
- ✅ Resultado de IA asociado
- ✅ Metadatos completos (TipoPlano, Nomenclatura)
- ⚠️ Falta verificar si tiene Dirección y Expediente

---

## 🔍 Validación de Datos Completos

Para verificar si el archivo está completo para finalización:

```sql
SELECT 
	ri.dsTipoPlano,
	ri.dsExpediente,
	ri.dsSeccion,
	ri.dsManzana,
	ri.dsParcela,
	ri.dsDireccion,
	eal.dsEstado AS EstadoArchivo
FROM IAP_TD_RESULTADOS_IA ri
INNER JOIN IAP_TD_LOTE_ARCHIVOS la ON ri.cdLoteArchivo = la.cdLoteArchivo
INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE eal ON la.cdEstadoArchivoLote = eal.cdEstadoArchivoLote
WHERE ri.cdResultadoIA = 3;
```

---

**Fecha:** 2026-06-10
**Versión:** 1.3 (Corrección JOIN ResultadoIA)
