/* Copyright (C) Luaek - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{

    public static class GraphicsOptions
    {

        public static int ChunkLoaderRadius { get; set; }
		public static bool Lod { get; set; }

        static GraphicsOptions()
        {
            ChunkLoaderRadius = 10;
			Lod = true;
        }
    }
}
