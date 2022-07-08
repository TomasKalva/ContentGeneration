using Assets.InteractiveObject;
using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace ContentGeneration.Assets.UI
{
    public class World : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Transform ArchitectureParent { get; }
        public Transform EntitiesParent { get; }

        List<InteractiveObjectState> interactiveObjects;
        public ObservableCollection<CharacterState> Enemies { get; }
        List<Transform> architectureElements;
        public PlayerCharacterState PlayerState { get; }
        public Grave Grave { get; set; }

        public delegate void WorldCreated();
        public static event WorldCreated OnCreated;

        public WorldGeometry WorldGeometry { get; }

        public IEnumerable<CharacterState> AliveEnemies => Enemies.Where(ch => ch != null && !ch.Dead);
        public IEnumerable<InteractiveObjectState> InteractiveObjects => interactiveObjects.Where(io => io != null && io.InteractiveObject != null);

        // Start is called before the first frame update
        public World(WorldGeometry worldGeometry, PlayerCharacterState playerState)
        {
            WorldGeometry = worldGeometry;
            var worldParent = worldGeometry.WorldParent;
            ArchitectureParent = new GameObject("Architecture").transform;
            ArchitectureParent.SetParent(worldParent);
            EntitiesParent = new GameObject("Entities").transform;
            EntitiesParent.SetParent(worldParent);
            PlayerState = playerState;
            PlayerState.World = this;

            interactiveObjects = new List<InteractiveObjectState>();
            Enemies = new ObservableCollection<CharacterState>();
            architectureElements = new List<Transform>();
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
            enemy.World = this;
            enemy.Agent.transform.SetParent(EntitiesParent);
            Enemies.Add(enemy);
        }

        public void AddItem(InteractiveObjectState item)
        {
            item.World = this;
            item.InteractiveObject.transform.SetParent(EntitiesParent);
            interactiveObjects.Add(item);
        }

        public void RemoveItem(InteractiveObjectState item)
        {
            interactiveObjects.Remove(item);
            GameObject.Destroy(item.InteractiveObject.gameObject);
        }

        public void RemoveEnemy(CharacterState enemy)
        {
            Enemies.Remove(enemy);
            GameObject.Destroy(enemy.Agent.gameObject, 1f);
        }

        public void AddInteractiveObject(InteractiveObjectState interactiveObject)
        {
            interactiveObject.World = this;
            if (interactiveObject.InteractiveObject.transform.parent == null) // Don't reparent door
            {
                interactiveObject.InteractiveObject.transform.SetParent(EntitiesParent);
            }
            interactiveObjects.Add(interactiveObject);
        }

        public void AddArchitectureElement(Transform el)
        {
            el.SetParent(ArchitectureParent);
            architectureElements.Add(el);
        }

        public void OnPlayerDeath()
        {
            Grave.SpawnPlayer();
        }

        public void Destroy()
        {
            GameObject.Destroy(ArchitectureParent.gameObject);
            GameObject.Destroy(EntitiesParent.gameObject);
            PlayerState.Agent?.Die();
        }

        public void Created()
        {
            //Grave = GameObject.FindGameObjectWithTag("DefaultSpawnPoint").GetComponent<InteractiveObject>().State as Grave;

            if (Grave != null)
            {
                var player = Grave.SpawnPlayer();
                player.World = this;
                player.Agent.GetComponent<PlayerController>().World = this;
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
}