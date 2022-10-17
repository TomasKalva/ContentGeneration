using UnityEngine;
using ContentGeneration.Assets.UI.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System;
using Assets.Characters.SpellClasses;

namespace ContentGeneration.Assets.UI
{
    /// <summary>
    /// Used for placing the grid geometry.
    /// </summary>
    public interface IGridGeometryOwner
    {
        void AddArchitectureElement(Transform el);
        WorldGeometry WorldGeometry { get; }
    }

    public class World : INotifyPropertyChanged, IGridGeometryOwner
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Transform ArchitectureParent { get; }
        public Transform EntitiesParent { get; }
        /// <summary>
        /// Contains objects such as sea and skybox.
        /// </summary>
        public Transform SpecialObjectsParent { get; }
        public Transform CachedObjectsParent { get; }

        List<InteractiveObjectState> interactiveObjects { get; }
        List<InteractiveObjectState> interactivePersistentObjects { get; }
        public ObservableCollection<CharacterState> Enemies { get; }
        List<Transform> architectureElements;
        public PlayerCharacterState PlayerState { get; }
        OccurrenceManager Occurences { get; set; }
        public GraveState Grave { get; set; }

        //public delegate void WorldCreated();
        //public static event WorldCreated OnCreated;

        public WorldGeometry WorldGeometry { get; }

        public IEnumerable<CharacterState> AliveEnemies => Enemies;//.Where(ch => ch != null && !ch.Dead);
        public IEnumerable<InteractiveObjectState> InteractiveObjects => interactiveObjects.Concat(interactivePersistentObjects).Where(io => io != null && io.InteractiveObject != null);

        public Action OnLevelStart { get; set; }
        public Action OnLevelRestart { get; set; }

        // Start is called before the first frame update
        public World(WorldGeometry worldGeometry, PlayerCharacterState playerState)
        {
            WorldGeometry = worldGeometry;
            var worldParent = worldGeometry.WorldParent;
            
            ArchitectureParent = new GameObject("Architecture").transform;
            ArchitectureParent.SetParent(worldParent);
            
            EntitiesParent = new GameObject("Entities").transform;
            EntitiesParent.SetParent(worldParent);
            
            SpecialObjectsParent = new GameObject("SpecialObjects").transform;
            SpecialObjectsParent.SetParent(worldParent);

            CachedObjectsParent = new GameObject("CachedObjects").transform;
            CachedObjectsParent.SetParent(worldParent);

            Occurences = new OccurrenceManager();
            PlayerState = playerState;
            PlayerState.World = this;

            interactiveObjects = new List<InteractiveObjectState>();
            interactivePersistentObjects = new List<InteractiveObjectState>();
            Enemies = new ObservableCollection<CharacterState>();
            architectureElements = new List<Transform>();

            OnLevelStart = () => { };
            OnLevelRestart = () => { };
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
            enemy.Reset();
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

        public void AddInteractivePersistentObject(InteractiveObjectState interactivePersistentObject)
        {
            interactivePersistentObject.World = this;
            if (interactivePersistentObject.InteractiveObject.transform.parent == null) // Don't reparent door
            {
                interactivePersistentObject.InteractiveObject.transform.SetParent(EntitiesParent);
            }
            interactivePersistentObjects.Add(interactivePersistentObject);
        }

        public void AddArchitectureElement(Transform el)
        {
            el.SetParent(ArchitectureParent);
            architectureElements.Add(el);
        }

        public void AddSpecialObject(Transform specialObject)
        {
            specialObject.SetParent(SpecialObjectsParent);
        }

        public void PutToCache(Transform specialObject)
        {
            specialObject.SetParent(CachedObjectsParent);
        }

        public void Destroy()
        {
            GameObject.Destroy(ArchitectureParent.gameObject);
            GameObject.Destroy(EntitiesParent.gameObject);
            GameObject.Destroy(SpecialObjectsParent.gameObject);
            GameObject.Destroy(CachedObjectsParent.gameObject);
            if (PlayerState.Agent != null)
            {
                GameObject.Destroy(PlayerState.Agent.gameObject);
            }
        }

        public void InitializePlayer()
        {
            if (Grave != null)
            {
                var player = Grave.SpawnPlayer();
                player.World = this;
            }
            else
            {
                Debug.LogError("DefaultBonfire wasn't created!");
            }
        }

        public void CreateOccurence(Selector selector, params Effect[] effects)
        {
            Occurences.CreateOccurrence(selector, effects);
        }

        public void Update(float deltaT)
        {
            Occurences.Update(deltaT);
        }

        /// <summary>
        /// Resets the player and all the enemies inside the world.
        /// </summary>
        public void Reset()
        {
            // Destroy entities, cached objects and the player
            foreach(Transform child in EntitiesParent)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Transform child in CachedObjectsParent)
            {
                GameObject.Destroy(child.gameObject);
            }
            if (PlayerState.Agent != null)
            {
                GameObject.Destroy(PlayerState.Agent.gameObject);
            }

            // Reset occurences, interactive objects and enemies
            Occurences = new OccurrenceManager();
            interactiveObjects.Clear();
            Enemies.Clear();

            OnLevelRestart();
        }
    }
    public class WorldGeometry
    {
        public Transform WorldParent { get; }
        public Vector3 ParentPosition { get; }
        public float WorldScale { get; }

        public WorldGeometry(Transform worldParent, float worldScale)
        {
            this.WorldParent = worldParent;
            this.WorldScale = worldScale;
            ParentPosition = worldParent.position;
        }

        public Vector3 GridToWorld(Vector3 gridPos)
        {
            return ParentPosition + WorldScale * gridPos;
        }

        public Vector3 WorldToGrid(Vector3 worldPos)
        {
            return (worldPos - ParentPosition) / WorldScale;
        }
    }

    class OccurrenceManager
    {
        List<Occurrence> CurrentOccurrences { get; set; }
        //HashSet<Occurence> FinishedOccurences { get; }

        public OccurrenceManager()
        {
            CurrentOccurrences = new List<Occurrence>();
            //FinishedOccurences = new HashSet<Occurence>();
        }

        /// <summary>
        /// Something that happens inside of the world.
        /// </summary>
        class Occurrence
        {
            Selector selector;
            Effect[] effects;

            public Occurrence(Selector selector, params Effect[] effects)
            {
                this.selector = selector;
                this.effects = effects;
            }

            /// <summary>
            /// Returns true iff the occurence has finished.
            /// </summary>
            public bool Update(float deltaT)
            {
                var affectedCharacters = selector.Select(deltaT).ToList();
                affectedCharacters.ForEach(character =>
                    effects.ForEach(effect => effect(character)));

                return selector.Finished(deltaT);
            }
        }

        public void CreateOccurrence(Selector selector, params Effect[] effects)
        {
            CurrentOccurrences.Add(new Occurrence(selector, effects));
        }

        public void Update(float deltaT)
        {
            // todo: somehow optimize this to avoid allocations each update
            CurrentOccurrences = CurrentOccurrences.Where(occurence => !occurence.Update(deltaT)).ToList();
            /*CurrentOccurences.ForEach(occurence =>
            {
                if (occurence.Update(deltaT))
                {
                    //FinishedOccurences.Add(occurence);
                }
            });*/
            //CurrentOccurences.RemoveAll(occurence => FinishedOccurences.Contains(occurence));
            //FinishedOccurences.Clear();
        }


    }
}