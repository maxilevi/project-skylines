using System;
using UnityEngine;
using Assets.Rendering;

namespace Assets.Generation
{
	public class Chunk : MonoBehaviour, IDisposable
    {
        public const int ChunkSize = 16;
		public const int Bitshift = 4;
        public Vector3 Position { get; private set; }
		public bool ShouldBuild;
		public bool IsGenerated;
		public int Lod;
		public bool Disposed {get; private set; }

		private Mesh _mesh;
        private World _world;
        private readonly float[][][] _blocks = new float[ChunkSize][][];
        private readonly WorldGenerator _generator = new WorldGenerator();

		void Start(){
			_mesh = new Mesh (); 
			this.GetComponent<MeshFilter>().mesh = _mesh;
			this.GetComponent<MeshRenderer> ().material = _world.WorldMaterial;
		}

		public void Init(Vector3 Position, World World)
        {
            this._world = World;
			this.Position = Position;
            this.Lod = 1;
        }

        public void Generate()
        {
            this._generator.BuildArray(this._blocks);
			this._generator.Generate(this._blocks, this.Position);
			this.IsGenerated = true;

            this.ShouldBuild = true;
        }

		public void Build() 
		{
			this.ShouldBuild = false;
			bool completelyBuilded = true;

			int WIDTH = _blocks.Length;
			int HEIGHT = _blocks[0].Length;
			int DEPTH = _blocks[0][0].Length;

			bool Next = false;
			GridCell Cell = new GridCell();
			Cell.P = new Vector3[8];
			Cell.Density = new double[8];

			VertexData BlockData = new VertexData ();
			Chunk RightChunk = _world.GetChunkByOffset (this.Position + new Vector3(ChunkSize, 0, 0));
			Chunk TopChunk = _world.GetChunkByOffset (this.Position + new Vector3(0, ChunkSize, 0));
			Chunk FrontChunk = _world.GetChunkByOffset (this.Position + new Vector3(0, 0, ChunkSize));

			Chunk RightFrontChunk  = _world.GetChunkByOffset (this.Position + new Vector3(ChunkSize, 0, ChunkSize));
			Chunk TopFrontChunk  = _world.GetChunkByOffset (this.Position + new Vector3(0, ChunkSize, ChunkSize));
			Chunk TopRightFrontChunk  = _world.GetChunkByOffset (this.Position + new Vector3(ChunkSize, ChunkSize, ChunkSize));
			Chunk TopRightChunk  = _world.GetChunkByOffset (this.Position + new Vector3(ChunkSize, ChunkSize, 0));


			for (int y = 0; y < ChunkSize; y++) {
				
				for (int x = 0; x < ChunkSize; x+=1) {
					Next = !Next;
					for (int z = 0; z < ChunkSize; z+=1) {
						Next = !Next;

						bool Success;
						this.CreateCell (x,y,z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk,
							TopFrontChunk, TopRightFrontChunk, TopChunk, Cell, out Success);

						if (!Success)
							completelyBuilded = false;

						//if (!MarchingCubes.Usable (0f, Cell))
						//	continue;


						MarchingCubes.Process(0f, Cell, new Color(1f,0,0,1f), Next, BlockData);

					}
				}
			}

			if (!completelyBuilded)
				ShouldBuild = true;

			ThreadManager.ExecuteOnMainThread (delegate {
				_mesh.Clear();
				_mesh.SetVertices (BlockData.Vertices);
				_mesh.SetNormals (BlockData.Normals);
				_mesh.SetColors (BlockData.Colors);
				_mesh.SetIndices (BlockData.Indices.ToArray (), MeshTopology.Triangles, 0);
			});
		}

