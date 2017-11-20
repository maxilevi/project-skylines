/*
 *Written by Maxi Levi <maxilevi@live.com>, November 2017
*/
 
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Assets.Generation
{
	
	public class ClosestChunk: IComparer<Chunk> {
		
		public Vector3 PlayerPos;
		public ClosestChunk(){ 
		}
		public ClosestChunk(Vector3 pos){
			PlayerPos = pos;
		}
	
		public int Compare(Chunk V1, Chunk V2){
			try{
				if(V1 == V2) return 0;
				
				if(V1 == null)
					return -1;
				
				if(V2 == null)
					return 1;
				
				float V1f = (V1.Position - PlayerPos).sqrMagnitude;
				float V2f = (V2.Position - PlayerPos).sqrMagnitude;

				if(V1f < V2f){
					return -1;
				}else if(V1f == V2f){
					return 0;
				}else{
					return 1;
				}
			}catch(ArgumentException e){
				Debug.Log("Unable to sort the chunks properly. " +  e.Message);
				return 0;
			}
		}
	}
}
