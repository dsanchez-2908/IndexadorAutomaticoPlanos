# FINALIZACIÓN DE LOTES - GUÍA DE IMPLEMENTACIÓN Y PRUEBA

## 📋 Resumen

Esta funcionalidad permite finalizar lotes que han pasado el control de calidad, realizando:

1. **Renombrado de archivos PDF** según nomenclatura definida
2. **Generación/actualización del archivo INDEX.csv** con metadatos
3. **Cambio de estado del lote** a "Finalizado"

---

## 🗂️ Archivos Implementados

### Backend - Datos y Lógica

| Archivo | Descripción |
|---------|-------------|
| `Entities/ArchivoParaFinalizar.cs` | Entidad que encapsula metadatos de archivos para renombrado y CSV |
| `DataAccess/LoteRepository.cs` | Métodos: `ObtenerLotesPendientesFinalizacion()`, `ObtenerArchivosParaFinalizacion()`, `CambiarEstadoLote()` |
| `Utils/FinalizadorLotes.cs` | Lógica transaccional: renombrado, detección de duplicados, generación de CSV, rollback |
| `Scripts/12_FinalizarLotes.sql` | Extensión de schema: estados 5/6, índice, vista `IAP_VW_LOTES_PARA_FINALIZACION` |

### Frontend - UI

| Archivo | Descripción |
|---------|-------------|
| `UI/FrmFinalizarLotes.cs` | Formulario con grilla multiselect, barra de progreso, procesamiento asíncrono |
| `UI/FrmFinalizarLotes.Designer.cs` | Layout: título azul, instrucciones amarillas, controles de acción |
| `UI/FrmPrincipal.cs` | Integración en menú: "6. Finalizar Lotes" |

---

## 🔧 Configuración Previa

### 1. Ejecutar Script SQL

```sql
-- En SQL Server Management Studio, conectado a la base Capturador:
USE Capturador;
GO

-- Ejecutar:
12_FinalizarLotes.sql
```

**Verificar creación:**

```sql
-- Estados de lote:
SELECT * FROM IAP_TV_ESTADOS_LOTE WHERE cdEstadoLote IN (5, 6);

-- Vista:
SELECT * FROM IAP_VW_LOTES_PARA_FINALIZACION;
```

### 2. Verificar App.config

```xml
<appSettings>
  <add key="PATH_REPOSITORIO" value="C:\Repositorio\Planos" />
</appSettings>
```

---

## 🧪 Plan de Prueba

### Escenario 1: Flujo Completo Normal

**Datos de prueba:**

1. Lote con 3 archivos en estado "Pendiente de Finalizar" (cdEstadoLote = 5)
2. Archivos con metadatos completos:
   - TipoPlano: "OBRA"
   - Expediente: "EXP-2024-001"
   - Nomenclatura: 29A / 123 / 45
   - Dirección: "Calle Falsa 123"

**Pasos:**

1. Abrir aplicación → Menú "Procesos" → "6. Finalizar Lotes"
2. Verificar que aparece el lote en la grilla
3. Seleccionar el checkbox del lote
4. Click en "Finalizar Lotes Seleccionados"
5. Confirmar el diálogo de advertencia

**Resultado esperado:**

✅ Archivos renombrados:
```
C:\Repositorio\Planos\<CarpetaLote>\
  ├── EXP-2024-001_OBRA_Calle Falsa 123_29A_123_45.pdf
  ├── EXP-2024-001_OBRA_Calle Falsa 123_29A_123_46.pdf
  ├── EXP-2024-001_OBRA_Calle Falsa 123_29A_123_47.pdf
  └── INDEX.csv
```

✅ INDEX.csv contiene:
```csv
TipoPlano,Expediente,Seccion,Manzana,Parcela,Direccion,NombreArchivo
OBRA,EXP-2024-001,29A,123,45,Calle Falsa 123,EXP-2024-001_OBRA_Calle Falsa 123_29A_123_45.pdf
OBRA,EXP-2024-001,29A,123,46,Calle Falsa 123,EXP-2024-001_OBRA_Calle Falsa 123_29A_123_46.pdf
OBRA,EXP-2024-001,29A,123,47,Calle Falsa 123,EXP-2024-001_OBRA_Calle Falsa 123_29A_123_47.pdf
```

✅ Estado del lote: "Finalizado" (cdEstadoLote = 6)

