using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Building : Template
{
    Box3Int boundingBox;

    public Building(Box3Int boundingBox, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.boundingBox = boundingBox;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        for (int j = boundingBox.leftBottomBack.y; j < boundingBox.rightTopFront.y; j++)
        {
            var room = new Room(boundingBox.FlattenY(), j, moduleLibrary, styles);
            room.Generate(moduleGrid);
        }

        var openArea = new Outdoor(new Box3Int(boundingBox.leftBottomBack, boundingBox.rightTopFront).Padding(Vector3Int.one), moduleLibrary, styles);
        openArea.Generate(moduleGrid);

        return true;
    }
}

public class UniformTown : Template
{
    Box2Int boundingBox;
    Vector2Int buildingExtents;

    public UniformTown(Box2Int boundingBox, Vector2Int buildingExtents, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.boundingBox = boundingBox;
        this.buildingExtents = buildingExtents;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        foreach(var subbox in boundingBox.GetSubboxes(buildingExtents))
        {
            var buildilng = new ModernBuilding(subbox.InflateY(0, moduleGrid.Height), moduleLibrary, styles);
            buildilng.Generate(moduleGrid);
        }

        return true;
    }
}

public class ModernBuilding : Template
{
    Box3Int boundingBox;

    public ModernBuilding(Box3Int boundingBox, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.boundingBox = boundingBox;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        var flatBoundingBox = boundingBox.FlattenY();
        for (int j = boundingBox.leftBottomBack.y; j < boundingBox.rightTopFront.y; j++)
        {
            var room = new Room(ExtensionMethods.RandomBox(flatBoundingBox), j, moduleLibrary, styles);
            room.Generate(moduleGrid);
        }

        return true;
    }
}
