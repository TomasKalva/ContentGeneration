using OurFramework.UI.Util;
using OurFramework.Gameplay.Data;

namespace OurFramework.Gameplay.RealWorld
{
    public class GraveState : InteractiveObjectState<InteractiveObject>
    {
        AgentSpawner PlayerSpawner => IntObj.GetComponentInChildren<AgentSpawner>();

        public PlayerCharacterState SpawnPlayer()
        {
            var playerAgent = PlayerSpawner.Spawn();
            var playerState = GameViewModel.ViewModel.PlayerState;
            playerAgent.CharacterState = playerState;
            return playerState;
        }
    }
}
