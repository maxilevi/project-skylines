using System.Collections;
using System.Collections.Generic;
using Assets.Generation;
using UnityEngine;

public class World : MonoBehaviour {

	private readonly Dictionary<Vector3, Chunk> _chunks = new Dictionary<Vector3, Chunk>();
    private readonly CommonQueue _queue = new CommonQueue();

    public void AddChunk(Vector3 Offset, Chunk Chunk)
    {
        if (!this._chunks.ContainsKey(Offset))
        {
            this._chunks.Add(Offset, Chunk);
            this._queue.Add(Chunk, QueueType.Generate);
        }
    }
    public void RemoveChunk(Vector3 Offset) { }

}
