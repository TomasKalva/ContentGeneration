using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Building : Template
{
    Vector3Int leftBottomBack;
    Vector3Int rightTopFront;

    public Building(Vector3Int leftBottomBack, Vector3Int rightTopFront, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.leftBottomBack = leftBottomBack;
        this.rightTopFront = rightTopFront;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        for (int j = leftBottomBack.y; j < rightTopFront.y; j++)
        {
            var room = new Room(leftBottomBack.XZ(), rightTopFront.XZ(), j, moduleLibrary, styles);
            room.Generate(moduleGrid);
        }

        var openArea = new Outdoor(new Box3Int(leftBottomBack, rightTopFront).Padding(Vector3Int.one), moduleLibrary, styles);
        openArea.Generate(moduleGrid);

        /*var roofArea = new OpenArea(new Box3Int(new Vector3Int(leftBottomBack.x, rightTopFront.y, leftBottomBack.z), new Vector3Int(rightTopFront.x, rightTopFront.y + 1, rightTopFront.z)), moduleLibrary);
        roofArea.Generate(moduleGrid);*/

        return true;
    }
}
