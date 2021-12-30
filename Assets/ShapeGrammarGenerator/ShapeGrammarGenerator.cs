using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShapeGrammar.Grid;
using System.Linq;
using Assets.Util;

namespace ShapeGrammar
{
    public class ShapeGrammarGenerator : WorldGenerator
    {
        [SerializeField]
        Transform parent;

        [SerializeField]
        ShapeGrammarStyle Style;

        public override void Generate(World world)
        {
            //world.AddEnemy(enemies.sculpture, new Vector3(0, 0, -54));

            Debug.Log("Generating world");

            var grid = new Grid(new Vector3Int(20, 10, 20));

            /*var cube = grid[1, 1, 1];
            var face = cube.FacesHor[Vector3Int.forward];
            face.FaceType = FACE_HOR.Wall;
            face.Style = Style;
            cube.FacesHor[Vector3Int.forward] = face;
            cube.Changed = true;
            */
            var qc = new QueryContext(grid);

            var shapeGrammar = new ShapeGrammarGen(grid, qc);

            //shapeGrammar.Room(new Box3Int(new Vector3Int(1, 0, 1), new Vector3Int(3, 3, 4)), Style);
            //shapeGrammar.Room(new Box3Int(new Vector3Int(3, 1, 1), new Vector3Int(6, 5, 4)), Style);
            //shapeGrammar.Platform(new Box2Int(new Vector2Int(6, 5), new Vector2Int(8, 9)), 5, Style);

            //var room = shapeGrammar.House(new Box2Int(new Vector2Int(5, 2), new Vector2Int(8, 5)), 5, Style);
            //shapeGrammar.Balcony(room, Style);
            //shapeGrammar.BalconyWide(room, Style);

            var room = qc.GetRandomHorConnected(new Vector3Int(10, 0, 10), 30);
            shapeGrammar.Room(room, Style);

            /*room.AllBoundaryFacesH().Extrude(3).AllBoundaryFacesH().SetStyle(Style).Fill(FACE_HOR.Wall);
            room.BoundaryFacesV(Vector3Int.up).Extrude(2).BoundaryFacesV(Vector3Int.down).SetStyle(Style).Fill(FACE_VER.Floor);
            room.AllBoundaryCorners().Extrude(1).AllBoundaryFacesH().SetStyle(Style).Fill(FACE_HOR.Wall);
            */

            grid.Generate(2f, parent);
            //world.AddItem(items.BlueIchorEssence, new Vector3(0, 0, -54));


            world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, 0));
        }


    }



    public class Grid : IEnumerable<Cube>
    {
        public class QueryContext
        {
            Grid QueriedGrid { get; }

            public QueryContext(Grid queriedGrid)
            {
                QueriedGrid = queriedGrid;
            }

            public CubeGroup GetBox(Box3Int box)
            {
                var cubes = QueriedGrid.grid.GetBoxItems(box);
                return new CubeGroup(QueriedGrid, cubes);
            }

            public CubeGroup GetRandomHorConnected(Vector3Int start, int n)
            {
                var grp = new CubeGroup(QueriedGrid, new List<Cube>() { QueriedGrid[start] });
                while(n > 0)
                {
                    var newCube = grp.AllBoundaryFacesH().Facets.GetRandom().OtherCube;
                    // todo: remove null check after implementing infinite grid
                    if (newCube != null)
                    {
                        grp = new CubeGroup(QueriedGrid, grp.Cubes.Append(newCube).ToList());
                        n--;
                    }
                }
                return grp;
            }
        }

        Vector3Int sizes;

        Cube[,,] grid;

        public int Width => sizes.x;
        public int Height => sizes.y;
        public int Depth => sizes.z;

        public bool ValidCoords(Vector3Int coords) => coords.AtLeast(Vector3Int.zero) && coords.Less(sizes);

        public Vector3Int Sizes => sizes;

        public Cube this[int x, int y, int z]
        {
            get => GetCube(new Vector3Int(x, y, z));
            set => SetCube(new Vector3Int(x, y, z), value);
        }

        public Cube this[Vector3Int coords]
        {
            get => GetCube(coords);
            set => SetCube(coords, value);
        }

        Cube GetCube(Vector3Int coords)
        {
            var cube = ValidCoords(coords) ? grid[coords.x, coords.y, coords.z] : null;
            return cube;
        }
        
        void SetCube(Vector3Int coords, Cube cube)
        {
            if (ValidCoords(coords))
            {
                grid[coords.x, coords.y, coords.z] = cube;
            }
        }

        public Grid(Vector3Int sizes)
        {
            this.sizes = sizes;
            grid = new Cube[sizes.x, sizes.y, sizes.z];
            foreach(var coords in new Box3Int(Vector3Int.zero, sizes))
            {
                this[coords] = new Cube(this, coords);
            }
            /*
            // Merge the same faces
            foreach (var coords in new Box3Int(Vector3Int.zero, sizes - Vector3Int.one))
            {
                var cube = this[coords];
                Action<Vector3Int> mergeFaces = v => cube.Facets[v] = this[coords + v].Facets[-v];
                mergeFaces(Vector3Int.forward);
                mergeFaces(Vector3Int.up);
                mergeFaces(Vector3Int.right);
            }
            // Merge the same corners
            foreach (var coords in new Box3Int(Vector3Int.zero, sizes - Vector3Int.one))
            {
                var cube = this[coords];
                Action<Vector3Int> mergeFaces = v => {
                    cube.Facets[v] = this[coords + v].Facets[-v]
                    };
                var corner = cube.C
                this.coor
                mergeFaces(Vector3Int.forward);
                mergeFaces(Vector3Int.up);
                mergeFaces(Vector3Int.right);
            }*/
        }

        public Box2Int Bottom()
        {
            return new Box2Int(Vector2Int.zero, sizes.XZ());
        }

        public IEnumerator<Cube> GetEnumerator()
        {
            foreach (var cube in grid)
            {
                yield return cube;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Generate(float cubeSide, Transform parent)
        {
            grid.ForEachNN(cube => cube.Generate(cubeSide, parent));
        }
    }

    public class Cube
    {
        public Grid Grid { get; }
        public Vector3Int Position { get; }
        public Dictionary<Vector3Int, Facet> Facets { get; }
        public FaceHor FacesHor(Vector3Int dir) => Facets[dir] as FaceHor;
        public FaceVer FacesVer(Vector3Int dir) => Facets[dir] as FaceVer;
        public Corner Corners(Vector3Int dir) => Facets[dir] as Corner;
        public bool Changed { get; set; }
        
        public Cube(Grid grid, Vector3Int position)
        {
            Grid = grid;
            Position = position;
            Facets = new Dictionary<Vector3Int, Facet>();
            SetHorizontalFaces(() => new FaceHor());
            SetVerticalFaces(() => new FaceVer());
            SetCorners(() => new Corner());
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

        public void Generate(float cubeSide, Transform parent)
        {
            if (!Changed)
                return;

            foreach (var facet in Facets.Values)
            {
                facet.Generate(cubeSide, parent, Position);
            }
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
    }
}