/*
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour {

	private static List<KeyValuePair<Action, Action>> Functions = new List<KeyValuePair<Action, Action>>();
	public static bool isPlaying = true;

	/// <summary>
	/// Executes the give method on the main thread after a frame has passed.
	/// </summary>
     public static void ExecuteOnMainThread(Action func)
     {
     	lock(Functions){
     		Functions.Add( new KeyValuePair<Action, Action>(func, () => NullCallBack()) );
     	}
     }
     
      public static void ExecuteOnMainThread(Action func, Action Callback)
     {
     	lock(Functions){
      		Functions.Add( new KeyValuePair<Action, Action>(func, Callback));
     	}
     }

	 /// <summary>
	 /// Custom sleep method
	 /// </summary>
	 public static void Sleep(int Milliseconds){
		long start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		while(true){
			long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			if (now - start >= Milliseconds)
				break;
		}
	 }
      
	 private static void NullCallBack(){}
     
     void Update()
     {
		isPlaying = Application.isPlaying;
     	lock(Functions){
	        for(int i = Functions.Count-1; i > -1; i--)
	        {
	         	Functions[i].Key();
	         	Functions[i].Value();
	         	Functions.RemoveAt(i);
	        }
     	}
    }
}
