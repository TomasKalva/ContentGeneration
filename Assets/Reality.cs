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
        var worldGenrator = GetComponent<WorldGenerator>();
        var world = GameObject.Find("World").GetComponent<World>();

        worldGenrator.DestroyWorld();
        worldGenrator.Generate(world);
        ModuleGrid = (worldGenrator as GridWorldGenerator).moduleGrid;

        world.Created();
    }
}
