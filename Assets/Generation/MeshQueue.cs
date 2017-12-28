/*
 *Written by Maxi Levi <maxilevi@live.com>, November 2017
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
		public List<Chunk> Queue = new List<Chunk>();
		public Dictionary<Chunk, int> _queueDict = new Dictionary<Chunk, int>();
		public bool Stop {get; set;}
		private World _world;
		private ClosestChunk _closestChunkComparer = new ClosestChunk(); 
		private int _exceptionCount = 0;
		
		public MeshQueue(World World){
			bool useThreadPool = false;

			if (useThreadPool) {
				ThreadPool.QueueUserWorkItem ( new WaitCallback( delegate(object state)
					{ Start(); }) );
			} else {
				new Thread (Start).Start ();
			}
			this._player = World.Player;
			this._world = World;
		}

		public void Sort(){
			_closestChunkComparer.PlayerPos = _world.PlayerPosition + _world.PlayerOrientation * Chunk.ChunkSize * 4f;
			Queue.Sort (_closestChunkComparer);
		}

		public bool Contains(Chunk ChunkToCheck){
			lock (Queue) return _queueDict.ContainsKey(ChunkToCheck);
		}
		
		public void Add(Chunk ChunkToBuild){
			lock (Queue) {
				_queueDict.Add (ChunkToBuild, 0);
				Queue.Add (ChunkToBuild);
			}
		}

		public void Remove(Chunk ChunkToBuild){
			lock (Queue) {
				Queue.Remove (ChunkToBuild);
				if(_queueDict.ContainsKey(ChunkToBuild))
					_queueDict.Remove (ChunkToBuild);
			}
		}
		
		public void Start(){
			try{
				while(true){
					if( Stop)
						break;

					Thread.Sleep(5);
					_world.MeshQueue = Queue.Count;

					Chunk workingChunk = null;
					lock(Queue) {
						Sort();
						workingChunk = Queue.FirstOrDefault();
						Queue.Remove(workingChunk);
						if(workingChunk != null && _queueDict.ContainsKey(workingChunk))
							_queueDict.Remove(workingChunk);
					}

					if(workingChunk != null)
						workingChunk.Build();
				}
			}catch(Exception e){
				if (_exceptionCount >= 3)
					return;
				new Thread (Start).Start ();
				_exceptionCount++;
				Debug.Log (e.ToString());
			}
		}
	}
}
