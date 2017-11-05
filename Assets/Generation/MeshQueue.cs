/*
 * Author: Zaphyk
 * Date: 18/02/2016
 * Time: 05:11 p.m.
 *
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
		public const int SleepTime = 5;
		public const int ThreadCount = 2;
		public List<Chunk> Queue = new List<Chunk>();
		public bool Stop {get; set;}
		private World _world;
		private ClosestChunk _closestChunkComparer = new ClosestChunk();
		
		public MeshQueue(World World){
			Thread[] Threads = new Thread[ThreadCount];
			for(int i = 0; i < ThreadCount; i++){
				Threads[i] = new Thread(Start);
				Threads[i].Start();
			}
			this._player = World.Player;
			this._world = World;
		}
		
		public bool Contains(Chunk ChunkToCheck){
			lock(Queue)
				return Queue.Contains(ChunkToCheck);
		}
		
		public void Add(Chunk ChunkToBuild){
			lock(Queue){
				if(!Queue.Contains(ChunkToBuild))
					Queue.Add(ChunkToBuild);
			}
		}
		public bool Discard;
		public void SafeDiscard(){
			Discard = true;
		}
		
		public void Start(){
			try{
				while(ThreadManager.isPlaying){
					Thread.Sleep(SleepTime * ThreadCount);
					if( Stop)
						break;
					
					lock(Queue){
						if(Discard){
							Queue.Clear();
							Discard = false;
							continue;
						}
						if(Queue.Count != 0 ){
							
							if(_player != null){
								_closestChunkComparer.PlayerPos = _player.gameObject.transform.position;
								lock(Queue)
									Queue.Sort(_closestChunkComparer);
							}
							
							if(Queue[0] != null){
								Queue[0].Build();
								if(Queue.Count != 0)
									Queue.RemoveAt(0);
							}else
								Queue.RemoveAt(0);
						}
					}
				}
			}catch(Exception e){
				Debug.Log(e.ToString());
			}
		}
	}
}
