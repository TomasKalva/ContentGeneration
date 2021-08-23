using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{

    List<InteractiveObject> interactiveObjects;
    List<Agent> agents;
    public Bonfire Bonfire { get; set; }

    public IEnumerable<Agent> Agents => agents.Where(a => a != null && !a.CharacterState.Dead);
    public IEnumerable<InteractiveObject> InteractiveObjects => interactiveObjects.Where(io => io != null);

    // Start is called before the first frame update
    void Start()
    {
        interactiveObjects = new List<InteractiveObject>(FindObjectsOfType<InteractiveObject>());
        agents = new List<Agent>(FindObjectsOfType<Agent>());
        Bonfire = GameObject.Find("DefaultBonfire").GetComponent<Bonfire>();
        Bonfire.SpawnPlayer();
    }

    public IEnumerable<InteractiveObject> ObjectsCloseTo(Vector3 point, float dist)
    {
        return InteractiveObjects.Where(o => (o.transform.position - point).sqrMagnitude <= dist * dist);
    }

    public void OnPlayerDeath()
    {
        Bonfire.SpawnPlayer();
    }
}
