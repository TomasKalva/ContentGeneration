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
        //Style = new List<Style>() { styles.gothic, styles.mayan, styles.garden }.GetRandom();
        for (int j = boundingBox.leftBottomBack.y; j < boundingBox.rightTopFront.y; j++)
        {
            var room = new Room(boundingBox.FlattenY(), j, moduleLibrary, styles);
            room.Style = Style;
            room.Generate(moduleGrid);
        }

        var outdoorExtents = new Box3Int(boundingBox.leftBottomBack, boundingBox.rightTopFront).Padding(Vector3Int.one);
        var outdoor = new Outdoor(outdoorExtents, moduleLibrary, styles);
        outdoor.Generate(moduleGrid);

        foreach(var module in outdoorExtents.Select(coords => moduleGrid[coords]))
        {
            module.GetProperty<AreaModuleProperty>().Area.Inside = true;
        }

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
            Style = styles.gothic;

            var building = new ModernBuilding(subbox.InflateY(0, moduleGrid.Height), moduleLibrary, styles);
            building.Style = Style;
            building.Generate(moduleGrid);
        }

        return true;
    }
}

public class UniformRectangles : Template
{
    Box2Int boundingBox;
    Vector2Int buildingExtents;

    public UniformRectangles(Box2Int boundingBox, Vector2Int buildingExtents, Modules moduleLibrary, Styles styles) : base(moduleLibrary, styles)
    {
        this.boundingBox = boundingBox;
        this.buildingExtents = buildingExtents;
    }

    public override bool Generate(ModuleGrid moduleGrid)
    {
        foreach (var subbox in boundingBox.GetSubboxes(buildingExtents))
        {
            Style = styles.gothic;
            var building = new Building(subbox.InflateY(0, moduleGrid.Height), moduleLibrary, styles);
            building.Style = Style;
            building.Generate(moduleGrid);
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
            var room = new Room(ExtensionMethods.RandomHalfBox(flatBoundingBox), j, moduleLibrary, styles);
            room.Generate(moduleGrid);
        }

        return true;
    }
}
