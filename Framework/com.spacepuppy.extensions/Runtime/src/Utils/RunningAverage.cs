using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy.Utils
{

    public struct RunningAverage
    {
        public float Average;
        public int Count;
        public int MaxCount;

        public RunningAverage(int max)
        {
            this.Average = 0f;
            this.Count = 0;
            this.MaxCount = max;
        }

        public void Add(float value)
        {
            this.Count++;
            int cnt = this.MaxCount > 0 ? System.Math.Min(this.Count, this.MaxCount) : this.Count;
            this.Average = ((this.Average * cnt) - this.Average + value) / cnt;
        }

    }

}
