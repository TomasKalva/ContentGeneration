using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Grave : InteractiveObjectState<InteractiveObject>
{
    AgentSpawner PlayerSpawner => InteractiveObject.GetComponentInChildren<AgentSpawner>();

    public override void Interact(global::Agent agent)
    {
        World.Grave = this;
    }

    public PlayerCharacterState SpawnPlayer()
    {
        var playerAgent = PlayerSpawner.Spawn();
        playerAgent.CharacterState = InteractiveObject.Reality.PlayerState;
        return InteractiveObject.Reality.PlayerState;
    }
}
