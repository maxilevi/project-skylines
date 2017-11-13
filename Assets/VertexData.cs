/* Copyright (C) Luaek - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Rendering{
	public class VertexData {
		
		public List<Vector3> Vertices = new List<Vector3>();
		public List<Vector3> Normals = new List<Vector3>();
		public List<int> Indices = new List<int>();
	}
}