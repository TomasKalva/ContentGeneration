using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField]
    Enemies enemies;

    [SerializeField]
    Items items;

    [SerializeField]
    InteractiveObjects interactiveObjects;

    [SerializeField]
    Objects objects;

    public void Generate(World world)
    {
        world.AddEnemy(enemies.sculpture, new Vector3(0, 0, -54));

        world.AddItem(items.blueIchorEssence, new Vector3(0, 0, -54));


        world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, -54));
    }
}
