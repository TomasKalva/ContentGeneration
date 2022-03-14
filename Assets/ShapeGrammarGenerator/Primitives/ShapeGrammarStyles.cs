using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class ShapeGrammarStyles
    {
        Grid<Cube> GridView { get; }
        QueryContext QC { get; }
        ShapeGrammarObjectStyle DefaultHouseStyle { get; }
        ShapeGrammarObjectStyle DefaultGardenStyle { get; }

        public ShapeGrammarStyles(Grid<Cube> gridView, ShapeGrammarObjectStyle defaultHouseStyle, ShapeGrammarObjectStyle gardenStyle)
        {
            GridView = gridView;
            QC = new QueryContext(GridView);
            DefaultHouseStyle = defaultHouseStyle;
            DefaultGardenStyle = gardenStyle;
        }

        public CubeGroup RoomStyle(CubeGroup roomArea)
        {
            roomArea.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Wall);
            roomArea.CubeGroupMaxLayer(Vector3Int.down).BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);
            roomArea.AllBoundaryCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);
            return roomArea;
        }

        public CubeGroup ColonnadeStyle(CubeGroup colonadeArea)
        {
            var floorPart = colonadeArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor).CubeGroup();
            floorPart.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Railing);
            colonadeArea.AllBoundaryCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);
            return colonadeArea;
        }

        public CubeGroup PlainRoomStyle(CubeGroup roomArea)
        {
            roomArea.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Wall);
            roomArea.AllSpecialCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);
            roomArea.CubeGroupMaxLayer(Vector3Int.down).BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);
            return roomArea;
        }

        public CubeGroup OpenRoomStyle(CubeGroup roomArea)
        {
            roomArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);
            //roomArea.AllBoundaryCorners().SetStyle(ObjectStyle).Fill(CORNER.Pillar);
            return roomArea;
        }

        public CubeGroup BridgeStyle(CubeGroup roomArea)
        {
            roomArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);
            roomArea.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Open);
            roomArea.AllBoundaryCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.Nothing);
            return roomArea;
        }

        public CubeGroup NoFloor(CubeGroup roomArea)
        {
            roomArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Nothing);
            return roomArea;
        }


        public CubeGroup EmptyStyle(CubeGroup emptyArea)
        {
            return emptyArea;
        }

        public CubeGroup GardenStyle(CubeGroup roomArea)
        {
            roomArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultGardenStyle).Fill(FACE_VER.Floor);
            return roomArea;
        }

        public CubeGroup FlatRoofStyle(CubeGroup roofArea)
        {
            var floorParth = roofArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor).CubeGroup();
            floorParth.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Railing);
            floorParth.AllBoundaryCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.RailingPillar);
            return roofArea;
        }

        public CubeGroup PlainFlatRoofStyle(CubeGroup roofArea)
        {
            var floorParth = roofArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor).CubeGroup();
            return roofArea;
        }

        public CubeGroup FoundationStyle(CubeGroup foundationArea)
        {
            foundationArea.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Wall);
            foundationArea.AllSpecialCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);
            return foundationArea;
        }

        public CubeGroup PlatformStyle(CubeGroup platformArea)
        {
            platformArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);
            var platformTop = platformArea.WithFloor();
            platformTop.SpecialCorners(ExtensionMethods.HorizontalDirections().ToArray())
                .MoveBy(-Vector3Int.up)
                .MoveInDirUntil(Vector3Int.down, corner => corner.MyCube.Position.y < 0)
                .SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);

            return platformArea;
        }

        public CubeGroup PlatformRailingStyle(CubeGroup platformArea)
        {
            platformArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);
            var platformTop = platformArea.WithFloor();
            platformTop.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Railing);
            platformTop.SpecialCorners(ExtensionMethods.HorizontalDirections().ToArray())
                .MoveBy(-Vector3Int.up)
                .MoveInDirUntil(Vector3Int.down, corner => corner.MyCube.Position.y < 0)
                .SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);

            return platformArea;
        }

        public CubeGroup BalconyStyle(CubeGroup balcony, CubeGroup house)
        {
            // Floor
            balcony.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);

            // Add railing to the balcony
            var facesNearHouse = balcony.AllBoundaryFacesH()
               .Neighboring(house);
            var railingFaces = balcony.AllBoundaryFacesH()
               .Minus(facesNearHouse);

            railingFaces
               .SetStyle(DefaultHouseStyle)
               .Fill(FACE_HOR.Railing);
            railingFaces
               .Corners()
               .SetStyle(DefaultHouseStyle)
               .Fill(CORNER.RailingPillar);

            // Door to house
            facesNearHouse
               .Facets.GetRandom()?.Group()
               .Fill(FACE_HOR.Door);

            return balcony;
        }

        public CubeGroup StairsPathStyle(CubeGroup path)
        {
            var hor = path.Where3Cycle(
                (prev, cube, next) => 
                {
                    var dirTo = cube.Position - prev.Position;
                    var dirFrom = next.Position - cube.Position;
                    if(dirTo.y == 0 && dirFrom.y == 0)
                    {
                        return true;
                    }
                    return false;
                }
            );
            hor.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);

            var verTop = path.Cubes.Where3(
                (prev, cube, next) =>
                {
                    var dirTo = cube.Position - prev.Position;
                    var dirFrom = next.Position - cube.Position;
                    return dirTo.y > 0 || dirFrom.y < 0;
                    }).ToCubeGroup(GridView);

            // empty vertical faces between cubes
            verTop.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Nothing);

            // empty horizontal faces between cubes
            var horFacesInside = path.InsideFacesH().Facets;
            horFacesInside.ForEach(faceH =>
            {
                //faceH.FaceType = FACE_HOR.Door;
                //return;

                var otherFace = faceH.OtherFacet();
                if(faceH.FaceType == FACE_HOR.Door || otherFace.FaceType == FACE_HOR.Door)
                {
                    return;
                }
                if (faceH.FaceType == FACE_HOR.Wall || otherFace.FaceType == FACE_HOR.Wall)
                {
                    faceH.FaceType = FACE_HOR.Door;
                    otherFace.FaceType = FACE_HOR.Nothing;
                    return;
                }
                faceH.FaceType = FACE_HOR.Nothing;
            });

            //hor.Intersect(path.AllBoundaryFacesH()).SetStyle(ObjectStyle).Fill(FACE_HOR.Railing);
            //hor.AllBoundaryCorners().Intersect(path.AllBoundaryCorners()).SetStyle(ObjectStyle).Fill(CORNER.RailingPillar);

            // add stairs
            path.Cubes.ForEach3(
                (prev, cube, next) =>
                {
                    var dirTo = cube.Position - prev.Position;
                    var dirFrom = next.Position - cube.Position;
                    if (dirTo.y != 0)
                    {
                        var stairsCube = dirTo.y > 0 ? prev : cube;
                        var stairsDirection = dirTo.y > 0 ? -dirFrom : dirFrom;

                        stairsCube.Style = this.DefaultHouseStyle;
                        stairsCube.Object = CUBE.Stairs;
                        stairsCube.ObjectDir = stairsDirection;
                    }
                }
            );

            return path;
        }

        public CubeGroup ElevatorStyle(CubeGroup elevatorShaftArea, Libraries lib)
        {
            // assumes that first and last move are horizontal, vertical shaft inbetween

            var shaftCubes = elevatorShaftArea.Cubes.Skip(1).Reverse().Skip(1).Reverse().ToCubeGroup(GridView);

            var bottom = shaftCubes.Cubes.ArgMin(cube => cube.Position.y).Position;
            var height = elevatorShaftArea.Extents().y;

            
            // remove floor from shaft
            var shaftFloors = new FaceVerGroup(GridView, shaftCubes.Cubes.Select2((_, topCube) => topCube.FacesVer(Vector3Int.down)).ToList());
            shaftFloors.Fill(FACE_VER.Nothing).SetStyle(DefaultHouseStyle);

            // wall
            shaftCubes.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Wall);
            shaftCubes.AllSpecialCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);

            // doors
            var doorCubes = new CubeGroup(GridView, new List<Cube>() 
            {
                elevatorShaftArea.Cubes.First(), elevatorShaftArea.Cubes.Last() 
            });
            doorCubes.NeighborsInGroupH(elevatorShaftArea).SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Door);

            // elevator
            var elevator = lib.InteractiveObjects.Elevator((height - 1), false);
            elevator.Object.transform.position = (Vector3)bottom * 2.8f;
            
            // levers
            doorCubes.Cubes.ForEach(cube =>
            {
                var lever = lib.InteractiveObjects.Lever(elevator.Activate);
                lever.transform.position = (Vector3)cube.Position * 2.8f;
            });

            return elevatorShaftArea;
        }

        public CubeGroup DoorStyle(CubeGroup doorFromFirst)
        {
            Debug.Assert(doorFromFirst.Cubes.Count() == 2);

            doorFromFirst.Cubes.First().Group().NeighborsInGroupH(doorFromFirst).SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Door);
            doorFromFirst.Cubes.Skip(1).First().Group().NeighborsInGroupH(doorFromFirst).SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Nothing);

            return doorFromFirst;
        }

    }
}
