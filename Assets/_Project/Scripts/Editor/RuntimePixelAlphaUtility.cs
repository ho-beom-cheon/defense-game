using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class RuntimePixelAlphaUtility
    {
        public static readonly string[] ActorSpritePaths =
        {
            "Assets/_Project/Art/RuntimePixel/Monsters/GateImp/gate_imp_idle.png",
            "Assets/_Project/Art/RuntimePixel/Monsters/OrcBrute/orc_brute_idle.png",
            "Assets/_Project/Art/RuntimePixel/Monsters/DireWolf/dire_wolf_idle.png",
            "Assets/_Project/Art/RuntimePixel/Monsters/CaveBat/cave_bat_idle.png",
            "Assets/_Project/Art/RuntimePixel/Monsters/CoreSlime/core_slime_idle.png",
            "Assets/_Project/Art/RuntimePixel/Monsters/BoneSoldier/bone_soldier_idle.png",
            "Assets/_Project/Art/RuntimePixel/Bosses/Grumbar/grumbar_idle.png"
        };

        private const byte OpaqueThreshold = 250;
        private const byte DarkThreshold = 2;

        [MenuItem("Tools/RuneGate/Repair RuntimePixel Actor Alpha")]
        public static void RepairFromMenu()
        {
            int changedFiles = RepairActorSprites();
            EditorUtility.DisplayDialog(
                "RuneGate RuntimePixel Alpha",
                changedFiles > 0
                    ? $"Repaired border-connected black pixels in {changedFiles} actor sprite(s)."
                    : "No border-connected black pixels were found.",
                "OK");
        }

        public static void RepairFromCommandLine()
        {
            int changedFiles = RepairActorSprites();
            Debug.Log($"RUNEGATE_RUNTIME_PIXEL_ALPHA_REPAIR_COMPLETE: changedFiles={changedFiles}");
        }

        public static int CountOpaqueDarkBorderPixels(string assetPath)
        {
            if (!TryLoadPixels(assetPath, out Texture2D texture, out Color32[] pixels))
            {
                return 0;
            }

            int width = texture.width;
            int height = texture.height;
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                if (IsOpaqueDark(pixels[x]))
                {
                    count++;
                }

                int topIndex = (height - 1) * width + x;
                if (height > 1 && IsOpaqueDark(pixels[topIndex]))
                {
                    count++;
                }
            }

            for (int y = 1; y < height - 1; y++)
            {
                int rowStart = y * width;
                if (IsOpaqueDark(pixels[rowStart]))
                {
                    count++;
                }

                if (width > 1 && IsOpaqueDark(pixels[rowStart + width - 1]))
                {
                    count++;
                }
            }

            UnityEngine.Object.DestroyImmediate(texture);
            return count;
        }

        private static int RepairActorSprites()
        {
            int changedFiles = 0;
            for (int i = 0; i < ActorSpritePaths.Length; i++)
            {
                if (RepairBorderConnectedBlackPixels(ActorSpritePaths[i]) > 0)
                {
                    changedFiles++;
                }
            }

            if (changedFiles > 0)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            return changedFiles;
        }

        private static int RepairBorderConnectedBlackPixels(string assetPath)
        {
            if (!TryLoadPixels(assetPath, out Texture2D texture, out Color32[] pixels))
            {
                Debug.LogWarning($"RuntimePixel alpha repair skipped missing or unreadable texture: {assetPath}");
                return 0;
            }

            int width = texture.width;
            int height = texture.height;
            bool[] visited = new bool[pixels.Length];
            Queue<int> pending = new Queue<int>();

            for (int x = 0; x < width; x++)
            {
                EnqueueIfOpaqueDark(x, pixels, visited, pending);
                EnqueueIfOpaqueDark((height - 1) * width + x, pixels, visited, pending);
            }

            for (int y = 1; y < height - 1; y++)
            {
                int rowStart = y * width;
                EnqueueIfOpaqueDark(rowStart, pixels, visited, pending);
                EnqueueIfOpaqueDark(rowStart + width - 1, pixels, visited, pending);
            }

            int repairedPixels = 0;
            while (pending.Count > 0)
            {
                int index = pending.Dequeue();
                Color32 pixel = pixels[index];
                pixel.a = 0;
                pixels[index] = pixel;
                repairedPixels++;

                int x = index % width;
                int y = index / width;
                if (x > 0)
                {
                    EnqueueIfOpaqueDark(index - 1, pixels, visited, pending);
                }

                if (x < width - 1)
                {
                    EnqueueIfOpaqueDark(index + 1, pixels, visited, pending);
                }

                if (y > 0)
                {
                    EnqueueIfOpaqueDark(index - width, pixels, visited, pending);
                }

                if (y < height - 1)
                {
                    EnqueueIfOpaqueDark(index + width, pixels, visited, pending);
                }
            }

            if (repairedPixels > 0)
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
                Debug.Log($"Repaired {repairedPixels} border-connected black pixels: {assetPath}");
            }

            UnityEngine.Object.DestroyImmediate(texture);
            return repairedPixels;
        }

        private static void EnqueueIfOpaqueDark(int index, Color32[] pixels, bool[] visited, Queue<int> pending)
        {
            if (index < 0 || index >= pixels.Length || visited[index] || !IsOpaqueDark(pixels[index]))
            {
                return;
            }

            visited[index] = true;
            pending.Enqueue(index);
        }

        private static bool IsOpaqueDark(Color32 pixel)
        {
            return pixel.a >= OpaqueThreshold &&
                   pixel.r <= DarkThreshold &&
                   pixel.g <= DarkThreshold &&
                   pixel.b <= DarkThreshold;
        }

        private static bool TryLoadPixels(string assetPath, out Texture2D texture, out Color32[] pixels)
        {
            texture = null;
            pixels = null;
            string fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(File.ReadAllBytes(fullPath), false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                texture = null;
                return false;
            }

            pixels = texture.GetPixels32();
            return pixels != null && pixels.Length == texture.width * texture.height;
        }
    }
}
