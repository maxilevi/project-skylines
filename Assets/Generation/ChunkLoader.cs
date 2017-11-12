/* Copyright (C) Luaek - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
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
        private Vector3 _lastOffset, _lastOffset2;
        private int _prevChunkCount;
        private float _lastRadius, _lastRadius2;
		private Vector3 _playerPosition, _position;
		private Thread _t1, _t2;
		private bool Stop;

		void Awake(){
			StartCoroutine (this.LoadChunks());
			StartCoroutine (this.ManageChunksMesh());

			StartCoroutine(FogLerpCoroutine());
		}

		void OnApplicationQuit(){
			Stop = true;
		}

		void Update(){
			_playerPosition = Player.transform.position;
			_position = transform.position;
		}

        private void UpdateFog(float f1, float f2)
        {
            _targetMax = (float)(Chunk.ChunkSize * Math.Sqrt(_activeChunks) * 2.0f);
            _targetMin = (float)(Chunk.ChunkSize * (Math.Sqrt(_activeChunks) - 2) * 2.0f);
        }

        private IEnumerator FogLerpCoroutine()
        {
            while (true)
            {
                if ((_maxFog != _targetMax || _minFog != _targetMin))
                {
                    _maxFog = Mathf.Lerp(_maxFog, _targetMax, Time.deltaTime * 8f);
                    _minFog = Mathf.Lerp(_minFog, _targetMin, Time.deltaTime * 8f);
                //    RenderSettings.fogEndDistance = _maxFog;
                //    RenderSettings.fogStartDistance = _minFog;
                }

                yield return null;
                yield return null;
            }
        }


		private IEnumerator LoadChunks()
        {
			while (true)
            {
				
				if(Stop) break;


                if (!Enabled)
                    goto SLEEP;

				Offset = World.ToChunkSpace(_playerPosition);

				if (Offset == Vector3.zero)
                    goto SLEEP;

                if (Offset != _lastOffset)
                {
					
					for (int _x = -GraphicsOptions.ChunkLoaderRadius / 2; _x < GraphicsOptions.ChunkLoaderRadius / 2; _x++)
                    {
						for (int _z = -GraphicsOptions.ChunkLoaderRadius / 2; _z < GraphicsOptions.ChunkLoaderRadius / 2; _z++)
                        {
							for (int _y = -4; _y < 4; _y++)
                            {
								int x = _x, y = _y ,z  = _z;

								if (World.GetChunkByOffset(Offset + Vector3.Scale(new Vector3(x, y, z), new Vector3(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize)) ) == null)
                                {
									Vector3 chunkPos = Offset + Vector3.Scale(new Vector3(x, y, z), new Vector3(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize));
									GameObject NewChunk = new GameObject("Chunk "+ (chunkPos.x) + " "+ (chunkPos.y) + " "+ (chunkPos.z) );
									NewChunk.transform.position = chunkPos;
									NewChunk.transform.SetParent(World.gameObject.transform);
									NewChunk.AddComponent<MeshFilter>();
									NewChunk.AddComponent<MeshCollider> ();
									MeshRenderer Renderer = NewChunk.AddComponent<MeshRenderer>();
									Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
									Renderer.receiveShadows = false;
									Chunk chunk = NewChunk.AddComponent<Chunk>();
									chunk.Init(chunkPos, World);
									World.AddChunk(chunkPos, chunk);
                                }
                            }
                        }
                    }
                    _lastRadius = GraphicsOptions.ChunkLoaderRadius;
                    _lastOffset = Offset;

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

                if (!Enabled)
                    continue;

                Chunk[] Chunks;
                lock (World.Chunks)
                {
					Chunks = World.Chunks.Values.ToList().ToArray();
                }
				yield return null;
                //_left += 0.25f;

                //if (_left >= 1.5f)
                //{
                    _activeChunks = 0;
                    for (int i = Chunks.Length - 1; i > -1; i--)
                    {

                        if (Chunks[i].Disposed)
                        {
                            continue;
                        }

						if (Chunks[i].IsGenerated && !Chunks[i].ShouldBuild)
                        {
                            _activeChunks++;
                        }

						if ((Chunks[i].Position - _playerPosition).sqrMagnitude > (GraphicsOptions.ChunkLoaderRadius) * Chunk.ChunkSize * (GraphicsOptions.ChunkLoaderRadius) * Chunk.ChunkSize  )
                        {
                            World.RemoveChunk(Chunks[i]);
                            continue;
                        }


                        if (Chunks[i] != null && Chunks[i].IsGenerated)
                        {

							float CameraDist = (Chunks[i].Position - _position).sqrMagnitude;

                           // if (CameraDist > 288 * 288 && CameraDist < 576 * 576 && GraphicsOptions.Lod)
                            //    Chunks[i].Lod = 2;
                           // else if (CameraDist > 576 * 576 && GraphicsOptions.Lod)
                           //     Chunks[i].Lod = 4;
                           // else
                                Chunks[i].Lod = 1;
                        }


						if (Chunks[i] != null && Chunks[i].IsGenerated && !World.ContainsMeshQueue(Chunks[i]) && Chunks[i].ShouldBuild)
                        {
                           World.AddToQueue(Chunks[i], true);
                        }

                    }
                  //  _left = 0f;
                //}
                if (_activeChunks != _prevChunkCount)
                {
                    _prevChunkCount = _activeChunks;
                    ThreadManager.ExecuteOnMainThread(() => UpdateFog(2f, 3f));
                }
            }
        }
    }
}
