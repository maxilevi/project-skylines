using System.Collections;
using System.Collections.Generic;
using Assets.Generation;
using UnityEngine;

public class World : MonoBehaviour {

	private readonly Dictionary<Vector3, Chunk> _chunks = new Dictionary<Vector3, Chunk>();
    private readonly MeshQueue _meshQueue = new MeshQueue();
    private readonly GenerationQueue _generationQueue = new GenerationQueue();

    public void AddChunk(Vector3 Offset, Chunk Chunk)
    {
        if (!this._chunks.ContainsKey(Offset))
        {
            this._chunks.Add(Offset, Chunk);
            this._generationQueue.Add(Chunk);
        }
    }
    public void RemoveChunk(Chunk Chunk) { }

    public Vector3 ToChunkSpace(Vector3 Position) { }

    public Chunk GetChunkByOffset(Vector3 Offset) { }

    public Dictionary<Vector3, Chunk> Chunks
    {
        get { return _chunks; }
    }

}
