using System;

namespace Assets.Generation
{
    public class World
    {
        public World()
        {
            OpenSimplexNoise.Load( new Random().Next(int.MinValue, int.MaxValue) );
        }
    }

}