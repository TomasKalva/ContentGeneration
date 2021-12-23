using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Reality : MonoBehaviour
{
    [SerializeField]
    PlayerCharacterState playerState;
    public PlayerCharacterState PlayerState
    {
        get => playerState;
    }

    public ModuleGrid ModuleGrid { get; set; }

    private void Start()
    {
        var worldGenerator = GetComponent<WorldGenerator>();
        var world = GameObject.Find("World").GetComponent<World>();

        worldGenerator.DestroyWorld();
        worldGenerator.Generate(world);
        if(worldGenerator is GridWorldGenerator gridWorldGenerator)
            ModuleGrid = gridWorldGenerator.moduleGrid;

        world.Created();
    }
}
