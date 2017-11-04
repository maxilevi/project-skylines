/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 07:36 p.m.
 *
 */
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.WSA;


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

        public ChunkLoader()
        {
            var t1 = new Thread(this.LoadChunks);
            var t2 = new Thread(this.ManageChunks);
            var t3 = new Thread(this.ManageChunksMesh);

            t1.Start();
            t2.Start();
            t3.Start();

            StartCoroutine(FogLerpCoroutine());

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
                    RenderSettings.fogEndDistance = _maxFog;
                    RenderSettings.fogStartDistance = _minFog;
                }

                yield return null;
                yield return null;
            }
        }


        private void LoadChunks()
        {
            try
            {
                while (true)
                {

                    if (!Enabled || World.MeshQueue.Discard)
                        goto SLEEP;

                    Offset = World.ToChunkSpace(Player.transform.position);
                    if (Offset == Vector3.Zero)
                        goto SLEEP;

                    if (Offset != _lastOffset)
                    {

                        for (int x = -GraphicsOptions.ChunkLoaderRadius / 2; x < GraphicsOptions.ChunkLoaderRadius / 2; x++)
                        {
                            for (int z = -GraphicsOptions.ChunkLoaderRadius / 2; z < GraphicsOptions.ChunkLoaderRadius / 2; z++)
                            {
                                for (int y = -GraphicsOptions.ChunkLoaderRadius / 2; y < GraphicsOptions.ChunkLoaderRadius / 2; y++)
                                {

                                    if (World.GetChunkByOffset(Offset + new Vector3(x, y, z) * new Vector4(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize)) == null)
                                    {
                                        Vector3 chunkPos = Offset + new Vector3(x, z) * new Vector3(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize);
                                        Chunk chunk = new Chunk(chunkPos, World);
                                        World.AddChunk(chunk);
                                    }
                                }
                            }
                        }
                        _lastRadius = GraphicsOptions.ChunkLoaderRadius;
                        _lastOffset = Offset;

                    }
                    SLEEP:
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void ManageChunksMesh()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(250);

                    if (!Enabled)
                        continue;

                    Chunk[] Chunks;
                    lock (World.Chunks)
                    {
                        Chunks = World.Chunks.ToArray();
                    }

                    _left += 0.25f;

                    if (_left >= 1.5f)
                    {
                        _activeChunks = 0;
                        for (int i = Chunks.Length - 1; i > -1; i--)
                        {

                            if (Chunks[i].Disposed)
                            {
                                continue;
                            }

                            if (Chunks[i].IsGenerated && Chunks[i].BuildedWithStructures)
                            {
                                _activeChunks++;
                            }

                            if ((Chunks[i].Position.Xz - Player.Position.Xz).LengthSquared > (GraphicsOptions.ChunkLoaderRadius) * .5f * Chunk.CHUNK_WIDTH * (GraphicsOptions.ChunkLoaderRadius) * .5f * Chunk.CHUNK_WIDTH)
                            {
                                World.RemoveChunk(Chunks[i]);
                                continue;
                            }


                            if (Chunks[i] != null && Chunks[i].IsGenerated)
                            {

                                float CameraDist = (Chunks[i].Position - transform.position).LengthSquared;

                                if (CameraDist > 288 * 288 && CameraDist < 576 * 576 && GraphicsOptions.LOD)
                                    Chunks[i].Lod = 2;
                                else if (CameraDist > 576 * 576 && GraphicsOptions.LOD)
                                    Chunks[i].Lod = 4;
                                else
                                    Chunks[i].Lod = 1;
                            }


                            if (Chunks[i] != null && Chunks[i].IsGenerated !Player.World.MeshQueue.Contains(Chunks[i]) && Chunks[i].ShouldBuild)
                            {
                               World.AddChunkToQueue(Chunks[i], true);
                            }

                        }
                        _left = 0f;
                    }
                    if (_activeChunks != _prevChunkCount)
                    {
                        _prevChunkCount = _activeChunks;
                        ThreadManager.ExecuteOnMainThread(() => UpdateFog(2f, 3f));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
}
