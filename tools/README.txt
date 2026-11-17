================================================================================
ACTR DECRYPTION TOOLS
================================================================================

This directory contains tools for decrypting the ACTR.dll container.

================================================================================
FILES IN THIS DIRECTORY
================================================================================

ACTRDecryptor.cs                Source code (C#)
AppDllPassManaged.cs            C# port of AppDllPass.dll (no native dependency)
ACTRDecryptor.csproj            Project file (builds win-x64 / amd64)
ACTRDecryptor.exe               Pre-built x64 tool (self-contained single file,
                                no .NET installation needed)

================================================================================
USAGE
================================================================================

Basic Usage:
------------

To decrypt ACTR.dll:

    ACTRDecryptor.exe decode "C:\Program Files (x86)\USART HMI\ACTR.dll"

Output will be in: C:\Program Files (x86)\USART HMI\decrypted\


Automated Script:
-----------------

Or use the automated script in ..\scripts\:

    cd ..\scripts
    decrypt_actr.bat

================================================================================
OUTPUT
================================================================================

The decryptor extracts 21 DLL files to C:\Program Files (x86)\USART HMI\decrypted\:

  1. achmiface.dll              (666 KB)
  2. AxInterop.WMPLib.dll       (61 KB)
  3. ColListBox.dll             (23 KB)
  4. Colpanel.dll               (11 KB)
  5. ControlInterFace.dll       (16 KB)
  6. DevComponents.DotNetBar2.dll (5.5 MB)
  7. hmiapp_old.dll             (168 KB)
  8. HMIFORM.dll                (1.5 MB)
  9. hmioldapp.dll              (318 KB)
 10. hmitype.dll                (1.2 MB) ← MODIFY THIS ONE
 11. Interop.WMPLib.dll         (339 KB)
 12. Microsoft.mshtml.dll       (8 MB)
 13. msvcm90d.dll               (311 KB)
 14. msvcp90d.dll               (868 KB)
 15. msvcr90d.dll               (1.2 MB)
 16. Newtonsoft.Json.dll        (475 KB)
 17. PicListBox.dll             (19 KB)
 18. rsapp.dll                  (356 KB)
 19. Tcode.dll                  (273 KB)
 20. TFTEDIT.dll                (184 KB)
 21. TFTRUN.dll                 (22 KB)

TOTAL: ~23 MB

================================================================================
TECHNICAL DETAILS
================================================================================

What It Does:
-------------

1. Reads ACTR.dll (9.8 MB encrypted container)
2. Parses 64-byte headers for each assembly
3. XOR decrypts headers with key 0x09
4. Extracts assembly name, size, compression flag
5. Reads encrypted payload
6. Decrypts using AppDllPassManaged.Decode() (C# port of AppDllPass_Decode)
7. Decompresses GZIP data (if compression flag = 1)
8. Writes decrypted DLL to C:\Program Files (x86)\USART HMI\decrypted\

Encryption:
-----------

- Algorithm: custom add/xor/shift cipher on 32-bit words, 3-word state machine
- Key: constant string embedded in AppDllPass.dll (data decryption does NOT
  depend on the time; the original DLL only wrote a time-based anti-tamper
  value into the caller's key buffer, which is not needed)
- Trailing length % 4 bytes are left unencrypted

Compression:
------------

- Format: GZIP (RFC 1952)
- Signature: 0x1F 0x8B 0x08
- All 21 files are compressed in ACTR.dll

Container Format:
-----------------

[Header 1: 64 bytes XOR 0x09]
  Format: "AssemblyName.dll#Size#CompressionFlag"
[Payload 1: encrypted + compressed]

[Header 2: 64 bytes XOR 0x09]
[Payload 2: encrypted + compressed]

...

[Header 21: 64 bytes XOR 0x09]
[Payload 21: encrypted + compressed]

================================================================================
REQUIREMENTS
================================================================================

- Windows x64
- Pre-built exe: nothing else (the .NET runtime is embedded)
- Own build: .NET 9.0 Runtime (x64)
- ACTR.dll file (from USART HMI installation)

================================================================================
REBUILDING FROM SOURCE
================================================================================

If you need to modify the decryptor:

1. Edit ACTRDecryptor.cs
2. Open command prompt
3. Run:

   dotnet build ACTRDecryptor.csproj -c Release

4. Output:

   bin\Release\net9.0-windows\win-x64\ACTRDecryptor.exe

5. Copy to this directory

================================================================================
TROUBLESHOOTING
================================================================================

Window opens and closes immediately
-----------------------------------
Use the current exe: double-clicking it keeps the window open and finds
ACTR.dll automatically. Or run it from a command prompt / decrypt_actr.bat.


Error: "Could not load AppDllPass.dll" / "BadImageFormatException"
------------------------------------------------------------------
You are running an old 32-bit exe. Use the x64 exe from this folder.
It does not use AppDllPass.dll.


Error: "ACTR.dll not found"
---------------------------
Solution: Provide correct path to ACTR.dll:

  ACTRDecryptor.exe decode "C:\path\to\ACTR.dll"


Error: "Decompression failed"
-----------------------------
This is normal - the tool now skips decompression by default.
Decompression is handled automatically in the latest version.

================================================================================
COMMAND-LINE OPTIONS
================================================================================

Decode Mode (decrypt ACTR.dll):
-------------------------------

  ACTRDecryptor.exe decode <path_to_ACTR.dll>

  Example:
    ACTRDecryptor.exe decode "C:\Program Files (x86)\USART HMI\ACTR.dll"

  Output:
    C:\Program Files (x86)\USART HMI\decrypted\*.dll (21 files)


Encode Mode (re-encrypt DLLs):
------------------------------

  ACTRDecryptor.exe encode <path_to_directory>

  Example:
    ACTRDecryptor.exe encode C:\Program Files (x86)\USART HMI\decrypted\

  Output:
    ACTR_translated.dll (in parent directory)

  Note: Encoding is NOT recommended. Use filesystem loading instead.

================================================================================
NOTES
================================================================================

- The decryption process is deterministic (same input = same output)
- Data decryption is deterministic and does not depend on the date/time
- Output files are standard .NET assemblies (can be opened in dnSpy)
- Original ACTR.dll is never modified (read-only operation)
- The tool creates the output folder if it doesn't exist

================================================================================
PERFORMANCE
================================================================================

Decryption speed: ~1-2 seconds for all 21 files
Bottleneck: File I/O and GZIP decompression

Largest files (by compressed size):
  - DevComponents.DotNetBar2.dll: 2.6 MB compressed
  - Microsoft.mshtml.dll: 2.5 MB compressed
  - HMIFORM.dll: 1.7 MB compressed

================================================================================
SECURITY
================================================================================

This tool is for analysis and educational purposes only.

The decryption algorithm is custom and specific to USART HMI.
Source code is provided for transparency.

DO NOT use this tool to:
  - Pirate software
  - Circumvent licensing
  - Distribute decrypted files

ONLY use for:
  - Personal translation/localization
  - Bug analysis
  - Educational purposes

================================================================================
END OF README
================================================================================
