# FASE 6: Control de Calidad de Lotes Procesados por IA

## Descripción General

Esta fase implementa un sistema completo de **validación y control de calidad** para los lotes que han sido procesados por OpenAI. Permite a los usuarios revisar, corregir y validar los datos extraídos por la inteligencia artificial antes de finalizar el lote.

---

## Funcionalidades Principales

### 1. **Lista de Lotes Pendientes** (`FrmControlCalidad.cs`)

**Ubicación:** `Menú → Procesos → 5. Validación de Lotes`

#### Características:
- Lista todos los lotes en estado **"Pendiente de Control de Calidad"** (estado 3)
- Muestra estadísticas por lote: `Total / Controlados / Ilegibles / Pendientes`
- Doble clic o botón "Abrir Lote" para abrir el modal de control detallado
- Botón "Refrescar" para actualizar la lista
- Se cierra automáticamente al finalizar todos los lotes

---

### 2. **Control Detallado de Lote** (`FrmControlLote.cs`)

**Ventana modal maximizada** con layout complejo de 3 paneles:

#### **Panel Izquierdo: Lista de Archivos**
- Grid con todos los documentos del lote
- Columnas:
  - Estado del archivo
  - Porcentajes de confianza (Tipo, Exp, Sec, Manz, Parc)
  - Datos extraídos (Tipo Plano, Expediente, Sección, Manzana, Parcela, Modalidad)
- **Colores visuales:**
  - 🟢 Verde: Controlado
  - 🔴 Rojo: Carátula Ilegible
  - 🟡 Amarillo: Campos con confianza < 70%

#### **Filtros Avanzados:**
- Por Tipo de Plano
- Por Estado (Pendiente / Controlado / Ilegible)
- Solo documentos con campos faltantes
- Confianza menor a X%
- Botones: Aplicar Filtros / Limpiar Filtros

#### **Panel Derecho: Visor de Imagen**
- Muestra la imagen JPG del documento seleccionado
- **Zoom interactivo:**
  - TrackBar de 10% a 500%
  - Mouse wheel (scroll) para zoom rápido
- Ruta de la imagen mostrada en la parte inferior
- Imagen se libera correctamente al cambiar de documento

#### **Panel Inferior: Edición y Acciones**

**Sección de Edición:**
- **Información del archivo:** Nombre, Estado, Modalidad de procesamiento
- **Porcentajes de confianza** (solo lectura) en panel lateral
- **Campos editables:**
  - *Tipo de Plano (combo box) - **Obligatorio**
  - Expediente (text box)
  - *Sección (text box) - **Obligatorio**
  - *Manzana (text box) - **Obligatorio**
  - *Parcela (text box) - **Obligatorio**
- Los campos obligatorios sin completar se marcan en **rojo**
- Botón "Guardar Cambios" actualiza el resultado de IA en la base de datos

**Navegación:**
- Botones "◄ Anterior" / "Siguiente ►"
- **Navegación por teclado:** Flechas izquierda/derecha
- Contador: "Archivo X de Y"
- Detecta cambios pendientes y pregunta antes de cambiar de documento

**Acciones:**
- 🟢 **Marcar Controlado:** Valida que tenga todos los campos obligatorios y marca el archivo como "Controlado"
- 🔴 **Marcar Ilegible:** Marca el archivo como "Carátula Ilegible" (con confirmación)
- 🔵 **Finalizar Lote:** Cambia el lote al estado "Pendiente de Finalizar" (estado 4)
  - ⚠️ **Advertencia:** Si quedan archivos pendientes, muestra alerta con estadísticas completas
- ⚫ **Cerrar:** Cierra la ventana modal (detecta cambios sin guardar)

---

## Flujo de Trabajo Completo

```
┌───────────────────────────────────────────────────────┐
│ 1. Usuario abre "5. Validación de Lotes"             │
└───────────────────────────────┬───────────────────────┘
								▼
┌───────────────────────────────────────────────────────┐
│ 2. Se muestra lista de lotes en estado 3             │
│    (Pendiente de Control de Calidad)                 │
└───────────────────────────────┬───────────────────────┘
								▼
┌───────────────────────────────────────────────────────┐
│ 3. Usuario abre un lote (doble clic / botón)         │
└───────────────────────────────┬───────────────────────┘
								▼
┌───────────────────────────────────────────────────────┐
│ 4. Modal FrmControlLote carga archivos + imágenes    │
│    • Grid con documentos y estadísticas              │
│    • Filtros para priorizar revisión                 │
└───────────────────────────────┬───────────────────────┘
								▼
┌───────────────────────────────────────────────────────┐
│ 5. Usuario revisa documento a documento:             │
│    a) Visualiza imagen con zoom                       │
│    b) Compara datos extraídos vs. imagen             │
│    c) Corrige campos si es necesario                 │
│    d) Guarda cambios                                 │
│    e) Marca como Controlado / Ilegible               │
│    f) Navega al siguiente (teclado / botones)        │
└───────────────────────────────┬───────────────────────┘
								▼
┌───────────────────────────────────────────────────────┐
│ 6. Cuando todos los documentos están controlados:    │
│    • Finalizar Lote (pasa a estado 4)                │
└───────────────────────────────┬───────────────────────┘
								▼
┌───────────────────────────────────────────────────────┐
│ 7. Lote listo para fase de Finalización              │
└───────────────────────────────────────────────────────┘
```

