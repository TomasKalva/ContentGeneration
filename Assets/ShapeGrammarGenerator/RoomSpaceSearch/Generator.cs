using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ShapeGrammar
{
    public class Generator : MonoBehaviour
    {
        string commandOutput = "";
        MinizincSolver solver;

        [SerializeField]
        Transform gridParent;
        [SerializeField]
        Transform squareCubePref;
        
        [SerializeField]
        Transform quadPref;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(20, 40, 80, 20), "Run cmd"))
            {
                commandOutput = solver.Run();
            }

            GUI.Label(new Rect(20, 80, 800, 200), commandOutput);
        }

        public void Run()
        {
            //solver = new MinizincSolver(@"C:\Users\tomka\Desktop\ContentGeneration\ContentGeneration\Assets\ShapeGrammarGenerator\Minizinc\TopologySolver1D.mzn", "");
            var shapes = new List<Shape>() { Shape.Rectangle(new Vector3Int(2, 1, 3), new Vector3Int(1, 1, 1)), Shape.Rectangle(new Vector3Int(2, 1, 3), new Vector3Int(1, 3, 1)) };
            var grid = new ShapesGrid(new Vector3Int(10, 10, 10), shapes);
            //grid.Visualize(gridParent, squareCubePref);

            InitSquare();
            QuadPlane.InitQuad(quadPref);

            var shapeTypes = new List<ShapeType>() 
            {
                //ShapeType.Rectangle(new Vector3Int(2, 2, 1)),
                ShapeType.Rectangle(new Vector3Int(2, 2, 1))
                    .Layer(0, square => square.SetConnection(Vector3Int.back, Connection.Both))
                    .Layer(1, square => square.SetConnection(Vector3Int.forward, Connection.Both)),
                /*ShapeType.Rectangle(new Vector3Int(2, 2, 1))
                    .Layer(1, square => square.SetHorizontalConnections(Connection.Both)),
                ShapeType.Rectangle(new Vector3Int(1, 2, 1))
                    .Layer(1, square => square.SetHorizontalConnections(Connection.Both)),
                */
            };
            
            var evAlg = new EvolutionaryAlgorithm<ShapesGrid>(
                () => GridOperations.Initialize(new Vector3Int(5, 5, 5), 10, shapeTypes),
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

            /*QuadPlane.InitQuad(quadPref);

            var plane = new GameObject().AddComponent<QuadPlane>();
            plane.Init(new Vector2Int(10, 10));
            var block = new GameObject().AddComponent<QuadBlock>();
            block.Init(new Vector3Int(10, 10, 10));
            
            /*for(int i= 0; i<10_000; i++)
            {
                var newObj = Instantiate(squareCubePref, gridParent);
                newObj.position = new Vector3(0, i / 100, i % 100);
            }*/
        }

        [SerializeField]
        Material inOut;
        [SerializeField]
        Material no;

        void InitSquare()
        {

            var ConnectionTypeToMaterial = new Dictionary<Connection, Material>();
            ConnectionTypeToMaterial.Add(Connection.Both, inOut);
            ConnectionTypeToMaterial.Add(Connection.No, no);

            Square.SetMaterials(ConnectionTypeToMaterial);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    /*enum Direction
    {
        Up, Down, Left, Right, Forward, Back
    }*/

    enum Connection
    {
        In, Out, Both, No
    }

    class Square
    {
        #region Visualisation
        static Dictionary<Vector3Int, int> DirectionToMaterialIndex { get; }
        static Dictionary<Connection, Color> ConnectionTypeToColor { get; }
        static Dictionary<Connection, Material> ConnectionTypeToMaterial { get; set;  }

        static Square()
        {
            DirectionToMaterialIndex = new Dictionary<Vector3Int, int>();
            DirectionToMaterialIndex.Add(Vector3Int.up, 0);
            DirectionToMaterialIndex.Add(Vector3Int.back, 1);
            DirectionToMaterialIndex.Add(Vector3Int.left, 2);
            DirectionToMaterialIndex.Add(Vector3Int.down, 3);
            DirectionToMaterialIndex.Add(Vector3Int.right, 4);
            DirectionToMaterialIndex.Add(Vector3Int.forward, 5);

            ConnectionTypeToColor = new Dictionary<Connection, Color>();
            ConnectionTypeToColor.Add(Connection.Both, new Color(0.3f, 0.3f, 1f));
            ConnectionTypeToColor.Add(Connection.No, new Color(0f, 0f, 0f));
        }

        public static void SetMaterials(Dictionary<Connection, Material> materials)
        {
            ConnectionTypeToMaterial = materials;
        }

        public void Visualize(MeshRenderer meshRenderer, Color shapeColor)
        {
            meshRenderer.materials[6].color = shapeColor;
            // Constraints on faces
            foreach (var kvp in DirectionToMaterialIndex)
            {
                var connection = Connections[kvp.Key];
                var connectionMaterial = ConnectionTypeToMaterial[connection];
                int faceIndex = kvp.Value;
                meshRenderer.materials[faceIndex] = connectionMaterial;
            }
        }

        #endregion

        public Vector3Int Position { get; }
        public Dictionary<Vector3Int, Connection> Connections { get; }


        public Square(Vector3Int position)
        {
            Position = position;
            Connections = new Dictionary<Vector3Int, Connection>();
            SetAllConnections(Connection.No);
        }

        void SetAllConnections(Connection connection)
        {
            Connections.Add(Vector3Int.up, connection);
            Connections.Add(Vector3Int.back, connection);
            Connections.Add(Vector3Int.left, connection);
            Connections.Add(Vector3Int.down, connection);
            Connections.Add(Vector3Int.right, connection);
            Connections.Add(Vector3Int.forward, connection);
        }

        public Square SetConnection(Vector3Int dir, Connection con)
        {
            Connections[dir] = con;
            return this;
        }

        public Square SetHorizontalConnections(Connection con)
        {
            SetConnection(Vector3Int.left, con);
            SetConnection(Vector3Int.right, con);
            SetConnection(Vector3Int.forward, con);
            SetConnection(Vector3Int.back, con);
            return this;
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

        public ShapeType Layer(int y, Action<Square> manipulator)
        {
            var layer = this.Squares.Where(sq => sq.Position.y == y);
            layer.ForEach(manipulator);
            return this;
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

    class ShapesGrid
    {
        public List<Shape> Shapes { get; }

        Square[,,] squares;

        public Box3Int BoundingBox { get; }

        public ShapesGrid(Vector3Int extents, IEnumerable<Shape> shapes)
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
            // multiple shapes at same location
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

            // inconsistent connections
            foreach (var shape in Shapes)
            {
                foreach (var square in shape.ShapeType.Squares)
                {
                    inconsistentCount += InconsistentConnectionsCount(shape, square);
                }
            }
            return inconsistentCount;
        }

        bool CanBePutToGrid(Shape shape, Square square)
        {
            var pos = square.Position + shape.Position;
            return BoundingBox.Contains(pos) && squares[pos.x, pos.y, pos.z] == null;
        }

        int InconsistentConnectionsCount(Shape shape, Square square)
        {
            var pos = square.Position + shape.Position;
            int inconsistentCount = 0;
            foreach (var dir in ExtensionMethods.Directions())
            {
                var neighborPos = pos + dir;
                if (BoundingBox.Contains(neighborPos))
                {
                    var connection = square.Connections[dir];
                    var neighborConnection = squares[neighborPos.x, neighborPos.y, neighborPos.z]?.Connections[-dir];
                    inconsistentCount += ConsistentConnection(connection, neighborConnection) ? 0 : 1;
                }
            }
            return inconsistentCount;
        }

        bool ConsistentConnection(Connection? c1, Connection? c2)
        {
            return !c1.HasValue || !c2.HasValue || (c1.Value == c2.Value);
        }

        public void Visualize(Transform parent, Transform squareCubePref)
        {
            // destroy all objects in parent
            for (int i = 0; i < parent.childCount; i++)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }

            // visualize boundary constraints
            QuadBlock gridQuadBlock = new GameObject().AddComponent<QuadBlock>();
            gridQuadBlock.transform.SetParent(parent);
            gridQuadBlock.transform.localPosition = (BoundingBox.rightTopFront - Vector3.one) / 2f;
            gridQuadBlock.Init(BoundingBox.rightTopFront);

            // visualize objects
            Func<int, float> intens = i => i / 10f;
            var shapeColors = Enumerable.Range(0, 10).Select(i => new Color(intens(i), intens(i), intens(i))).ToList();
            int shapeColorI = 0;
            foreach(var shape in Shapes)
            {
                foreach(var square in shape.ShapeType.Squares)
                {
                    var squareCube = GameObject.Instantiate(squareCubePref, parent);
                    var meshRenderer = squareCube.GetComponent<MeshRenderer>();
                    square.Visualize(meshRenderer, shapeColors[shapeColorI]);

                    squareCube.localPosition = shape.Position + square.Position;
                }
                shapeColorI = (shapeColorI + 1) % shapeColors.Count;
            }
        }
    }

    static class GridOperations
    {
        public static ShapesGrid Initialize(Vector3Int extents, int shapesCount, List<ShapeType> shapeTypes)
        {
            var shapes = new List<Shape>(shapesCount);
            for(int i=0; i < shapesCount; i++)
            {
                var shapeType = shapeTypes.GetRandom();
                var position = ExtensionMethods.RandomVector3Int(Vector3Int.zero, extents - shapeType.BoundingBoxExtents);
                var newShape = new Shape(shapeType, position);
                shapes.Add(newShape);
            }
            return new ShapesGrid(extents, shapes);
        }

        public static ShapesGrid ChangeShapePos(ShapesGrid grid, float changeProb)
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
            return new ShapesGrid(grid.BoundingBox.rightTopFront, newShapes);
        }

        public static ShapesGrid ExchangeShapes(ShapesGrid grid1, ShapesGrid grid2)
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
            return new ShapesGrid(grid1.BoundingBox.rightTopFront, newShapes);
        }

        public static float GridFitness(ShapesGrid grid)
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
        int genCount = 50;
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
