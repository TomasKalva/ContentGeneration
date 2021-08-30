using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RoomArea : Area
{
    Vector2Int leftFront;
    Vector2Int rightBack;
    int height;

    public RoomArea(Vector2Int leftFront, Vector2Int rightBack, int height, Modules moduleLibrary) : base(moduleLibrary)
    {
        this.leftFront = leftFront;
        this.rightBack = rightBack;
        this.height = height;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        for (int i = leftFront.x; i < rightBack.x; i++)
        {
            for (int k = leftFront.y; k < rightBack.y; k++)
            {
                var coords = new Vector3Int(i, height, k);
                var module = moduleLibrary.RoomModule();
                moduleGrid[coords] = module;
                module.AddProperty(new AreaModuleProperty(this));
            }
        }
        var stairsPos = ExtensionMethods.RandomVector2Int(leftFront, rightBack);
        var stairsCoords = new Vector3Int(stairsPos.x, height, stairsPos.y);

        var stModule = moduleLibrary.StairsModule();
        moduleGrid[stairsCoords] = stModule;
        stModule.AddProperty(new AreaModuleProperty(this));

        return true;
    }
}
