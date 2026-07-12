using System;
using CleanEnergy.Map;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Deterministic multi-octave height map generator (no Unity component dependency).
    /// </summary>
    public sealed class HeightMapGenerator
    {
        /// <summary>
        /// Generates a normalized height map in the [0, 1] range.
        /// </summary>
        public float[,] Generate(MapGenerationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (!settings.Validate(out var error))
            {
                throw new InvalidOperationException(error);
            }

            var width = settings.GridWidth;
            var height = settings.GridHeight;
            var map = new float[width, height];
            var seedOffset = DeriveSeedOffset(settings.Seed);

            var min = float.MaxValue;
            var max = float.MinValue;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var amplitude = 1f;
                    var frequency = 1f;
                    var noiseHeight = 0f;
                    var amplitudeSum = 0f;

                    for (var octave = 0; octave < settings.Octaves; octave++)
                    {
                        var sampleX = (x + seedOffset.x) * settings.NoiseScale * frequency;
                        var sampleY = (y + seedOffset.y) * settings.NoiseScale * frequency;
                        var perlin = DeterministicNoise.Perlin(sampleX, sampleY);
                        noiseHeight += perlin * amplitude;
                        amplitudeSum += amplitude;
                        amplitude *= settings.Persistence;
                        frequency *= settings.Lacunarity;
                    }

                    noiseHeight /= amplitudeSum;
                    map[x, y] = noiseHeight;

                    if (noiseHeight < min) min = noiseHeight;
                    if (noiseHeight > max) max = noiseHeight;
                }
            }

            var range = max - min;
            if (range < 1e-6f)
            {
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        map[x, y] = 0.5f;
                    }
                }

                return map;
            }

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    map[x, y] = (map[x, y] - min) / range;
                }
            }

            return map;
        }

        private static (float x, float y) DeriveSeedOffset(string seed)
        {
            var hash = HashString(seed ?? string.Empty);
            var x = (hash & 0xFFFF) / 65535f * 10000f;
            var y = ((hash >> 16) & 0xFFFF) / 65535f * 10000f;
            return (x, y);
        }

        private static int HashString(string value)
        {
            unchecked
            {
                var hash = 23;
                for (var i = 0; i < value.Length; i++)
                {
                    hash = hash * 31 + value[i];
                }

                return hash == 0 ? 1 : hash;
            }
        }
    }

    /// <summary>
    /// Classic gradient Perlin noise, deterministic for identical inputs.
    /// </summary>
    public static class DeterministicNoise
    {
        private static readonly int[] Permutation;

        static DeterministicNoise()
        {
            var p = new int[256];
            for (var i = 0; i < 256; i++)
            {
                p[i] = i;
            }

            // Fixed shuffle for stable cross-run results.
            var random = new Random(1337);
            for (var i = 255; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }

            Permutation = new int[512];
            for (var i = 0; i < 512; i++)
            {
                Permutation[i] = p[i & 255];
            }
        }

        public static float Perlin(float x, float y)
        {
            var x0 = FastFloor(x);
            var y0 = FastFloor(y);
            var xf = x - x0;
            var yf = y - y0;
            var xi = x0 & 255;
            var yi = y0 & 255;

            var u = Fade(xf);
            var v = Fade(yf);

            var aa = Permutation[Permutation[xi] + yi];
            var ab = Permutation[Permutation[xi] + yi + 1];
            var ba = Permutation[Permutation[xi + 1] + yi];
            var bb = Permutation[Permutation[xi + 1] + yi + 1];

            var x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1f, yf), u);
            var x2 = Lerp(Grad(ab, xf, yf - 1f), Grad(bb, xf - 1f, yf - 1f), u);
            return (Lerp(x1, x2, v) + 1f) * 0.5f;
        }

        private static int FastFloor(float value) => value >= 0 ? (int)value : (int)value - 1;

        private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        private static float Lerp(float a, float b, float t) => a + t * (b - a);

        private static float Grad(int hash, float x, float y)
        {
            return (hash & 1) == 0
                ? ((hash & 2) == 0 ? x : -x) + ((hash & 4) == 0 ? y : -y)
                : ((hash & 2) == 0 ? y : -y) + ((hash & 4) == 0 ? x : -x);
        }
    }
}
