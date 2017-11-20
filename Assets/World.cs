/*
 *Written by Maxi Levi <maxilevi@live.com>, November 2017
*/

using System.Collections;
using System.Collections.Generic;
using Assets.Generation;
using UnityEngine;

public class World : MonoBehaviour {

	public GameObject Player;
	public Material WorldMaterial;
	public Vector3 PlayerPosition, PlayerOrientation;
	public int GenQueue, MeshQueue;
	public readonly Dictionary<Vector3, Chunk> Chunks = new Dictionary<Vector3, Chunk>();
	private MeshQueue _meshQueue;
	private GenerationQueue _generationQueue;
	public int ChunkLoaderRadius = 8;
	public bool Loaded {get; set;}

	void Awake(){
		Application.targetFrameRate = -1;
		_meshQueue = new MeshQueue (this);
		_generationQueue = new GenerationQueue (this);
		Loaded = true;
	}

	void Update(){

		PlayerPosition = Player.transform.position;
		PlayerOrientation = Player.transform.forward;

		int _genCount = 0, _meshCount = 0;
		foreach (KeyValuePair<Vector3, Chunk> Pair in Chunks) {
			if (!Pair.Value.IsGenerated)
				_genCount++;

			if (Pair.Value.ShouldBuild)
				_meshCount++;
		}

		GenQueue = _genCount;
		//MeshQueue = _meshCount;
	}

	void OnApplicationQuit(){
		_meshQueue.Stop = true;
		_generationQueue.Stop = true;
	}

	public void SortGenerationQueue(){
		_generationQueue.Sort ();
	}

	public void SortMeshQueue(){
		_meshQueue.Sort ();
	}

	public void AddToQueue(Chunk Chunk, bool DoMesh)
	{
		if (DoMesh) {
			_meshQueue.Add (Chunk);
		} else {
			_generationQueue.Add (Chunk);
		}
	}

    public void AddChunk(Vector3 Offset, Chunk Chunk)
    {
		lock (this.Chunks) {
			if (!this.Chunks.ContainsKey (Offset)) {
				this.Chunks.Add (Offset, Chunk);
				this._generationQueue.Add (Chunk);
			}
		}
    }
    public void RemoveChunk(Chunk Chunk) { 
		lock(Chunks){
			if (Chunks.ContainsKey (Chunk.Position))
				Chunks.Remove (Chunk.Position);

			_meshQueue.Remove (Chunk);
			_generationQueue.Remove (Chunk);
		}
		Chunk.Dispose ();
	}

	public bool ContainsMeshQueue(Chunk chunk){
		return _meshQueue.Contains(chunk);
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
}
