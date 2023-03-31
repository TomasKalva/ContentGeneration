using OurFramework.UI.Util;
using OurFramework.Gameplay.State;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// State of a grave that can spawn player.
    /// </summary>
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
