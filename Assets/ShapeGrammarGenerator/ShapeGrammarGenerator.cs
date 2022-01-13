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
        ShapeGrammarObjectStyle FountainheadStyle;

        private void Start()
        {
            // Keep scene view
            if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }


            Debug.Log("Generating world");

            var grid = new Grid(new Vector3Int(20, 10, 20));

            var qc = new QueryContext(grid);

            var sgStyles = new ShapeGrammarStyles(grid, qc, FountainheadStyle);
            var sgShapes = new ShapeGrammarShapes(grid, qc);

            var houseStyleRules = new StyleRules(
                new StyleRule(g => g.WithAreaType(AreaType.Room), g => g.SetGrammarStyle(sgStyles.RoomStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Roof), g => g.SetGrammarStyle(sgStyles.FlatRoofStyle)),
                new StyleRule(g => g.WithAreaType(AreaType.Foundation), g => g.SetGrammarStyle(sgStyles.FoundationStyle))
                );

            // island
            var island = sgShapes.IslandExtrudeIter(grid[0,0,0].Group(AreaType.Garden), 13, 0.3f);
            island.SetGrammarStyle(sgStyles.PlatformStyle);
            
            // house
            var house = sgShapes.House(new Box2Int(new Vector2Int(2, 2), new Vector2Int(8, 5)), 5);

            var houseBottom = house.WithAreaType(AreaType.Foundation).CubeGroup().CubesLayer(Vector3Int.down);
            var houseToIslandDir = island.MinkowskiMinus(houseBottom).GetRandom();
            house = house.MoveBy(houseToIslandDir);

            house.ApplyGrammarStyleRules(houseStyleRules);

            // wall
            var wallTop = island.ExtrudeHor().MoveBy(Vector3Int.up).SetGrammarStyle(sgStyles.FlatRoofStyle);
            sgShapes.Foundation(wallTop.MoveBy(Vector3Int.down)).SetGrammarStyle(sgStyles.FoundationStyle);

            // balcony
            var balcony = sgShapes.BalconyWide(house.WithAreaType(AreaType.Room).CubeGroup());
            house = house.Add(balcony);
            house.WithAreaType(AreaType.Balcony).SetGrammarStyle(cg => sgStyles.BalconyStyle(cg, house.WithAreaType(AreaType.Room).CubeGroup()));

            // house 2
            var house2 = house.MoveBy(Vector3Int.right * 8).ApplyGrammarStyleRules(houseStyleRules);

            grid.Generate(2f, parent);
        }

        public override void Generate(World world)
        {
            //world.AddEnemy(libraries.Enemies.MayanSwordsman(), new Vector3(0, 1, 0));
            world.AddEnemy(libraries.Enemies.SkinnyWoman(), new Vector3(0, 1, 0));

            Debug.Log("Generating world");

            world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, 0));
        }


    }

    public delegate CubeGroupGroup StyleSelector(CubeGroupGroup cubeGroupGroup);

    public class StyleRule
    {
        public StyleSelector Selector { get; }
        public StyleSetter Setter { get; }

        public StyleRule(StyleSelector selector, StyleSetter setter)
        {
            Selector = selector;
            Setter = setter;
        }
    }

    public class StyleRules 
    {
        StyleRule[] rules;

        public StyleRules(params StyleRule[] rules) 
        {
            this.rules = rules;
        }

        public void Apply(CubeGroupGroup cubeGroupGroup)
        {
            rules.ForEach(rule => rule.Selector(cubeGroupGroup).SetGrammarStyle(rule.Setter));
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
                var cubes = box.Select(i => QueriedGrid[i]).ToList();
                return new CubeGroup(QueriedGrid, AreaType.None, cubes);
            }

            public CubeGroup GetPlatform(Box2Int areaXZ, int posY)
            {
                var box = areaXZ.InflateY(posY, posY + 1);
                return GetBox(box);
            }

            public CubeGroup MoveDownUntilGround(CubeGroup topLayer)
            {
                var foundationCubes = topLayer.MoveInDirUntil(Vector3Int.down, cube => cube.Position.y < 0);
                return foundationCubes;
            }

            /*
            public CubeGroup MoveNear(CubeGroup what, CubeGroup where)
            {
                var offset = where.MinkowskiMinus(what).Cubes.GetRandom().Position;
                var movedWhat = what.MoveBy(offset);
                return movedWhat;
            }*/

            public CubeGroup GetRandomHorConnected(Vector3Int start, CubeGroup boundingGroup, int n)
            {
                var grp = new CubeGroup(QueriedGrid, AreaType.None, new List<Cube>() { QueriedGrid[start] });
                while (n > 0)
                {
                    var newGrp = GetRandomHorConnected1(grp, boundingGroup);
                    if (newGrp.Cubes.Count == grp.Cubes.Count)
                        return grp;

                    grp = newGrp;
                    n--;
                }
                return grp;
            }

            public CubeGroup GetRandomHorConnected1(CubeGroup start, CubeGroup boundingGroup)
            {
                var possibleCubes = start.AllBoundaryFacesH().Extrude(1).Cubes.Intersect(boundingGroup.Cubes);
                if (possibleCubes.Count() == 0)
                    return start;
                var newCube = possibleCubes.GetRandom();
                return new CubeGroup(QueriedGrid, AreaType.None, start.Cubes.Append(newCube).ToList());
            }

            public CubeGroup ExtrudeRandomly(CubeGroup start, float keptRatio)
            {
                var possibleCubes = start.AllBoundaryFacesH().Extrude(1).Cubes;
                if (possibleCubes.Count() == 0)
                    return start;
                var newCubes = possibleCubes.Shuffle().Take((int)(keptRatio * possibleCubes.Count()));
                return new CubeGroup(QueriedGrid, start.AreaType, start.Cubes.Concat(newCubes).ToList());
            }

            public List<CubeGroup> Partition(Func<CubeGroup, CubeGroup, CubeGroup> groupGrower, CubeGroup boundingGroup, int groupN)
            {
                var groups = boundingGroup.Cubes
                    .Select(cube => cube.Position)
                    .Shuffle()
                    .Take(groupN)
                    .Select(pos => new CubeGroup(QueriedGrid, AreaType.None, QueriedGrid[pos].Group(boundingGroup.AreaType).Cubes))
                    .ToList();
                var totalSize = 0;
                boundingGroup = boundingGroup.Minus(new CubeGroup(QueriedGrid, AreaType.None, groups.SelectManyNN(grp => grp.Cubes).ToList()));
                // Iterate until no group can be grown
                while (groups.Select(grp => grp.Cubes.Count).Sum() > totalSize)
                {
                    totalSize = groups.Select(grp => grp.Cubes.Count).Sum();
                    var newGroups = new List<CubeGroup>();
                    // Update each group and remove newly added cube from bounding box
                    foreach(var grp in groups)
                    {
                        var newGrp = groupGrower(grp, boundingGroup);
                        boundingGroup = boundingGroup.Minus(newGrp);
                        newGroups.Add(newGrp);
                    }
                    groups = newGroups;
                }
                return groups;
            }
        }

        Vector3Int sizes;

        Dictionary<Vector3Int, Cube[,,]> chunks;

        public int Width => sizes.x;
        public int Height => sizes.y;
        public int Depth => sizes.z;

        public bool ValidCoords(Vector3Int coords) => chunks.ContainsKey(GetChunkCoords(coords));

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

        Vector3Int TryCreateChunk(Vector3Int coords)
        {
            var chunkCoords = GetChunkCoords(coords);
            if (!ValidCoords(coords))
            {
                CreateChunk(chunkCoords);
            }
            return chunkCoords;
        }

        Cube GetCube(Vector3Int coords)
        {
            var chunkCoords = TryCreateChunk(coords);
            var localCoords = coords.Mod(sizes);
            var cube = chunks[chunkCoords][localCoords.x, localCoords.y, localCoords.z];
            Debug.Assert(cube != null, $"chunks[{chunkCoords}][{localCoords}] is null");
            return cube;
        }
        
        void SetCube(Vector3Int coords, Cube cube)
        {
            var chunkCoords = TryCreateChunk(coords);
            var localCoords = coords.Mod(sizes);
            chunks[chunkCoords][localCoords.x, localCoords.y, localCoords.z] = cube;
        }

        Vector3Int GetChunkCoords(Vector3Int coords) => coords.Div(sizes);

        void CreateChunk(Vector3Int chunkCoords)
        {
            var chunk = new Cube[sizes.x, sizes.y, sizes.z];
            var lbb = chunkCoords * sizes;
            foreach (var c in new Box3Int(Vector3Int.zero, sizes))
            {
                chunk[c.x, c.y, c.z] = new Cube(this, lbb + c);
            }
            chunks.Add(chunkCoords, chunk);
        }

        public Grid(Vector3Int sizes)
        {
            this.sizes = sizes;
            chunks = new Dictionary<Vector3Int, Cube[,,]>();
            /*grid = new Cube[sizes.x, sizes.y, sizes.z];
            foreach(var coords in new Box3Int(Vector3Int.zero, sizes))
            {
                this[coords] = new Cube(this, coords);
            }*/
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
            foreach (var chunk in chunks.Values)
            {
                foreach (var cube in chunk)
                {
                    yield return cube;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Generate(float cubeSide, Transform parent)
        {
            ((IEnumerable<Cube>)this).ForEach(cube => cube.Generate(cubeSide, parent));
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

        public CubeGroup Group(AreaType areaType) => new CubeGroup(Grid, areaType, new List<Cube>() { this });
    }
}