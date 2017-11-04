using System;
using UnityEngine;

namespace Assets.Generation
{
    public class Chunk : IDisposable
    {
        public const int ChunkSize = 16;
        public Vector3 Offset { get; private set; }
        public bool ShouldBuild { get; private set; }
        public int Lod { get; set; }

        private readonly World _world;
        private readonly float[][][] _blocks = new float[ChunkSize][][];
        private readonly WorldGenerator _generator = new WorldGenerator();


        public Chunk(Vector3 Offset, World World)
        {
            this._world = World;
            this.Offset = Offset;
            this.Lod = 1;
        }

        public void Generate()
        {
            this._generator.BuildArray(this._blocks);
            this._generator.Generate(this._blocks, Offset);

            this.ShouldBuild = true;
        }

        public void Build() { }

        public void Dispose()
        {
            this._generator.Dispose();
        }
    }

}