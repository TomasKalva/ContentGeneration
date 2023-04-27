using OurFramework.Util;
using OurFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OurFramework.Game;

namespace OurFramework.Environment.GridMembers
{
    /// <summary>
    /// Basic block of the grid. Stores faces and edges.
    /// </summary>
    public class Cube : IFacet
    {
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
        public bool Changed { get; set; }

        Cube IFacet.MyCube => this;
        Vector3Int IFacet.Direction => Vector3Int.zero;
        Action<Transform> IFacet.OnObjectCreated { get; } = _ => { };

        public Cube(Grid<Cube> grid, Vector3Int position)
        {
            Grid = grid;
            Position = position;
            Facets = new Dictionary<Vector3Int, Facet>();
            CreateHorizontalFaces();
            CreateVerticalFaces();
            CreateCorners();
            CubePrimitive = new CubePrimitive();
            Changed = false;
        }

        public Cube CreateHorizontalFaces()
        {
            ExtensionMethods.HorizontalDirections().ForEach(dir =>
            {
                var face = new FaceHor(this, dir);
                Facets.TryAdd(dir, face);
            });
            return this;
        }

        public Cube CreateVerticalFaces()
        {
            ExtensionMethods.VerticalDirections().ForEach(dir =>
            {
                var face = new FaceVer(this, dir);
                Facets.TryAdd(dir, face);
            });
            return this;
        }

        public Cube CreateCorners()
        {
            ExtensionMethods.HorizontalDiagonals().ForEach(dir =>
            {
                var corner = new Corner(this, dir);
                Facets.TryAdd(dir, corner);
            });
            return this;
        }

        public void CreateGeometry(float scale, IGridGeometryOwner world)
        {
            if (!Changed)
                return;

            CubePrimitive.PlacePrimitive(world, this, null);

            foreach (var facet in Facets.Values)
            {
                facet.CreateGeometry(scale, world);
            }
        }

        public Cube MoveBy(Vector3Int offset)
        {
            return Grid[Position + offset];
        }

        public IEnumerable<Cube> MoveInDirUntil(Vector3Int dir, Func<Cube, bool> stopPred)
        {
            var ray = new Ray3Int(Position, dir);
            var validCubes = ray.TakeWhile(v => !stopPred(Grid[v])).Select(v => Grid[v]);
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
    }
}
