using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class WorldGenerator : MonoBehaviour
{
    [SerializeField]
    protected Enemies enemies;

    [SerializeField]
    protected PhysicalItems items;

    [SerializeField]
    protected InteractiveObjects interactiveObjects;

    [SerializeField]
    protected Objects objects;

    public virtual void Generate(World world)
    {
        //world.AddEnemy(enemies.sculpture, new Vector3(0, 0, -54));

        Debug.Log("Generating world");

        //world.AddItem(items.BlueIchorEssence, new Vector3(0, 0, -54));


        world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, -54));
    }

    public abstract void DestroyWorld();
}
