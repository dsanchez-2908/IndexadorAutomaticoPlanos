# CORRECCIONES - Finalización de Lotes

## 🐛 Problemas Encontrados

### 1. **Nombres de Tablas Incorrectos**
- ❌ `IAP_TV_ESTADO_LOTE` (singular)
- ✅ `IAP_TV_ESTADOS_LOTE` (plural)

- ❌ `IAP_TV_ESTADO_ARCHIVO_LOTE` (singular)
- ✅ `IAP_TV_ESTADOS_ARCHIVO_LOTE` (plural)

### 2. **Nombres de Columnas Incorrectos**
- ❌ `el.dsEstadoLote`
- ✅ `el.dsEstado` (con alias `AS DsEstadoLote`)

- ❌ `eal.dsEstadoArchivoLote`
- ✅ `eal.dsEstado` (con alias `AS DsEstadoArchivoLote`)

### 3. **Estados de Lote Incorrectos**
Según la base de datos real del usuario:
- Estado 4: "Pendiente de Finalizar" (existente)
- Estado 5: "Finalizado" (nuevo, reemplaza el 6)

**Antes:** El código buscaba estado 5 y cambiaba a 6
**Ahora:** El código busca estado 4 y cambia a 5

### 4. **Propiedad Faltante en Entidad Lote**
- Faltaba: `DsCarpetaOrigen`
- Faltaba: `CantidadArchivos` (para binding de grilla)

---

## ✅ Archivos Corregidos

### `IndexadorAutomaticoPlanos/DataAccess/LoteRepository.cs`

**Método: `ObtenerLotesPendientesFinalizacion()`**
```csharp
// ANTES:
INNER JOIN IAP_TV_ESTADO_LOTE el ...
WHERE l.cdEstadoLote = 5

// AHORA:
INNER JOIN IAP_TV_ESTADOS_LOTE el ...
el.dsEstado AS DsEstadoLote,
WHERE l.cdEstadoLote = 4
```

**Método: `ObtenerArchivosParaFinalizacion()`**
```csharp
// ANTES:
INNER JOIN IAP_TV_ESTADO_ARCHIVO_LOTE eal ...
eal.dsEstadoArchivoLote

// AHORA:
INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE eal ...
eal.dsEstado AS DsEstadoArchivoLote
```

**Método: `MapearLote()`**
```csharp
// Agregado:
DsCarpetaOrigen = row.Table.Columns.Contains("dsCarpetaOrigen") ? row["dsCarpetaOrigen"]?.ToString() : null,
CantidadArchivos = row.Table.Columns.Contains("CantidadArchivos") ? Convert.ToInt32(row["CantidadArchivos"]) : 0,
```

---

### `IndexadorAutomaticoPlanos/Entities/Lote.cs`

```csharp
// Propiedades agregadas:
public string? DsCarpetaOrigen { get; set; }
public int CantidadArchivos { get; set; }
```

---

### `IndexadorAutomaticoPlanos/UI/FrmFinalizarLotes.cs`

```csharp
// ANTES:
int cdEstadoFinalizado = 6;

// AHORA:
int cdEstadoFinalizado = 5;
```

---

### `IndexadorAutomaticoPlanos/Scripts/12_FinalizarLotes.sql`

**Estados actualizados:**
```sql
-- Estado 4: Pendiente de Finalizar (con UPDATE si existe)
IF NOT EXISTS (SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote = 4)
BEGIN
	INSERT INTO IAP_TV_ESTADOS_LOTE (cdEstadoLote, dsEstado, dsDescripcion)
	VALUES (4, 'Pendiente de Finalizar', 'Lote controlado y pendiente de finalización');
END
ELSE
BEGIN
	UPDATE IAP_TV_ESTADOS_LOTE 
	SET dsEstado = 'Pendiente de Finalizar',
		dsDescripcion = 'Lote controlado y pendiente de finalización'
	WHERE cdEstadoLote = 4;
END

-- Estado 5: Finalizado (NUEVO)
INSERT INTO IAP_TV_ESTADOS_LOTE (cdEstadoLote, dsEstado, dsDescripcion)
VALUES (5, 'Finalizado', 'Lote finalizado con archivos renombrados e índice generado');
```

**Vista actualizada:**
```sql
-- ANTES:
WHERE l.cdEstadoLote = 5

-- AHORA:
WHERE l.cdEstadoLote = 4
```

---

## 🧪 Verificación

### Paso 1: Ejecutar Script SQL

```sql
USE Capturador;
GO

-- Ejecutar:
12_FinalizarLotes.sql
```

**Resultado esperado:**
```
Estado de lote 4 "Pendiente de Finalizar" actualizado
Estado de lote 5 "Finalizado" creado
Índice IX_IAP_TD_LOTES_cdEstadoLote creado
Vista IAP_VW_LOTES_PARA_FINALIZACION creada
```

---

### Paso 2: Verificar Estados

```sql
SELECT * FROM IAP_TV_ESTADOS_LOTE ORDER BY cdEstadoLote;
```

**Esperado:**
| cdEstadoLote | dsEstado | dsDescripcion |
|--------------|----------|---------------|
| 1 | Pendiente | Lote creado, sin procesar |
| 2 | En Proceso | ... |
| 3 | Procesado | ... |
| 4 | Pendiente de Finalizar | Lote controlado y pendiente de finalización |
| 5 | Finalizado | Lote finalizado con archivos renombrados e índice generado |

---

### Paso 3: Verificar Lote de Prueba

```sql
-- Ver el lote actual:
SELECT 
	l.cdLote, 
	l.dsNombreLote, 
	l.cdEstadoLote, 
	el.dsEstado,
	COUNT(la.cdLoteArchivo) AS TotalArchivos
FROM IAP_TD_LOTES l
INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
LEFT JOIN IAP_TD_LOTE_ARCHIVOS la ON l.cdLote = la.cdLote
WHERE l.cdLote = 1
GROUP BY l.cdLote, l.dsNombreLote, l.cdEstadoLote, el.dsEstado;
```

**Esperado:**
```
cdLote: 1
dsNombreLote: LOTE_000001
cdEstadoLote: 4
dsEstado: Pendiente de Finalizar
TotalArchivos: <número de archivos>
```

---

### Paso 4: Probar en la Aplicación

1. ✅ Ejecutar aplicación
2. ✅ Menú → Procesos → "6. Finalizar Lotes"
3. ✅ Debe aparecer **LOTE_000001** en la grilla
4. ✅ Columnas visibles:
   - Lote
   - Carpeta Origen
   - Archivos (cantidad)
   - Fecha Creación
   - Estado

---

## 📊 Resumen de Cambios

| Archivo | Cambios |
|---------|---------|
| `LoteRepository.cs` | 3 queries corregidas (nombres tabla + columna + estado) |
| `Lote.cs` | 2 propiedades agregadas (`DsCarpetaOrigen`, `CantidadArchivos`) |
| `FrmFinalizarLotes.cs` | Estado finalizado: 6 → 5 |
| `12_FinalizarLotes.sql` | Estados: (5,6) → (4,5) con UPDATE |

---

## 🚀 Estado Actual

✅ **Compilación exitosa**
✅ **Queries SQL corregidas**
✅ **Estados alineados con BD real**
✅ **Entidad Lote completa**

**Próximo paso:** Ejecutar script SQL y probar carga de lotes en la aplicación.

---

**Fecha:** 2026-06-10
**Versión:** 1.1 (Correcciones post-implementación)
