# FUNCIONALIDAD - Manejo de Archivos Ilegibles en Finalización

## 🎯 Requerimiento

Cuando un archivo está marcado como **"Carátula Ilegible"** (Estado 6), debe ser procesado de forma especial en la finalización del lote:

1. **Renombrado**: El archivo debe nombrarse `CARATULA_ILEGIBLE.pdf`
2. **Carpeta destino**: Debe moverse a una subcarpeta `ILEGIBLE` dentro de la carpeta del lote
3. **Duplicados**: Si hay múltiples ilegibles, agregar contador: `CARATULA_ILEGIBLE(2).pdf`, `CARATULA_ILEGIBLE(3).pdf`, etc.
4. **CSV**: Debe registrarse en `INDEX.csv` con nombre original y nombre nuevo

---

## ✅ Implementación Actual

### 1. Detección de Archivo Ilegible

**Entidad: `ArchivoParaFinalizar.cs`**
```csharp
/// <summary>
/// Indica si el archivo está marcado como "Carátula Ilegible" (Estado 6)
/// </summary>
public bool EsIlegible => CdEstadoArchivoLote == EstadosArchivoLote.CaratulaIlegible;
```

**Mejora Aplicada:**
- ✅ Usa comparación por **ID** (6) en lugar de nombre
- ✅ Inmune a problemas de codificación de caracteres

---

### 2. Renombrado y Movimiento a Subcarpeta

**Clase: `FinalizadorLotes.cs`**
```csharp
private OperacionRenombrado RenombrarArchivo(ArchivoParaFinalizar archivo)
{
	string rutaCompleta = Path.Combine(_pathRepositorio, archivo.DsRutaCompleta, archivo.DsNombreArchivo);
	string nuevoNombre;
	string carpetaDestino = Path.GetDirectoryName(rutaCompleta) ?? string.Empty;

	// Procesar archivos ilegibles
	if (archivo.EsIlegible)
	{
		nuevoNombre = "CARATULA_ILEGIBLE.pdf";
		carpetaDestino = Path.Combine(carpetaDestino, "ILEGIBLE");

		// Crear carpeta ILEGIBLE si no existe
		if (!Directory.Exists(carpetaDestino))
		{
			Directory.CreateDirectory(carpetaDestino);
		}
	}
	else
	{
		// Generar nombre según nomenclatura
		nuevoNombre = GenerarNombreArchivo(archivo);
	}

	// Resolver duplicados
	string rutaDestino = Path.Combine(carpetaDestino, nuevoNombre);
	rutaDestino = ResolverNombreDuplicado(rutaDestino);

	// Renombrar/mover archivo
	File.Move(rutaCompleta, rutaDestino);

	return new OperacionRenombrado
	{
		Archivo = archivo,
		RutaOrigen = rutaCompleta,
		RutaDestino = rutaDestino,
		NombreOriginal = archivo.DsNombreArchivo,
		NombreNuevo = Path.GetFileName(rutaDestino)
	};
}
```

**Funcionalidades:**
- ✅ Nombre fijo: `CARATULA_ILEGIBLE.pdf`
- ✅ Crea subcarpeta `ILEGIBLE` automáticamente
- ✅ Manejo de duplicados integrado

---

### 3. Manejo de Duplicados

**Método: `ResolverNombreDuplicado()`**
```csharp
/// <summary>
/// Resuelve nombres duplicados agregando sufijo numérico
/// Formato: archivo.pdf → archivo(2).pdf → archivo(3).pdf
/// </summary>
private string ResolverNombreDuplicado(string rutaArchivo)
{
	if (!File.Exists(rutaArchivo))
	{
		return rutaArchivo;
	}

	string directorio = Path.GetDirectoryName(rutaArchivo) ?? string.Empty;
	string nombreSinExtension = Path.GetFileNameWithoutExtension(rutaArchivo);
	string extension = Path.GetExtension(rutaArchivo);

	int contador = 2; // Empieza en 2 porque el primero no tiene número
	string nuevaRuta;

	do
	{
		string nuevoNombre = $"{nombreSinExtension}({contador}){extension}";
		nuevaRuta = Path.Combine(directorio, nuevoNombre);
		contador++;
	}
	while (File.Exists(nuevaRuta));

	return nuevaRuta;
}
```

