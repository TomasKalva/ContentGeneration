using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.InteractiveObject
{
    public class AscensionKiln : InteractiveObjectState
    {
        Kiln Kiln => InteractiveObject.GetComponentInChildren<Kiln>();

        public override void Interact(global::Agent agent)
        {
            agent.CharacterState.Will.Maximum += 10f;
            Kiln?.BurstFire();
        }
    }
}
