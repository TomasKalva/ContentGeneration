using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{

    List<InteractiveObject> interactiveObjects;
    List<Agent> agents;
    public Bonfire Bonfire { get; set; }

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

    public void AddItem(PhysicalItem itemPrefab, Vector3 position)
    {
        var item = Instantiate(itemPrefab);
        item.transform.position = position;
    }

    public void AddInteractiveObject(InteractiveObject interactiveObjectPrefab, Vector3 position)
    {
        var interactiveObject = Instantiate(interactiveObjectPrefab);
        interactiveObject.transform.position = position;
    }

    public void AddObject(GameObject objectPrefab, Vector3 position)
    {
        var obj = Instantiate(objectPrefab);
        obj.transform.position = position;
    }

    public void OnPlayerDeath()
    {
        Bonfire.SpawnPlayer();
    }

    public void Created()
    {
        Initialize();
        Bonfire = GameObject.FindGameObjectWithTag("DefaultSpawnPoint").GetComponent<Bonfire>();

        if (Bonfire != null)
        {
            Bonfire.SpawnPlayer();
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
