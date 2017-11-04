using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Assets.Generation
{

    public class CommonQueue
    {
        public const int NumThreads = 2;
        public Thread[] Threads = new Thread[NumThreads];

        public CommonQueue()
        {
            for (int i = 0; i < this.Threads.Length; i++)
            {
                //this.Threads[i] = new Thread();
            }
        }

    }

    public enum QueueType
    {
        Generate,
        Build
    }
}