**Corrección Aplicada:**
- ❌ **ANTES**: `CARATULA_ILEGIBLE_(1).pdf` (formato incorrecto)
- ✅ **AHORA**: `CARATULA_ILEGIBLE(2).pdf` (formato correcto)

**Secuencia de Nombres:**
1. Primer archivo ilegible: `CARATULA_ILEGIBLE.pdf`
2. Segundo archivo ilegible: `CARATULA_ILEGIBLE(2).pdf`
3. Tercer archivo ilegible: `CARATULA_ILEGIBLE(3).pdf`
4. Y así sucesivamente...

---

### 4. Registro en CSV

**Método: `GenerarActualizarCSV()`**
```csharp
private void GenerarActualizarCSV(string rutaCSV, List<OperacionRenombrado> operaciones)
{
	bool archivoExiste = File.Exists(rutaCSV);

	using (var writer = new StreamWriter(rutaCSV, append: true, Encoding.UTF8))
	{
		// Escribir encabezado solo si el archivo no existe
		if (!archivoExiste)
		{
			writer.WriteLine("NombreLote,NombreOriginal,NombreNuevo,TipoPlano,Expediente,Seccion,Manzana,Parcela,Direccion");
		}

		// Escribir registros (incluye ilegibles y normales)
		foreach (var op in operaciones)
		{
			var archivo = op.Archivo;
			string linea = $"{EscaparCSV(archivo.DsNombreLote)}," +
						  $"{EscaparCSV(op.NombreOriginal)}," +
						  $"{EscaparCSV(op.NombreNuevo)}," +
						  $"{EscaparCSV(archivo.DsTipoPlano ?? "")}," +
						  $"{EscaparCSV(archivo.DsExpediente ?? "")}," +
						  $"{EscaparCSV(archivo.DsSeccion ?? "")}," +
						  $"{EscaparCSV(archivo.DsManzana ?? "")}," +
						  $"{EscaparCSV(archivo.DsParcela ?? "")}," +
						  $"{EscaparCSV(archivo.DsDireccion ?? "")}";

			writer.WriteLine(linea);
		}
	}
}
```

**Características:**
- ✅ Incluye **todos** los archivos (ilegibles y normales)
- ✅ Registra nombre original y nombre nuevo
- ✅ Campos de metadatos vacíos para ilegibles (no aplican)
- ✅ Modo **append** para múltiples lotes en la misma carpeta

---

## 📊 Ejemplo de Proceso Completo

### Escenario: Lote con 3 Archivos (2 Controlados + 1 Ilegible)

**Estado Inicial:**
```
C:\Repositorio\LOTE_000005\
├── EX-2007-00067880-MGEYA-DGROC-PLANOS.pdf       (Estado: 5 - Controlado)
├── EX-2006-00041690-MGEYA-DGROC-PLANOS.pdf       (Estado: 6 - Carátula Ilegible)
└── EX-1993-00094543-MGEYA-DGROC-PLANOS.pdf       (Estado: 5 - Controlado)
```

