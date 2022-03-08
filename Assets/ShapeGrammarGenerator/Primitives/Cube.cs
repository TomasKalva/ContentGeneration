using Assets.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Cube
    {
        public Grid<Cube> Grid { get; }
        public Vector3Int Position { get; }
        public Dictionary<Vector3Int, Facet> Facets { get; }
        public FaceHor FacesHor(Vector3Int dir) => Facets[dir] as FaceHor;
        public FaceVer FacesVer(Vector3Int dir) => Facets[dir] as FaceVer;
        public Corner Corners(Vector3Int dir) => Facets[dir] as Corner;
        public CUBE Object;
        public Vector3Int ObjectDir;
        public bool Changed { get; set; }
        public ShapeGrammarObjectStyle Style { get; set; }

        public Cube(Grid<Cube> grid, Vector3Int position)
        {
            Grid = grid;
            Position = position;
            Facets = new Dictionary<Vector3Int, Facet>();
            SetHorizontalFaces(() => new FaceHor());
            SetVerticalFaces(() => new FaceVer());
            SetCorners(() => new Corner());
            Object = CUBE.Nothing;
            Changed = false;
        }

        public Cube SetHorizontalFaces(Func<FaceHor> faceFac)
        {
            ExtensionMethods.HorizontalDirections().ForEach(dir =>
            {
                var face = faceFac();
                Facets.TryAdd(dir, face);
                face.Direction = dir;
                face.MyCube = this;
            });
            return this;
        }

        public Cube SetVerticalFaces(Func<FaceVer> faceFac)
        {
            ExtensionMethods.VerticalDirections().ForEach(dir =>
            {
                var face = faceFac();
                Facets.TryAdd(dir, face);
                face.Direction = dir;
                face.MyCube = this;
            });
            return this;
        }

        public Cube SetCorners(Func<Corner> cornerFac)
        {
            ExtensionMethods.HorizontalDiagonals().ForEach(dir =>
            {
                var corner = cornerFac();
                Facets.TryAdd(dir, corner);
                corner.Direction = dir;
                corner.MyCube = this;
            });
            return this;
        }

        public void Generate(float scale, Transform parent)
        {
            if (!Changed)
                return;

            GenerateObject(scale, parent);

            foreach (var facet in Facets.Values)
            {
                facet.Generate(scale, parent, Position);
            }
        }

        public void GenerateObject(float cubeSide, Transform parent)
        {
            if (Style == null)
            {
                if(Object != CUBE.Nothing)
                {
                    Debug.LogError($"Trying to create an object {Object} when no style is set!");
                }
                return;
            }

            var obj = Style.GetCube(Object);

            obj.SetParent(parent);
            obj.localScale = cubeSide * Vector3.one;
            obj.localPosition = ((Vector3)Position) * cubeSide;
            obj.rotation = Quaternion.LookRotation(ObjectDir, Vector3.up);
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
