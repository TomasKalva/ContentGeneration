using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField]
    bool spawnOnAwake = false;

    [SerializeField]
    Agent spawnedAgent;

    private void Awake()
    {
        if (spawnOnAwake)
        {
            Spawn();
        }
    }

    public void Spawn()
    {
        Instantiate(spawnedAgent);
        spawnedAgent.transform.position = transform.position;
        spawnedAgent.transform.rotation = transform.rotation;
    }
}
