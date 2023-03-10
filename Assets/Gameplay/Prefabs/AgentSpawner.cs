using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Can be used to spawn an agent.
    /// </summary>
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
}