**Proceso:**
1. **Archivo 1** (Controlado):
   - Nombre original: `EX-2007-00067880-MGEYA-DGROC-PLANOS.pdf`
   - Nombre nuevo: `EX-2007-00067880-Obra-Av Corrientes 1234-27_101_006A.pdf`
   - Carpeta: `C:\Repositorio\LOTE_000005\`

2. **Archivo 2** (Ilegible):
   - Nombre original: `EX-2006-00041690-MGEYA-DGROC-PLANOS.pdf`
   - Nombre nuevo: `CARATULA_ILEGIBLE.pdf`
   - Carpeta: `C:\Repositorio\LOTE_000005\ILEGIBLE\` ← Crea subcarpeta

3. **Archivo 3** (Controlado):
   - Nombre original: `EX-1993-00094543-MGEYA-DGROC-PLANOS.pdf`
   - Nombre nuevo: `EX-1993-00094543-Obra-Av Libertador 5678-28_102_007B.pdf`
   - Carpeta: `C:\Repositorio\LOTE_000005\`

**Estado Final:**
```
C:\Repositorio\LOTE_000005\
├── EX-2007-00067880-Obra-Av Corrientes 1234-27_101_006A.pdf
├── EX-1993-00094543-Obra-Av Libertador 5678-28_102_007B.pdf
├── ILEGIBLE\
│   └── CARATULA_ILEGIBLE.pdf                     ← Archivo ilegible movido
└── INDEX.csv                                     ← Archivo de índice generado
```

**Contenido de `INDEX.csv`:**
```csv
NombreLote,NombreOriginal,NombreNuevo,TipoPlano,Expediente,Seccion,Manzana,Parcela,Direccion
LOTE_000005,EX-2007-00067880-MGEYA-DGROC-PLANOS.pdf,EX-2007-00067880-Obra-Av Corrientes 1234-27_101_006A.pdf,Obra,EX-2007-00067880,27,101,006A,Av Corrientes 1234
LOTE_000005,EX-2006-00041690-MGEYA-DGROC-PLANOS.pdf,CARATULA_ILEGIBLE.pdf,,,,,,
LOTE_000005,EX-1993-00094543-MGEYA-DGROC-PLANOS.pdf,EX-1993-00094543-Obra-Av Libertador 5678-28_102_007B.pdf,Obra,EX-1993-00094543,28,102,007B,Av Libertador 5678
```

**Nota:** El archivo ilegible tiene campos vacíos (TipoPlano, Expediente, etc.) porque no aplican.

---

## 📊 Ejemplo con Múltiples Ilegibles

### Escenario: Lote con 5 Archivos (2 Controlados + 3 Ilegibles)

**Estado Inicial:**
```
C:\Repositorio\LOTE_000006\
├── archivo1.pdf  (Estado: 5 - Controlado)
├── archivo2.pdf  (Estado: 6 - Carátula Ilegible)
├── archivo3.pdf  (Estado: 6 - Carátula Ilegible)
├── archivo4.pdf  (Estado: 5 - Controlado)
└── archivo5.pdf  (Estado: 6 - Carátula Ilegible)
```

**Proceso:**
1. `archivo1.pdf` → `Expediente-Obra-Direccion-Nomenclatura.pdf` (carpeta raíz)
2. `archivo2.pdf` → `CARATULA_ILEGIBLE.pdf` (carpeta ILEGIBLE) ← Primer ilegible
3. `archivo3.pdf` → `CARATULA_ILEGIBLE(2).pdf` (carpeta ILEGIBLE) ← Segundo ilegible
4. `archivo4.pdf` → `Expediente-Obra-Direccion-Nomenclatura.pdf` (carpeta raíz)
5. `archivo5.pdf` → `CARATULA_ILEGIBLE(3).pdf` (carpeta ILEGIBLE) ← Tercer ilegible

**Estado Final:**
```
C:\Repositorio\LOTE_000006\
├── Expediente-Obra-Direccion-Nomenclatura.pdf
├── Expediente-Obra-Direccion-Nomenclatura(2).pdf   ← Si hay duplicado normal
├── ILEGIBLE\
│   ├── CARATULA_ILEGIBLE.pdf
│   ├── CARATULA_ILEGIBLE(2).pdf
│   └── CARATULA_ILEGIBLE(3).pdf
└── INDEX.csv
```

---

## 🔄 Manejo de Transacciones

### Rollback en Caso de Error

Si ocurre un error durante el procesamiento, **todas las operaciones se revierten**:

```csharp
public ResultadoFinalizacion ProcesarLote(List<ArchivoParaFinalizar> archivos)
{
	try
	{
		// Renombrar archivos (ilegibles y normales)
		foreach (var archivo in archivos)
		{
			var operacion = RenombrarArchivo(archivo);
			_operacionesRealizadas.Add(operacion);
		}

		// Generar CSV
		GenerarActualizarCSV(archivoCSV, _operacionesRealizadas);

		return ResultadoExitoso;
	}
	catch (Exception ex)
	{
		// ⚠️ Si falla → Rollback de TODAS las operaciones
		RealizarRollback();
		return ResultadoError;
	}
}
```

**Rollback:**
```csharp
private void RealizarRollback()
{
	// Revertir operaciones en orden inverso
	foreach (var operacion in _operacionesRealizadas.AsEnumerable().Reverse())
	{
		if (File.Exists(operacion.RutaDestino))
		{
			File.Move(operacion.RutaDestino, operacion.RutaOrigen);
		}
	}
}
```

**Ejemplo de Fallo:**
```
1. archivo1.pdf → renombrado exitosamente ✅
2. archivo2.pdf (ilegible) → CARATULA_ILEGIBLE.pdf ✅
3. archivo3.pdf → ERROR: archivo abierto ❌

