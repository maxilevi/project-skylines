/*
 *Written by Maxi Levi <maxilevi@live.com>, November 2017
*/
 
using System;
using UnityEngine;

namespace Assets.Generation
{
    public class WorldGenerator : IDisposable
    {
		public const float SpawnRadius = 64f;
		public static Vector3 SpawnPosition = Vector3.forward * 32f;

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
            float scale = 0.025f, amplitude = 64f;
			int lerp = 4;

            for (int x = 0; x < Chunk.ChunkSize; x++)
            {
                for (int y = 0; y < Chunk.ChunkSize; y++)
                {
					float[] values = new float[Chunk.ChunkSize / lerp];
					for(int i = 0; i < values.Length; i++)
						values[i] = (float) OpenSimplexNoise.Evaluate( (x+ Offsets.x) * scale, (y+ Offsets.y) * scale, (i*lerp+ Offsets.z) * scale) * amplitude;
                    
					for (int z = 0; z < Chunk.ChunkSize; z++)
                    {
						float prev = values [ (int) (z / lerp) ];
						float next = values [ (int) Mathf.Min(z / lerp+1,values.Length-1) ];
						Densities [x] [y] [z] = Mathf.Lerp (prev, next, (float) (z / (float) lerp) );

						//Make a sphere on spawn point
						Densities [x] [y] [z] = ( ( SpawnPosition - new Vector3(x + Offsets.x, y + Offsets.y, z + Offsets.z) ).sqrMagnitude < SpawnRadius*SpawnRadius) ? 0 : Densities[x][y][z]; 
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}