using Assets.InteractiveObject;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{

    List<InteractiveObject> interactiveObjects;
    List<Agent> agents;
    public GraveState Grave { get; set; }

    public delegate void WorldCreated();
    public static event WorldCreated OnCreated;

    public IEnumerable<Agent> Agents => agents.Where(a => a != null && !a.CharacterState.Dead);
    public IEnumerable<InteractiveObject> InteractiveObjects => interactiveObjects.Where(io => io != null);

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        interactiveObjects = new List<InteractiveObject>(FindObjectsOfType<InteractiveObject>());
        agents = new List<Agent>(FindObjectsOfType<Agent>());
    }

    public IEnumerable<InteractiveObject> ObjectsCloseTo(Vector3 point, float dist)
    {
        return InteractiveObjects.Where(o => (o.transform.position - point).sqrMagnitude <= dist * dist);
    }

    public void AddEnemy(Agent enemy, Vector3 position)
    {
        enemy.transform.position = position;
        agents.Add(enemy);
    }

    public void AddItem(InteractiveObject item, Vector3 position)
    {
        item.transform.position = position;
    }

    public void AddInteractiveObject(InteractiveObject interactiveObject, Vector3 position)
    {
        interactiveObject.transform.position = position;
    }

    public void AddObject(GameObject objectPrefab, Vector3 position)
    {
        var obj = Instantiate(objectPrefab);
        obj.transform.position = position;
    }

    public void OnPlayerDeath()
    {
        Grave.SpawnPlayer();
    }

    public void Created()
    {
        Initialize();
        Grave = GameObject.FindGameObjectWithTag("DefaultSpawnPoint").GetComponent<InteractiveObject>().State as GraveState;

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
