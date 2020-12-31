using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using System.Linq;

public class Generator : MonoBehaviour
{
    public List<GameObject> objects;
    public Transform location;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunAlgorithm());
        //Generate();
    }

    IEnumerator RunAlgorithm()
    {
        var ga = new GeneticAlgorithm();
        foreach (var _ in ga.Run())
        {
            foreach (Transform child in location.transform)
            {
                Destroy(child.gameObject);
            }
            ga.Best.GenerateGeometry(objects, location);
            Debug.Log($"Best fitness: {ga.Fitness(ga.Best)}");
            yield return new WaitForSeconds(0.01f);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void Generate()
    {
        /*var m = new Map(3, 3, 20);
        m.GenerateGeometry(objects, location);*/
        var r = new Region(20, 20);
        r.GenerateGeometry(objects, location);
        Destroy(location.gameObject);
    }
}

class GeneticAlgorithm
{
    int popSize = 50;
    int genCount = 200;
    float mutationProb = 0.05f;
    float crossoverProb = 0.4f;
    List<Region> population;
    public Region Best { get; private set; }

    public GeneticAlgorithm()
    {
        Initialize();
    }

    private void Initialize()
    {
        population = new List<Region>(popSize);
        for(int i = 0; i < popSize; i++)
        {
            population.Add(CreateInidividual());
        }
    }

    private void Mutation()
    {
        var newPop = new List<Region>(popSize);
        foreach(var reg in population)
        {
            newPop.Add(reg.Mutate(mutationProb));
        }
        population = newPop;
    }

    private void Crossover()
    {
        var newPop = new List<Region>(popSize);
        for(int i=0; i<popSize/2;i++)
        {
            var r1 = population.GetRandom();
            var r2 = population.GetRandom();
            if (Random.value < crossoverProb)
            {
                var newRs = r1.Cross(r2);
                newPop.Add(newRs.Item1);
                newPop.Add(newRs.Item2);
            }
            else
            {
                newPop.Add(r1.Clone());
                newPop.Add(r2.Clone());
            }
        }
        population = newPop;
    }

    public float Fitness(Region reg)
    {
        var wallsCount = reg.WallsCount();
        var connectedComponents = new Region.ComponentFinder(reg).ConnectedComponents();
        var connectedPathsCount = connectedComponents.Where(c => c.ComponentType == 0).Count();
        var averagePathSize = (float)connectedComponents.Where(c => c.ComponentType == 0).Average(c => c.Count);
        var averageWallSize = (float)connectedComponents.Where(c => c.ComponentType == 1).Average(c => c.Count);
        var oneComponentsCount = connectedComponents.Where(c => c.Count == 1).Count();
        /*if (wallsCount > 180)
            return 0;*/
        return (4 * averageWallSize + 5f * averagePathSize) / connectedComponents.Count;
    }

    private void Selection()
    {
        var newPop = new List<Region>(popSize);
        for (int i=0; i < popSize; i++)
        {
            var r1 = population.GetRandom();
            var r2 = population.GetRandom();

            if(Fitness(r1) > Fitness(r2))
            {
                newPop.Add(r1.Clone());
            }
            else
            {
                newPop.Add(r2.Clone());
            }
        }

        population = newPop;
    }

    private Region CreateInidividual()
    {
        return new Region(30, 30);
    }

    public IEnumerable Run()
    {
        for (int i = 0; i < genCount; i++)
        {
            Best = population.MaxArg(Fitness).Clone();
            Crossover();
            Mutation();
            Selection();
            // elitism
            population[0] = Best;
            yield return null;
        }
    }
}

class Map
{
    Region[,] regions;
    int width;
    int height;
    int regionSize;

    public Map(int width, int height, int regionSize)
    {
        this.width = width;
        this.height = height;
        this.regionSize = regionSize;
        regions = new Region[width, height];
        for(int i=0; i < width; i++)
        {
            for(int j=0; j< height; j++)
            {
                var region = new Region(regionSize, regionSize);
                regions[i, j] = region;
            }
        }
    }

    public void GenerateGeometry(List<GameObject> objects, Transform parent)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var region = new GameObject();
                region.transform.SetParent(parent);
                region.transform.localPosition = new Vector3(i * regionSize, 0, j * regionSize);
                regions[i, j].GenerateGeometry(objects, region.transform);
            }
        }
    }
}

