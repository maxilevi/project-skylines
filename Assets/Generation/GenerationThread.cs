/*
 * Author: Zaphyk
 * Date: 07/04/2016
 * Time: 01:41 p.m.
 *
 */
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Generation
{
	/// <summary>
	/// Description of GenerationThread.
	/// </summary>
	public class GenerationThread
	{
		public Thread WorkingThread;
		public bool IsWorking{ get; set;}
		private GenerationQueue _queue;
		
		public Chunk CurrentChunk = null;
		public GenerationThread(GenerationQueue _queue)
		{
			this._queue = _queue;
			this.IsWorking = false;
			this.WorkingThread = new Thread(Start);
			this.WorkingThread.IsBackground = true;
			this.WorkingThread.Start();

		}
		
		public void Generate(Chunk c){
			IsWorking = true;
			CurrentChunk = c;

		}
		
		public void Start(){
			try{
				while(true){
					if(_queue.Stop) break;

					ThreadManager.Sleep(GenerationQueue.ThreadTime * GenerationQueue.ThreadCount);
					if(CurrentChunk != null && !CurrentChunk.Disposed){
						
						if(!CurrentChunk.IsGenerated){
							if(CurrentChunk != null) CurrentChunk.Generate();
						}
						CurrentChunk = null;
						IsWorking = false;
					}
					CurrentChunk = null;
					IsWorking = false;
				}
			}catch(Exception e){
				Debug.Log(e.ToString());
			}
		}
	}
}
