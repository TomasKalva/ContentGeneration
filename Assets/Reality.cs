using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class Reality : MonoBehaviour
{
    [SerializeField]
    PlayerCharacterState playerState;
    public PlayerCharacterState PlayerState
    {
        get => playerState;
    }

    private void Awake()
    {
        //var world = GameObject.Find("World").GetComponent<World>();
        //world.Initialize();

        CreateWorld();
    }

    public void CreateWorld()
    {
        var worldGenerator = GetComponent<WorldGenerator>();
        //var world = GameObject.Find("World").GetComponent<World>();

        //worldGenerator.DestroyWorld();
        worldGenerator.Generate();
;
    }
}