		private void CreateCell(float x, float y, float z, Chunk RightChunk, Chunk FrontChunk, Chunk RightFrontChunk, Chunk TopRightChunk, Chunk TopFrontChunk, Chunk TopRightFrontChunk, Chunk TopChunk, GridCell Cell, out bool Success){
			Success = true;
			this.BuildCell (x,y,z,Cell);
			int _x = (int)x, _y = (int)y, _z = (int)z;

			Cell.Density [0] = this.GetNeighbourDensity ( _x,  _y,  _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
			Cell.Density [1] = this.GetNeighbourDensity ( _x+1,  _y,  _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
			Cell.Density [2] = this.GetNeighbourDensity ( _x+1,  _y,  _z+1, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
			Cell.Density [3] = this.GetNeighbourDensity ( _x,  _y,  _z+1, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
			Cell.Density [4] = this.GetNeighbourDensity ( _x,  _y+1,  _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
			Cell.Density [5] = this.GetNeighbourDensity ( _x+1,  _y+1,  _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
			Cell.Density [6] = this.GetNeighbourDensity ( _x+1,  _y+1,  _z+1, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
			Cell.Density [7] = this.GetNeighbourDensity ( _x,  _y+1,  _z+1, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);

			for (int i = 0; i < Cell.Density.Length; i++) {
				if (Cell.Density [i] == 0) {
					Success = false;
					break;
				}
			}
		}

		private void BuildCell(float x, float y, float z, GridCell Cell){
			Cell.P[0] = new Vector3(x,y,z);
			Cell.P[1] = new Vector3(x+1,y,z);
			Cell.P[2] = new Vector3(x+1,y,z+1);
			Cell.P[3] = new Vector3(x,y,z+1);
			Cell.P[4] = new Vector3(x,y+1,z);
			Cell.P[5] = new Vector3(x+1,y+1,z);
			Cell.P[6] = new Vector3(x+1,y+1,z+1);
			Cell.P[7] = new Vector3(x,y+1,z+1);
		}

		private float GetNeighbourDensity( int x, int y, int z, Chunk RightChunk, Chunk FrontChunk, Chunk RightFrontChunk, Chunk TopRightChunk, Chunk TopFrontChunk, Chunk TopRightFrontChunk, Chunk TopChunk){
			int WIDTH = ChunkSize;
			int HEIGHT = ChunkSize;
			int DEPTH = ChunkSize;

			bool bX = x >= ChunkSize;
			bool bY = y >= ChunkSize;
			bool bZ = z >= ChunkSize;


			if(bX && !bY && bZ && RightFrontChunk != null && !RightFrontChunk.Disposed && RightFrontChunk.IsGenerated)
				return RightFrontChunk.GetBlockAt(x-WIDTH,y,z-DEPTH);

			if(bZ && !bY && !bX && FrontChunk != null && !FrontChunk.Disposed && FrontChunk.IsGenerated)
				return FrontChunk.GetBlockAt(x,y,z-DEPTH);

			if(bX && !bY && !bZ && RightChunk != null && !RightChunk.Disposed && RightChunk.IsGenerated)
				return RightChunk.GetBlockAt(x-WIDTH,y,z);

			if(bX && bY && bZ && TopRightFrontChunk != null && !TopRightFrontChunk.Disposed && TopRightFrontChunk.IsGenerated)
				return TopRightFrontChunk.GetBlockAt(x-WIDTH,y-HEIGHT,z-DEPTH);

			if(!bX && bY && bZ && TopFrontChunk != null && !TopFrontChunk.Disposed && TopFrontChunk.IsGenerated)
				return TopFrontChunk.GetBlockAt(x,y-HEIGHT,z-DEPTH);

			if(bX && bY && !bZ && TopRightChunk != null && !TopRightChunk.Disposed && TopRightChunk.IsGenerated)
				return TopRightChunk.GetBlockAt(x-WIDTH,y-HEIGHT,z);

			if(!bX && bY && !bZ && TopChunk != null && !TopChunk.Disposed && TopChunk.IsGenerated)
				return TopChunk.GetBlockAt(x,y-HEIGHT,z);

			if (!bX && !bY && !bZ)
				return _blocks[x][y][z];


			return 0;
		}

		public float GetBlockAt(int x, int y, int z){
			return _blocks[x][y][z];
		}

        public void Dispose()
        {
			this.Disposed = true;
            this._generator.Dispose();
			ThreadManager.ExecuteOnMainThread( () => Destroy (this.gameObject) );
        }
    }

}