using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonfire : InteractiveObject
{
    [SerializeField]
    AgentSpawner playerSpawner;

    public override void Interact(Agent agent)
    {
        var playerState = agent.CharacterState as PlayerCharacterState;
        if (playerState != null)
        {
            playerState.SpawnPoint = this;
        }
        GameViewModel.ViewModel.Message = "Spawn point set";
    }

    public void SpawnPlayer()
    {
        playerSpawner.Spawn();
}
}
