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
        interactiveObjects = new List<InteractiveObject>(FindObjectsOfType<InteractiveObject>());
        agents = new List<Agent>(FindObjectsOfType<Agent>());
    }

    public IEnumerable<InteractiveObject> ObjectsCloseTo(Vector3 point, float dist)
    {
        return InteractiveObjects.Where(o => (o.transform.position - point).sqrMagnitude <= dist * dist);
    }

    public void AddAgent(Agent agent)
    {
        agents.Add(agent);
    }

    public void OnPlayerDeath()
    {
        Bonfire.SpawnPlayer();
    }

    public void Created()
    {
        Bonfire = GameObject.Find("DefaultBonfire").GetComponent<Bonfire>();

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
