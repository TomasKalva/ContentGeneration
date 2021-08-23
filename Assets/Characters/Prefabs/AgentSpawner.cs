using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField]
    bool spawnOnAwake = false;

    [SerializeField]
    Agent agentPrefab;

    private void Awake()
    {
        if (spawnOnAwake)
        {
            Spawn();
        }
    }

    public Agent Spawn()
    {
        var spawnedAgent = Instantiate(agentPrefab);
        spawnedAgent.transform.position = transform.position;
        spawnedAgent.transform.rotation = transform.rotation;
        return spawnedAgent;
    }
}
