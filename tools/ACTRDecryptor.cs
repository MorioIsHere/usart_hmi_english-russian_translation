using System;
using System.IO;

namespace ACTRDecryptor
{
    class Program
    {
        // El cifrado de AppDllPass.dll (nativa, solo 32-bit) está reimplementado en C#
        // en AppDllPassManaged.cs, por lo que la herramienta funciona en x64 (amd64).

        static readonly string[] DefaultActrPaths =
        {
            @"C:\Program Files (x86)\USART HMI\ACTR.dll",
            @"C:\Program Files\USART HMI\ACTR.dll",
        };

        static int Main(string[] args)
        {
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }

            // Sin argumentos = doble clic en el Explorador: no cerrar la ventana al terminar.
            bool interactive = args.Length == 0;
            int code;
            try
            {
                code = Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR: {ex.Message}");
                code = 1;
            }

            if (interactive)
            {
                Console.WriteLine("\nPresione Enter para salir / Press Enter to exit...");
                try { Console.ReadLine(); } catch { }
            }
            return code;
        }

        static int Run(string[] args)
        {
            Console.WriteLine("ACTR.dll Decryptor/Encryptor (x64)");
            Console.WriteLine("==================================\n");

            // Doble clic: buscar ACTR.dll automáticamente (o en la carpeta del exe)
            if (args.Length == 0)
            {
                string found = FindDefaultActr();
                if (found == null)
                {
                    Console.WriteLine("No se encontró ACTR.dll / ACTR.dll not found in:");
                    foreach (var p in DefaultActrPaths) Console.WriteLine("  " + p);
                    Console.WriteLine("\nArrastre ACTR.dll sobre ACTRDecryptor.exe, o escriba la ruta aquí");
                    Console.WriteLine("Drag ACTR.dll onto ACTRDecryptor.exe, or type/paste its path here:");
                    Console.Write("> ");
                    found = (Console.ReadLine() ?? "").Trim().Trim('"');
                    if (found.Length == 0) { PrintUsage(); return 1; }
                }
                args = new[] { "decode", found };
            }
            // Arrastrar y soltar un archivo sobre el exe
            else if (args.Length == 1 && File.Exists(args[0]))
            {
                args = new[] { "decode", args[0] };
            }

            if (args.Length < 2)
            {
                PrintUsage();
                return 1;
            }

            string mode = args[0].ToLower();
            string path = args[1].Trim('"');

            if (mode == "decode")
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Error: No se encuentra el archivo / file not found: {path}");
                    return 1;
                }
                DecodeACTR(path);
            }
            else if (mode == "encode")
            {
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Error: No se encuentra el directorio / directory not found: {path}");
                    return 1;
                }
                EncodeACTR(path);
            }
            else
            {
                Console.WriteLine("Error: Modo debe ser 'decode' o 'encode'");
                return 1;
            }
            return 0;
        }

        static string FindDefaultActr()
        {
            string local = Path.Combine(AppContext.BaseDirectory, "ACTR.dll");
            if (File.Exists(local)) return local;
            foreach (var p in DefaultActrPaths)
                if (File.Exists(p)) return p;
            return null;
        }

        static void PrintUsage()
        {
            Console.WriteLine("Uso / Usage:");
            Console.WriteLine("  Doble clic: busca ACTR.dll en C:\\Program Files (x86)\\USART HMI\\");
            Console.WriteLine("  Arrastrar ACTR.dll sobre ACTRDecryptor.exe");
            Console.WriteLine("  Desencriptar: ACTRDecryptor.exe decode <archivo_ACTR.dll>");
            Console.WriteLine("  Encriptar:    ACTRDecryptor.exe encode <directorio_assemblies>");
        }

        /// <summary>
        /// Carpeta de salida: "decrypted" junto a ACTR.dll; si no hay permiso de escritura
        /// (p. ej. Program Files sin admin), Documentos\USART_HMI_decrypted.
        /// </summary>
        static string GetOutputDir(string actrPath)
        {
            string preferred = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(actrPath)) ?? ".", "decrypted");
            try
            {
                Directory.CreateDirectory(preferred);
                string probe = Path.Combine(preferred, ".write_test");
                File.WriteAllText(probe, "");
                File.Delete(probe);
                return preferred;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (string.IsNullOrEmpty(docs)) docs = AppContext.BaseDirectory;
                string fallback = Path.Combine(docs, "USART_HMI_decrypted");
                Directory.CreateDirectory(fallback);
                Console.WriteLine($"Sin permiso de escritura en / no write access to: {preferred}");
                Console.WriteLine($"Usando / using: {fallback}");
                return fallback;
            }
        }

        static void DecodeACTR(string actrPath)
        {
            Console.WriteLine($"Leyendo {actrPath}...");
            byte[] actrData = File.ReadAllBytes(actrPath);

            Console.WriteLine($"Tamaño: {actrData.Length} bytes");
            Console.WriteLine("\nDesencriptando contenedor ACTR...");

            // El archivo ACTR tiene múltiples assemblies encriptados
            // Formato: [Header 64 bytes XOR 0x09][Payload]...

            string outputDir = GetOutputDir(actrPath);

            using (MemoryStream ms = new MemoryStream(actrData))
            using (BinaryReader br = new BinaryReader(ms))
            {
                int assemblyCount = 0;

                while (ms.Position + 64 < ms.Length)
                {
                    // Leer header (64 bytes)
                    byte[] header = br.ReadBytes(64);

                    // XOR con 0x09 para obtener el header real
                    for (int i = 0; i < header.Length; i++)
                    {
                        header[i] ^= 9;
                    }

                    string headerStr = System.Text.Encoding.ASCII.GetString(header).Replace("\0", "").Trim();
                    string[] parts = headerStr.Split('#');

                    if (parts.Length < 2)
                        break;

                    string dllName = parts[0];
                    int size = int.Parse(parts[1]);
                    string compressed = parts.Length > 2 ? parts[2] : "0";

                    Console.WriteLine($"\n[{assemblyCount}] {dllName} - {size} bytes (compressed: {compressed})");

                    // Leer payload encriptado
                    Console.WriteLine("  Leyendo payload...");
                    byte[] encryptedData = br.ReadBytes(size);

                    // Desencriptar (port administrado de AppDllPass_Decode)
                    Console.WriteLine("  Desencriptando...");
                    AppDllPassManaged.Decode(encryptedData);

                    // Si está comprimido (flag "1"), descomprimir
                    if (compressed == "1")
                    {
                        Console.WriteLine("  Descomprimiendo GZIP...");
                        encryptedData = Decompress(encryptedData);
                        Console.WriteLine($"  Descomprimido: {encryptedData.Length} bytes");
                    }

                    // Guardar
                    Console.WriteLine("  Guardando archivo...");
                    string outputPath = Path.Combine(outputDir, dllName);
                    File.WriteAllBytes(outputPath, encryptedData);
                    Console.WriteLine($"  Guardado: {outputPath}");

                    Console.WriteLine($"  Completado #{assemblyCount}");
                    assemblyCount++;
                }

                Console.WriteLine($"\n✓ {assemblyCount} assemblies desencriptados en: {outputDir}");
            }
        }

        static void EncodeACTR(string inputDir)
        {
            if (!Directory.Exists(inputDir))
            {
                Console.WriteLine($"Error: Directorio no encontrado: {inputDir}");
                return;
            }

            string outputPath = Path.Combine(Path.GetDirectoryName(inputDir) ?? ".", "ACTR_translated.dll");

            Console.WriteLine($"Encriptando assemblies desde {inputDir}...");

            // Lista de assemblies en orden
            string[] assemblies = { "hmitype.dll", "achmiface.dll", "rsapp.dll", "HMIFORM.dll", "TFTEDIT.dll", "TFTRUN.dll" };

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                foreach (string dllName in assemblies)
                {
                    string dllPath = Path.Combine(inputDir, dllName);
                    if (!File.Exists(dllPath))
                    {
                        Console.WriteLine($"  Advertencia: {dllName} no encontrado, saltando...");
                        continue;
                    }

                    Console.WriteLine($"  Procesando {dllName}...");

                    // Leer DLL
                    byte[] dllData = File.ReadAllBytes(dllPath);

                    // TODO: Comprimir si es necesario
                    // Por ahora, sin comprimir
                    byte[] finalData = dllData;
                    string compressFlag = "0";

                    // Encriptar (port administrado de AppDllPass_Encode)
                    AppDllPassManaged.Encode(finalData);

                    // Crear header: "nombre.dll#tamaño#comprimido"
                    string headerStr = $"{dllName}#{finalData.Length}#{compressFlag}";
                    byte[] header = new byte[64];
                    byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(headerStr);
                    Array.Copy(headerBytes, header, Math.Min(headerBytes.Length, 64));

                    // XOR header con 0x09
                    for (int i = 0; i < header.Length; i++)
                    {
                        header[i] ^= 9;
                    }

                    // Escribir al stream
                    bw.Write(header);
                    bw.Write(finalData);

                    Console.WriteLine($"    ✓ {dllName} encriptado ({finalData.Length} bytes)");
                }

                // Guardar ACTR.dll
                File.WriteAllBytes(outputPath, ms.ToArray());
                Console.WriteLine($"\n✓ ACTR.dll creado: {outputPath}");
                Console.WriteLine($"\nPara usar:");
                Console.WriteLine($"  1. Backup: copy \"C:\\Program Files (x86)\\USART HMI\\ACTR.dll\" ACTR.dll.backup");
                Console.WriteLine($"  2. Reemplazar: copy /Y \"{outputPath}\" \"C:\\Program Files (x86)\\USART HMI\\ACTR.dll\"");
                Console.WriteLine($"  3. Probar: \"C:\\Program Files (x86)\\USART HMI\\USART HMI.exe\"");
            }
        }

        static byte[] Decompress(byte[] data)
        {
            // ACTR usa algún tipo de compresión
            // Probablemente GZip o Deflate
            // Necesitarás identificar el formato específico
            try
            {
                using (var input = new MemoryStream(data))
                using (var output = new MemoryStream())
                using (var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
                {
                    gzip.CopyTo(output);
                    return output.ToArray();
                }
            }
            catch
            {
                // Si no es GZip, intentar Deflate
                try
                {
                    using (var input = new MemoryStream(data))
                    using (var output = new MemoryStream())
                    using (var deflate = new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Decompress))
                    {
                        deflate.CopyTo(output);
                        return output.ToArray();
                    }
                }
                catch
                {
                    // No está comprimido o formato desconocido
                    return data;
                }
            }
        }
    }
}
