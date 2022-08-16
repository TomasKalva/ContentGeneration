using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AscensionKiln : InteractiveObjectState<Kiln>
{
    //Kiln Kiln => InteractiveObject.GetComponentInChildren<Kiln>();

    public override void Interact(global::Agent agent)
    {
        agent.CharacterState.Stamina.Maximum += 10f;
        IntObj?.BurstFire();
    }
}