---

## Cambios en Base de Datos

### Estados Agregados

1. **Estado de Lote:**
   - **ID 4:** `Pendiente de Finalizar` - Lote validado, listo para finalizar

2. **Estados de Archivo:**
   - `Controlado` - Archivo revisado y aprobado
   - `Carátula Ilegible` - Archivo con carátula ilegible

### Tabla Nueva: `IAP_TV_TIPOS_PLANO`

Catálogo de tipos de plano válidos:
- `cdTipoPlano` (PK, INT, IDENTITY)
- `dsTipoPlano` (VARCHAR(100))
- `dsDescripcion` (VARCHAR(500), nullable)
- `snActivo` (BIT, default 1)
- `feAlta` (DATETIME, default GETDATE())

**Tipos precargados:**
- Obra
- Mensura
- Instalaciones

### Vista: `VW_IAP_CONTROL_CALIDAD`

Vista consolidada que combina:
- Datos de lotes (`IAP_TD_LOTES`)
- Datos de archivos (`IAP_TD_LOTE_ARCHIVOS`)
- Resultados de IA (`IAP_TD_RESULTADOS_IA`)
- Estados de lotes y archivos
- Usuario de modificación

**Campos principales:**
- `CdLote`, `DsNombreLote`, `DsEstadoLote`
- `CdLoteArchivo`, `DsNombreArchivo`, `DsRutaImagenJpg`, `DsEstadoArchivoLote`
- `CdResultadoIA`, `DsTipoPlano`, `DsExpediente`, `DsSeccion`, `DsManzana`, `DsParcela`, `DsDireccion`
- `NuConfianzaTipoPlano`, `NuConfianzaExpediente`, `NuConfianzaSeccion`, `NuConfianzaManzana`, `NuConfianzaParcela`
- `DsModalidadProcesamiento` (Base64 / URL)
- `DsUsuarioModificacion`, `FeModificacion`

### Índices Agregados
- `IX_IAP_TD_LOTES_EstadoLote` en `IAP_TD_LOTES(cdEstadoLote)`
- `IX_IAP_TD_RESULTADOS_IA_Lote` en `IAP_TD_RESULTADOS_IA(cdLote)`

---

## Archivos Creados/Modificados

### Archivos Nuevos

1. **`Scripts/11_ExtenderControlCalidad.sql`**
   - Script de extensión de base de datos para FASE 6
   - Agrega estados, tabla de tipos, vista y índices

2. **`Entities/ArchivoConResultadoIA.cs`**
   - Entidad compuesta que combina archivo + resultado de IA
   - Helpers: `TieneCamposFaltantes()`, `ObtenerMenorConfianza()`

3. **`UI/FrmControlCalidad.cs`** y `.Designer.cs` / `.resx`
   - Formulario de lista de lotes pendientes
   - Grid con estadísticas por lote

4. **`UI/FrmControlLote.cs`** y `.Designer.cs` / `.resx`
   - Formulario modal complejo de control detallado
   - Layout de 3 paneles (lista, visor, edición)
   - Filtros, navegación, zoom, validación

### Archivos Modificados

1. **`Entities/TipoPlano.cs`**
   - Agregadas propiedades: `DsDescripcion`, `FeAlta`

2. **`DataAccess/LoteRepository.cs`**
   - Nueva región: `#region FASE 6: Control de Calidad`
   - Métodos agregados:
	 - `ObtenerLotesPendientesControlCalidad()`
	 - `ObtenerArchivosConResultadosIA(int cdLote)`
	 - `ActualizarResultadoIA(ResultadoIA resultado)`
	 - `MarcarArchivoComoControlado(int cdLoteArchivo, int cdUsuario)`
	 - `MarcarArchivoComoIlegible(int cdLoteArchivo, int cdUsuario)`
	 - `FinalizarControlLote(int cdLote, int cdUsuarioModificacion, out string mensaje)`
	 - `ObtenerEstadisticasLote(int cdLote)` → `(Total, Controlados, Ilegibles, Pendientes)`
	 - `ObtenerTiposPlano()`

3. **`UI/FrmPrincipal.cs`**
   - Agregado método: `menuControlCalidad_Click`
   - Abre `FrmControlCalidad` como formulario MDI hijo

