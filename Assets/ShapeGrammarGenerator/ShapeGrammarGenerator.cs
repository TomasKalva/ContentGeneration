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

            var grid = new Grid(new Vector3Int(10, 10, 10));

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

            var room = shapeGrammar.House(new Box2Int(new Vector2Int(5, 2), new Vector2Int(8, 5)), 5, Style);
            shapeGrammar.Balcony(room, Style);

            /*room.AllBoundaryFacesH().Extrude(3).AllBoundaryFacesH().SetStyle(Style).Fill(FACE_HOR.Wall);
            room.BoundaryFacesV(Vector3Int.up).Extrude(2).BoundaryFacesV(Vector3Int.down).SetStyle(Style).Fill(FACE_VER.Floor);
            room.AllBoundaryCorners().Extrude(1).AllBoundaryFacesH().SetStyle(Style).Fill(FACE_HOR.Wall);
            */

            grid.Generate(2f, parent);
            //world.AddItem(items.BlueIchorEssence, new Vector3(0, 0, -54));


            world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, 0));
        }


    }

    public class ShapeGrammarGen
    {
        Grid Grid { get; }
        QueryContext QC { get; }

        public ShapeGrammarGen(Grid grid, QueryContext qC)
        {
            Grid = grid;
            QC = qC;
        }

        public CubeGroup Room(Box3Int area, ShapeGrammarStyle style)
        {
            var room = QC.GetBox(area);
            room.AllBoundaryFacesH().SetStyle(style).Fill(FACE_HOR.Wall);
            room.BoundaryFacesV(Vector3Int.down).SetStyle(style).Fill(FACE_VER.Floor);
            room.AllBoundaryCorners().SetStyle(style).Fill(CORNER.Pillar);
            return room;
        }

        public CubeGroup House(Box2Int areaXZ, int posY, ShapeGrammarStyle style)
        {
            var room = Room(areaXZ.InflateY(posY, posY + 2), style);
            var cubesBelowRoom = room.BoundaryFacesV(Vector3Int.down).Cubes().MoveBy(Vector3Int.down);
            var foundation = Foundation(cubesBelowRoom, style);
            return room;
        }

        public CubeGroup Foundation(CubeGroup topLayer, ShapeGrammarStyle style)
        {
            var foundationCubes = topLayer.MoveInDirUntil(Vector3Int.down, cube => cube == null);
            foundationCubes.AllBoundaryFacesH().SetStyle(style).Fill(FACE_HOR.Wall);
            foundationCubes.AllBoundaryCorners().SetStyle(style).Fill(CORNER.Pillar);
            return foundationCubes;
        }

        public CubeGroup Platform(Box2Int areaXZ, int posY, ShapeGrammarStyle style)
        {
            var box = areaXZ.InflateY(posY, posY + 1);
            var platform = QC.GetBox(box);
            platform.BoundaryFacesV(Vector3Int.down).SetStyle(style).Fill(FACE_VER.Floor);
            platform.BoundaryCorners(ExtensionMethods.HorizontalDirections().ToArray())
                .MoveBy(-Vector3Int.up)
                .MoveInDirUntil(Vector3Int.down, corner => corner == null)
                .SetStyle(style).Fill(CORNER.Pillar);

            return platform;
        }

        public CubeGroup Balcony(CubeGroup house, ShapeGrammarStyle style)
        {
            // Find a cube for the balcony
            var balcony = house.Where(cube => cube.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor)
               .AllBoundaryFacesH()
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Facets.GetRandom()
               .OtherCube.Group();

            // Floor
            balcony.BoundaryFacesV(Vector3Int.down).SetStyle(style).Fill(FACE_VER.Floor);

            // Add railing to the balcony
            var facesNearHouse = balcony.AllBoundaryFacesH()
               .Neighboring(house);
            var railingFaces = balcony.AllBoundaryFacesH()
               .Minus(facesNearHouse);

            railingFaces
               .SetStyle(style)
               .Fill(FACE_HOR.Railing);
            railingFaces
               .Corners()
               .SetStyle(style)
               .Fill(CORNER.Pillar);
            // Door to house
            facesNearHouse
               .Facets.GetRandom().Group()
               .Fill(FACE_HOR.Door);
            
            return balcony;
        }
    }

    public class Group
    {
        public Grid Grid { get; }

        public Group(Grid grid)
        {
            Grid = grid;
        }
    }

    public class CubeGroup : Group
    {
        public List<Cube> Cubes { get; }

        public CubeGroup(Grid grid, List<Cube> cubes) : base(grid)
        {
            Cubes = cubes;
        }
        
        public CubeGroup CubesLayer(Vector3Int dir)
        {
            return new CubeGroup(Grid, Cubes.Where(cube => !Cubes.Contains(Grid[cube.Position + dir])).ToList());
        }

        public CubeGroup MoveBy(Vector3Int offset)
        {
            var movedCubes = Cubes.SelectNN(cube => cube.MoveBy(offset));
            return new CubeGroup(Grid, movedCubes.ToList());
        }

        public CubeGroup MoveInDirUntil(Vector3Int dir, Func<Cube, bool> stopPred)
        {
            var validCubes = Cubes.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CubeGroup(Grid, validCubes.ToList());
        }

        public CubeGroup Where(Func<Cube, bool> pred) => new CubeGroup(Grid, Cubes.Where(pred).ToList());

        #region FacesH

        public FaceHorGroup FacesH(params Vector3Int[] horDirs)
        {
            var faces =
                from horDir in horDirs
                from cube in Cubes
                select cube.FacesHor(horDir);
            return new FaceHorGroup(Grid, faces.ToList());
        }

        public FaceHorGroup BoundaryFacesH(params Vector3Int[] horDirs)
        {
            return new FaceHorGroup(Grid, horDirs.Select(horDir => CubesLayer(horDir).FacesH(horDir)).SelectMany(i=>i.Facets).ToList());
        }

        public FaceHorGroup AllBoundaryFacesH()
        {
            return BoundaryFacesH(ExtensionMethods.HorizontalDirections().ToArray());
        }
        #endregion

        #region FacesV

        public FaceVerGroup FacesV( params Vector3Int[] verDirs)
        {
            var faces =
                from horDir in verDirs
                from cube in Cubes
                select cube.FacesVer(horDir);
            return new FaceVerGroup(Grid, faces.ToList());
        }

        public FaceVerGroup BoundaryFacesV(params Vector3Int[] verDirs)
        {
            return new FaceVerGroup(Grid, verDirs.Select(verDir => CubesLayer(verDir).FacesV(verDir)).SelectMany(i => i.Facets).ToList());
        }

        public FaceVerGroup AllBoundaryFacesV()
        {
            return BoundaryFacesV(ExtensionMethods.VerticalDirections().ToArray());
        }
        #endregion

        #region Corners

        public CornerGroup Corners(params Vector3Int[] horDirs)
        {

            var cornerPairs =
                from horDir in horDirs
                let orthDir = ExtensionMethods.OrthogonalHorizontalDir(horDir)
                from cube in Cubes
                select new { i0 = cube.Corners(horDir + orthDir), i1 = cube.Corners(horDir - orthDir) };
            var corners = cornerPairs.Select(twoCorners => twoCorners.i0).Concat(cornerPairs.Select(twoCorners => twoCorners.i1)).Distinct();
            return new CornerGroup(Grid, corners.ToList());
        }

        public CornerGroup BoundaryCorners(params Vector3Int[] horDirs)
        {
            return new CornerGroup(Grid, horDirs.Select(verDir => CubesLayer(verDir).Corners(verDir)).SelectMany(i => i.Facets).ToList());
        }

        public CornerGroup AllBoundaryCorners()
        {
            return BoundaryCorners(ExtensionMethods.HorizontalDirections().ToArray());
        }

        #endregion
    }

    public abstract class FacetGroup<FacetT> : Group where FacetT : Facet
    {
        public List<FacetT> Facets { get; }

        public FacetGroup(Grid grid, List<FacetT> facets) : base(grid)
        {
            Facets = facets;
        }

        public CubeGroup Cubes()
        {
            return new CubeGroup(Grid, Facets.Select(face => face.MyCube).ToList());
        }

        public CubeGroup Extrude(int dist)
        {
            Func<Func<Cube, bool>> countdownMaker = () =>
            {
                var countdown = new Countdown(dist);
                return cube => countdown.Tick();
            };
            return new CubeGroup(Grid, Facets.SelectManyNN(face => face.OtherCube?.MoveInDirUntil(face.Direction, countdownMaker()))
                .ToList());
        }

        protected IEnumerable<FacetT> NeighboringIE(CubeGroup cubeGroup)
        {
            return Facets.Where(facet => facet.OtherCube.In(cubeGroup));
        }
    }

    public class FaceHorGroup : FacetGroup<FaceHor>
    {
        public FaceHorGroup(Grid grid, List<FaceHor> faces) : base(grid, faces)
        {
        }

        public FaceHorGroup Fill(FACE_HOR faceType)
        {
            Facets.ForEach(face => face.FaceType = faceType);
            return this;
        }

        public FaceHorGroup SetStyle(ShapeGrammarStyle style)
        {
            Facets.ForEach(face => face.Style = style);
            return this;
        }

        public CornerGroup Corners()
        {
            return new CornerGroup(Grid, Facets.SelectManyNN(faceHor => faceHor.Corners()).Distinct().ToList());
        }

        public FaceHorGroup Where(Func<FaceHor, bool> pred) => new FaceHorGroup(Grid, Facets.Where(pred).ToList());
        public FaceHorGroup Neighboring(CubeGroup cubeGroup) => new FaceHorGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public FaceHorGroup Minus(FaceHorGroup faceHorGroup) => new FaceHorGroup(Grid, Facets.Except(faceHorGroup.Facets).ToList());
    }

    class Countdown
    {
        public int Elapsed { get; private set; }
        public Countdown(int elapsed)
        {
            Elapsed = elapsed;
        }
        /// <summary>
        /// Returns true after finished.
        /// </summary>
        public bool Tick()
        {
            Elapsed--;
            return Elapsed < 0;
        }
    }

    public class FaceVerGroup : FacetGroup<FaceVer>
    {
        public FaceVerGroup(Grid grid, List<FaceVer> faces) : base(grid, faces)
        {
        }

        public FaceVerGroup Fill(FACE_VER faceType)
        {
            Facets.ForEach(face => face.FaceType = faceType);
            return this;
        }

        public FaceVerGroup SetStyle(ShapeGrammarStyle style)
        {
            Facets.ForEach(face => face.Style = style);
            return this;
        }
        public FaceVerGroup Where(Func<FaceVer, bool> pred) => new FaceVerGroup(Grid, Facets.Where(pred).ToList());
        public FaceVerGroup Neighboring(CubeGroup cubeGroup) => new FaceVerGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public FaceVerGroup Minus(FaceVerGroup faceVerGroup) => new FaceVerGroup(Grid, Facets.Except(faceVerGroup.Facets).ToList());
    }

    public class CornerGroup : FacetGroup<Corner>
    {
        public CornerGroup(Grid grid, List<Corner> faces) : base(grid, faces)
        {
        }

        public CornerGroup Fill(CORNER cornerType)
        {
            Facets.ForEach(corner => corner.CornerType = cornerType);
            return this;
        }

        public CornerGroup SetStyle(ShapeGrammarStyle style)
        {
            Facets.ForEach(corner => corner.Style = style);
            return this;
        }

        public CornerGroup MoveBy(Vector3Int offset)
        {
            var movedCorners = Facets.SelectNN(corner => corner.MoveBy(offset));
            return new CornerGroup(Grid, movedCorners.ToList());
        }

        public CornerGroup MoveInDirUntil(Vector3Int dir, Func<Corner, bool> stopPred)
        {
            var validCorners = Facets.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CornerGroup(Grid, validCorners.ToList());
        }

        public CornerGroup Where(Func<Corner, bool> pred) => new CornerGroup(Grid, Facets.Where(pred).ToList());
        public CornerGroup Neighboring(CubeGroup cubeGroup) => new CornerGroup(Grid, NeighboringIE(cubeGroup).ToList());
        public CornerGroup Minus(CornerGroup cornerGroup) => new CornerGroup(Grid, Facets.Except(cornerGroup.Facets).ToList());
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

    public abstract class Facet
    {
        public Vector3Int Direction { get; set; }
        public ShapeGrammarStyle Style { get; set; }
        public Cube MyCube { get; set; }
        public Cube OtherCube => MyCube.Grid[MyCube.Position + Direction];

        public abstract void Generate(float cubeSide, Transform parent, Vector3Int cubePosition);

        public FacetT MoveBy<FacetT>(Vector3Int offset) where FacetT : Facet
        {
            var offsetCube = MyCube.MoveBy(offset);
            return (FacetT)offsetCube?.Facets[Direction];
        }

        public IEnumerable<FacetT> MoveInDirUntil<FacetT>(Vector3Int dir, Func<FacetT, bool> stopPred) where FacetT : Facet
        {
            var validFacets = MyCube.MoveInDirUntil(dir, cube => stopPred((FacetT)cube.Facets[Direction])).Select(cube => (FacetT)cube.Facets[Direction]);
            return validFacets;
        }
    }

    public class FaceHor : Facet
    {
        private FACE_HOR faceType;

        public FACE_HOR FaceType 
        { 
            get => faceType; 
            set  
            {
                faceType = value;
                MyCube.Changed = true;
            }
        }

        public override void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var otherFaceType = OtherCube.FacesHor(-Direction).FaceType;
            if(FaceType < otherFaceType) 
                return;

            var offset = (Vector3)Direction * 0.5f;
            var obj = Style.GetFaceHor(FaceType);

            obj.SetParent(parent);
            obj.localPosition = (cubePosition + offset) * cubeSide;
            obj.rotation = Quaternion.LookRotation(Direction, Vector3.up);
        }

        public IEnumerable<Corner> Corners()
        {
            var ortDir = ExtensionMethods.OrthogonalHorizontalDir(Direction);
            yield return MyCube.Corners(Direction + ortDir);
            yield return MyCube.Corners(Direction - ortDir);
        }

        public FaceHor MoveBy(Vector3Int offset) => MoveBy<FaceHor>(offset);
        public IEnumerable<FaceHor> MoveInDirUntil(Vector3Int dir, Func<FaceHor, bool> stopPred) => MoveInDirUntil<FaceHor>(dir, stopPred);
        public FaceHorGroup Group() => new FaceHorGroup(MyCube.Grid, new List<FaceHor>() { this });
    }

    public class FaceVer : Facet
    {
        private FACE_VER faceType;

        public FACE_VER FaceType
        {
            get => faceType;
            set
            {
                faceType = value;
                MyCube.Changed = true;
            }
        }

        public override void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = Vector3.up * Math.Max(0, Direction.y);
            var obj = Style.GetFaceVer(FaceType);

            obj.SetParent(parent);
            obj.localPosition = (cubePosition + offset) * cubeSide;
        }

        public FaceVer MoveBy(Vector3Int offset) => MoveBy<FaceVer>(offset);
        public IEnumerable<FaceVer> MoveInDirUntil(Vector3Int dir, Func<FaceVer, bool> stopPred) => MoveInDirUntil<FaceVer>(dir, stopPred);
        public FaceVerGroup Group() => new FaceVerGroup(MyCube.Grid, new List<FaceVer>() { this });
    }

    public class Corner : Facet
    {
        private CORNER cornerType;

        public CORNER CornerType
        {
            get => cornerType;
            set
            {
                cornerType = value;
                MyCube.Changed = true;
            }
        }

        public override void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = (Vector3)Direction * 0.5f;
            var obj = Style.GetCorner(CornerType);

            obj.SetParent(parent);
            obj.localPosition = (cubePosition + offset) * cubeSide;
        }

        public Corner MoveBy(Vector3Int offset) => MoveBy<Corner>(offset);
        public IEnumerable<Corner> MoveInDirUntil(Vector3Int dir, Func<Corner, bool> stopPred) => MoveInDirUntil<Corner>(dir, stopPred);
        public CornerGroup Group() => new CornerGroup(MyCube.Grid, new List<Corner>() { this });
    }

    public enum FACE_HOR
    {
        Nothing,
        Railing,
        Fence,
        Window,
        Wall,
        Door,
        Special
    }

    public enum FACE_VER
    {
        Nothing,
        Floor
    }

    public enum CORNER
    {
        Nothing,
        Pillar
    }
}