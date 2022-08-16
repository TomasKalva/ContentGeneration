using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class Lever : InteractiveObjectState
    {
        Action onPulled;

        public Lever(Action onPulled)
        {
            this.onPulled = onPulled;
        }

        public override void Interact(global::Agent agent)
        {
            onPulled();
        }
    }
