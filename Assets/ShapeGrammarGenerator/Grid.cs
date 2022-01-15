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

        public QueryContext(Grid queriedGrid)
        {
            QueriedGrid = queriedGrid;
        }

        public CubeGroup GetBox(Box3Int box)
        {
            var cubes = box.Select(i => QueriedGrid[i]).ToList();
            return new CubeGroup(QueriedGrid, AreaType.None, cubes);
        }

        public CubeGroupGroup GetOverlappingBoxes(Box2Int box, int boxesCount)
        {
            var smallBox = new Box2Int(box.leftBottom, box.leftBottom + (box.rightTop - box.leftBottom) / 4);
            var randomBoundingBox = new Box2Int(new Vector2Int(-2, -2), new Vector2Int(2, 2));
            var boxes = Enumerable.Range(0, boxesCount).Select(i => 
            {
                var b = smallBox.Border(ExtensionMethods.RandomBox(randomBoundingBox));
                return GetBox((box.GetRandom() - b.Extents() + b).InflateY(0, 1));
            });
            var cubeGroup = new CubeGroupGroup(QueriedGrid, AreaType.None, Enumerable.Empty<CubeGroup>().ToList());
            boxes.ForEach(g => cubeGroup = cubeGroup.Add(g.Minus(cubeGroup.CubeGroup())));
            return cubeGroup;
        }

        public CubeGroupGroup LiftRandomly(CubeGroupGroup cgg, Func<int> liftingF)
        {
            var newGroups = cgg.Groups.Select(g => new CubeGroup(g.Grid, g.AreaType, g.MoveBy(Vector3Int.up * liftingF()).Cubes.ToList()));
            return new CubeGroupGroup(cgg.Grid, cgg.AreaType, newGroups.ToList());
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

        public CubeGroupGroup Partition(Func<CubeGroup, CubeGroup, CubeGroup> groupGrower, CubeGroup boundingGroup, int groupN)
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
                foreach (var grp in groups)
                {
                    var newGrp = groupGrower(grp, boundingGroup);
                    boundingGroup = boundingGroup.Minus(newGrp);
                    newGroups.Add(newGrp);
                }
                groups = newGroups;
            }
            return new CubeGroupGroup(QueriedGrid, boundingGroup.AreaType, groups);
        }
    }

}