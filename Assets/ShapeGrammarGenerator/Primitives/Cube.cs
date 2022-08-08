using Assets.ShapeGrammarGenerator;
using Assets.Util;
using ContentGeneration.Assets.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Cube : IFacet
    {
        Cube IFacet.MyCube => this;
        Vector3Int IFacet.Direction => Vector3Int.zero;
        Action<Transform> IFacet.OnObjectCreated { get; } = _ => { };

        public Grid<Cube> Grid { get; }
        public Vector3Int Position { get; }
        public Dictionary<Vector3Int, Facet> Facets { get; }
        public FaceHor FacesHor(Vector3Int dir) => Facets[dir] as FaceHor;
        public FaceVer FacesVer(Vector3Int dir) => Facets[dir] as FaceVer;
        public Corner Corners(Vector3Int dir) => Facets[dir] as Corner;


        private CubePrimitive cubePrimitive;

        public CubePrimitive CubePrimitive
        {
            get => cubePrimitive;
            set
            {
                cubePrimitive = value;
                Changed = true;
            }
        }
        public CUBE CubeType => CubePrimitive.CubeType;
        public Vector3Int ObjectDir;
        public bool Changed { get; set; }
        public ShapeGrammarObjectStyle Style { get; set; }

        public Cube(Grid<Cube> grid, Vector3Int position)
        {
            Grid = grid;
            Position = position;
            Facets = new Dictionary<Vector3Int, Facet>();
            SetHorizontalFaces();
            SetVerticalFaces();
            SetCorners();
            CubePrimitive = new CubePrimitive();// CUBE.Nothing;
            Changed = false;
        }

        public Cube SetHorizontalFaces()
        {
            ExtensionMethods.HorizontalDirections().ForEach(dir =>
            {
                var face = new FaceHor(this, dir);
                Facets.TryAdd(dir, face);
            });
            return this;
        }

        public Cube SetVerticalFaces()
        {
            ExtensionMethods.VerticalDirections().ForEach(dir =>
            {
                var face = new FaceVer(this, dir);
                Facets.TryAdd(dir, face);
            });
            return this;
        }

        public Cube SetCorners()
        {
            ExtensionMethods.HorizontalDiagonals().ForEach(dir =>
            {
                var corner = new Corner(this, dir);
                Facets.TryAdd(dir, corner);
            });
            return this;
        }

        public void Generate(float scale, World world)
        {
            if (!Changed)
                return;

            GenerateObject(scale, world);

            foreach (var facet in Facets.Values)
            {
                facet.Generate(scale, world);
            }
        }

        public void GenerateObject(float cubeSide, World world)
        {
            CubePrimitive.PlacePrimitive(world, this, null);

            /*
            if (Style == null)
            {
                if(CubeType != CUBE.Nothing)
                {
                    Debug.LogError($"Trying to create an object {CubeType} when no style is set!");
                }
                return;
            }

            var obj = Style.GetCube(CubeType);

            obj.localScale = cubeSide * Vector3.one;
            obj.localPosition = ((Vector3)Position) * cubeSide;
            obj.rotation = Quaternion.LookRotation(ObjectDir, Vector3.up);
            world.AddArchitectureElement(obj);*/
        }

        public Cube MoveBy(Vector3Int offset)
        {
            return Grid[Position + offset];
        }

        public IEnumerable<Cube> MoveInDirUntil(Vector3Int dir, Func<Cube, bool> stopPred)
        {
            var ray = new Ray3Int(Position, dir);
            var validCubes = ray.TakeWhile(v => Grid[v] != null && !stopPred(Grid[v])).Select(v => Grid[v]);
            return validCubes;
        }

        public bool In(CubeGroup cubeGroup) => cubeGroup.Cubes.Contains(this);

        public CubeGroup Group() => new CubeGroup(Grid, new List<Cube>() { this });

        public IEnumerable<Cube> NeighborsHor()
        {
            return ExtensionMethods.HorizontalDirections().Select(dir => Grid[Position + dir]);
        }
        public IEnumerable<Cube> NeighborsVer()
        {
            return ExtensionMethods.VerticalDirections().Select(dir => Grid[Position + dir]); ;
        }

        public IEnumerable<Cube> NeighborsDirections(IEnumerable<Vector3Int> directions)
        {
            return directions.Select(dir => Grid[Position + dir]); ;
        }

        public CubeGroup LineTo(Cube other)
        {
            // todo: use faster algorithm than a*

            Neighbors<Cube> neighbors =
                cube => ExtensionMethods.Directions()
                    .Select(dir => cube.Grid[cube.Position + dir]);
            // create graph for searching for the path
            var graph = new ImplicitGraph<Cube>(neighbors);
            var graphAlgs = new GraphAlgorithms<Cube, Edge<Cube>, ImplicitGraph<Cube>>(graph);

            var path = graphAlgs.FindPath(
                new Cube[1] { this }, 
                cube => cube == other,
                (c0, c1) => (c0.Position - c1.Position).sqrMagnitude,
                c => (c.Position - other.Position).Sum(x => Mathf.Abs(x)),
                EqualityComparer<Cube>.Default);
            return path.ToCubeGroup(Grid);
        }
    }
}
