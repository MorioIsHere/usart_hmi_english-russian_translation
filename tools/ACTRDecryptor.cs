using System;
using System.IO;

namespace ACTRDecryptor
{
    class Program
    {
        // El cifrado de AppDllPass.dll (nativa, solo 32-bit) está reimplementado en C#
        // en AppDllPassManaged.cs, por lo que la herramienta funciona en x64 (amd64).

        static void Main(string[] args)
        {
            Console.WriteLine("ACTR.dll Decryptor/Encryptor");
            Console.WriteLine("=============================\n");

            if (args.Length < 2)
            {
                Console.WriteLine("Uso:");
                Console.WriteLine("  Desencriptar: ACTRDecryptor.exe decode <archivo_ACTR.dll>");
                Console.WriteLine("  Encriptar:    ACTRDecryptor.exe encode <directorio_assemblies>");
                Console.WriteLine("\nEjemplos:");
                Console.WriteLine("  ACTRDecryptor.exe decode \"C:\\Program Files (x86)\\USART HMI\\ACTR.dll\"");
                Console.WriteLine("  ACTRDecryptor.exe encode decrypted\\");
                return;
            }

            string mode = args[0].ToLower();
            string path = args[1];

            if (mode == "decode")
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Error: No se encuentra el archivo {path}");
                    return;
                }
                DecodeACTR(path);
            }
            else if (mode == "encode")
            {
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Error: No se encuentra el directorio {path}");
                    return;
                }
                EncodeACTR(path);
            }
            else
            {
                Console.WriteLine("Error: Modo debe ser 'decode' o 'encode'");
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

            string outputDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(actrPath)) ?? ".", "decrypted");
            Directory.CreateDirectory(outputDir);

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
