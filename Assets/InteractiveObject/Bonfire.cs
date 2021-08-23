using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonfire : InteractiveObject
{
    [SerializeField]
    AgentSpawner playerSpawner;

    [SerializeField]
    PlayerCharacterState playerState;

    protected override void InteractLogic(Agent agent)
    {
        //if (agent.CharacterState is PlayerCharacterState playerState)
        {
            world.Bonfire = this;
            //playerState.SpawnPoint = this;
        }
    }

    public void SpawnPlayer()
    {
        var playerAgent = playerSpawner.Spawn();
        Debug.Log("before state set");
        playerAgent.CharacterState = playerState;
    }
}