✅ Mensaje de éxito: "Lotes procesados correctamente: 1"

---

### Escenario 2: Archivos Sin Expediente

**Datos de prueba:**

- Lote con archivos sin `dsExpediente`
- Resto de metadatos completos

**Resultado esperado:**

✅ Archivos renombrados sin prefijo de expediente:
```
OBRA_Calle Falsa 123_29A_123_45.pdf
OBRA_Calle Falsa 123_29A_123_46.pdf
```

✅ INDEX.csv con columna `Expediente` vacía

---

### Escenario 3: Caracteres Especiales en Datos

**Datos de prueba:**

- Dirección: `Av. San Martín 12/A - 3° Piso`
- Expediente: `EXP/2024/001`

**Resultado esperado:**

✅ Caracteres especiales eliminados (no reemplazados):
```
EXP2024001_OBRA_Av San Martín 12A  3 Piso_29A_123_45.pdf
```

---

### Escenario 4: Nombres Duplicados

**Datos de prueba:**

- 2 archivos con misma nomenclatura (29A / 123 / 45)
- Mismo tipo y dirección

**Resultado esperado:**

✅ Sufijo numérico añadido al segundo archivo:
```
EXP-2024-001_OBRA_Calle Falsa 123_29A_123_45.pdf
EXP-2024-001_OBRA_Calle Falsa 123_29A_123_45_(1).pdf
```

---

### Escenario 5: Carátula Ilegible

**Datos de prueba:**

- Archivo con `dsEstadoArchivoLote` = "Carátula Ilegible"

**Resultado esperado:**

✅ Archivo movido a subcarpeta:
```
C:\Repositorio\Planos\<CarpetaLote>\ILEGIBLE\
  └── CARATULA_ILEGIBLE.pdf
```

✅ **NO** se incluye en INDEX.csv

---

### Escenario 6: Archivo PDF Abierto

**Datos de prueba:**

1. Abrir uno de los PDFs del lote con Adobe Reader (dejar bloqueado)
2. Intentar finalizar el lote

**Resultado esperado:**

❌ Error: "El archivo está en uso por otro proceso"

✅ Rollback automático: ningún archivo movido

✅ Estado del lote permanece en "Pendiente de Finalizar"

✅ Mensaje de error detallado en el resumen

---

### Escenario 7: INDEX.csv Existente

**Datos de prueba:**

1. Finalizar un lote → genera INDEX.csv
2. Agregar otro lote en la misma carpeta
3. Finalizar el segundo lote

**Resultado esperado:**

✅ INDEX.csv actualizado (append) sin perder encabezados:
```csv
TipoPlano,Expediente,Seccion,Manzana,Parcela,Direccion,NombreArchivo
OBRA,EXP-001,29A,123,45,Calle A,EXP-001_OBRA_Calle A_29A_123_45.pdf
MENSURA,EXP-002,30B,456,78,Calle B,EXP-002_MENSURA_Calle B_30B_456_78.pdf
```

---

### Escenario 8: Múltiples Lotes Simultáneos

**Datos de prueba:**

- Seleccionar 3 lotes con checkbox
- Click en "Finalizar Lotes Seleccionados"

**Resultado esperado:**

✅ Barra de progreso: "Procesando lote X (1/3)..."

✅ Si un lote falla, los otros continúan

✅ Resumen final:
```
Lotes procesados correctamente: 2
Lotes con error: 1

Errores:
Lote_002: El archivo está en uso por otro proceso
```

---

## 🐛 Troubleshooting

### Error: "PATH_REPOSITORIO no configurado"

**Causa:** Falta la key en App.config

**Solución:**
```xml
<add key="PATH_REPOSITORIO" value="C:\Repositorio\Planos" />
```

---

### Error: "El lote no tiene archivos asociados"

**Causa:** No hay registros en `IAP_TD_LOTE_ARCHIVOS` con `snActivo = 1`

**Solución:** Verificar en SQL:
```sql
SELECT * FROM IAP_TD_LOTE_ARCHIVOS WHERE cdLote = <ID> AND snActivo = 1;
```

---

### Archivos no se renombran

**Causa:** Ruta incorrecta en `IAP_TD_ARCHIVOS.dsRutaCompleta`

**Verificación:**
```sql
SELECT dsRutaCompleta + dsNombreArchivo AS RutaFisica
FROM IAP_TD_ARCHIVOS
WHERE cdArchivo IN (
  SELECT cdArchivo FROM IAP_TD_LOTE_ARCHIVOS WHERE cdLote = <ID>
);
```

