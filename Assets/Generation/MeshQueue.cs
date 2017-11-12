/* Copyright (C) Luaek - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */
 
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation
{
	/// <summary>
	/// Description of MeshBuilderQueue.
	/// </summary>
	public class MeshQueue
	{
		public GameObject _player;
		public const int SleepTime = 1;
		public const int ThreadCount = 2;
		public List<Chunk> Queue = new List<Chunk>();
		public bool Stop {get; set;}
		private World _world;
		private ClosestChunk _closestChunkComparer = new ClosestChunk();
		
		public MeshQueue(World World){
			//new Thread (Start).Start ();
			this._player = World.Player;
			this._world = World;
		}
		
		public bool Contains(Chunk ChunkToCheck){
			return Queue.Contains(ChunkToCheck);
		}
		
		public void Add(Chunk ChunkToBuild){
			if(!Queue.Contains(ChunkToBuild))
				Queue.Add(ChunkToBuild);

			//_closestChunkComparer.PlayerPos = _world.PlayerPosition;
			//Queue.Sort (_closestChunkComparer);
		}
		
		public void Start(){
			while(true){
				if( Stop)
					break;
							
				//ThreadManager.Sleep (1);
					
				if (Queue.Count != 0) {
					Queue [0].Build();
					Queue.RemoveAt (0);
				}
			}
		}
	}
}
