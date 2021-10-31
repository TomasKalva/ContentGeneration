using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ShapeGrammarGenerator
{
    public class Generator : MonoBehaviour
    {
        string commandOutput = "";
        MinizincSolver solver;

        [SerializeField]
        Transform gridParent;
        [SerializeField]
        Transform squareCubePref;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(20, 40, 80, 20), "Run cmd"))
            {
                commandOutput = solver.Run();
            }

            GUI.Label(new Rect(20, 80, 800, 200), commandOutput);
        }

        // Start is called before the first frame update
        void Start()
        {
            //solver = new MinizincSolver(@"C:\Users\tomka\Desktop\ContentGeneration\ContentGeneration\Assets\ShapeGrammarGenerator\Minizinc\TopologySolver1D.mzn", "");
            var shapes = new List<Shape>() { Shape.Rectangle(new Vector3Int(2, 1, 3), new Vector3Int(1, 1, 1)), Shape.Rectangle(new Vector3Int(2, 1, 3), new Vector3Int(1, 3, 1)) };
            var grid = new Grid(new Vector3Int(10, 10, 10), shapes);
            //grid.Visualize(gridParent, squareCubePref);

            var shapeTypes = new List<ShapeType>() 
            { 
                ShapeType.Rectangle(new Vector3Int(2, 2, 1)),
                ShapeType.Rectangle(new Vector3Int(2, 1, 3)),
                ShapeType.Rectangle(new Vector3Int(2, 1, 1)),
                ShapeType.Rectangle(new Vector3Int(1, 4, 1)),
            };
            var evAlg = new EvolutionaryAlgorithm<Grid>(
                () => GridOperations.Initialize(new Vector3Int(10, 5, 5), 20, shapeTypes),
                grid => GridOperations.ChangeShapePos(grid, 0.1f),
                GridOperations.ExchangeShapes,
                GridOperations.GridFitness
                );

            int genN = 0;
            foreach(var _ in evAlg.Run())
            {
                var bestGrid = evAlg.Best;
                Debug.Log($"Generation {genN++}, inconsistencies: {bestGrid.InconsistentCount()}");
                bestGrid.Visualize(gridParent, squareCubePref);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    enum Direction
    {
        Up, Down, Left, Right, Forward, Back
    }

    enum Connection
    {
        In, Out, Both, No
    }

    class Square
    {
        public Vector3Int Position { get; }
        Dictionary<Direction, Connection> connections;

        public Square(Vector3Int position)
        {
            Position = position;
        }
    }

    class ShapeType
    {
        public List<Square> Squares { get; }
        public Vector3Int BoundingBoxExtents { get; }
        /*public static List<ShapeType> CreatedShapeTypes;

        static ShapeType()
        {
            CreatedShapeTypes = new List<ShapeType>();
        }*/


        public ShapeType(IEnumerable<Square> squares)
        {
            Squares = squares.ToList();
            BoundingBoxExtents = new Vector3Int(
                squares.Select(sq => sq.Position.x).ArgMax(c => c),
                squares.Select(sq => sq.Position.y).ArgMax(c => c),
                squares.Select(sq => sq.Position.z).ArgMax(c => c)
                );
        }

        public static ShapeType Rectangle(Vector3Int extents)
        {
            var rectangleArea = new Box3Int(Vector3Int.zero, extents);
            var squares = rectangleArea.Select(pos => new Square(pos));
            var rectangle = new ShapeType(squares);
            return rectangle;
        }

    }

    class Shape
    {
        public ShapeType ShapeType { get; }
        public Vector3Int Position { get; }

        public Shape(ShapeType shapeType, Vector3Int position)
        {
            ShapeType = shapeType;
            Position = position;
        }

        public static Shape Rectangle(Vector3Int extents, Vector3Int position) 
        {
            return new Shape(ShapeType.Rectangle(extents), position);
        }
    }

    class Grid
    {
        public List<Shape> Shapes { get; }

        Square[,,] squares;

        public Box3Int BoundingBox { get; }

        public Grid(Vector3Int extents, IEnumerable<Shape> shapes)
        {
            BoundingBox = new Box3Int(Vector3Int.zero, extents);
            squares = new Square[extents.x, extents.y, extents.z];
            this.Shapes = shapes.ToList();
        }

        public int InconsistentCount()
        {
            foreach(var p in BoundingBox)
            {
                squares[p.x, p.y, p.z] = null;
            }

            int inconsistentCount = 0;
            foreach (var shape in Shapes)
            {
                foreach (var square in shape.ShapeType.Squares)
                {
                    if (CanBePutToGrid(shape, square))
                    {
                        var pos = square.Position + shape.Position;
                        squares[pos.x, pos.y, pos.z] = square;
                    }
                    else
                    {
                        inconsistentCount++;
                    }
                }
            }
            return inconsistentCount;
        }

        bool CanBePutToGrid(Shape shape, Square square)
        {
            var pos = square.Position + shape.Position;
            return BoundingBox.Contains(pos) && squares[pos.x, pos.y, pos.z] == null;
        }

        public void Visualize(Transform parent, Transform squareCubePref)
        {
            //destroy all objects in parent
            for (int i = 0; i < parent.childCount; i++)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }

            Func<int, float> intens = i => i / 10f;
            var shapeColors = Enumerable.Range(0, 10).Select(i => new Color(intens(i), intens(i), intens(i))).ToList();
            int shapeColorI = 0;
            foreach(var shape in Shapes)
            {
                foreach(var square in shape.ShapeType.Squares)
                {
                    var squareCube = GameObject.Instantiate(squareCubePref, parent);
                    var meshRenderer = squareCube.GetComponent<MeshRenderer>();
                    meshRenderer.material.color = shapeColors[shapeColorI];

                    squareCube.localPosition = shape.Position + square.Position;
                }
                shapeColorI = (shapeColorI + 1) % shapeColors.Count;
            }
        }
    }

    static class GridOperations
    {
        public static Grid Initialize(Vector3Int extents, int shapesCount, List<ShapeType> shapeTypes)
        {
            var shapes = new List<Shape>(shapesCount);
            for(int i=0; i < shapesCount; i++)
            {
                var shapeType = shapeTypes.GetRandom();
                var position = ExtensionMethods.RandomVector3Int(Vector3Int.zero, extents - shapeType.BoundingBoxExtents);
                var newShape = new Shape(shapeType, position);
                shapes.Add(newShape);
            }
            return new Grid(extents, shapes);
        }

        public static Grid ChangeShapePos(Grid grid, float changeProb)
        {
            var newShapes = new List<Shape>(grid.Shapes.Count);
            foreach(var shape in grid.Shapes)
            {
                var shapeType = shape.ShapeType;
                if(UnityEngine.Random.value < changeProb)
                {
                    var position = ExtensionMethods.RandomVector3Int(Vector3Int.zero, grid.BoundingBox.rightTopFront - shapeType.BoundingBoxExtents);
                    var newShape = new Shape(shapeType, position);
                    newShapes.Add(newShape);
                }
                else
                {
                    newShapes.Add(shape);
                }
            }
            return new Grid(grid.BoundingBox.rightTopFront, newShapes);
        }

        public static Grid ExchangeShapes(Grid grid1, Grid grid2)
        {
            var shapes1 = grid1.Shapes;
            var shapes2 = grid2.Shapes;
            int shapesCount = Mathf.Min(shapes1.Count, shapes2.Count);
            var newShapes = new List<Shape>(shapesCount);
            for (int i = 0; i <shapesCount; i++)
            {
                var shape1 = shapes1[i];
                var shape2 = shapes2[i];
                var newShape = UnityEngine.Random.value < 0.5f ? shape1 : shape2;
                newShapes.Add(newShape);
            }
            return new Grid(grid1.BoundingBox.rightTopFront, newShapes);
        }

        public static float GridFitness(Grid grid)
        {
            return -grid.InconsistentCount();
        }
    }

    delegate Individual Initialize<Individual>();
    delegate Individual Mutation<Individual>(Individual ind);
    delegate Individual Crossover<Individual>(Individual indA, Individual indB);
    delegate float Fitness<Individual>(Individual ind);

    class EvolutionaryAlgorithm<IndT>
    {
        int popSize = 50;
        int genCount = 200;
        float mutationProb = 0.05f;
        float crossoverProb = 0.4f;
        List<IndT> population;
        public IndT Best { get; private set; }
        Mutation<IndT> mutation;
        Crossover<IndT> crossover;
        Initialize<IndT> initialize;
        Fitness<IndT> fitness;

        public EvolutionaryAlgorithm(Initialize<IndT> initialize, Mutation<IndT> mutation, Crossover<IndT> crossover, Fitness<IndT> fitness)
        {
            this.mutation = mutation;
            this.crossover = crossover;
            this.initialize = initialize;
            this.fitness = fitness;

            Initialize();
        }

        private void Initialize()
        {
            population = new List<IndT>(popSize);
            for (int i = 0; i < popSize; i++)
            {
                population.Add(initialize());
            }
        }

        private void Mutation()
        {
            var newPop = new List<IndT>(popSize);
            foreach (var ind in population)
            {
                var newInd = UnityEngine.Random.value < mutationProb ? mutation(ind) : ind;
                newPop.Add(newInd);
            }
            population = newPop;
        }

        private void Crossover()
        {
            var newPop = new List<IndT>(popSize);
            for (int i = 0; i < popSize / 2; i++)
            {
                var r1 = population.GetRandom();
                var r2 = population.GetRandom();
                if (UnityEngine.Random.value < crossoverProb)
                {
                    var newInd1 = crossover(r1, r2);
                    var newInd2 = crossover(r2, r1);
                    newPop.Add(newInd1);
                    newPop.Add(newInd2);
                }
                else
                {
                    newPop.Add(r1);
                    newPop.Add(r2);
                }
            }
            population = newPop;
        }

        private void Selection()
        {
            var newPop = new List<IndT>(popSize);
            for (int i = 0; i < popSize; i++)
            {
                var r1 = population.GetRandom();
                var r2 = population.GetRandom();

                if (fitness(r1) > fitness(r2))
                {
                    newPop.Add(r1);
                }
                else
                {
                    newPop.Add(r2);
                }
            }

            population = newPop;
        }

        public IEnumerable Run()
        {
            for (int i = 0; i < genCount; i++)
            {
                Best = population.ArgMax(fitness.Invoke);
                Crossover();
                Mutation();
                Selection();
                // elitism
                population[0] = Best;
                yield return null;
            }
        }
    }

    class MinizincSolver
    {
        string programPath;
        string dataPath;

        public MinizincSolver(string programPath, string dataPath)
        {
            this.programPath = programPath;
            this.dataPath = dataPath;
        }

        public string Run()
        {
            string arguments;
            arguments = $"{programPath} {dataPath}";
            var p = new Process();
            p.StartInfo.FileName = @"C:\Program Files\MiniZinc\minizinc.exe";
            p.StartInfo.Arguments = arguments;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            var commandOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return commandOutput;
        }
    }
}
