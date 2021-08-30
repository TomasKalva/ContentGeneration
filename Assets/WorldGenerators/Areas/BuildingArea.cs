using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingArea : Area
{
    Vector3Int leftBottomBack;
    Vector3Int rightTopFront;

    public BuildingArea(Vector3Int leftBottomBack, Vector3Int rightTopFront, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.leftBottomBack = leftBottomBack;
        this.rightTopFront = rightTopFront;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        for (int j = leftBottomBack.y; j < rightTopFront.y; j++)
        {
            var room = new RoomArea(leftBottomBack.XZ(), rightTopFront.XZ(), j, moduleLibrary);
            room.Generate(moduleGrid);
        }

        var openArea = new OpenArea(new Box3Int(leftBottomBack, rightTopFront).Padding(Vector3Int.one), moduleLibrary);
        openArea.Generate(moduleGrid);

        /*var roofArea = new OpenArea(new Box3Int(new Vector3Int(leftBottomBack.x, rightTopFront.y, leftBottomBack.z), new Vector3Int(rightTopFront.x, rightTopFront.y + 1, rightTopFront.z)), moduleLibrary);
        roofArea.Generate(moduleGrid);*/

        return true;
    }
}
