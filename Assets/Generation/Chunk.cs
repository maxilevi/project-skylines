using System;
using UnityEngine;

namespace Assets.Generation
{
	public class Chunk : MonoBehaviour, IDisposable
    {
        public const int ChunkSize = 16;
		public const int Bitshift = 4;
        public Vector3 Position { get; private set; }
        public bool ShouldBuild { get; private set; }
		public bool IsGenerated { get; set; }
        public int Lod { get; set; }
		public bool Disposed {get; private set; }

        private World _world;
        private readonly float[][][] _blocks = new float[ChunkSize][][];
        private readonly WorldGenerator _generator = new WorldGenerator();


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
			bool completelyBuilded = true;
			var mesh = this.GetComponent<MeshFilter>().mesh;
			mesh.Clear ();

			//if(completelyBuilded)
		}

        public void Dispose()
        {
			this.Disposed = true;
            this._generator.Dispose();
			ThreadManager.ExecuteOnMainThread( () => Destroy (this.gameObject) );
        }
    }

}