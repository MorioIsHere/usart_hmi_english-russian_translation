using System;
using System.Buffers.Binary;

namespace ACTRDecryptor
{
    /// <summary>
    /// Pure managed port of the native 32-bit AppDllPass.dll (AppDllPass_Decode / AppDllPass_Encode).
    /// Removes the dependency on the x86-only native library so the tool runs on any
    /// architecture (x64 / amd64, ARM64, x86).
    ///
    /// Notes on the original:
    ///  - Both functions process the buffer in place, 4 bytes at a time; a trailing
    ///    remainder (length % 4) is left untouched.
    ///  - The cipher key is a constant string embedded in the DLL, plus three constant
    ///    32-bit state words. The data transform does NOT depend on the time of day.
    ///  - AppDllPass_Decode additionally writes a time-based "check" value into the
    ///    caller's key buffer (an anti-tamper side channel used by USART HMI). That value
    ///    does not affect the decrypted data and is not needed here, so it is omitted.
    /// </summary>
    public static class AppDllPassManaged
    {
        private const string RawKey = "hgsd87HH6654dfll88ghfY987Gt3L";
        private const uint InitP = 0x0901F2B7;
        private const uint InitQ = 0x16EEF93B;
        private const uint InitR = 0x1870AFBC;

        private static readonly byte[] Key = BuildKey();

        private static byte[] BuildKey()
        {
            byte[] k = new byte[RawKey.Length];
            for (int i = 0; i < k.Length; i++)
                k[i] = (byte)((RawKey[i] >> 1) + 2);
            return k;
        }

        /// <summary>Adds <paramref name="value"/> to byte <paramref name="index"/> (little-endian) of <paramref name="word"/>.</summary>
        private static uint AddToByte(uint word, int index, byte value)
        {
            int shift = index * 8;
            byte b = (byte)((word >> shift) + value);
            return (word & ~(0xFFu << shift)) | ((uint)b << shift);
        }

        public static void Decode(Span<byte> data)
        {
            byte[] key = Key;
            uint keyLen = (uint)key.Length;
            int klen3 = (int)(keyLen & 3);
            int len = data.Length & ~3;

            uint p = InitP, q = InitQ, r = InitR;
            int k = 0;

            for (int off = 0; off < len; off += 4)
            {
                uint v = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(off, 4));
                uint p0 = p, q0 = q, r0 = r;
                byte s = key[k];
                int b3 = s & 3;

                p <<= 1;
                p = AddToByte(p, (int)((keyLen + (uint)off + s) & 3), (byte)(s ^ 0xC7));
                q <<= 2;
                q = AddToByte(q, 3 - b3, (byte)(s ^ 0xDD));
                r <<= 3;
                r = AddToByte(r, 3 - b3, (byte)(s ^ 0x33));

                int sh1 = s & 1, sh7 = s & 7;
                uint sum = p + q + r;

                uint t = (sum << sh1) + (uint)b3;
                v = (v - ((r + t) >> sh1)) ^ t;

                t = ((p - r - q) << b3) + (uint)b3;
                v = (v - ((q + t) >> b3)) ^ t;

                t = (sum << sh7) + (uint)b3;
                v = (v - ((p + t) >> sh7)) ^ t;

                t = ~((q + p) ^ r0) - (uint)off - keyLen;
                v = (v - t - r0) ^ t;

                t = (p << klen3) + r0 + q0;
                v = (v - t - q0) ^ t;

                t = (r0 << (int)((keyLen + s) & 7)) + q0 + p0;
                v = (v - t - p0) ^ t;

                BinaryPrimitives.WriteUInt32LittleEndian(data.Slice(off, 4), v);
                k = (k + 1 == key.Length) ? 0 : k + 1;
            }
        }

        public static void Encode(Span<byte> data)
        {
            byte[] key = Key;
            uint keyLen = (uint)key.Length;
            int klen3 = (int)(keyLen & 3);
            int len = data.Length & ~3;

            uint p = InitP, q = InitQ, r = InitR;
            int k = 0;

            for (int off = 0; off < len; off += 4)
            {
                uint v = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(off, 4));
                byte s = key[k];
                int b3 = s & 3, sh1 = s & 1, sh7 = s & 7;

                uint t = (r << (int)((s + keyLen) & 7)) + q + p;
                v = (v ^ t) + t + p;
                p <<= 1;
                p = AddToByte(p, (int)(((uint)off + s + keyLen) & 3), (byte)(s ^ 0xC7));

                t = (p << klen3) + r + q;
                v = (t ^ v) + t + q;
                q <<= 2;
                q = AddToByte(q, 3 - b3, (byte)(s ^ 0xDD));

                t = ~((p + q) ^ r) - (uint)off - keyLen;
                v = (t ^ v) + t + r;
                r <<= 3;
                r = AddToByte(r, 3 - b3, (byte)(s ^ 0x33));

                uint sum = p + q + r;
                t = (sum << sh7) + (uint)b3;
                v = ((t + p) >> sh7) + (t ^ v);

                t = ((p - r - q) << b3) + (uint)b3;
                v = ((t + q) >> b3) + (t ^ v);

                t = (sum << sh1) + (uint)b3;
                v = ((r + t) >> sh1) + (t ^ v);

                BinaryPrimitives.WriteUInt32LittleEndian(data.Slice(off, 4), v);
                k = (k + 1 == key.Length) ? 0 : k + 1;
            }
        }
    }
}
