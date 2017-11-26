/*
 *Written by Maxi Levi <maxilevi@live.com>, November 2017
*/

 
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Assets.Generation
{

    public class ChunkLoader : MonoBehaviour
    {
        public Vector3 Offset;
        public bool Enabled = true;
        public GameObject Player;
        public World World;
        private int _activeChunks;
        private float _targetMin = 1;
        private float _targetMax = 1;
        private float _minFog;
        private float _maxFog;
        private float _left = 0;
		private Vector3 _lastOffset = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        private int _prevChunkCount;
        private float _lastRadius;
		private Vector3 _playerPosition, _position;
		private Thread _t1, _t2;
		private bool Stop;

		void Awake(){
			StartCoroutine (this.LoadChunks());
			StartCoroutine (this.ManageChunksMesh());
		}

		void OnApplicationQuit(){
			Stop = true;
		}

		void Update(){
			_playerPosition = Player.transform.position;
			_position = transform.position;
		}


		private IEnumerator LoadChunks()
        {
			while (true)
            {
				//break;
				if(Stop) break;


				if (!Enabled || !World.Loaded)
                    goto SLEEP;

				Offset = World.ToChunkSpace(_playerPosition);

				if (Offset != _lastOffset)
                {

					for (int _x = -World.ChunkLoaderRadius / 2; _x < World.ChunkLoaderRadius / 2; _x++)
                    {
						//yield return null;
						for (int _z = -World.ChunkLoaderRadius / 2; _z < World.ChunkLoaderRadius / 2; _z++)
                        {
							for (int _y = -World.ChunkLoaderRadius / 2; _y < World.ChunkLoaderRadius / 2; _y++)
                            {
								int x = _x, y = _y ,z  = _z;

								if (World.GetChunkByOffset(Offset + Vector3.Scale(new Vector3(x, y, z), new Vector3(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize)) ) == null)
                                {
									Vector3 chunkPos = Offset + Vector3.Scale(new Vector3(x, y, z), new Vector3(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize));
									GameObject NewChunk = new GameObject("Chunk "+ (chunkPos.x) + " "+ (chunkPos.y) + " "+ (chunkPos.z) );
									NewChunk.transform.position = chunkPos;
									NewChunk.transform.SetParent(World.gameObject.transform);
									Chunk chunk = NewChunk.AddComponent<Chunk>();
									chunk.Init(chunkPos, World);
									chunk.Lod = 2;
									World.AddChunk(chunkPos, chunk);
                                }
                            }
                        }
                    }
                    _lastRadius = Options.ChunkLoaderRadius;
                    _lastOffset = Offset;
					World.SortGenerationQueue();
                }
                SLEEP:
				yield return null;
            }
        }

		private IEnumerator ManageChunksMesh()
        {
			while (true)
            {
				if(Stop) break;

				yield return null;


                Chunk[] Chunks;
                lock (World.Chunks)
					Chunks = World.Chunks.Values.ToList().ToArray();

				bool AddedNew = false;
                for (int i = Chunks.Length - 1; i > -1; i--)
                {

                    if (Chunks[i].Disposed)
                    {
                        continue;
                    }

					if ((Chunks[i].Position - _playerPosition).sqrMagnitude > (Options.ChunkLoaderRadius) * .5f * Chunk.ChunkSize * (Options.ChunkLoaderRadius) * Chunk.ChunkSize * .5f  )
                    {
                        World.RemoveChunk(Chunks[i]);
                        continue;
                    }

					if (Chunks [i].ShouldBuild && Chunks [i].IsGenerated && !World.ContainsMeshQueue(Chunks[i]) && Chunks[i].NeighboursExists) {
						World.AddToQueue (Chunks[i], true);
						AddedNew = true;
					}
                }
				//if (AddedNew)
				//	World.SortMeshQueue ();
            }
        }
    }
}
