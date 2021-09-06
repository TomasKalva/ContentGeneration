using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Room : Template
{
    Vector2Int leftFront;
    Vector2Int rightBack;
    int height;

    public Room(Vector2Int leftFront, Vector2Int rightBack, int height, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.leftFront = leftFront;
        this.rightBack = rightBack;
        this.height = height;
    }

    public Room(Box2Int box, int height, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.leftFront = box.leftBottom;
        this.rightBack = box.rightTop;
        this.height = height;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        var roomArea = new DisconnectedArea(new RoomDesigner(moduleGrid), styles.gothic);
        for (int i = leftFront.x; i < rightBack.x; i++)
        {
            for (int k = leftFront.y; k < rightBack.y; k++)
            {
                var coords = new Vector3Int(i, height, k);
                var module = moduleLibrary.RoomModule(roomArea);
                moduleGrid[coords] = module;
            }
        }
        var stairsPos = ExtensionMethods.RandomVector2Int(leftFront, rightBack);
        var stairsCoords = new Vector3Int(stairsPos.x, height, stairsPos.y);

        var stModule = moduleLibrary.RoomModule(roomArea);
        moduleGrid[stairsCoords] = stModule;
        stModule.SetObject(ObjectType.Stairs);

        return true;
    }
}
