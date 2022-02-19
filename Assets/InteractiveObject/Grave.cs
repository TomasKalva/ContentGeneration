using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.InteractiveObject
{
    public class Grave : InteractiveObjectState
    {
        AgentSpawner PlayerSpawner => InteractiveObject.GetComponentInChildren<AgentSpawner>();

        public override void Interact(global::Agent agent)
        {
            InteractiveObject.World.Grave = this;
        }

        public void SpawnPlayer()
        {
            var playerAgent = PlayerSpawner.Spawn();
            playerAgent.CharacterState = InteractiveObject.Reality.PlayerState;
        }
    }
}
