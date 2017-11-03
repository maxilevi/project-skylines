using System;
using UnityEngine;

namespace Assets.Generation
{
    public class WorldGenerator : IDisposable
    {
        public void BuildArray(float[][][] Densities)
        {
            for (int x = 0; x < Chunk.ChunkSize; x++)
            {
                Densities[x] = new float[Chunk.ChunkSize][];

                for (int y = 0; y < Chunk.ChunkSize; y++)
                {
                    Densities[x][y] = new float[Chunk.ChunkSize];
                }
            }
        }

        public void Generate(float[][][] Densities, Vector3 Offsets)
        {
            float scale = 0.05f, amplitude = 24f;

            for (int x = 0; x < Chunk.ChunkSize; x++)
            {
                for (int y = 0; y < Chunk.ChunkSize; y++)
                {
                    for (int z = 0; z < Chunk.ChunkSize; z++)
                    {
                        Densities[x][y][z] = (float) OpenSimplexNoise.Evaluate( (x+ Offsets.x) * scale, (y+ Offsets.y) * scale, (z+ Offsets.z) * scale) * amplitude;
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}