**Validar** que las rutas existen físicamente en el servidor.

---

### INDEX.csv con caracteres raros (ñ, á, etc.)

**Causa:** Encoding UTF-8

**Comportamiento esperado:** Es correcto. Los caracteres especiales se codifican en UTF-8 con BOM.

**Para abrir correctamente:**
- Excel → Datos → Desde texto/CSV → Detectar automáticamente UTF-8

---

## 📊 Validación Post-Finalización

### SQL - Estado del lote

```sql
SELECT 
	l.cdLote,
	l.dsNombreLote,
	el.dsEstado,
	l.feUltimaModificacion,
	l.cdUsuarioModificacion
FROM IAP_TD_LOTES l
INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
WHERE l.cdLote = <ID>;
```

**Esperado:** `dsEstado = 'Finalizado'`, `feUltimaModificacion` actualizado

---

### SQL - Auditoría de archivos

```sql
SELECT 
	a.dsNombreArchivo AS NombreOriginal,
	la.cdEstadoArchivoLote,
	ea.dsEstado
FROM IAP_TD_LOTE_ARCHIVOS la
INNER JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
INNER JOIN IAP_TV_ESTADOS_ARCHIVO_LOTE ea ON la.cdEstadoArchivoLote = ea.cdEstadoArchivoLote
WHERE la.cdLote = <ID>;
```

---

### Filesystem - Archivos físicos

**PowerShell:**
```powershell
# Listar archivos renombrados
Get-ChildItem "C:\Repositorio\Planos\<CarpetaLote>" -Filter *.pdf | Format-Table Name

# Verificar INDEX.csv
Get-Content "C:\Repositorio\Planos\<CarpetaLote>\INDEX.csv"
```

---

## 🔒 Seguridad y Rollback

### Mecanismo de Rollback

Si **cualquier** error ocurre durante el renombrado:

1. Se detiene el procesamiento
2. Todos los archivos ya movidos se revierten a sus nombres originales
3. No se genera INDEX.csv
4. Estado del lote permanece en "Pendiente de Finalizar"
5. Se registra error en log

### Logs

Ubicación: `C:\Logs\IndexadorAutomaticoPlanos\`

**Buscar errores:**
```powershell
Select-String -Path "C:\Logs\IndexadorAutomaticoPlanos\*.log" `
			  -Pattern "FinalizadorLotes|ERROR" `
			  -Context 2,5 | Out-GridView
```

---

## 📝 Nomenclatura de Archivos

### Formato Completo (con expediente)

```
<EXPEDIENTE>_<TIPO>_<DIRECCION>_<SECCION>_<MANZANA>_<PARCELA>.pdf
```

**Ejemplo:**
```
EXP-2024-001_OBRA_Av Libertador 456_29A_123_45.pdf
```

---

### Formato Sin Expediente

```
<TIPO>_<DIRECCION>_<SECCION>_<MANZANA>_<PARCELA>.pdf
```

**Ejemplo:**
```
MENSURA_Calle San Martín 789_30B_456_78.pdf
```

---

### Formato Ilegible

```
ILEGIBLE/CARATULA_ILEGIBLE.pdf
```

> **Nota:** Todos los archivos ilegibles se agrupan en la subcarpeta `ILEGIBLE/` con el mismo nombre.

---

## ✅ Checklist Final

Antes de usar en producción:

- [ ] Script SQL 12 ejecutado en BD de producción
- [ ] PATH_REPOSITORIO configurado en App.config
- [ ] Permisos de escritura en carpeta de repositorio
- [ ] Prueba de escenario 1 (flujo normal) exitosa
- [ ] Prueba de escenario 6 (rollback) exitosa
- [ ] Logs funcionando correctamente
- [ ] Backup de base de datos antes de primera finalización masiva

---

## 📞 Soporte

**Logs de aplicación:**
- `C:\Logs\IndexadorAutomaticoPlanos\app_<fecha>.log`

**Errores comunes:**
- Archivo en uso → Cerrar PDFs abiertos
- Ruta no encontrada → Verificar PATH_REPOSITORIO y rutas en BD
- Caracteres especiales → Comportamiento esperado (se eliminan)

---

**Implementación completada:** 2026-06-10
**Versión:** 1.0
