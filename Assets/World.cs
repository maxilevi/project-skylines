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
			this._generationQueue.Queue.Add(Chunk);
        }
    }
    public void RemoveChunk(Chunk Chunk) { 
		lock(_chunks){
			if (_chunks.ContainsKey (Chunk.Position))
				_chunks.Remove (Chunk.Position);
		}
		Chunk.Dispose ();
	}

    public Vector3 ToBlockSpace(Vector3 Vec3){
		
		int ChunkX = (int) Vec3.x >> Chunk.Bitshift;
		int ChunkY = (int) Vec3.y >> Chunk.Bitshift;
		int ChunkZ = (int) Vec3.z >> Chunk.Bitshift;
			
		ChunkX *= Chunk.ChunkSize;
		ChunkY *= Chunk.ChunkSize;
		ChunkZ *= Chunk.ChunkSize;
			
		int X = (int) Mathf.Floor( (Vec3.x - ChunkX) / Chunk.ChunkSize );
		int Y = (int) Mathf.Floor( (Vec3.y - ChunkX) / Chunk.ChunkSize );
		int Z = (int) Mathf.Floor( (Vec3.z - ChunkZ) / Chunk.ChunkSize );
			
		return new Vector3(X, Y ,Z);
	}
		
	public Chunk GetChunkAt(Vector3 Vec3){
		int ChunkX = (int) Vec3.x >> Chunk.Bitshift;
		int ChunkY = (int) Vec3.y >> Chunk.Bitshift;
		int ChunkZ = (int) Vec3.z >> Chunk.Bitshift;
			
		ChunkX *= Chunk.ChunkSize;
		ChunkY *= Chunk.ChunkSize;
		ChunkZ *= Chunk.ChunkSize;
			
		return this.GetChunkByOffset(ChunkX, ChunkY, ChunkZ);
	}
		
	public Vector3 ToChunkSpace(Vector3 Vec3){
		int ChunkX = (int) Vec3.x >> Chunk.Bitshift;
		int ChunkY = (int) Vec3.y >> Chunk.Bitshift;
		int ChunkZ = (int) Vec3.z >> Chunk.Bitshift;

		ChunkX *= Chunk.ChunkSize;
		ChunkY *= Chunk.ChunkSize;
		ChunkZ *= Chunk.ChunkSize;	
		
		return new Vector3(ChunkX, ChunkY, ChunkZ);
	}

    public Chunk GetChunkByOffset(Vector3 Offset) {
		lock(_chunks){
			if (_chunks.ContainsKey (Offset))
				return _chunks [Offset];
		}
		return null;
	}

    public Dictionary<Vector3, Chunk> Chunks
    {
        get { return _chunks; }
    }

}