4. **`UI/FrmPrincipal.Designer.cs`**
   - Conectado evento `Click` del ítem de menú `menuValidacionLotes`

---

## Configuración Requerida

### App.config (ya existente)
```xml
<add key="PATH_REPOSITORIO" value="C:\IndexadorPlanos\Repositorio\" />
```

Este path se usa para cargar las imágenes JPG en el visor.

---

## Validaciones Implementadas

1. **Campos Obligatorios:**
   - Tipo de Plano, Sección, Manzana, Parcela
   - Se marcan en rojo si están vacíos
   - No se permite marcar como "Controlado" si faltan

2. **Cambios Pendientes:**
   - Detecta ediciones no guardadas al navegar
   - Pregunta si desea guardar antes de cambiar de documento

3. **Finalización de Lote:**
   - Permite finalizar con archivos pendientes
   - Muestra advertencia con estadísticas detalladas
   - Requiere confirmación del usuario

4. **Carátula Ilegible:**
   - Requiere confirmación antes de marcar
   - No requiere campos obligatorios completos

---

## Casos de Uso

### Caso 1: Revisión Rápida (Todo Correcto)
1. Abrir lote → Revisar imagen del primer documento
2. Verificar que datos extraídos coincidan
3. Click "Marcar Controlado" → Avanza automáticamente al siguiente
4. Repetir hasta finalizar todos
5. Click "Finalizar Lote"

### Caso 2: Corrección de Datos
1. Abrir lote → Revisar documento con confianza baja (filtro < 70%)
2. Comparar imagen vs. datos extraídos
3. Corregir campos incorrectos
4. Click "Guardar Cambios"
5. Click "Marcar Controlado"

### Caso 3: Documento Ilegible
1. Abrir lote → Revisar documento
2. La imagen no permite identificar datos
3. Click "Marcar Ilegible" → Confirmar
4. El documento no requiere campos completos

### Caso 4: Filtros para Priorizar
1. Aplicar filtro "Solo campos faltantes"
2. Completar los documentos sin datos obligatorios
3. Aplicar filtro "Confianza < 70%"
4. Revisar y corregir los documentos con baja confianza
5. Limpiar filtros y revisar el resto

---

## Próximos Pasos (FASE 7)

Una vez que los lotes están en estado **"Pendiente de Finalizar"** (estado 4), la siguiente fase incluirá:

1. **Exportación de datos validados** a formato final (Excel, JSON, etc.)
2. **Movimiento de archivos** a carpeta definitiva
3. **Generación de reportes** de lote finalizado
4. **Notificaciones** al usuario que cargó el lote
5. **Cierre y archivado** del lote

---

## Notas Técnicas

### Liberación de Recursos
- Las imágenes se cargan desde `FileStream` y se disponen correctamente
- El `PictureBox` se limpia al cambiar de documento
- El formulario modal libera la imagen en `OnFormClosing`

### Performance
- Los índices agregados optimizan las consultas por estado de lote
- La vista `VW_IAP_CONTROL_CALIDAD` materializa los joins complejos
- La carga de archivos es paginada (solo el lote actual)

### Seguridad
- Todas las acciones registran el usuario de modificación (`CdUsuarioModificacion`)
- Se valida la sesión activa antes de cualquier operación
- Los cambios se persisten en transacciones implícitas

---

## Comandos de Prueba SQL

### Ver lotes pendientes de control de calidad:
```sql
SELECT * FROM IAP_TD_LOTES WHERE cdEstadoLote = 3;
```

### Ver datos consolidados de un lote:
```sql
SELECT * FROM VW_IAP_CONTROL_CALIDAD WHERE CdLote = @cdLote;
```

### Ver estadísticas de un lote:
```sql
SELECT 
	COUNT(*) AS Total,
	SUM(CASE WHEN cdEstadoArchivoLote = (SELECT cdEstadoArchivoLote FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Controlado') THEN 1 ELSE 0 END) AS Controlados,
	SUM(CASE WHEN cdEstadoArchivoLote = (SELECT cdEstadoArchivoLote FROM IAP_TV_ESTADOS_ARCHIVO_LOTE WHERE dsEstado = 'Carátula Ilegible') THEN 1 ELSE 0 END) AS Ilegibles
FROM IAP_TD_LOTE_ARCHIVOS
WHERE cdLote = @cdLote;
```

---

## Conclusión

**FASE 6** implementa un sistema robusto y completo de control de calidad para los datos procesados por inteligencia artificial, garantizando que los resultados sean validados por usuarios humanos antes de finalizar el lote. El diseño modular y la UI intuitiva permiten una revisión eficiente de cientos de documentos por lote.

---

**Autor:** Sistema de Indexación Automática de Planos  
**Fecha:** Junio 2026  
**Versión:** 1.0  
