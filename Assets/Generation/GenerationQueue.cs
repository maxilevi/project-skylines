using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace Assets.Generation{
	
	public class GenerationQueue {

		public World _world;
		public List<Chunk> Queue = new List<Chunk>();
		public static int ThreadCount = 1;
		public const int ThreadTime = 20;
		public bool Stop {get; set;}
		private ClosestChunk _closestChunkComparer = new ClosestChunk();
		private List<GenerationThread> _threads = new List<GenerationThread>();

		public GenerationQueue(World World){
			Thread MainLoop = new Thread(ProccessQueueThread);
			MainLoop.IsBackground = true;
			MainLoop.Start();
			for(int i = 0; i < ThreadCount; i++){
				_threads.Add(new GenerationThread(this));
			}
			this._world = World;
		}
		
		public bool Discard;
		public void SafeDiscard(){
			Discard = true;
		}
		
		private void ProccessQueueThread(){
			while(true){
				if(Stop)
					break;
				
				ThreadManager.Sleep(5);
				
				if(Discard){
					Queue.Clear();
					Discard = false;
					continue;
				}
				
				if(Queue.Count != 0){

					if(_world.Player != null){
						_closestChunkComparer.PlayerPos = _world.PlayerPosition;
						
						lock(Queue){
							try{
								Queue.Sort(_closestChunkComparer);
							}catch(Exception e){
								Debug.Log(e.ToString());
							}
						}
					}
					if(Queue[0] == null){
						Queue.RemoveAt(0);
						continue;
					}

					for(int i = 0; i < _threads.Count; i++){
						if(!_threads[i].IsWorking && Queue.Count != 0){
							_threads[i].Generate(Queue[0]);
							Queue.RemoveAt(0);
						}
					}
				}
			}
		}
	}
}
