using OurFramework.UI;
using OurFramework.Environment.GridMembers;
using OurFramework.Environment.ShapeCreation;
using OurFramework.Environment.ShapeGrammar;
using OurFramework.Environment.StylingAreas;
using OurFramework.Gameplay.State;
using System;
using UnityEngine;
using OurFramework.Util;

/// <summary>
/// Used for testing and visualisation of algorithms operating on LevelElements and CubeGroups.
/// Some features (e.g. connections) might not work correctly.
/// </summary>
public class ExamplesRunner : MonoBehaviour
{
    [SerializeField]
    Transform worldParent;

    [SerializeField]
    Transform empty;

    [SerializeField]
    GeometricPrimitives GeometricPrimitives;

    public string CurrentExampleName;

    WorldGeometryState WorldState { get; set; }

    GridOwner GO { get; set; }

    public Examples Examples { get; private set; }

    bool initialized = false;

    public void Init()
    {
        if (initialized && GO != null && GO.ArchitectureParent != null)
        {
            return;
        }

        var newWorldParent = Instantiate(empty);
        newWorldParent.SetParent(worldParent);

        Examples = new Examples(GeometricPrimitives);
        WorldState = new WorldGeometryState(LevelElement.Empty(Examples.grid), Examples.grid, le => le.ApplyGrammarStyles());
        GO = new GridOwner(newWorldParent, 2.8f);
        initialized = true;
    }

    public void Run(Func<LevelElement> exampleF)
    {
        initialized = false;
        var example = exampleF();
        WorldState.Add(example);

        WorldState.Added.CreateGeometry(GO);
        WorldState.CreateGeometry(GO).Evaluate();
    }

    class GridOwner : IGridGeometryOwner
    {
        public WorldGeometry WorldGeometry { get; }

        public Transform ArchitectureParent { get; }

        public GridOwner(Transform architectureParent, float worldScale)
        {
            ArchitectureParent = architectureParent;
            WorldGeometry = new WorldGeometry(architectureParent, worldScale);
        }

        public void AddArchitectureElement(Transform el)
        {
            el.SetParent(ArchitectureParent.transform);
        }

        public void AddInteractivePersistentObject(InteractiveObjectState interactivePersistentObject)
        {
            // No need to handle this for testing purposes
        }
    }
}
