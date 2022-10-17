using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;

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
