using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField]
    Agent spawnedAgent;

    private void Awake()
    {
        Spawn();
    }

    void Spawn()
    {
        Instantiate(spawnedAgent);
        spawnedAgent.transform.position = transform.position;
        spawnedAgent.transform.rotation = transform.rotation;
    }
}