→ ROLLBACK:
   - archivo2.pdf restaurado desde ILEGIBLE\CARATULA_ILEGIBLE.pdf
   - archivo1.pdf restaurado a nombre original
```

---

## 🧪 Verificación

### Prueba Manual:

1. Abrir **FrmControlLote** con el lote `LOTE_000005`
2. Marcar archivo como **"Carátula Ilegible"** (botón "Marcar Ilegible")
3. Click en **"Finalizar Lote"**
4. Abrir **FrmFinalizarLotes**
5. Seleccionar `LOTE_000005`
6. Click en **"Finalizar Lotes Seleccionados"**
7. Verificar estructura resultante:
   ```
   C:\Repositorio\LOTE_000005\
   ├── [archivos renombrados normalmente]
   ├── ILEGIBLE\
   │   └── CARATULA_ILEGIBLE.pdf    ← Archivo ilegible aquí
   └── INDEX.csv
   ```

8. Verificar contenido de `INDEX.csv`:
   - ✅ Contiene registro del archivo ilegible
   - ✅ Nombre original correcto
   - ✅ Nombre nuevo = `CARATULA_ILEGIBLE.pdf`
   - ✅ Campos de metadatos vacíos

---

### Prueba con Múltiples Ilegibles:

1. Crear lote de prueba con 3 archivos
2. Marcar **2 archivos** como ilegibles
3. Procesar finalización
4. Verificar:
   ```
   ILEGIBLE\
   ├── CARATULA_ILEGIBLE.pdf      ← Primer ilegible
   └── CARATULA_ILEGIBLE(2).pdf   ← Segundo ilegible
   ```

5. Verificar `INDEX.csv`:
   ```csv
   LOTE_TEST,archivo1.pdf,CARATULA_ILEGIBLE.pdf,,,,,,
   LOTE_TEST,archivo2.pdf,CARATULA_ILEGIBLE(2).pdf,,,,,,
   ```

---

## ✅ Validación Final

### Compilación:
```
✅ Compilación correcta
```

### Funcionalidades:
```
✅ Detección de ilegibles por ID (6) en lugar de nombre
✅ Renombrado a "CARATULA_ILEGIBLE.pdf"
✅ Creación automática de subcarpeta "ILEGIBLE"
✅ Contador de duplicados correcto: (2), (3), (4)...
✅ Registro en CSV con nombre original y nuevo
✅ Campos de metadatos vacíos para ilegibles
✅ Rollback en caso de error
```

---

## 📋 Resumen de Cambios

| Archivo | Cambio | Línea |
|---------|--------|-------|
| **ArchivoParaFinalizar.cs** | 🔧 `EsIlegible` usa ID en lugar de nombre | 35 |
| **FinalizadorLotes.cs** | 🔧 Formato de duplicados: `(2)` en lugar de `_(1)` | 233 |

---

**Fecha:** 2026-06-10  
**Versión:** 2.2 (Corrección Manejo Archivos Ilegibles)  
**Impacto:** Medio - Corrige formato de duplicados y detección por ID
