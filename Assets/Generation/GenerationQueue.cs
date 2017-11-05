using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace Assets.Generation{
	
	public class GenerationQueue {

		public GameObject Player;
		public List<Chunk> Queue = new List<Chunk>();
		public static int ThreadCount = 2;
		public const int ThreadTime = 20;
		public bool Stop {get; set;}
		private ClosestChunk _closestChunkComparer = new ClosestChunk();
		private List<GenerationThread> _threads = new List<GenerationThread>();

		public GenerationQueue(){
			Thread MainLoop = new Thread(ProccessQueueThread);
			MainLoop.Start();
			for(int i = 0; i < ThreadCount; i++){
				_threads.Add(new GenerationThread());
			}
		}
		
		public bool Discard;
		public void SafeDiscard(){
			Discard = true;
		}
		
		private void ProccessQueueThread(){
			while(true){
				Thread.Sleep(5);
				if(!Stop)
					break;
				
				if(Discard){
					Queue.Clear();
					Discard = false;
					continue;
				}
				
				if(Queue.Count != 0){
					
					if(Player != null){
						_closestChunkComparer.PlayerPos = Player.transform.position;
						
						lock(Queue){
							try{
								Queue.Sort(_closestChunkComparer);
							}catch(Exception e){
								Debug.Log("Error while sorting chunks. Retrying...");
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
							Queue[0].IsGenerated = true;
							Queue.RemoveAt(0);
						}
					}
				}
			}
		}
	}
}
