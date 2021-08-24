using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonfire : InteractiveObject
{
    [SerializeField]
    AgentSpawner playerSpawner;

    protected override void InteractLogic(Agent agent)
    {
        world.Bonfire = this;
    }

    public void SpawnPlayer()
    {
        var playerAgent = playerSpawner.Spawn();
        playerAgent.CharacterState = reality.PlayerState;
    }
}
