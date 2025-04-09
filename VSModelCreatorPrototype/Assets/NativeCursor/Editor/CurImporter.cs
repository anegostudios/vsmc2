using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
// ReSharper disable UnusedVariable

namespace Riten.Native.Cursors.Editor.Importers
{
    public struct CURSOR_RESULT
    {
        public Texture2D texture;
        public Vector2 hotspot;

        public bool isMask;
        public Color32 backgroundColor;
        public Color32 foregroundColor;
    }
    
    public struct CURSOR
    {
        public byte width;
        public byte height;
        public byte colorCount;
        public byte reserved;
        public ushort xhotspot;
        public ushort yhotspot;
        public uint sizeInBytes;
        public uint fileOffset;
    }
    
    [ScriptedImporter(1, "cur")]
    public class CurImporter : ScriptedImporter
    {
        [SerializeField] bool _invertXHotspot;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var cursors = LoadCursorFromFile(ctx.assetPath, _invertXHotspot);
            
            VirtualCursor firstCursor = null;
            
            for (int i = cursors.Count - 1; i >= 0; --i)
            {
                var cursor = ScriptableObject.CreateInstance<VirtualCursor>();
                var idx = cursors.Count - i;
                cursor.name = $"cursor_{idx}";
                cursor.texture = cursors[i].texture;
                cursor.texture.name = $"cursor_{idx}_texture";
                cursor.hotspot = cursors[i].hotspot;
                cursor.isMask = cursors[i].isMask;
                cursor.backgroundColor = cursors[i].backgroundColor;
                cursor.foregroundColor = cursors[i].foregroundColor;
                ctx.AddObjectToAsset($"cursor_{idx}_texture", cursors[i].texture, cursor.texture);
                ctx.AddObjectToAsset($"cursor_{idx}", cursor, cursor.texture);

                var newCur = Instantiate(cursor);
                newCur.name = $"cursor_{idx}_cursor";
                ctx.AddObjectToAsset($"cursor_{idx}_cursor", newCur, cursor.texture);

                firstCursor ??= cursor;
            }
            
            ctx.SetMainObject(firstCursor);
        }

        static List<CURSOR_RESULT> LoadCursorFromFile(string file, bool invertXHotspot = false)
        {
            if (!File.Exists(file))
                return null;
            
            using var br = new BinaryReader(File.OpenRead(file));
            
            LoadCursorFromBinary(br, out var res, invertXHotspot);
            return res;
        }

        public static bool LoadCursorFromBinary(BinaryReader br, out List<CURSOR_RESULT> result, bool invertXHotspot = false)
        {
            result = new List<CURSOR_RESULT>();
            
            var begin = br.BaseStream.Position;
            
            if (br.ReadUInt16() != 0)
                return false;

            if (br.ReadUInt16() != 2)
                return false;
            
            ushort count = br.ReadUInt16();
            var cursors = new List<CURSOR>();
            var results = new List<CURSOR_RESULT>();

            for (int i = 0; i < count; ++i)
            {
                var c = new CURSOR
                {
                    width = br.ReadByte(),
                    height = br.ReadByte(),
                    colorCount = br.ReadByte(),
                    reserved = br.ReadByte(),
                    xhotspot = br.ReadUInt16(),
                    yhotspot = br.ReadUInt16(),
                    sizeInBytes = br.ReadUInt32(),
                    fileOffset = br.ReadUInt32()
                };
                
                if (invertXHotspot)
                    c.xhotspot = (ushort)(c.width - c.xhotspot);
                
                cursors.Add(c); 
            }

            for (int i = 0; i < cursors.Count; ++i)
            {
                var cursor = cursors[i];
                
                br.BaseStream.Seek(begin + cursors[i].fileOffset, SeekOrigin.Begin);

                var sizeOfStruct = br.ReadUInt32();
                var skipRest = sizeOfStruct != 40;

                if (skipRest)
                    br.BaseStream.Seek(-4, SeekOrigin.Current);
                
                if (skipRest || cursor is { width: 0, height: 0 })
                {
                    var rawData = br.ReadBytes((int)cursor.sizeInBytes);
                    var rawTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false) {
                        alphaIsTransparency = true
                    };
                    
                    rawTexture.LoadImage(rawData);
                    rawTexture.Apply();
                    
                    var finalTexture = new Texture2D(rawTexture.width, rawTexture.height, TextureFormat.RGBA32, false) {
                        alphaIsTransparency = true
                    };
                    
                    finalTexture.SetPixels32(rawTexture.GetPixels32());

                    var c = new CURSOR_RESULT
                    {
                        texture = finalTexture,
                        isMask = false,
                        backgroundColor = default,
                        foregroundColor = default,
                        hotspot = new Vector2(
                            cursor.xhotspot / (float)finalTexture.width,
                            cursor.yhotspot / (float)finalTexture.height
                        )
                    };
                    
                    results.Add(c);
                    br.BaseStream.Seek(begin + cursors[i].fileOffset + cursor.sizeInBytes, SeekOrigin.Begin);
                    continue;
                }
                
                var cursorWidth = br.ReadUInt32();
                var cursorHeight = br.ReadUInt32();
                
                var planes = br.ReadUInt16();
                var bitPetPixel = br.ReadUInt16();
                var compression = br.ReadUInt32();
                var imageSizeInBytes = br.ReadUInt32();
                var xPelsPerMeter = br.ReadUInt32();
                var yPelsPerMeter = br.ReadUInt32();
                var colorsUsed = br.ReadUInt32();
                var colorsImportant = br.ReadUInt32();
                
                var pixelCount = cursor.width * cursor.height;
                var pixels = new Color32[pixelCount];
                
                Color32 backgroundColor = default;
                Color32 foregroundColor = default;

                var isMask = LoadPixelsData(br, bitPetPixel, pixels, cursor, imageSizeInBytes, ref backgroundColor, ref foregroundColor);

                // Skip mask
                br.BaseStream.Seek(begin + cursors[i].fileOffset + cursor.sizeInBytes, SeekOrigin.Begin);
                
                var texture = new Texture2D(cursor.width, cursor.height, TextureFormat.RGBA32, false)
                {
                    alphaIsTransparency = true
                };
                
                texture.SetPixels32(pixels);
                texture.filterMode = FilterMode.Bilinear;
                texture.Apply();
                
                results.Add(new CURSOR_RESULT
                {
                    texture = texture,
                    isMask = isMask,
                    backgroundColor = backgroundColor,
                    foregroundColor = foregroundColor,
                    hotspot = new Vector2(
                        cursor.xhotspot / (float)cursor.width, 
                        cursor.yhotspot / (float)cursor.height
                    )
                });
            }
            
