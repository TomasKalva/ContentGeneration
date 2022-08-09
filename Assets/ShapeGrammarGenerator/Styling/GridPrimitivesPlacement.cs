using Assets.ShapeGrammarGenerator;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class GridPrimitivesPlacement
    {
        Grid<Cube> GridView { get; }
        QueryContext QC { get; }

        public GridPrimitivesPlacement(Grid<Cube> gridView)
        {
            GridView = gridView;
            QC = new QueryContext(GridView);
        }

        public CubeGroup RoomStyle(GridPrimitivesStyle gpStyle, CubeGroup roomArea)
        {
            roomArea.AllBoundaryFacesH().Fill(gpStyle.Wall);
            roomArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.CladdedWall);
            roomArea.CubeGroupMaxLayer(Vector3Int.down).BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor);
            roomArea.AllBoundaryCorners().Fill(gpStyle.Beam);
            return roomArea;
        }

        public CubeGroup ColonnadeStyle(GridPrimitivesStyle gpStyle, CubeGroup colonadeArea)
        {
            var floorPart = colonadeArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor).CG();
            floorPart.AllBoundaryFacesH().Fill(gpStyle.Railing);
            colonadeArea.AllBoundaryCorners().Fill(gpStyle.Beam);
            return colonadeArea;
        }

        public CubeGroup ReservationHighlighterStyle(GridPrimitivesStyle gpStyle, CubeGroup colonadeArea)
        {
            colonadeArea.AllBoundaryCorners().Fill(gpStyle.Beam);
            return colonadeArea;
        }

        public CubeGroup PlainRoomStyle(GridPrimitivesStyle gpStyle, CubeGroup roomArea)
        {
            roomArea.AllBoundaryFacesH().Fill(gpStyle.Wall);
            roomArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.CladdedWall);
            roomArea.AllSpecialCorners().Fill(gpStyle.Beam);
            roomArea.CubeGroupMaxLayer(Vector3Int.down).BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor);
            return roomArea;
        }

        public CubeGroup OpenRoomStyle(GridPrimitivesStyle gpStyle, CubeGroup roomArea)
        {
            roomArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor);
            //roomArea.AllBoundaryCorners().SetStyle(ObjectStyle).Fill(CORNER.Pillar);
            return roomArea;
        }

        /*
        public CubeGroup NoFloor(GridPrimitivesStyle gpStyle, CubeGroup roomArea)
        {
            roomArea.BoundaryFacesV(Vector3Int.down).Fill(FACE_VER.Nothing);
            return roomArea;
        }*/


        public CubeGroup EmptyStyle(GridPrimitivesStyle gpStyle, CubeGroup emptyArea)
        {
            return emptyArea;
        }

        public CubeGroup GardenStyle(GridPrimitivesStyle gpStyle, CubeGroup roomArea)
        {
            var floorParth = roomArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor).CG();

            floorParth.AllBoundaryFacesH().Fill(gpStyle.Railing);
            floorParth.AllBoundaryCorners().Fill(gpStyle.RailingPillar);
            return roomArea;
        }

        public CubeGroup FlatRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            var floorParth = roofArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor).CG();
            floorParth.AllBoundaryFacesH().Fill(gpStyle.Railing);
            floorParth.AllBoundaryCorners().Fill(gpStyle.RailingPillar);
            return roofArea;
        }

        public CubeGroup PlainFlatRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            var floorParth = roofArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor).CG();
            return roofArea;
        }

        public CubeGroup FoundationStyle(GridPrimitivesStyle gpStyle, CubeGroup foundationArea)
        {
            foundationArea.AllBoundaryFacesH().Fill(gpStyle.FoundationWall);
            foundationArea.AllSpecialCorners().Fill(gpStyle.Beam);
            return foundationArea;
        }

        public CubeGroup CliffFoundationStyle(GridPrimitivesStyle gpStyle, CubeGroup foundationArea)
        {
            foundationArea.AllBoundaryFacesH().Fill(gpStyle.FoundationWall);
            foundationArea.AllSpecialCorners().Fill(gpStyle.Beam);
            return foundationArea;
        }

        public CubeGroup PlatformStyle(GridPrimitivesStyle gpStyle, CubeGroup platformArea)
        {
            platformArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor);
            var platformTop = platformArea.BottomLayer();
            platformTop.SpecialCorners(ExtensionMethods.HorizontalDirections().ToArray())
                .MoveBy(-Vector3Int.up)
                .MoveInDirUntil(Vector3Int.down, corner => corner.MyCube.Position.y < 0)
                .Fill(gpStyle.Beam);

            return platformArea;
        }

        public CubeGroup PlatformRailingStyle(GridPrimitivesStyle gpStyle, CubeGroup platformArea)
        {
            platformArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor);
            var platformTop = platformArea.BottomLayer();
            platformTop.AllBoundaryFacesH().Fill(gpStyle.Railing);
            platformTop.SpecialCorners(ExtensionMethods.HorizontalDirections().ToArray())
                .MoveBy(-Vector3Int.up)
                .MoveInDirUntil(Vector3Int.down, corner => corner.MyCube.Position.y < 0)
                .Fill(gpStyle.Beam);

            return platformArea;
        }

        public CubeGroup StairsPathStyle(GridPrimitivesStyle gpStyle, CubeGroup path)
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
            hor.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.PathFullFloor);

            var verTop = path.Cubes.Where3(
                (prev, cube, next) =>
                {
                    var dirTo = cube.Position - prev.Position;
                    var dirFrom = next.Position - cube.Position;
                    return dirTo.y > 0 || dirFrom.y < 0;
                    }).ToCubeGroup(GridView);

            // empty vertical faces between cubes
            verTop.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.NoFloor);

            // empty horizontal faces between cubes
            var horFacesInside = path.InsideFacesH().Facets;
            horFacesInside.ForEach(faceH =>
            {
                //faceH.FaceType = FACE_HOR.Door;
                //return;
                var otherFace = faceH.OtherFacet();

                if (faceH.FaceType == FACE_HOR.Door || otherFace.FaceType == FACE_HOR.Door)
                {
                    return;
                }
                if (faceH.FaceType == FACE_HOR.Wall || otherFace.FaceType == FACE_HOR.Wall)
                {
                    faceH.FacePrimitive = gpStyle.Door();
                    //otherFace.FaceType = FACE_HOR.Nothing;
                    return;
                }
                if (faceH.FaceType == FACE_HOR.Railing || otherFace.FaceType == FACE_HOR.Railing)
                {
                    faceH.FacePrimitive = gpStyle.RailingDoor();
                    //otherFace.FaceType = FACE_HOR.Nothing;
                    return;
                }
                faceH.FacePrimitive = gpStyle.NoWall();
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

                        stairsCube.CubePrimitive = gpStyle.Stairs(stairsDirection);
                    }
                }
            );

            return path;
        }

        public CubeGroup FallStyle(GridPrimitivesStyle gpStyle, CubeGroup fallArea)
        {
            // assumes that first and last move are horizontal, vertical shaft inbetween

            var shaftCubes = fallArea.Cubes.Skip(1).Reverse().Skip(1).Reverse().ToCubeGroup(GridView);

            // remove floor from the fall area
            var shaftFloors = new FaceVerGroup(GridView, shaftCubes.Cubes.Select2((c1, c2) => (c1.Position.y > c2.Position.y ? c1 : c2).FacesVer(Vector3Int.down)).ToList());
            shaftFloors.Fill(gpStyle.NoFloor);

            // no walls between horizontal
            var horFacesInside = fallArea.InsideFacesH().Fill(gpStyle.NoWall);

            return fallArea;
        }

        public CubeGroup DoorStyle(GridPrimitivesStyle gpStyle, CubeGroup doorFromFirst)
        {
            Debug.Assert(doorFromFirst.Cubes.Count() == 2);

            doorFromFirst.Cubes.First().Group().NeighborsInGroupH(doorFromFirst).Fill(gpStyle.Door);
            //doorFromFirst.Cubes.Skip(1).First().Group().NeighborsInGroupH(doorFromFirst).Fill(gpStyle.NoWall);

            return doorFromFirst;
        }

        Vector3Int DefaultRoofDirection(CubeGroup roofArea)
        {
            var extents = roofArea.Extents();
            return extents.x > extents.z ? Vector3Int.right : Vector3Int.forward;
        }
        CubeGroup RoofStyle(CubeGroup roofArea, GeometricPrimitive roofPrim, Vector3Int direction)
        {
            var extents = roofArea.Extents();
            var halfExtents = ((Vector3)extents) / 2f;
            var scaleExtents = direction.x == 0 ? extents : new Vector3Int(extents.z, extents.y, extents.x);

            var roof = roofPrim.New().transform;
            roof.transform.localScale = Vector3.Scale(roof.transform.localScale, scaleExtents);

            var lbb = roofArea.LeftBottomBack();
            var center = new Vector3(lbb.x + halfExtents.x - 0.5f, lbb.y, lbb.z + halfExtents.z - 0.5f);

            roof.position = center * 2.8f;
            roof.rotation = Quaternion.LookRotation(direction);
            return roofArea;
        }

        public CubeGroup GableRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return RoofStyle(roofArea, gpStyle.GableRoof(), DefaultRoofDirection(roofArea));
        }
        
        public CubeGroup PointyRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return RoofStyle(roofArea, gpStyle.PointyRoof(), Vector3Int.forward);
        }

        public CubeGroup CrossRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return RoofStyle(roofArea, gpStyle.CrossRoof(), Vector3Int.forward);
        }

        public CubeGroup DirectionalRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea, Vector3Int direction)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return RoofStyle(roofArea, gpStyle.DirectionalRoof(), direction);
        }
    }
}
