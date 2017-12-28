/*
 *Written by Maxi Levi <maxilevi@live.com>, November 2017
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Linq;


namespace Assets.Generation{

	public class GenerationQueue{

		public World _world;
		public List<Chunk> Queue = new List<Chunk>();
		public bool Stop {get; set;}
		private ClosestChunk _closestChunkComparer = new ClosestChunk();
		private int _exceptionCount = 0;

		public GenerationQueue(World World){
			bool useThreadPool = false;

			if (useThreadPool) {
				ThreadPool.QueueUserWorkItem ( new WaitCallback( delegate(object state)
					{ Start(); }) );
			}else{
				new Thread (Start).Start ();
			}
			this._world = World;
		}

		public void Sort(){
			_closestChunkComparer.PlayerPos = _world.PlayerPosition + _world.PlayerOrientation * Chunk.ChunkSize * 4f;
			Queue.Sort (_closestChunkComparer);
		}

		public void Add(Chunk c){
			lock (Queue)
				Queue.Add (c);
		}

		public void Remove(Chunk c){
			lock (Queue)
				Queue.Remove (c);
		}

		public void Start(){
			try{
				while(true){
					if( Stop)
						break;

					//Thread.Sleep(5);
					_world.MeshQueue = Queue.Count;

					Chunk workingChunk = null;
					lock(Queue) {

						workingChunk = Queue.FirstOrDefault();
						Queue.Remove(workingChunk);

					}

					if(workingChunk != null)
						workingChunk.Generate();
				}
			}catch(Exception e){
				if (_exceptionCount >= 3)
					return;
				new Thread (Start).Start ();
				Debug.Log (e.ToString());
			}
		}
	}
}