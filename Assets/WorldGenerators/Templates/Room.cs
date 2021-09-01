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

    public override bool Generate(ModuleGrid moduleGrid)
    {
        var roomArea = new Area(new RoomDesigner(), styles.gothic);
        for (int i = leftFront.x; i < rightBack.x; i++)
        {
            for (int k = leftFront.y; k < rightBack.y; k++)
            {
                var coords = new Vector3Int(i, height, k);
                var module = moduleLibrary.RoomModule();
                moduleGrid[coords] = module;
                module.AddProperty(new AreaModuleProperty(roomArea));
            }
        }
        var stairsPos = ExtensionMethods.RandomVector2Int(leftFront, rightBack);
        var stairsCoords = new Vector3Int(stairsPos.x, height, stairsPos.y);

        var stModule = moduleLibrary.RoomModule();
        moduleGrid[stairsCoords] = stModule;
        stModule.SetObject(ObjectType.Stairs);
        stModule.AddProperty(new AreaModuleProperty(roomArea));

        return true;
    }
}
