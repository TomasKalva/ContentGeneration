using Assets.InteractiveObject;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    List<InteractiveObjectState> interactiveObjects;
    List<CharacterState> enemies;
    List<Transform> objects;
    public Grave Grave { get; set; }

    public delegate void WorldCreated();
    public static event WorldCreated OnCreated;

    public IEnumerable<CharacterState> Enemies => enemies.Where(ch => ch != null && !ch.Dead);
    public IEnumerable<InteractiveObjectState> InteractiveObjects => interactiveObjects.Where(io => io != null);

    // Start is called before the first frame update
    void Awake()
    {
        //Initialize();
        interactiveObjects = new List<InteractiveObjectState>();// (FindObjectsOfType<InteractiveObject>());
        enemies = new List<CharacterState>();// (FindObjectsOfType<Agent>());
        objects = new List<Transform>();
    }

    /*
    public void Initialize()
    {
        interactiveObjects = new List<InteractiveObjectState>();// (FindObjectsOfType<InteractiveObject>());
        enemies = new List<CharacterState>();// (FindObjectsOfType<Agent>());
        objects = new List<Transform>();
    }*/

    public IEnumerable<InteractiveObjectState> ObjectsCloseTo(Vector3 point, float dist)
    {
        return InteractiveObjects.Where(o => (o.InteractiveObject.transform.position - point).sqrMagnitude <= dist * dist);
    }

    public void AddEnemy(CharacterState enemy)
    {
        enemies.Add(enemy);
    }

    public void AddItem(InteractiveObjectState item)
    {
        interactiveObjects.Add(item);
    }

    public void RemoveItem(InteractiveObjectState item)
    {
        interactiveObjects.Remove(item);
        GameObject.Destroy(item.InteractiveObject.gameObject);
    }

    public void AddInteractiveObject(InteractiveObjectState interactiveObject)
    {
        interactiveObjects.Add(interactiveObject);
    }

    public void AddObject(Transform obj)
    {
        objects.Add(obj);
    }

    public void OnPlayerDeath()
    {
        Grave.SpawnPlayer();
    }

    public void Created()
    {
        //Grave = GameObject.FindGameObjectWithTag("DefaultSpawnPoint").GetComponent<InteractiveObject>().State as Grave;

        if (Grave != null)
        {
            Grave.SpawnPlayer();
        }
        else
        {
            Debug.LogError("DefaultBonfire wasn't created!");
        }

        if (OnCreated != null)
        {
            OnCreated();
        }
    }
}
