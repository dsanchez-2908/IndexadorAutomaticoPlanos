# Diagnóstico del Problema de Login

## Situación Actual
El hash BCrypt está correcto en la base de datos, pero el login aún falla.

## Cambios Realizados para Diagnóstico

He agregado logging detallado en:
1. **UsuarioRepository.Autenticar()** - Muestra cada paso de la autenticación
2. **Encriptacion.VerificarClave()** - Muestra la comparación BCrypt

## Instrucciones para Diagnosticar

### Paso 1: Ejecutar la Aplicación
1. Presiona **F5** en Visual Studio
2. Cuando aparezca el formulario de login, ingresa:
   - Usuario: `admin`
   - Contraseña: `123`
3. Haz clic en **Ingresar**

### Paso 2: Revisar los Logs

#### A) Log de Archivo
Abre el archivo de log más reciente en:
```
C:\Users\danie\source\repos\IndexadorAutomaticoPlanos\IndexadorAutomaticoPlanos\bin\Debug\net10.0-windows\Logs\
```

Busca las líneas que contengan:
- `Intentando autenticar usuario`
- `Hash almacenado`
- `Contraseña ingresada`
- `Resultado verificación`

#### B) Output de Visual Studio
En Visual Studio, ve a **Ver → Salida** (o Ctrl+Alt+O)
Busca las líneas que digan `VerificarClave`

### Paso 3: Compartir la Información

Copia y pega EXACTAMENTE las siguientes líneas del log:
```
Hash almacenado: [el hash que aparezca]
Contraseña ingresada: [la contraseña que aparezca]
Resultado verificación: [True o False]
```

También comparte la salida del Debug que dice:
```
VerificarClave - Clave: '...'
VerificarClave - Hash: '...'
VerificarClave - Resultado: ...
```

## Posibles Causas

### 1. Espacios en blanco
Si el hash almacenado tiene espacios al principio o al final, BCrypt fallará.

### 2. Codificación de caracteres
Si hay algún problema con la codificación al leer de la base de datos.

### 3. Versión de BCrypt
Hay diferentes versiones de BCrypt (2a, 2b, 2y). El hash usa $2a$ que debería funcionar con BCrypt.Net-Next.

### 4. Hash truncado
Si el campo dsClave en la base de datos es VARCHAR(100) y el hash completo tiene más caracteres, podría estar truncado.

## Verificaciones Adicionales

### Verificar longitud del campo en BD
```sql
USE Capturador;
SELECT 
	COLUMN_NAME, 
	DATA_TYPE, 
	CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'IAP_TD_USUARIOS' 
AND COLUMN_NAME = 'dsClave';
```

### Verificar contenido exacto sin espacios
```sql
USE Capturador;
SELECT 
	dsUsuario,
	LEN(dsClave) as LongitudHash,
	DATALENGTH(dsClave) as BytesHash,
	dsClave
FROM IAP_TD_USUARIOS 
WHERE dsUsuario = 'admin';
```

Un hash BCrypt completo debe tener exactamente **60 caracteres**.

## Una Vez que Tengamos los Logs

Sabré exactamente qué está pasando y podré corregirlo.
