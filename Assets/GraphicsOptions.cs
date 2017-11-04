using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{

    public static class GraphicsOptions
    {

        public static int ChunkLoaderRadius { get; set; }

        static GraphicsOptions()
        {
            ChunkLoaderRadius = 20;
        }
    }
}
