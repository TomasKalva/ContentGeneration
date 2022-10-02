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

        public CubeGroup BridgeStyle(GridPrimitivesStyle gpStyle, CubeGroup bridgeArea, Vector3Int bridgeDirection)
        {
            var floorPart = bridgeArea.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.Floor).CG();
            floorPart.AllBoundaryFacesH().Fill(gpStyle.Railing);
            floorPart.AllBoundaryCorners().Fill(gpStyle.RailingPillar);

            // remove railing and pillars from the incoming direction
            var incomingFloor = floorPart.OpAdd().ExtrudeDir(-bridgeDirection).OpNew();
            incomingFloor.FacesH(ExtensionMethods.HorizontalDirections().ToArray()).Minus(incomingFloor.AllBoundaryFacesH()).Fill(gpStyle.NoWall);
            incomingFloor.Corners(ExtensionMethods.HorizontalDirections().ToArray()).Minus(incomingFloor.AllBoundaryCorners()).Fill(gpStyle.NoPillar);

            return bridgeArea;
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

        /// <summary>
        /// Returns horizontal connection between two neighboring cubes. The connection will fit well into the context
        /// given by the face.
        /// </summary>
        HorFacePrimitive GetFittingDoorOrEmpty(GridPrimitivesStyle gpStyle, FaceHor faceH)
        {
            var otherFace = faceH.OtherFacet();

            if (faceH.FaceType == FACE_HOR.Door || otherFace.FaceType == FACE_HOR.Door)
            {
                return faceH.FacePrimitive;
            }
            if (faceH.FaceType == FACE_HOR.Wall || otherFace.FaceType == FACE_HOR.Wall)
            {
                return gpStyle.Door();
            }
            if (faceH.FaceType == FACE_HOR.Railing || otherFace.FaceType == FACE_HOR.Railing)
            {
                return gpStyle.RailingDoor();
            }
            return gpStyle.NoWall();
        }

        /// <summary>
        /// Returns horizontal connection between two neighboring cubes. The connection will fit well into the context
        /// given by the face.
        /// </summary>
        HorFacePrimitive GetFittingHoleOrEmpty(GridPrimitivesStyle gpStyle, FaceHor faceH)
        {
            var otherFace = faceH.OtherFacet();

            if (faceH.FaceType == FACE_HOR.Door || otherFace.FaceType == FACE_HOR.Door)
            {
                return faceH.FacePrimitive;
            }
            if (faceH.FaceType == FACE_HOR.Wall || otherFace.FaceType == FACE_HOR.Wall)
            {
                return gpStyle.WallHole();
            }
            if (faceH.FaceType == FACE_HOR.Railing || otherFace.FaceType == FACE_HOR.Railing)
            {
                return gpStyle.WallHole();
            }
            return gpStyle.NoWall();
        }

        public CubeGroup StairsPathStyle(GridPrimitivesStyle gpStyle, CubeGroup path)
        {
            // find cubes with floor
            var floorCubes = path.Where3All(
                (prev, cube, next) => 
                {
                    var verDirTo = cube.Position.y - prev.Position.y;
                    var verDirFrom = next.Position.y - cube.Position.y;
                    if(verDirTo == 0 && verDirFrom == 0)
                    {
                        return true;
                    }
                    return false;
                }
            );
            floorCubes.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.PathFullFloor);
            var horFacesInside = path.ConsecutiveInsideFacesH();

            // add railing
            var middleFloorCubes = floorCubes.Cubes.Skip(1).SkipLast(1).ToCubeGroup(GridView);
            middleFloorCubes.AllBoundaryFacesH().Minus(horFacesInside).FillIfEmpty(gpStyle.Railing);
            middleFloorCubes.AllBoundaryCorners().FillIfEmpty(gpStyle.RailingPillar);

            var verTop = path.Cubes.Where3(
                (prev, cube, next) =>
                {
                    var dirTo = cube.Position - prev.Position;
                    var dirFrom = next.Position - cube.Position;
                    return dirTo.y > 0 || dirFrom.y < 0;
                    }).ToCubeGroup(GridView);

            // place railing around if found floor
            var verTopWithFloor = verTop.Where(cube => cube.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor);
            verTopWithFloor.AllBoundaryFacesH().FillIfEmpty(gpStyle.Railing);
            verTopWithFloor.AllBoundaryCorners().FillIfEmpty(gpStyle.RailingPillar);

            // empty vertical faces between cubes
            verTop.BoundaryFacesV(Vector3Int.down).Fill(gpStyle.NoFloor);

            // empty horizontal faces between cubes
            //var horFacesInside = path.InsideFacesH().Facets;
            horFacesInside.Facets.ForEach(faceH =>
            {
                faceH.FacePrimitive = GetFittingDoorOrEmpty(gpStyle, faceH);
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
            var horFacesInside = fallArea.ConsecutiveInsideFacesH();//.Fill(gpStyle.NoWall);
            horFacesInside.Facets.ForEach(faceH =>
            {
                faceH.FacePrimitive = GetFittingHoleOrEmpty(gpStyle, faceH);
            });

            return fallArea;
        }

        public CubeGroup ConnectionStyle(GridPrimitivesStyle gpStyle, CubeGroup twoNeighbors)
        {
            Debug.Assert(twoNeighbors.Cubes.Count() == 2);

            twoNeighbors.Cubes.First().Group().NeighborsInGroupH(twoNeighbors).Facets
                .ForEach(faceH => 
                    faceH.FacePrimitive = GetFittingDoorOrEmpty(gpStyle, faceH)
                );

            return twoNeighbors;
        }

        Vector3Int DefaultRoofDirection(CubeGroup roofArea)
        {
            var extents = roofArea.Extents();
            return extents.x > extents.z ? Vector3Int.right : Vector3Int.forward;
        }

        public CubeGroup PlaceInDirection(CubeGroup area, GeometricPrimitive roofPrim, Vector3Int direction)
        {
            area.MakeArchitectureElements =
                () =>
                {
                    var extents = area.Extents();
                    var halfExtents = ((Vector3)extents) / 2f;
                    var scaleExtents = direction.x == 0 ? extents : new Vector3Int(extents.z, extents.y, extents.x);

                    var obj = roofPrim.New().transform;
                    obj.transform.localScale = Vector3.Scale(obj.transform.localScale, scaleExtents);

                    var lbb = area.LeftBottomBack();
                    var center = new Vector3(lbb.x + halfExtents.x - 0.5f, lbb.y, lbb.z + halfExtents.z - 0.5f);

                    obj.position = center * 2.8f;
                    obj.rotation = Quaternion.LookRotation(direction);
                    return obj;
                };
            return area;
        }

        public CubeGroup GableRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return PlaceInDirection(roofArea, gpStyle.GableRoof, DefaultRoofDirection(roofArea));
        }
        
        public CubeGroup PointyRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return PlaceInDirection(roofArea, gpStyle.PointyRoof, Vector3Int.forward);
        }

        public CubeGroup CrossRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return PlaceInDirection(roofArea, gpStyle.CrossRoof, Vector3Int.forward);
        }

        public CubeGroup DirectionalRoofStyle(GridPrimitivesStyle gpStyle, CubeGroup roofArea, Vector3Int direction)
        {
            roofArea.BottomLayer().AllBoundaryFacesH().Fill(gpStyle.Cladding);
            return PlaceInDirection(roofArea, gpStyle.DirectionalRoof, direction);
        }
    }
}
