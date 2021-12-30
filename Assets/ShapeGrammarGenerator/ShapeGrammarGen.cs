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

        public CubeGroup Room(CubeGroup roomArea, ShapeGrammarStyle style)
        {
            roomArea.AllBoundaryFacesH().SetStyle(style).Fill(FACE_HOR.Wall);
            roomArea.BoundaryFacesV(Vector3Int.down).SetStyle(style).Fill(FACE_VER.Floor);
            roomArea.AllBoundaryCorners().SetStyle(style).Fill(CORNER.Pillar);
            return roomArea;
        }

        public CubeGroup FlatRoof(Box2Int areaXZ, int posY, ShapeGrammarStyle style)
        {
            var box = areaXZ.InflateY(posY, posY + 1);
            var room = QC.GetBox(box);
            room.AllBoundaryFacesH().SetStyle(style).Fill(FACE_HOR.Railing);
            room.BoundaryFacesV(Vector3Int.down).SetStyle(style).Fill(FACE_VER.Floor);
            room.AllBoundaryCorners().SetStyle(style).Fill(CORNER.Pillar);
            return room;
        }

        public CubeGroup House(Box2Int areaXZ, int posY, ShapeGrammarStyle style)
        {
            var room = Room(areaXZ.InflateY(posY, posY + 2), style);
            var roof = FlatRoof(areaXZ, posY + 2, style);
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

        public CubeGroup BalconyWide(CubeGroup house, ShapeGrammarStyle style)
        {
            // Find a cube for the balcony
            var balcony = house.Where(cube => cube.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor)
               .BoundaryFacesH(Vector3Int.left)
               .Where(face => !face.OtherCube.Changed && !face.OtherCube.In(house))
               .Extrude(1);

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
}
