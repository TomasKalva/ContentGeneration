using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Util
{
    class CountdownTimer
    {
        float timeUntilFinished;

        public CountdownTimer(float timeUntilFinished)
        {
            this.timeUntilFinished = timeUntilFinished;
        }

        public bool Finished(float deltaT)
        {
            timeUntilFinished -= deltaT;
            return timeUntilFinished <= 0f;
        }
    }
}
