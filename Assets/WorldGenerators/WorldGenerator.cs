using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField]
    protected Enemies enemies;

    [SerializeField]
    protected PhysicalItems physicalItems;

    [SerializeField]
    protected Items items;

    [SerializeField]
    protected InteractiveObjects interactiveObjects;

    [SerializeField]
    protected Objects objects;

    [SerializeField]
    protected Libraries libraries;

    public virtual void Generate(World world)
    {
        //world.AddEnemy(enemies.sculpture, new Vector3(0, 0, -54));

        Debug.Log("Generating world");

        //world.AddItem(items.BlueIchorEssence, new Vector3(0, 0, -54));


        world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, 0));
    }

    public virtual void DestroyWorld() { }
}
