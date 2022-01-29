using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class Grid : IEnumerable<Cube>
    {
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

    public class QueryContext
    {
        Grid QueriedGrid { get; }
        Placement pl { get; }

        public QueryContext(Grid queriedGrid)
        {
            QueriedGrid = queriedGrid;
            pl = new Placement(queriedGrid);
        }

        public CubeGroup GetBox(Box3Int box)
        {
            var cubes = box.Select(i => QueriedGrid[i]).ToList();
            return new CubeGroup(QueriedGrid, cubes);
        }

        public LevelGeometryElement GetFlatBox(Box2Int box, int y = 0)
        {
            var cubes = (box.InflateY(0, 1) + y * Vector3Int.up).Select(i => QueriedGrid[i]).ToList();
            return new CubeGroup(QueriedGrid, cubes).LevelElement(AreaType.None);
        }

        /// <summary>
        /// Returns shapes whose cubes that overlap are cut out.
        /// </summary>
        public LevelGroupElement RemoveOverlap(LevelGroupElement shapes)
        {
            /*
            var smallBox = new Box2Int(box.leftBottom, box.leftBottom + (box.rightTop - box.leftBottom) / 4);
            var randomBoundingBox = new Box2Int(new Vector2Int(-2, -2), new Vector2Int(2, 2));
            var boxes = Enumerable.Range(0, boxesCount).Select(i => 
            {
                var b = smallBox.Border(ExtensionMethods.RandomHalfBox(randomBoundingBox));
                return GetBox((box.GetRandom() - b.Extents() + b).InflateY(0, 1));
            });*/
            //var cubeGroup = new LevelGroupElement(QueriedGrid, AreaType.None, Enumerable.Empty<LevelElement>().ToList());
            //boxes.ForEach(g => cubeGroup = cubeGroup.Add(g.Minus(cubeGroup.CubeGroup()).LevelElement(AreaType.None)));
            var newShapes = shapes.LevelElements.Aggregate((IEnumerable<LevelElement>)new List<LevelElement>(),
                (overlapped, le) => overlapped.Append(le.Minus(overlapped.ToLevelGroupElement(shapes.Grid))))
                .Where(shape => shape.Cubes().Any())
                .ToLevelGroupElement(shapes.Grid);
            return newShapes;
        }

        /// <summary>
        /// Returns boxes in the bounding box that don't overlap.
        /// </summary>
        /*public LevelGroupElement GetNonOverlappingBoxes(Box2Int boundingBox, int boxesCount)
        {
            var boudingElem = GetFlatBox(boundingBox);
            var smallBox = new Box2Int(boundingBox.leftBottom, boundingBox.leftBottom + (boundingBox.rightTop - boundingBox.leftBottom) / 4);
            var randomBoundingBox = new Box2Int(new Vector2Int(-2, -2), new Vector2Int(2, 2));

            // get boxes
            var boxes = Enumerable.Range(0, boxesCount).SelectNN(i =>
            {
                var bBounds = smallBox.Border(ExtensionMethods.RandomHalfBox(randomBoundingBox));
                var b = GetBox(bBounds.InflateY(0, 1)).LevelElement(AreaType.None);
                return b;
            }).ToLevelGroupElement();

            // move boxes

            var movedBoxes = MoveLevelGroup(boxes,
                (moved, le) =>
                {
                    var movesInside = le.MovesToBeInside(boudingElem);
                    var movesNotIntersecting = le.Moves(movesInside, moved);
                    return movesNotIntersecting;
                },
                moves => moves.GetRandom()
                );

            var cubeGroup = new LevelGroupElement(QueriedGrid, AreaType.None, Enumerable.Empty<LevelElement>().ToList());
            movedBoxes.LevelElements.ForEach(g => cubeGroup = cubeGroup.Add(g.CubeGroup().Minus(cubeGroup.CubeGroup()).LevelElement(AreaType.None)));
            return cubeGroup;
        }*/


        public LevelGroupElement FlatBoxes(IEnumerable<Box2Int> boxes, int count)
        {
            return new LevelGroupElement(QueriedGrid, AreaType.None, boxes.Select(box => GetFlatBox(box)).Take(count).ToList<LevelElement>());
        }

        public LevelGroupElement FlatBoxes(int minSize, int maxSize, int count)
        {
            var boxSequence = ExtensionMethods.BoxSequence(() => ExtensionMethods.RandomBox(new Vector2Int(minSize, minSize), new Vector2Int(maxSize, maxSize)));
            return FlatBoxes(boxSequence, count);
        }

        public LevelGroupElement LiftRandomly(LevelGroupElement lge, Func<int> liftingF)
        {
            var newGroups = lge.LevelElements.Select(g => new CubeGroup(g.Grid, g.MoveBy(Vector3Int.up * liftingF()).Cubes().ToList()).LevelElement(g.AreaType));
            return new LevelGroupElement(lge.Grid, lge.AreaType, newGroups.ToList<LevelElement>());
        }

        public LevelGroupElement RaiseRandomly(LevelGroupElement lge, Func<int> liftingF)
        {
            var newGroups = lge.LevelElements.Select(g => new CubeGroup(g.Grid, g.Cubes().Concat(g.CubeGroup().ExtrudeVer(Vector3Int.up, liftingF()).Cubes).ToList()).LevelElement(g.AreaType));
            return new LevelGroupElement(lge.Grid, lge.AreaType, newGroups.ToList<LevelElement>());
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

        public CubeGroup GetRandomHorConnected(Vector3Int start, CubeGroup boundingGroup, int n)
        {
            var grp = new CubeGroup(QueriedGrid, new List<Cube>() { QueriedGrid[start] });
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
            return new CubeGroup(QueriedGrid, start.Cubes.Append(newCube).ToList());
        }

        public CubeGroup ExtrudeRandomly(CubeGroup start, float keptRatio)
        {
            var possibleCubes = start.AllBoundaryFacesH().Extrude(1).Cubes;
            if (possibleCubes.Count() == 0)
                return start;
            var newCubes = possibleCubes.Shuffle().Take((int)(keptRatio * possibleCubes.Count()));
            return new CubeGroup(QueriedGrid, start.Cubes.Concat(newCubes).ToList());
        }

        public LevelGroupElement Partition(Func<CubeGroup, CubeGroup, CubeGroup> groupGrower, CubeGroup boundingGroup, int groupN)
        {
            var groups = boundingGroup.Cubes
                .Select(cube => cube.Position)
                .Shuffle()
                .Take(groupN)
                .Select(pos => new CubeGroup(QueriedGrid, QueriedGrid[pos].Group().Cubes))
                .ToList();
            var totalSize = 0;
            boundingGroup = boundingGroup.Minus(new CubeGroup(QueriedGrid, groups.SelectManyNN(grp => grp.Cubes).ToList()));
            // Iterate until no group can be grown
            while (groups.Select(grp => grp.Cubes.Count).Sum() > totalSize)
            {
                totalSize = groups.Select(grp => grp.Cubes.Count).Sum();
                var newGroups = new List<CubeGroup>();
                // Update each group and remove newly added cube from bounding box
                foreach (var grp in groups)
                {
                    var newGrp = groupGrower(grp, boundingGroup);
                    boundingGroup = boundingGroup.Minus(newGrp);
                    newGroups.Add(newGrp);
                }
                groups = newGroups;
            }
            return new LevelGroupElement(QueriedGrid, AreaType.None, groups.Select(g => g.LevelElement()).ToList<LevelElement>());
        }
    }

}