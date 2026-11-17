# USART HMI — перевод на русский и английский (x64)

[Русский](#русский) | [English](#english) | [Español](#español)

Форк проекта [audiobrian/usart_hmi_english_translation](https://github.com/audiobrian/usart_hmi_english_translation), переведённый на **Windows x64 (amd64)**, с исправленными скриптами и русским переводом интерфейса.

---

## Русский

### 🎯 Что это?

**USART HMI** — программа для разработки интерфейсов на HMI-дисплеях TJC / Nextion. Она существует только на китайском языке, и официального способа сменить язык нет.

Этот репозиторий позволяет перевести интерфейс программы на **русский** или **английский**: перевод берётся из обычного текстового файла, который можно дополнять самому.

### ✨ Что изменено в этом форке

| | Оригинал | Этот форк |
|---|---|---|
| Платформа инструмента | только x86 (32-bit) | **x64 (amd64)** |
| `AppDllPass.dll` | нужна (нативная 32-bit DLL) | **не нужна** — шифрование переписано на C# |
| Установка .NET | нужна | **не нужна** — exe самодостаточный |
| Запуск двойным щелчком | окно сразу закрывается | находит `ACTR.dll` сам, окно не закрывается |
| `.bat`-скрипты | падают на пути `Program Files (x86)` | исправлены, папки находятся автоматически |
| Язык перевода | английский | **русский или английский** на выбор при установке |
| Правка в dnSpy | замена всего метода (ошибки компиляции на других версиях) | вставка одного блока — работает на любой версии |

Шифрование из `AppDllPass.dll` (`AppDllPassManaged.cs`) проверено на совпадение с оригинальной DLL байт в байт.

### ⚠️ Важно

- **Система:** Windows x64
- **Программа:** USART HMI v1.67.5 (сама программа остаётся 32-битной — это нормально)
- Только для личного и учебного использования. Это инструмент локализации, а не взлом лицензии.

### 🚀 Быстрый старт

#### Что понадобится

- USART HMI, установленная в `C:\Program Files (x86)\USART HMI\`
- Права администратора
- [dnSpy](https://github.com/dnSpy/dnSpy/releases) — бесплатный редактор .NET-сборок (`dnSpy-net-win64.zip` или `win32`)

#### Шаг 1. Расшифровать ACTR.dll

Дважды щёлкните `tools\ACTRDecryptor.exe` (или запустите `scripts\decrypt_actr.bat`).

Программа сама найдёт `ACTR.dll` и сохранит расшифрованные DLL в папку:
- `C:\Program Files (x86)\USART HMI\decrypted\`, или
- `Документы\USART_HMI_decrypted\`, если нет прав на запись в Program Files.

Путь будет показан в конце. Если Windows SmartScreen предупредит о неизвестной программе — «Подробнее» → «Выполнить в любом случае».

#### Шаг 2. Изменить hmitype.dll в dnSpy

1. Запустите dnSpy, **File → Close All**, затем **File → Open** → `hmitype.dll` из папки `decrypted`.
2. `Ctrl+Shift+K` → в настройках поиска **Search For: Method**, в **Options** включите *Match Whole Words* и *Case Sensitive* → найдите `Language`.
   Нужный метод: `string Language(this string text)`, внутри есть `ExeLanguage` и `ACLang`.
3. Правой кнопкой по коду метода → **Edit Method (C#)...**
4. **Ничего не удаляйте.** Сразу после открывающей `{` метода вставьте блок из [`docs/LANGUAGE_METHOD.md`](docs/LANGUAGE_METHOD.md) (раздел *Recommended*).
5. **Compile** → **File → Save Module...** → **OK**.

Подробно: [`docs/DNSPY_GUIDE.md`](docs/DNSPY_GUIDE.md).

#### Шаг 3. Установить

1. Закройте USART HMI.
2. Правой кнопкой по `scripts\install_dlls.bat` → **Запуск от имени администратора**.
3. Выберите язык: **2 — русский** (или 1 — английский).

Скрипт сделает резервную копию оригинальных файлов в `USART HMI\backup_original\`, скопирует DLL и установит файл перевода в `C:\devel\hmi_translation.txt`.

#### Шаг 4. Запустить USART HMI

Запустите программу как обычно — интерфейс будет переведён. Проверить установку можно скриптом `scripts\verify_installation.bat`.

### 📝 Как дополнить перевод

Фразы без перевода записываются в `C:\devel\hmi.log`. Чтобы перевести их, добавьте строки в `C:\devel\hmi_translation.txt`:

```
китайский текст<TAB>перевод
```

- Разделитель — **табуляция**, не пробелы.
- Кодировка файла — **UTF-8**.
- После правки перезапустите USART HMI.

Пример:
```
指令集	Документация
资料中心	Официальный сайт
按下事件	Нажатие
弹起事件	Отпускание
```

Готовые переводы лежат в папке `translation/`: `hmi_translation_rus.txt` (русский) и `hmi_translation.txt` (английский).

### ↩️ Как вернуть всё обратно

Скопируйте все файлы из `C:\Program Files (x86)\USART HMI\backup_original\` обратно в `C:\Program Files (x86)\USART HMI\`.

### 🐛 Решение проблем

| Проблема | Решение |
|---|---|
| `ACTRDecryptor.exe` не запускается | SmartScreen: «Подробнее» → «Выполнить в любом случае». Проверьте карантин антивируса. |
| Скрипт «моргает» и закрывается | Используйте скрипты из этого форка — в оригинале они падают на пути `(x86)`. |
| В dnSpy много ошибок при Compile | Не удаляйте код в окне редактора, вставляйте только блок (шаг 2). |
| Не находится метод `Language` | Токен `0x060005FB` в вашей версии может отличаться — ищите по имени, см. шаг 2. |
| Программа не запускается после установки | Скопированы не все 21 DLL — запустите `install_dlls.bat` ещё раз или верните резервную копию. |
| Перевод не появляется | Проверьте, что существует `C:\devel\hmi_translation.txt` (UTF-8, табуляция) и что сохранён изменённый `hmitype.dll`. |

### 📂 Структура репозитория

```
├── README.md                    # Это описание
├── INDEX.md                     # Навигация по документации
├── tools/
│   ├── ACTRDecryptor.exe        # Готовый инструмент x64 (самодостаточный)
│   ├── ACTRDecryptor.cs         # Исходный код
│   ├── AppDllPassManaged.cs     # Шифрование AppDllPass.dll на C#
│   ├── ACTRDecryptor.csproj     # Проект .NET 9.0 (win-x64)
│   └── README.txt               # Описание инструмента
├── scripts/
│   ├── decrypt_actr.bat         # Расшифровка
│   ├── install_dlls.bat         # Установка (от администратора)
│   └── verify_installation.bat  # Проверка установки
├── translation/
│   ├── hmi_translation_rus.txt  # Русский перевод
│   └── hmi_translation.txt      # Английский перевод
└── docs/
    ├── DNSPY_GUIDE.md           # Пошаговая инструкция по dnSpy
    └── LANGUAGE_METHOD.md       # Код для вставки в Language()
```

### 🔧 Как это работает

```
USART HMI.exe
  └─> ApplicationRUN.dll (загрузчик)
      └─> ACTR.dll (зашифрованный контейнер с 21 DLL)
          ├─> hmitype.dll  ← сюда вставляется перевод
          ├─> achmiface.dll
          └─> ещё 19 DLL
```

1. **Расшифровка** `ACTR.dll` — собственная реализация шифрования USART HMI на C#.
2. **Изменение** метода `Language()` в `hmitype.dll`: через него проходит весь текст интерфейса.
3. **Подмена**: расшифрованные DLL кладутся в папку программы и загружаются вместо зашифрованных.
4. **Перевод** берётся из `C:\devel\hmi_translation.txt`, непереведённое пишется в `C:\devel\hmi.log`.

### 🛠️ Сборка инструмента из исходников (необязательно)

Нужен .NET 9.0 SDK:

```batch
cd tools
dotnet build ACTRDecryptor.csproj -c Release
```

Результат: `tools\bin\Release\net9.0-windows\win-x64\ACTRDecryptor.exe` (`decrypt_actr.bat` использует его автоматически).

Самодостаточный exe, как в репозитории:

```batch
dotnet publish ACTRDecryptor.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### 🙏 Авторы

- Оригинальный проект и английский перевод — [Brian Sternari (audiobrian)](https://github.com/audiobrian/usart_hmi_english_translation)
- Улучшения английского перевода — Ginobeano
- Русский перевод — j0#n
- Порт на x64, исправления скриптов и документации — этот форк

### 📜 Лицензия

Проект предоставляется «как есть» для личного и учебного использования, не для коммерческого распространения. **USART HMI** принадлежит её правообладателям; репозиторий содержит только инструменты перевода, но не саму программу.

---

## English

### 🎯 What is this?

**USART HMI** is the IDE for TJC / Nextion HMI displays. It is only available in Chinese, with no official way to change the language.

This repository translates the application's UI into **Russian** or **English**, using a plain text file you can extend yourself.

### ✨ What this fork changes

| | Original | This fork |
|---|---|---|
| Tool platform | x86 (32-bit) only | **x64 (amd64)** |
| `AppDllPass.dll` | required (native 32-bit DLL) | **not needed** — crypto ported to C# |
| .NET installation | required | **not needed** — self-contained exe |
| Double-click launch | window closes immediately | finds `ACTR.dll` itself, window stays open |
| `.bat` scripts | crash on `Program Files (x86)` paths | fixed, folders detected automatically |
| Translation language | English | **Russian or English**, chosen at install |
| dnSpy edit | replace whole method (compile errors on other builds) | insert one block — works with any build |

The C# port of `AppDllPass.dll` (`AppDllPassManaged.cs`) was verified byte-for-byte against the original DLL.

### ⚠️ Important

- **OS:** Windows x64
- **Application:** USART HMI v1.67.5 (the application itself stays 32-bit — that's fine)
- Personal and educational use only. This is a localization tool, not license circumvention.

### 🚀 Quick Start

#### Prerequisites

- USART HMI installed in `C:\Program Files (x86)\USART HMI\`
- Administrator rights
- [dnSpy](https://github.com/dnSpy/dnSpy/releases) — free .NET assembly editor

#### Step 1: Decrypt ACTR.dll

Double-click `tools\ACTRDecryptor.exe` (or run `scripts\decrypt_actr.bat`).

It finds `ACTR.dll` automatically and writes the decrypted DLLs to:
- `C:\Program Files (x86)\USART HMI\decrypted\`, or
- `Documents\USART_HMI_decrypted\` if Program Files is not writable.

If Windows SmartScreen warns about an unknown app: "More info" → "Run anyway".

#### Step 2: Modify hmitype.dll in dnSpy

1. Open dnSpy, **File → Close All**, then **File → Open** → `hmitype.dll` from the `decrypted` folder.
2. `Ctrl+Shift+K` → **Search For: Method**, in **Options** enable *Match Whole Words* and *Case Sensitive* → search `Language`.
   The target is `string Language(this string text)`, containing `ExeLanguage` and `ACLang`.
3. Right-click the method code → **Edit Method (C#)...**
4. **Do not delete anything.** Right after the method's opening `{`, insert the block from [`docs/LANGUAGE_METHOD.md`](docs/LANGUAGE_METHOD.md) (*Recommended* section).
5. **Compile** → **File → Save Module...** → **OK**.

Details: [`docs/DNSPY_GUIDE.md`](docs/DNSPY_GUIDE.md).

#### Step 3: Install

1. Close USART HMI.
2. Right-click `scripts\install_dlls.bat` → **Run as administrator**.
3. Choose the language: **1 — English** or **2 — Russian**.

The script backs up the original files to `USART HMI\backup_original\`, copies the DLLs and installs the translation file to `C:\devel\hmi_translation.txt`.

#### Step 4: Launch USART HMI

Start the application as usual. You can check the setup with `scripts\verify_installation.bat`.

### 📝 Adding translations

Untranslated text is logged to `C:\devel\hmi.log`. Add lines to `C:\devel\hmi_translation.txt`:

```
Chinese text<TAB>translation
```

Use a **TAB** separator and **UTF-8** encoding, then restart USART HMI. Ready-made files are in `translation/`: `hmi_translation.txt` (English) and `hmi_translation_rus.txt` (Russian).

### ↩️ Uninstall

Copy everything from `C:\Program Files (x86)\USART HMI\backup_original\` back to `C:\Program Files (x86)\USART HMI\`.

### 🐛 Troubleshooting

| Problem | Solution |
|---|---|
| `ACTRDecryptor.exe` won't start | SmartScreen: "More info" → "Run anyway". Check antivirus quarantine. |
| A script flashes and closes | Use the scripts from this fork — the original ones crash on `(x86)` paths. |
| Many dnSpy compile errors | Don't delete code in the editor window; insert only the block (step 2). |
| `Language` method not found by token | Token `0x060005FB` may differ in your build — search by name (step 2). |
| App won't start after install | Not all 21 DLLs were copied — rerun `install_dlls.bat` or restore the backup. |
| No translation | Check `C:\devel\hmi_translation.txt` exists (UTF-8, TAB) and the modified `hmitype.dll` was saved. |

### 🛠️ Building from source (optional)

Requires the .NET 9.0 SDK:

```batch
cd tools
dotnet build ACTRDecryptor.csproj -c Release
```

Output: `tools\bin\Release\net9.0-windows\win-x64\ACTRDecryptor.exe` (used automatically by `decrypt_actr.bat`). Self-contained single file, as shipped:

```batch
dotnet publish ACTRDecryptor.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### 🙏 Credits

- Original project and English translation — [Brian Sternari (audiobrian)](https://github.com/audiobrian/usart_hmi_english_translation)
- English translation improvements — Ginobeano
- Russian translation — j0#n
- x64 port, script and documentation fixes — this fork

### 📜 License

Provided as-is for personal and educational use; not for commercial redistribution. **USART HMI** is property of its respective owners. This repository only provides translation tools, not the application itself.

---

## Español

### 🎯 ¿Qué es esto?

**USART HMI** es el IDE para las pantallas HMI TJC / Nextion. Solo está disponible en chino, sin forma oficial de cambiar el idioma. Este repositorio traduce la interfaz al **ruso** o al **inglés** mediante un archivo de texto que puedes ampliar.

Este fork funciona en **Windows x64**: el cifrado de `AppDllPass.dll` fue portado a C# (verificado byte a byte), el exe es autónomo (no requiere instalar .NET) y los scripts `.bat` ya no fallan con rutas `Program Files (x86)`.

### 🚀 Inicio rápido

1. **Desencriptar:** doble clic en `tools\ACTRDecryptor.exe` (o `scripts\decrypt_actr.bat`). Los DLL se guardan en `C:\Program Files (x86)\USART HMI\decrypted\` o en `Documentos\USART_HMI_decrypted\`.
2. **Modificar `hmitype.dll` en [dnSpy](https://github.com/dnSpy/dnSpy/releases):** busca el método `Language` (Search For: Method, *Match Whole Words*), **Edit Method (C#)...**, y **sin borrar nada** inserta el bloque de [`docs/LANGUAGE_METHOD.md`](docs/LANGUAGE_METHOD.md) (sección *Recommended*) justo después de la `{` inicial. **Compile** → **File → Save Module**.
3. **Instalar:** cierra USART HMI, ejecuta `scripts\install_dlls.bat` como administrador y elige el idioma (1 — inglés, 2 — ruso).
4. **Iniciar** USART HMI normalmente.

Textos sin traducir se registran en `C:\devel\hmi.log`; agrégalos a `C:\devel\hmi_translation.txt` con el formato `texto chino<TAB>traducción` (UTF-8).

Para deshacer: copia el contenido de `USART HMI\backup_original\` de vuelta a la carpeta de la aplicación.

### 🙏 Créditos

Proyecto original — [Brian Sternari (audiobrian)](https://github.com/audiobrian/usart_hmi_english_translation); mejoras de la traducción inglesa — Ginobeano; traducción rusa — j0#n; port a x64 y correcciones — este fork.

### 📜 Licencia

Proporcionado tal cual para uso personal y educativo; no para redistribución comercial. **USART HMI** pertenece a sus respectivos propietarios.
