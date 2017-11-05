using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour {

	private static List<KeyValuePair<Action, Action>> Functions = new List<KeyValuePair<Action, Action>>();
		
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
      
	 private static void NullCallBack(){}
     
     void Update()
     {
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
