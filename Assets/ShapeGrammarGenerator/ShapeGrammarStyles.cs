using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.Grid;

namespace ShapeGrammar
{
    public class ShapeGrammarStyles
    {
        Grid GridView { get; }
        QueryContext QC { get; }
        ShapeGrammarObjectStyle DefaultHouseStyle { get; }
        ShapeGrammarObjectStyle DefaultGardenStyle { get; }

        public ShapeGrammarStyles(Grid gridView, ShapeGrammarObjectStyle defaultHouseStyle, ShapeGrammarObjectStyle gardenStyle)
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

        public CubeGroup OpenRoomStyle(CubeGroup roomArea)
        {
            roomArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor);
            //roomArea.AllBoundaryCorners().SetStyle(ObjectStyle).Fill(CORNER.Pillar);
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
            var floorParth = roofArea.BoundaryFacesV(Vector3Int.down).SetStyle(DefaultHouseStyle).Fill(FACE_VER.Floor).Cubes();
            floorParth.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Railing);
            floorParth.AllBoundaryCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.RailingPillar);
            return roofArea;
        }

        public CubeGroup FoundationStyle(CubeGroup foundationArea)
        {
            foundationArea.AllBoundaryFacesH().SetStyle(DefaultHouseStyle).Fill(FACE_HOR.Wall);
            foundationArea.AllBoundaryCorners().SetStyle(DefaultHouseStyle).Fill(CORNER.Pillar);
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
            var horFacesInside = path.FacesH(ExtensionMethods.HorizontalDirections().ToArray()).Facets.SetMinus(path.AllBoundaryFacesH().Facets);
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
            //new FaceHorGroup(path.Grid, horFacesInside.ToList()).Fill(FACE_HOR.Nothing);

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

    }
}
