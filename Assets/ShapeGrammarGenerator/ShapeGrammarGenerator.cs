using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShapeGrammar.Grid;
using System.Linq;

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
           
            var house = qc.GetBox(new Box3Int(new Vector3Int(1,1,1), new Vector3Int(3,3,4)));
            
            /*foreach(var cube in house.Cubes)
            {
                var face = cube.FacesHor[Vector3Int.forward];
                face.FaceType = FACE_HOR.Wall;
                face.Style = Style;
                cube.FacesHor[Vector3Int.forward] = face;
                cube.Changed = true;
            }*/
            house.FacesH(Vector3Int.left).SetStyle(Style).Fill(FACE_HOR.Wall);

            grid.Generate(2f, parent);
            //world.AddItem(items.BlueIchorEssence, new Vector3(0, 0, -54));


            world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, 0));
        }


    }

    public class ShapeGrammarGen
    {

    }

    public class CubeGroup
    {
        public List<Cube> Cubes { get; }

        public CubeGroup(List<Cube> cubes)
        {
            Cubes = cubes;
        }

        public FaceHorGroup FacesH(Vector3Int horDir)
        {
            var faces = from cube in Cubes select cube.FacesHor[horDir];
            return new FaceHorGroup(faces.ToList());
        }
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
                return new CubeGroup(cubes);
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
                this[coords] = new Cube(coords);
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

        public Vector3Int Position { get; }
        public Dictionary<Vector3Int, FaceHor> FacesHor{ get; }
        public Dictionary<Vector3Int, FaceVer> FacesVer { get; }
        public Dictionary<Vector3Int, Corner> Corners { get; }
        public bool Changed { get; set; }

        public Cube(Vector3Int position)
        {
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

    public class FaceHor
    {
        public FACE_HOR FaceType { get; set; }
        public Vector3Int Direction { get; set; }
        public ShapeGrammarStyle Style { get; set; }

        public void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = (Vector3)Direction * cubeSide / 2f;
            var obj = Style.GetFaceHor(FaceType);

            obj.SetParent(parent);
            obj.localPosition = cubePosition + offset;

            Debug.Log("Added horizontal face");
        }
    }

    public class FaceVer
    {
        public FACE_VER FaceType { get; set; }
        public Vector3Int Direction { get; set; }
        public ShapeGrammarStyle Style { get; set; }

        public void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = (Vector3)Direction * cubeSide / 2f;
            var obj = Style.GetFaceVer(FaceType);

            obj.SetParent(parent);
            obj.localPosition = cubePosition + offset;
        }
    }

    public class Corner
    {
        public CORNER CornerType { get; set; }
        public Vector3Int Direction { get; set; }
        public ShapeGrammarStyle Style { get; set; }

        public void Generate(float cubeSide, Transform parent, Vector3Int cubePosition)
        {
            if (Style == null)
                return;

            var offset = (Vector3)Direction * cubeSide / 2f;
            var obj = Style.GetCorner(CornerType);

            obj.SetParent(parent);
            obj.localPosition = cubePosition + offset;
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