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

            shapeGrammar.Room(new Box3Int(new Vector3Int(1, 0, 1), new Vector3Int(3, 3, 4)), Style);
            shapeGrammar.Room(new Box3Int(new Vector3Int(3, 1, 1), new Vector3Int(6, 5, 4)), Style);
            shapeGrammar.Platform(new Box2Int(new Vector2Int(6, 5), new Vector2Int(8, 9)), 5, Style);


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
            room.BoundaryFacesH(ExtensionMethods.HorizontalDirections().ToArray()).SetStyle(style).Fill(FACE_HOR.Wall);
            room.BoundaryFacesV(Vector3Int.down).SetStyle(style).Fill(FACE_VER.Floor);
            room.BoundaryCorners(ExtensionMethods.HorizontalDirections().ToArray()).SetStyle(style).Fill(CORNER.Pillar);
            return room;
        }

        public CubeGroup Platform(Box2Int areaXZ, int height, ShapeGrammarStyle style)
        {
            var box = areaXZ.InflateY(height, height + 1);
            var platform = QC.GetBox(box);
            platform.BoundaryFacesV(Vector3Int.down).SetStyle(style).Fill(FACE_VER.Floor);
            platform.BoundaryCorners(ExtensionMethods.HorizontalDirections().ToArray())
                .MoveBy(-Vector3Int.up)
                .MoveInDirUntil(Vector3Int.down, corner => corner == null)
                .SetStyle(style).Fill(CORNER.Pillar);

            return platform;
        }
    }

    public class CubeGroup
    {
        public Grid Grid { get; }
        public List<Cube> Cubes { get; }

        public CubeGroup(Grid grid, List<Cube> cubes)
        {
            Grid = grid;
            Cubes = cubes;
        }
        
        public CubeGroup CubesLayer(Vector3Int dir)
        {
            return new CubeGroup(Grid, Cubes.Where(cube => !Cubes.Contains(Grid[cube.Position + dir])).ToList());
        }

        #region FacesH

        public FaceHorGroup FacesH(params Vector3Int[] horDirs)
        {
            var faces =
                from horDir in horDirs
                from cube in Cubes
                select cube.FacesHor[horDir];
            return new FaceHorGroup(faces.ToList());
        }

        public FaceHorGroup BoundaryFacesH(params Vector3Int[] horDirs)
        {
            return new FaceHorGroup(horDirs.Select(horDir => CubesLayer(horDir).FacesH(horDir)).SelectMany(i=>i.Faces).ToList());
        }
        #endregion

        #region FacesV

        public FaceVerGroup FacesV( params Vector3Int[] verDirs)
        {
            var faces =
                from horDir in verDirs
                from cube in Cubes
                select cube.FacesVer[horDir];
            return new FaceVerGroup(faces.ToList());
        }

        public FaceVerGroup BoundaryFacesV(params Vector3Int[] verDirs)
        {
            return new FaceVerGroup(verDirs.Select(verDir => CubesLayer(verDir).FacesV(verDir)).SelectMany(i => i.Faces).ToList());
        }
        #endregion

        #region Corners

        public CornerGroup Corners(params Vector3Int[] horDirs)
        {

            var cornerPairs =
                from horDir in horDirs
                let orthDir = ExtensionMethods.OrthogonalHorizontalDir(horDir)
                from cube in Cubes
                select new { i0 = cube.Corners[horDir + orthDir], i1 = cube.Corners[horDir - orthDir] };
            var corners = cornerPairs.Select(twoCorners => twoCorners.i0).Concat(cornerPairs.Select(twoCorners => twoCorners.i1)).Distinct();
            return new CornerGroup(corners.ToList());
        }

        public CornerGroup BoundaryCorners(params Vector3Int[] horDirs)
        {
            return new CornerGroup(horDirs.Select(verDir => CubesLayer(verDir).Corners(verDir)).SelectMany(i => i.Corners).ToList());
        }

        #endregion
    }

    public class FaceHorGroup
    {
        public List<FaceHor> Faces { get; }

        public FaceHorGroup(List<FaceHor> faces)
        {
            Faces = faces;
        }

        public FaceHorGroup Fill(FACE_HOR faceType)
        {
            Faces.ForEach(face => face.FaceType = faceType);
            return this;
        }

        public FaceHorGroup SetStyle(ShapeGrammarStyle style)
        {
            Faces.ForEach(face => face.Style = style);
            return this;
        }
    }

    public class FaceVerGroup
    {
        public List<FaceVer> Faces { get; }

        public FaceVerGroup(List<FaceVer> faces)
        {
            Faces = faces;
        }

        public FaceVerGroup Fill(FACE_VER faceType)
        {
            Faces.ForEach(face => face.FaceType = faceType);
            return this;
        }

        public FaceVerGroup SetStyle(ShapeGrammarStyle style)
        {
            Faces.ForEach(face => face.Style = style);
            return this;
        }
    }

    public class CornerGroup
    {
        public List<Corner> Corners { get; }

        public CornerGroup(List<Corner> corners)
        {
            Corners = corners;
        }

        public CornerGroup Fill(CORNER cornerType)
        {
            Corners.ForEach(corner => corner.CornerType = cornerType);
            return this;
        }

        public CornerGroup SetStyle(ShapeGrammarStyle style)
        {
            Corners.ForEach(corner => corner.Style = style);
            return this;
        }

        public CornerGroup MoveBy(Vector3Int offset)
        {
            var movedCorners = Corners.SelectNN(corner => corner.MoveBy(offset));
            return new CornerGroup(movedCorners.ToList());
        }

        public CornerGroup MoveInDirUntil(Vector3Int dir, Func<Corner, bool> stopPred)
        {
            var validCorners = Corners.SelectMany(corner => corner.MoveInDirUntil(dir, stopPred));
            return new CornerGroup(validCorners.ToList());
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
        }

        public Box2Int Bottom()
        {
            return new Box2Int(Vector2Int.zero, sizes.XZ());
        }

        /*
        public bool ContainsBuilding(Module module)
        {
            return module != null && !module.Empty;
        }

        public bool HasHorizontalNeighbor(Module module)
        {
            return ContainsBuilding(GetCube(module.coords + Vector3Int.right)) ||
                    ContainsBuilding(GetCube(module.coords - Vector3Int.right)) ||
                    ContainsBuilding(GetCube(module.coords + Vector3Int.forward)) ||
                    ContainsBuilding(GetCube(module.coords - Vector3Int.forward));
        }

        public bool IsEmpty(Module module)
        {
            return module.Empty;
        }
        */
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
        /*
        public Vector3Int WorldToGrid(Vector3 coords)
        {
            Vector3 localCoords = (parent.worldToLocalMatrix * coords);
            return (localCoords + 0.5f * extents).Divide(extents).ToVector3Int();
        }*/

        public void Generate(float cubeSide, Transform parent)
        {
            grid.ForEachNN(cube => cube.Generate(cubeSide, parent));
        }
    }

    public class Cube
    {
        public Grid Grid { get; }
        public Vector3Int Position { get; }
        public Dictionary<Vector3Int, FaceHor> FacesHor{ get; }
        public Dictionary<Vector3Int, FaceVer> FacesVer { get; }
        public Dictionary<Vector3Int, Corner> Corners { get; }
        public bool Changed { get; set; }

        public Cube(Grid grid, Vector3Int position)
        {
            Grid = grid;
            Position = position;
            FacesHor = new Dictionary<Vector3Int, FaceHor>();
            FacesVer = new Dictionary<Vector3Int, FaceVer>();
            Corners = new Dictionary<Vector3Int, Corner>();
            SetHorizontalFaces(() => new FaceHor());
            SetVerticalFaces(() => new FaceVer());
            SetCorners(() => new Corner());
            Changed = true;
        }

        public Cube SetFaceHor(Vector3Int dir, FaceHor face)
        {
            FacesHor.TryAddEx(dir, face, "Invalid horizontal direction");
            return this;
        }

        public Cube SetFaceVer(Vector3Int dir, FaceVer face)
        {
            FacesVer.TryAddEx(dir, face, "Invalid vertical direction");
            return this;
        }

        public Cube SetCorner(Vector3Int dir, Corner corner)
        {
            Corners.TryAddEx(dir, corner, "Invalid corner direction");
            return this;
        }

        public Cube SetHorizontalFaces(Func<FaceHor> faceFac)
        {
            ExtensionMethods.HorizontalDirections().ForEach(dir =>
            {
                var face = faceFac();
                FacesHor.TryAdd(dir, face);
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
                FacesVer.TryAdd(dir, face);
                face.Direction = dir;
                face.MyCube = this;
            });
            return this;
        }

        public Cube SetCorners(Func<Corner> cornerFac)
        {
            ExtensionMethods.VerticalDiagonals().ForEach(dir =>
            {
                var corner = cornerFac();
                Corners.TryAdd(dir, corner);
                corner.Direction = dir;
                corner.MyCube = this;
            });
            return this;
        }

        public void Generate(float cubeSide, Transform parent)
        {
            if (!Changed)
                return;

            foreach(var face in FacesHor.Values)
            {
                face.Generate(cubeSide, parent, Position);
            }
            foreach (var face in FacesVer.Values)
            {
                face.Generate(cubeSide, parent, Position);
            }
            foreach (var corner in Corners.Values)
            {
                corner.Generate(cubeSide, parent, Position);
            }
        }
    }

    public class Facet
    {
        public Vector3Int Direction { get; set; }
        public ShapeGrammarStyle Style { get; set; }
        public Cube MyCube { get; set; }
    }

    public class FaceHor : Facet
    {
        public FACE_HOR FaceType { get; set; }

        public void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = (Vector3)Direction * 0.5f;
            var obj = Style.GetFaceHor(FaceType);

            obj.SetParent(parent);
            obj.localPosition = (cubePosition + offset) * cubeSide;
            obj.rotation = Quaternion.LookRotation(Direction, Vector3.up);

            Debug.Log("Added horizontal face");
        }
    }

    public class FaceVer : Facet
    {
        public FACE_VER FaceType { get; set; }

        public void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = Vector3.up * Math.Max(0, Direction.y);
            var obj = Style.GetFaceVer(FaceType);

            obj.SetParent(parent);
            obj.localPosition = (cubePosition + offset) * cubeSide;
        }
    }

    public class Corner : Facet
    {
        public CORNER CornerType { get; set; }

        public void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = (Vector3)Direction * 0.5f;
            var obj = Style.GetCorner(CornerType);

            obj.SetParent(parent);
            obj.localPosition = (cubePosition + offset) * cubeSide;
        }

        public Corner MoveBy(Vector3Int offset)
        {
            var offsetCube = MyCube.Grid[MyCube.Position + offset];
            return offsetCube?.Corners[Direction];
        }

        public IEnumerable<Corner> MoveInDirUntil(Vector3Int dir, Func<Corner, bool> stopPred)
        {
            var grid = MyCube.Grid;
            var ray = new Ray3Int(MyCube.Position, dir);
            var validCorners = ray.TakeWhile(v => grid[v] != null && !stopPred(grid[v].Corners[Direction])).Select(v=>grid[v].Corners[Direction]);
            return validCorners;
        }
    }

    public enum FACE_HOR
    {
        Nothing,
        Wall,
        Window,
        Fence,
        Railing,
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