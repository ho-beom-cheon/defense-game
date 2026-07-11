using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace RuneGate
{
    internal static class RuneGatePngEncoder
    {
        public static byte[] EncodeRgb24(int width, int height, Color32[] pixels)
        {
            byte[] scanlines = BuildScanlines(width, height, pixels);
            byte[] compressed = Compress(scanlines);
            using (MemoryStream png = new MemoryStream())
            {
                png.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);
                byte[] header = new byte[13];
                WriteBigEndian(header, 0, (uint)width);
                WriteBigEndian(header, 4, (uint)height);
                header[8] = 8;
                header[9] = 2;
                WriteChunk(png, "IHDR", header);
                WriteChunk(png, "IDAT", compressed);
                WriteChunk(png, "IEND", Array.Empty<byte>());
                return png.ToArray();
            }
        }

        private static byte[] BuildScanlines(int width, int height, Color32[] pixels)
        {
            byte[] scanlines = new byte[height * (width * 3 + 1)];
            int destination = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                scanlines[destination++] = 0;
                int source = y * width;
                for (int x = 0; x < width; x++)
                {
                    Color32 pixel = pixels[source + x];
                    scanlines[destination++] = pixel.r;
                    scanlines[destination++] = pixel.g;
                    scanlines[destination++] = pixel.b;
                }
            }

            return scanlines;
        }

        private static byte[] Compress(byte[] scanlines)
        {
            using (MemoryStream compressedStream = new MemoryStream())
            {
                compressedStream.WriteByte(0x78);
                compressedStream.WriteByte(0x9C);
                using (System.IO.Compression.DeflateStream deflate = new System.IO.Compression.DeflateStream(
                           compressedStream,
                           System.IO.Compression.CompressionLevel.Optimal,
                           true))
                {
                    deflate.Write(scanlines, 0, scanlines.Length);
                }

                WriteBigEndian(compressedStream, Adler32(scanlines));
                return compressedStream.ToArray();
            }
        }

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            byte[] typeBytes = Encoding.ASCII.GetBytes(type);
            WriteBigEndian(stream, (uint)data.Length);
            stream.Write(typeBytes, 0, typeBytes.Length);
            stream.Write(data, 0, data.Length);

            byte[] crcInput = new byte[typeBytes.Length + data.Length];
            Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
            Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);
            WriteBigEndian(stream, Crc32(crcInput));
        }

        private static uint Crc32(byte[] bytes)
        {
            uint crc = 0xFFFFFFFFu;
            for (int i = 0; i < bytes.Length; i++)
            {
                crc ^= bytes[i];
                for (int bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1u) != 0u ? 0xEDB88320u ^ (crc >> 1) : crc >> 1;
                }
            }

            return crc ^ 0xFFFFFFFFu;
        }

        private static uint Adler32(byte[] bytes)
        {
            const uint modulus = 65521u;
            uint a = 1u;
            uint b = 0u;
            for (int i = 0; i < bytes.Length; i++)
            {
                a = (a + bytes[i]) % modulus;
                b = (b + a) % modulus;
            }

            return (b << 16) | a;
        }

        private static void WriteBigEndian(Stream stream, uint value)
        {
            byte[] bytes = new byte[4];
            WriteBigEndian(bytes, 0, value);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteBigEndian(byte[] bytes, int offset, uint value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }
    }
}
