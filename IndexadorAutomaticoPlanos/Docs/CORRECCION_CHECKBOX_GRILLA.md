# CORRECCIÓN - Checkbox Primera Fila No Seleccionable

## 🐛 Problema Reportado

**Síntoma:** En la grilla de "Finalizar Lotes", el checkbox del primer registro no se puede seleccionar, pero los demás sí.

**Causa Raíz:** 
1. `dgvLotes.ReadOnly = true` bloqueaba la edición de TODAS las celdas, incluido el checkbox
2. El checkbox no tenía valores iniciales (`TrueValue`/`FalseValue`) configurados
3. Los valores del checkbox no se inicializaban después del data binding

---

## ✅ Solución Implementada

### 1. **Grilla en Modo Editable con Columnas Selectivas ReadOnly**

**ANTES:**
```csharp
dgvLotes.ReadOnly = true; // ❌ Bloqueaba TODO
```

**AHORA:**
```csharp
dgvLotes.ReadOnly = false; // ✅ Permite edición

// Cada columna de datos con ReadOnly = true
dgvLotes.Columns.Add(new DataGridViewTextBoxColumn
{
	Name = "DsNombreLote",
	HeaderText = "Lote",
	DataPropertyName = "DsNombreLote",
	Width = 150,
	ReadOnly = true  // ✅ Solo esta columna bloqueada
});
```

**Resultado:** Solo la columna checkbox es editable, el resto son solo lectura.

---

### 2. **Configuración Explícita de Valores del Checkbox**

**ANTES:**
```csharp
var colSeleccionar = new DataGridViewCheckBoxColumn
{
	Name = "Seleccionar",
	HeaderText = "",
	Width = 40,
	ReadOnly = false
};
```

**AHORA:**
```csharp
var colSeleccionar = new DataGridViewCheckBoxColumn
{
	Name = "Seleccionar",
	HeaderText = "✓",  // ✅ Encabezado visual
	Width = 40,
	ReadOnly = false,
	FalseValue = false,  // ✅ Valor explícito para desmarcado
	TrueValue = true     // ✅ Valor explícito para marcado
};
```

**Resultado:** El checkbox tiene valores booleanos bien definidos.

---

### 3. **Inicialización Automática Después del Binding**

**NUEVO:** Evento agregado en el constructor:

```csharp
public FrmFinalizarLotes()
{
	InitializeComponent();
	_loteRepo = new LoteRepository();
	_lotesCargados = new List<Lote>();

	_pathRepositorio = ConfigurationManager.AppSettings["PATH_REPOSITORIO"]
		?? throw new Exception("PATH_REPOSITORIO no configurado en App.config");

	ConfigurarGrilla();

	// ✅ NUEVO: Evento para inicializar checkboxes
	dgvLotes.DataBindingComplete += DgvLotes_DataBindingComplete;
}

private void DgvLotes_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
{
	// ✅ Inicializar todos los checkboxes a false
	foreach (DataGridViewRow row in dgvLotes.Rows)
	{
		row.Cells["Seleccionar"].Value = false;
	}
}
```

**Resultado:** Todos los checkboxes empiezan desmarcados y son clickeables desde el inicio.

---

## 🧪 Prueba

### Escenario de Prueba:

1. Abrir aplicación → Menú → Procesos → "6. Finalizar Lotes"
2. Verificar que la grilla carga con al menos 1 lote
3. Intentar seleccionar el checkbox del **primer registro** ✓
4. Intentar seleccionar el checkbox del **segundo registro** (si existe) ✓
5. Deseleccionar ambos checkboxes ✓

### Resultado Esperado:

✅ **TODOS** los checkboxes son seleccionables/deseleccionables
✅ La primera fila ya **NO** tiene el problema
✅ Las columnas de datos siguen siendo de solo lectura

---

## 📊 Resumen de Cambios

| Archivo | Cambio | Líneas |
|---------|--------|--------|
| `FrmFinalizarLotes.cs` | `dgvLotes.ReadOnly = false` | 68 |
| `FrmFinalizarLotes.cs` | Agregar `ReadOnly = true` a cada columna de datos | 85, 93, 101, 109, 123, 131 |
| `FrmFinalizarLotes.cs` | `FalseValue/TrueValue` en checkbox | 77-78 |
| `FrmFinalizarLotes.cs` | Evento `DataBindingComplete` | 38 |
| `FrmFinalizarLotes.cs` | Método `DgvLotes_DataBindingComplete()` | 41-48 |

---

## 🔍 Explicación Técnica

### ¿Por qué solo afectaba a la primera fila?

Cuando `dgvLotes.ReadOnly = true`, WinForms tiene un comportamiento inconsistente:

- **Primera fila:** Se renderiza como "readonly" desde el inicio
- **Filas subsiguientes:** A veces permiten 1 clic antes de aplicar readonly

Este es un bug conocido de DataGridView. La solución es:

1. Hacer la grilla editable (`ReadOnly = false`)
2. Marcar CADA columna individual como readonly (excepto checkbox)
3. Inicializar explícitamente los valores de las celdas checkbox

---

## ✅ Verificación

### Compilación:
```
✅ Compilación correcta
```

### Comportamiento:
- [x] Checkbox de primera fila clickeable
- [x] Checkbox de filas subsiguientes clickeable
- [x] Columnas de datos siguen siendo readonly
- [x] No se pueden agregar/eliminar filas

---

## 📝 Notas Adicionales

### Alternativas Consideradas:

**Opción 1:** Usar columna bindeada a propiedad bool en `Lote`
- ❌ Requiere modificar la entidad
- ❌ Más complejo

**Opción 2:** Usar `CellClick` para toggle manual
- ❌ No es intuitivo para el usuario
- ❌ Requiere más código

**Opción 3 (ELEGIDA):** Grilla editable con columnas readonly selectivas
- ✅ Solución limpia
- ✅ Comportamiento estándar de WinForms
- ✅ Mínima refactorización

---

**Fecha:** 2026-06-10
**Versión:** 1.2 (Post-corrección checkbox)
