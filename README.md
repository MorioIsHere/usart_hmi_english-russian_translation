# USART HMI English Translation Guide

[English](#english) | [Español](#español)

---

## English

### 🎯 What is this?

This repository provides a complete solution to translate the **USART HMI** application from Chinese to English.

### 📖 Background Story

I purchased 3 TJC brand HMI display screens from AliExpress while living in Argentina. These displays can **only** be programmed using the USART HMI application, which is exclusively available in Chinese with no official way to change the language. After searching across multiple forums and communities, I found many people facing the same problem. This repository is my solution to help everyone in the same situation.

**Supported Version:** USART HMI v1.67.5 (32-bit)

### ⚠️ Important Notes

- **Platform:** Windows x64 (amd64). The decryption tool is now pure C# and builds as 64-bit (`win-x64`)
- **Target Application:** USART HMI v1.67.5 (the application itself remains 32-bit; the modified `hmitype.dll` is a .NET assembly and is not affected)
- This is a translation/localization tool, not for piracy or license circumvention
- Personal and educational use only

### 🚀 Quick Start (15 Minutes)

#### Prerequisites

- Windows x64
- USART HMI v1.67.5 installed at `C:\Program Files (x86)\USART HMI\`
- Administrator privileges
- [dnSpy](https://github.com/dnSpy/dnSpy/releases) - Free .NET assembly editor
- .NET 9.0 SDK (to build the x64 decryption tool)

#### Step 1: Decrypt ACTR.dll

The USART HMI application stores its UI assemblies in an encrypted container (`ACTR.dll`). We need to decrypt it first.

```batch
cd scripts
decrypt_actr.bat
```

This will:
- Extract 21 encrypted DLLs from `C:\Program Files (x86)\USART HMI\ACTR.dll`
- Save them to `E:\decrypted\`
- Take approximately 1-2 minutes

**Expected output:**
```
Decrypting ACTR.dll...
Successfully extracted 21 assemblies to E:\decrypted\
```

#### Step 2: Modify hmitype.dll

This is the critical step where we inject the translation logic.

1. **Open dnSpy (x86 version)**
2. **Load the DLL:**
   - File → Open → `E:\decrypted\hmitype.dll`
3. **Find the Language method:**
   - Press `Ctrl+Shift+K` (Search)
   - Type "Language" and search
   - Find the method with signature: `string Language(this string)`
   - Method Token: `0x060005FB`
4. **Edit the method:**
   - Right-click on the method → "Edit Method (C#)..."
   - Replace the entire method body with the code from `docs/LANGUAGE_METHOD.md`
   - Click "Compile" (verify no errors)
5. **Save:**
   - File → Save Module (Ctrl+S)

**Complete step-by-step guide with screenshots:** See `docs/DNSPY_GUIDE.md`

#### Step 3: Install Modified Files

**⚠️ Run as Administrator**

```batch
cd scripts
install_dlls.bat
```

This script will:
- Create backup of original files at `C:\Program Files (x86)\USART HMI\backup_original\`
- Copy all 21 decrypted DLLs to the installation directory
- Create `c:\devel\` folder
- Copy `hmi_translation.txt` with 36 pre-configured translations
- Set up `hmi.log` for tracking untranslated text

#### Step 4: Verify Installation

```batch
cd scripts
verify_installation.bat
```

Expected output:
```
✓ All 21 DLLs found
✓ Translation file exists
✓ Log directory ready
Installation verified successfully!
```

#### Step 5: Launch USART HMI

Start the application normally. The UI should now display English text for all translated items.

### 📂 Repository Structure

```
/
├── README.md                    # Complete technical documentation
├── GITHUB_README.md             # This file (user guide)
├── QUICKSTART.md                # 15-minute setup guide
├── INDEX.md                     # Documentation navigation
├── tools/
│   ├── ACTRDecryptor.exe        # Legacy 32-bit pre-built tool (needs AppDllPass.dll)
│   ├── ACTRDecryptor.cs         # Source code
│   ├── AppDllPassManaged.cs     # C# port of AppDllPass.dll (x64-ready)
│   ├── ACTRDecryptor.csproj     # .NET 9.0 project (win-x64)
│   ├── AppDllPass.dll           # Legacy native 32-bit crypto library (no longer needed by the x64 build)
│   └── README.txt               # Tool documentation
├── scripts/
│   ├── decrypt_actr.bat         # Automated decryption
│   ├── install_dlls.bat         # Install all files (requires admin)
│   └── verify_installation.bat  # Verify setup
├── translation/
│   └── hmi_translation.txt      # 36 pre-configured translations
└── docs/
    ├── DNSPY_GUIDE.md           # Step-by-step dnSpy modification guide
    └── LANGUAGE_METHOD.md       # Complete Language() method source
```

### 🔧 How It Works

The USART HMI application uses a three-tier encrypted assembly loading system:

```
USART HMI.exe
  └─> ApplicationRUN.dll (loader)
      └─> ACTR.dll (encrypted container with 21 DLLs)
          ├─> hmitype.dll ← Our modification target
          ├─> achmiface.dll
          └─> [19 more DLLs]
```

**Our approach:**

1. **Decrypt** ACTR.dll using the included tool (exploits the native `AppDllPass.dll` crypto library)
2. **Modify** `hmitype.dll` to intercept all UI text through the `Language()` extension method
3. **Bypass** encryption by placing decrypted DLLs directly in the installation folder
4. **Translate** using a simple text file (`hmi_translation.txt`)

The modified `Language()` method:
- Reads `c:\devel\hmi_translation.txt` (tab-separated Chinese→English mappings)
- Returns English translation if found
- Logs untranslated Chinese text to `c:\devel\hmi.log`
- Falls back to original behavior if translation fails

### 📝 Adding New Translations

As you use the application, untranslated Chinese text will be logged to `c:\devel\hmi.log`.

**To add translations:**

1. Check the log file:
   ```
   type c:\devel\hmi.log
   ```

2. Edit `c:\devel\hmi_translation.txt`:
   ```
   Chinese Text<TAB>English Translation
   ```

3. **Important:** Use TAB character (not spaces) as separator

4. Restart USART HMI - changes take effect immediately

**Example:**
```
指令集	Instruction Set
资料中心	Resource Center
按下事件	Press Event
弹起事件	Release Event
```

### 🐛 Troubleshooting

#### Application Won't Start
**Error:** `System.BadImageFormatException: Could not load file or assembly 'hmioldapp'`

**Solution:** Copy ALL 21 DLLs, not just the main ones. Run `scripts/install_dlls.bat` again.

#### Translations Not Working
1. Verify `c:\devel\hmi_translation.txt` exists
2. Check file encoding is UTF-8 (not ANSI)
3. Verify TAB separator (not spaces)
4. Confirm hmitype.dll was modified and saved correctly in dnSpy

#### Log File Not Created
1. Ensure `c:\devel\` folder exists: `mkdir c:\devel`
2. Verify translation file exists (log only writes when file exists but text not found)
3. Check application has write permissions

#### dnSpy Compilation Errors
**Error:** `The name 'Encoding' does not exist in the current context`

**Solution:** Add `using System.Text;` at the top of the file before compiling.

### 🤝 Contributing

**Found untranslated Chinese text?** Please report it!

1. Open an Issue on GitHub
2. Include the Chinese text from `c:\devel\hmi.log`
3. Suggest an English translation (optional)

I'll add it to the translation file and update the repository.

**Need the USART HMI v1.67.5 installer?**

If the application is no longer available from official sources, open an Issue and I'll provide a download link.

### 🛠️ Rebuilding ACTRDecryptor (Optional)

If you want to rebuild the decryption tool from source:

**Prerequisites:**
- .NET 9.0 SDK

**Build:**
```batch
cd tools
dotnet build ACTRDecryptor.csproj -c Release
```

**Output:** `tools\bin\Release\net9.0-windows\win-x64\ACTRDecryptor.exe`

**Note:** The encryption from the native 32-bit `AppDllPass.dll` has been ported to C# (`AppDllPassManaged.cs`, verified byte-for-byte against the original DLL), so the tool builds and runs as 64-bit without any native dependency. `decrypt_actr.bat` uses this x64 build automatically when it exists.

### 📜 License

This project is provided as-is for personal and educational use. Not for commercial redistribution.

**USART HMI** is property of its respective owners. This repository only provides translation tools, not the application itself.

### 💬 Questions?

Open an Issue on GitHub and I'll respond as soon as possible!

---

## Español

### 🎯 ¿Qué es esto?

Este repositorio proporciona una solución completa para traducir la aplicación **USART HMI** del chino al inglés.

### 📖 Historia

Compré 3 pantallas HMI de la marca TJC por AliExpress desde Argentina. Estas pantallas **solo** se pueden programar usando la aplicación USART HMI, que está disponible exclusivamente en chino sin forma oficial de cambiar el idioma. Después de buscar en múltiples foros y comunidades, encontré muchas personas enfrentando el mismo problema. Este repositorio es mi solución para ayudar a todos los que están en la misma situación.

**Versión Soportada:** USART HMI v1.67.5 (32-bit)

### ⚠️ Notas Importantes

- **Plataforma:** Windows x64 (amd64). La herramienta de desencriptación ahora es C# puro y se compila como 64-bit (`win-x64`)
- **Aplicación Objetivo:** USART HMI v1.67.5 (la aplicación sigue siendo de 32-bit; el `hmitype.dll` modificado es un ensamblado .NET y no se ve afectado)
- Esta es una herramienta de traducción/localización, no para piratería o elusión de licencias
- Solo para uso personal y educativo

### 🚀 Inicio Rápido (15 Minutos)

#### Requisitos Previos

- Windows x64
- USART HMI v1.67.5 instalado en `C:\Program Files (x86)\USART HMI\`
- Privilegios de Administrador
- [dnSpy](https://github.com/dnSpy/dnSpy/releases) - Editor gratuito de ensamblados .NET
- .NET 9.0 SDK (para compilar la herramienta x64)

#### Paso 1: Desencriptar ACTR.dll

La aplicación USART HMI almacena sus ensamblados de UI en un contenedor encriptado (`ACTR.dll`). Primero necesitamos desencriptarlo.

```batch
cd scripts
decrypt_actr.bat
```

Esto:
- Extraerá 21 DLLs encriptadas de `C:\Program Files (x86)\USART HMI\ACTR.dll`
- Las guardará en `E:\decrypted\`
- Tomará aproximadamente 1-2 minutos

**Salida esperada:**
```
Decrypting ACTR.dll...
Successfully extracted 21 assemblies to E:\decrypted\
```

#### Paso 2: Modificar hmitype.dll

Este es el paso crítico donde inyectamos la lógica de traducción.

1. **Abrir dnSpy (versión x86)**
2. **Cargar la DLL:**
   - File → Open → `E:\decrypted\hmitype.dll`
3. **Encontrar el método Language:**
   - Presionar `Ctrl+Shift+K` (Buscar)
   - Escribir "Language" y buscar
   - Encontrar el método con firma: `string Language(this string)`
   - Token del Método: `0x060005FB`
4. **Editar el método:**
   - Click derecho en el método → "Edit Method (C#)..."
   - Reemplazar todo el cuerpo del método con el código de `docs/LANGUAGE_METHOD.md`
   - Click "Compile" (verificar que no haya errores)
5. **Guardar:**
   - File → Save Module (Ctrl+S)

**Guía completa paso a paso con capturas:** Ver `docs/DNSPY_GUIDE.md`

#### Paso 3: Instalar Archivos Modificados

**⚠️ Ejecutar como Administrador**

```batch
cd scripts
install_dlls.bat
```

Este script:
- Creará backup de los archivos originales en `C:\Program Files (x86)\USART HMI\backup_original\`
- Copiará todas las 21 DLLs desencriptadas al directorio de instalación
- Creará la carpeta `c:\devel\`
- Copiará `hmi_translation.txt` con 36 traducciones pre-configuradas
- Configurará `hmi.log` para rastrear texto no traducido

#### Paso 4: Verificar Instalación

```batch
cd scripts
verify_installation.bat
```

Salida esperada:
```
✓ All 21 DLLs found
✓ Translation file exists
✓ Log directory ready
Installation verified successfully!
```

#### Paso 5: Iniciar USART HMI

Inicia la aplicación normalmente. La UI ahora debería mostrar texto en inglés para todos los elementos traducidos.

### 📂 Estructura del Repositorio

```
/
├── README.md                    # Documentación técnica completa
├── GITHUB_README.md             # Este archivo (guía de usuario)
├── QUICKSTART.md                # Guía de configuración de 15 minutos
├── INDEX.md                     # Navegación de documentación
├── tools/
│   ├── ACTRDecryptor.exe        # Herramienta 32-bit heredada (pre-compilada, requiere AppDllPass.dll)
│   ├── ACTRDecryptor.cs         # Código fuente
│   ├── AppDllPassManaged.cs     # Port en C# de AppDllPass.dll (listo para x64)
│   ├── ACTRDecryptor.csproj     # Proyecto .NET 9.0 (win-x64)
│   ├── AppDllPass.dll           # Biblioteca nativa 32-bit heredada (ya no la necesita la versión x64)
│   └── README.txt               # Documentación de herramientas
├── scripts/
│   ├── decrypt_actr.bat         # Desencriptación automatizada
│   ├── install_dlls.bat         # Instalar todos los archivos (requiere admin)
│   └── verify_installation.bat  # Verificar configuración
├── translation/
│   └── hmi_translation.txt      # 36 traducciones pre-configuradas
└── docs/
    ├── DNSPY_GUIDE.md           # Guía paso a paso de modificación con dnSpy
    └── LANGUAGE_METHOD.md       # Código fuente completo del método Language()
```

### 🔧 Cómo Funciona

La aplicación USART HMI usa un sistema de carga de ensamblados encriptados de tres niveles:

```
USART HMI.exe
  └─> ApplicationRUN.dll (cargador)
      └─> ACTR.dll (contenedor encriptado con 21 DLLs)
          ├─> hmitype.dll ← Nuestro objetivo de modificación
          ├─> achmiface.dll
          └─> [19 DLLs más]
```

**Nuestro enfoque:**

1. **Desencriptar** ACTR.dll usando la herramienta incluida (explota la biblioteca criptográfica nativa `AppDllPass.dll`)
2. **Modificar** `hmitype.dll` para interceptar todo el texto de UI a través del método de extensión `Language()`
3. **Bypasear** la encriptación colocando las DLLs desencriptadas directamente en la carpeta de instalación
4. **Traducir** usando un archivo de texto simple (`hmi_translation.txt`)

El método `Language()` modificado:
- Lee `c:\devel\hmi_translation.txt` (mapeos Chino→Inglés separados por TAB)
- Retorna la traducción al inglés si la encuentra
- Registra texto chino no traducido en `c:\devel\hmi.log`
- Vuelve al comportamiento original si la traducción falla

### 📝 Agregar Nuevas Traducciones

A medida que uses la aplicación, el texto chino no traducido se registrará en `c:\devel\hmi.log`.

**Para agregar traducciones:**

1. Revisar el archivo de log:
   ```
   type c:\devel\hmi.log
   ```

2. Editar `c:\devel\hmi_translation.txt`:
   ```
   Texto Chino<TAB>Traducción al Inglés
   ```

3. **Importante:** Usar carácter TAB (no espacios) como separador

4. Reiniciar USART HMI - los cambios surten efecto inmediatamente

**Ejemplo:**
```
指令集	Instruction Set
资料中心	Resource Center
按下事件	Press Event
弹起事件	Release Event
```

### 🐛 Solución de Problemas

#### La Aplicación No Inicia
**Error:** `System.BadImageFormatException: Could not load file or assembly 'hmioldapp'`

**Solución:** Copiar TODAS las 21 DLLs, no solo las principales. Ejecutar `scripts/install_dlls.bat` nuevamente.

#### Las Traducciones No Funcionan
1. Verificar que `c:\devel\hmi_translation.txt` existe
2. Verificar que la codificación del archivo es UTF-8 (no ANSI)
3. Verificar separador TAB (no espacios)
4. Confirmar que hmitype.dll fue modificada y guardada correctamente en dnSpy

#### El Archivo de Log No Se Crea
1. Asegurar que la carpeta `c:\devel\` existe: `mkdir c:\devel`
2. Verificar que el archivo de traducción existe (el log solo escribe cuando el archivo existe pero no se encuentra el texto)
3. Verificar que la aplicación tiene permisos de escritura

#### Errores de Compilación en dnSpy
**Error:** `The name 'Encoding' does not exist in the current context`

**Solución:** Agregar `using System.Text;` al inicio del archivo antes de compilar.

### 🤝 Contribuir

**¿Encontraste texto chino no traducido?** ¡Por favor repórtalo!

1. Abre un Issue en GitHub
2. Incluye el texto chino de `c:\devel\hmi.log`
3. Sugiere una traducción al inglés (opcional)

Lo agregaré al archivo de traducción y actualizaré el repositorio.

**¿Necesitas el instalador de USART HMI v1.67.5?**

Si la aplicación ya no está disponible desde fuentes oficiales, abre un Issue y proporcionaré un enlace de descarga.

### 🛠️ Recompilar ACTRDecryptor (Opcional)

Si quieres recompilar la herramienta de desencriptación desde el código fuente:

**Requisitos previos:**
- .NET 9.0 SDK

**Compilar:**
```batch
cd tools
dotnet build ACTRDecryptor.csproj -c Release
```

**Salida:** `tools\bin\Release\net9.0-windows\win-x64\ACTRDecryptor.exe`

**Nota:** El cifrado de la `AppDllPass.dll` nativa de 32-bit fue portado a C# (`AppDllPassManaged.cs`, verificado byte a byte contra la DLL original), por lo que la herramienta se compila y ejecuta en 64-bit sin dependencias nativas. `decrypt_actr.bat` usa automáticamente esta versión x64 si existe.

### 📜 Licencia

Este proyecto se proporciona tal cual para uso personal y educativo. No para redistribución comercial.

**USART HMI** es propiedad de sus respectivos dueños. Este repositorio solo proporciona herramientas de traducción, no la aplicación en sí.

### 💬 ¿Preguntas?

¡Abre un Issue en GitHub y responderé lo antes posible!

---

**Made with ❤️ by someone who just wanted to program their HMI displays**