            result = results;
            return true;
        }
        
        static Color32 Overlay(Color32 foreground, Color32 background)
        {
            // Normalize color and alpha values to [0, 1]
            float fgR = foreground.r / 255f;
            float fgG = foreground.g / 255f;
            float fgB = foreground.b / 255f;
            float fgA = foreground.a / 255f;

            float bgR = background.r / 255f;
            float bgG = background.g / 255f;
            float bgB = background.b / 255f;
            float bgA = background.a / 255f;

            // Calculate resulting alpha
            float outA = fgA + bgA * (1 - fgA);

            // Prevent division by zero if alpha is zero
            if (outA == 0) return new Color32(0, 0, 0, 0);

            // Calculate resulting color channels
            float outR = (fgR * fgA + bgR * bgA * (1 - fgA)) / outA;
            float outG = (fgG * fgA + bgG * bgA * (1 - fgA)) / outA;
            float outB = (fgB * fgA + bgB * bgA * (1 - fgA)) / outA;

            // Convert back to Color32 (0-255)
            return new Color32(
                (byte)(outR * 255),
                (byte)(outG * 255),
                (byte)(outB * 255),
                (byte)(outA * 255)
            );
        }

        private static bool LoadPixelsData(BinaryReader br, ushort bitPetPixel, IList<Color32> pixels,
            CURSOR cursor, uint imageSizeInBytes, ref Color32 backgroundColor, ref Color32 foregroundColor)
        {
            var pixelCount = cursor.width * cursor.height;
            bool isMask = false;
            
            switch (bitPetPixel)
            {
                case 32:
                {
                    for (int j = 0; j < pixelCount; ++j)
                    {
                        var b = br.ReadByte();
                        var g = br.ReadByte();
                        var r = br.ReadByte();
                        var a = br.ReadByte();

                        pixels[j] = new Color32(r, g, b, a);
                    }

                    break;
                }
                case 4:
                {
                    var padding = (32 - cursor.width % 32) % 32;
                    int pixelIdx = 0;
                    
                    br.BaseStream.Seek(32/2*4, SeekOrigin.Current);
                    
                    for (int j = 0; j < pixelCount / 2; ++j)
                    {
                        int x = j % (cursor.width + padding);
                        var val = br.ReadByte();
                        
                        var halfA = val >> 4;
                        var halfB = val & 0x0F;
                        
                        var b0 = (halfA & 1)        == 1 ? (byte)255 : (byte)0;
                        var b1 = ((halfA >> 1) & 1) == 1 ? (byte)255 : (byte)0;
                        var b2 = ((halfA >> 2) & 1) == 1 ? (byte)255 : (byte)0;
                        var b3 = ((halfA >> 3) & 1) == 1 ? (byte)255 : (byte)0;
                        
                        var b4 = (halfB & 1)        == 1 ? (byte)255 : (byte)0;
                        var b5 = ((halfB >> 1) & 1) == 1 ? (byte)255 : (byte)0;
                        var b6 = ((halfB >> 2) & 1) == 1 ? (byte)255 : (byte)0;
                        var b7 = ((halfB >> 3) & 1) == 1 ? (byte)255 : (byte)0;

                        if (x < cursor.width)
                        {
                            pixels[pixelIdx++] = new Color32(b2, b1, b0, b3);
                            pixels[pixelIdx++] = new Color32(b6, b5, b4, b7);
                        }
                    }

                    break;
                }
                case 1:
                {
                    isMask = true;
                    backgroundColor = new Color32(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);
                    br.ReadByte();
                    foregroundColor = new Color32(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);
                    br.ReadByte();

                    var padding = (32 - cursor.width % 32) % 32;
                    var values = br.ReadBytes((int)imageSizeInBytes);

                    int pixelIdx = 0;
                    bool isSecondPass = false;

                    for (int j = 0; j < imageSizeInBytes * 8; ++j)
                    {
                        int x = j % (cursor.width + padding);

                        var idx = j / 8;
                        var bit = 7 - j % 8;
                        var val = (values[idx] >> bit) & 1;

                        if (x < cursor.width)
                        {
                            if (pixelIdx >= pixelCount)
                            {
                                pixelIdx = 0;
                                isSecondPass = true;
                            }

                            if (isSecondPass)
                            {
                                var existing = pixels[pixelIdx];
                                
                                var color = backgroundColor;
                                color.a = val == 1 ? (byte)0 : (byte)255;
                                
                                pixels[pixelIdx++] = Overlay(existing, color);
                            }
                            else
                            {
                                var color = foregroundColor;
                                color.a = val == 1 ? (byte)255 : (byte)0;
                                pixels[pixelIdx++] = color;
                            }
                        }
                    }

                    break;
                }
                default: throw new System.NotImplementedException($"Bit per pixel {bitPetPixel} not implemented!");
            }

            return isMask;
        }
    }
}