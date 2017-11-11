/* Copyright (C) Luaek - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */

using System.Collections;
using System.Collections.Generic;
using Assets.Generation;
using UnityEngine;

public class World : MonoBehaviour {

	public GameObject Player;
	public Material WorldMaterial;
	public Vector3 PlayerPosition;
	public int GenQueue, MeshQueue;
	public readonly Dictionary<Vector3, Chunk> Chunks = new Dictionary<Vector3, Chunk>();
	private MeshQueue _meshQueue;
    private GenerationQueue _generationQueue;

	void Awake(){
		Application.targetFrameRate = -1;
		_meshQueue = new MeshQueue (this);
		_generationQueue = new GenerationQueue (this);
		StartCoroutine(_generationQueue.Start());
	}

	void Update(){
		PlayerPosition = Player.transform.position;
		GenQueue = _generationQueue.Queue.Count;
		MeshQueue = _meshQueue.Queue.Count;
	}

	void OnApplicationQuit(){
		_meshQueue.Stop = true;
		_generationQueue.Stop = true;
	}

	public void AddToQueue(Chunk Chunk, bool DoMesh)
	{
		if (DoMesh) {
			_meshQueue.Add (Chunk);
		} else {
			_generationQueue.Queue.Add (Chunk);
		}
	}

    public void AddChunk(Vector3 Offset, Chunk Chunk)
    {
		lock (this.Chunks) {
			if (!this.Chunks.ContainsKey (Offset)) {
				this.Chunks.Add (Offset, Chunk);
				this._generationQueue.Queue.Add (Chunk);
			}
		}
    }
    public void RemoveChunk(Chunk Chunk) { 
		lock(Chunks){
			if (Chunks.ContainsKey (Chunk.Position))
				Chunks.Remove (Chunk.Position);
		}
		Chunk.Dispose ();
	}

	public bool ContainsMeshQueue(Chunk chunk){
		lock(_meshQueue) return _meshQueue.Contains(chunk);
	}

	public bool ContainsGenerationQueue(Chunk chunk){
		lock(_generationQueue) return _generationQueue.Queue.Contains(chunk);
	}

    public Vector3 ToBlockSpace(Vector3 Vec3){
		
		int ChunkX = (int) Vec3.x >> Chunk.Bitshift;
		int ChunkY = (int) Vec3.y >> Chunk.Bitshift;
		int ChunkZ = (int) Vec3.z >> Chunk.Bitshift;
			
		ChunkX *= Chunk.ChunkSize;
		ChunkY *= Chunk.ChunkSize;
		ChunkZ *= Chunk.ChunkSize;
			
		int X = (int) Mathf.Floor( (Vec3.x - ChunkX) / (float) Chunk.ChunkSize );
		int Y = (int) Mathf.Floor( (Vec3.y - ChunkY) / (float) Chunk.ChunkSize );
		int Z = (int) Mathf.Floor( (Vec3.z - ChunkZ) / (float) Chunk.ChunkSize );

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
	public Chunk GetChunkByOffset(float X, float Y, float Z) {
		return this.GetChunkByOffset (new Vector3(X,Y,Z));
	}
    public Chunk GetChunkByOffset(Vector3 Offset) {
		lock(Chunks){
			if (Chunks.ContainsKey (Offset))
				return Chunks [Offset];
		}
		return null;
	}

	public bool Discard{
		get{ return (_meshQueue != null) ? _meshQueue.Discard : true; }
	}

}
