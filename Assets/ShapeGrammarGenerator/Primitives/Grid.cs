using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace ShapeGrammar
{

    public class Grid<T> : IEnumerable<T>
    {
        Vector3Int sizes;

        Dictionary<Vector3Int, T[,,]> chunks;

        public int Width => sizes.x;
        public int Height => sizes.y;
        public int Depth => sizes.z;

        public bool ValidCoords(Vector3Int coords) => chunks.ContainsKey(GetChunkCoords(coords));

        public Vector3Int Sizes => sizes;

        public delegate T ItemConstructor(Grid<T> grid, Vector3Int position);
        ItemConstructor Constr { get; }

        public T this[int x, int y, int z]
        {
            get => GetItem(new Vector3Int(x, y, z));
            set => SetItem(new Vector3Int(x, y, z), value);
        }

        public T this[Vector3Int coords]
        {
            get => GetItem(coords);
            set => SetItem(coords, value);
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

        T GetItem(Vector3Int coords)
        {
            Profiler.BeginSample("GetCube");
            var chunkCoords = TryCreateChunk(coords);
            var localCoords = coords.Mod(sizes);
            var cube = chunks[chunkCoords][localCoords.x, localCoords.y, localCoords.z];
            Debug.Assert(cube != null, $"chunks[{chunkCoords}][{localCoords}] is null");
            Profiler.EndSample();
            return cube;
        }

        void SetItem(Vector3Int coords, T item)
        {
            var chunkCoords = TryCreateChunk(coords);
            var localCoords = coords.Mod(sizes);
            chunks[chunkCoords][localCoords.x, localCoords.y, localCoords.z] = item;
        }

        Vector3Int GetChunkCoords(Vector3Int coords) => coords.Div(sizes);

        void CreateChunk(Vector3Int chunkCoords)
        {
            var chunk = new T[sizes.x, sizes.y, sizes.z];
            var lbb = chunkCoords * sizes;
            foreach (var c in new Box3Int(Vector3Int.zero, sizes))
            {
                chunk[c.x, c.y, c.z] = Constr(this, lbb + c);
            }
            chunks.Add(chunkCoords, chunk);
        }


        public Grid(Vector3Int sizes, ItemConstructor constructor)
        {
            this.sizes = sizes;
            chunks = new Dictionary<Vector3Int, T[,,]>();
            Constr = constructor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var chunk in chunks.Values)
            {
                foreach (var item in chunk)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Generate(float cubeSide, Transform parent)
        {
            ((IEnumerable<Cube>)this).ForEach(item => item.Generate(cubeSide, parent));
        }
    }

    public class QueryContext
    {
        Grid<Cube> QueriedGrid { get; }
        Placement pl { get; }

        public QueryContext(Grid<Cube> queriedGrid)
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

        public LevelGroupElement RecursivelySplitXZ(LevelGeometryElement levelElement, int maxSide)
        {
            return RecursivelySplitXZImpl(levelElement, maxSide).Leafs().ToLevelGroupElement(levelElement.Grid);
        }

        /// <summary>
        /// Can create cube groups with zero cubes.
        /// </summary>
        LevelElement RecursivelySplitXZImpl(LevelGeometryElement levelElement, int maxSide)
        {
            var cubeGroup = levelElement.CubeGroup();
            var lengthX = cubeGroup.LengthX();
            var lengthZ = cubeGroup.LengthZ();
            if (Mathf.Max(lengthX, lengthZ) <= maxSide)
            {
                return levelElement;
            }
            else
            {
                var dir = lengthX < lengthZ ?
                    Vector3Int.forward : Vector3Int.right;
                var relDist = UnityEngine.Random.Range(0.3f, 0.7f);
                var splitGroup = levelElement.SplitRel(dir, levelElement.AreaType, relDist);
                return splitGroup.Select(lg => RecursivelySplitXZImpl((LevelGeometryElement)lg, maxSide));
            }
        }

        public LevelGroupElement RecursivelySplit(LevelGeometryElement levelElement, int maxSide)
        {
            return RecursivelySplitImpl(levelElement, maxSide).Leafs().ToLevelGroupElement(levelElement.Grid);
        }

        LevelElement RecursivelySplitImpl(LevelGeometryElement levelElement, int maxSide)
        {
            var cubeGroup = levelElement.CubeGroup();
            var maxLengthDir = ExtensionMethods.PositiveDirections().ArgMax(dir => cubeGroup.ExtentsDir(dir));
            var maxLength = cubeGroup.ExtentsDir(maxLengthDir);
            if (maxLength <= maxSide)
            {
                return levelElement;
            }
            else
            {
                var relDist = UnityEngine.Random.Range(0.3f, 0.7f);
                var splitGroup = levelElement.SplitRel(maxLengthDir, levelElement.AreaType, relDist);
                return splitGroup.Select(lg => RecursivelySplitImpl((LevelGeometryElement)lg, maxSide));
            }
        }

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