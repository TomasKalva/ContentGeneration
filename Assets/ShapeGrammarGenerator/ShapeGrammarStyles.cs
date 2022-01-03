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

    public delegate CubeGroup StyleSetter(CubeGroup cubeGroup);

    public class ShapeGrammarStyles
    {
        Grid Grid { get; }
        QueryContext QC { get; }
        ShapeGrammarObjectStyle ObjectStyle { get; }

        public ShapeGrammarStyles(Grid grid, QueryContext qC, ShapeGrammarObjectStyle objectStyle)
        {
            Grid = grid;
            QC = qC;
            ObjectStyle = objectStyle;
        }

        public CubeGroup RoomStyle(CubeGroup roomArea)
        {
            roomArea.AllBoundaryFacesH().SetStyle(ObjectStyle).Fill(FACE_HOR.Wall);
            roomArea.BoundaryFacesV(Vector3Int.down).SetStyle(ObjectStyle).Fill(FACE_VER.Floor);
            roomArea.AllBoundaryCorners().SetStyle(ObjectStyle).Fill(CORNER.Pillar);
            return roomArea;
        }

        public CubeGroup FlatRoofStyle(CubeGroup roofArea)
        {
            roofArea.AllBoundaryFacesH().SetStyle(ObjectStyle).Fill(FACE_HOR.Railing);
            roofArea.BoundaryFacesV(Vector3Int.down).SetStyle(ObjectStyle).Fill(FACE_VER.Floor);
            roofArea.AllBoundaryCorners().SetStyle(ObjectStyle).Fill(CORNER.Pillar);
            return roofArea;
        }

        public CubeGroup FoundationStyle(CubeGroup foundationArea)
        {
            foundationArea.AllBoundaryFacesH().SetStyle(ObjectStyle).Fill(FACE_HOR.Wall);
            foundationArea.AllBoundaryCorners().SetStyle(ObjectStyle).Fill(CORNER.Pillar);
            return foundationArea;
        }

        public CubeGroup PlatformStyle(CubeGroup platformTop)
        {
            platformTop.BoundaryFacesV(Vector3Int.down).SetStyle(ObjectStyle).Fill(FACE_VER.Floor);
            platformTop.BoundaryCorners(ExtensionMethods.HorizontalDirections().ToArray())
                .MoveBy(-Vector3Int.up)
                .MoveInDirUntil(Vector3Int.down, corner => corner.MyCube.Position.y < 0)
                .SetStyle(ObjectStyle).Fill(CORNER.Pillar);

            return platformTop;
        }

        public CubeGroup BalconyStyle(CubeGroup balcony, CubeGroup house)
        {
            // Floor
            balcony.BoundaryFacesV(Vector3Int.down).SetStyle(ObjectStyle).Fill(FACE_VER.Floor);

            // Add railing to the balcony
            var facesNearHouse = balcony.AllBoundaryFacesH()
               .Neighboring(house);
            var railingFaces = balcony.AllBoundaryFacesH()
               .Minus(facesNearHouse);

            railingFaces
               .SetStyle(ObjectStyle)
               .Fill(FACE_HOR.Railing);
            railingFaces
               .Corners()
               .SetStyle(ObjectStyle)
               .Fill(CORNER.Pillar);

            // Door to house
            facesNearHouse
               .Facets.GetRandom().Group()
               .Fill(FACE_HOR.Door);

            return balcony;
        }
    }
}
