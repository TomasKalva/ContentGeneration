using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Region;

public class TerrainGenerator : MonoBehaviour
{
    

    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        var meshCollider = GetComponent<MeshCollider>();
        var terrainMap = new TerrainMap(200, 200, 0.1f);
        terrainMap.UpdateMesh(mesh, meshCollider);
        var b1 = new Vector2(0.1f, 0.0f);
        var b2 = new Vector2(0.0f, 0.1f);

        var agents = new Agents();
        /*agents.AddAgent(new IncreasingAgent());
        agents.AddAgent(new IncreasingAgent());*/
        var averagerBrush = new AveragerBrush(new Rectangle(-2, -2, 2, 2), new Linear(1 / 25f), 0.01f);
        var strongAveragerBrush = new AveragerBrush(new Rectangle(-2, -2, 2, 2), new Linear(1 / 25f), 0.1f);
        var largeHillBrush = new AreaChangerBrush(new Circle(5), new Parabola(0.004f, 0.5f, b1, b2));
        var smallHillBrush = new AreaChangerBrush(new Circle(3),new Parabola(0.03f, 0.1f, b1, b2));
        var riverBrush = new AreaChangerBrush(new Circle(4), new Parabola(-0.006f, 0.3f, b1, b2));

        agents.AddAgent(new BrushAgent(averagerBrush));
        agents.AddAgent(new BrushAgent(averagerBrush));
        agents.AddAgent(new BrushAgent(averagerBrush));
        agents.AddAgent(new BrushAgent(strongAveragerBrush));

        agents.AddAgent(new BrushAgent(largeHillBrush));
        agents.AddAgent(new BrushAgent(smallHillBrush));
        agents.AddAgent(new BrushAgent(smallHillBrush));
        agents.AddAgent(new BrushAgent(riverBrush));

        StartCoroutine(agents.Step(terrainMap, mesh, meshCollider));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

class TerrainMap
{
    Vector3[] vertices;
    //Vector2[] newUV;
    int[] triangles;
    public int Width { get; }
    public int Height { get; }
    public float this[int x, int y]
    {
        get => vertices[VertexIndex((x + Width) % Width, (y + Height) % Height)].y;
        set => vertices[VertexIndex((x + Width) % Width, (y + Height) % Height)].y = value;
    }
    public float this[Point p] {
        get => this[p.x, p.y];
        set => this[p.x, p.y] = value;
    }


    public TerrainMap(int width, int height, float side)
    {
        this.Width = width;
        this.Height = height;

        vertices = new Vector3[width * height];
        triangles = new int[2 * 3 * width * height];

        //vertices
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                vertices[i + j * height] = new Vector3(i * side, 0, j * side);
            }
        }

        int trInd = 0;
        for (int i = 0; i < width - 1; i++)
        {
            for(int j = 0; j < height - 1; j++)
            {
                var bl = VertexIndex(i, j);
                var br = VertexIndex(i + 1, j);
                var tl = VertexIndex(i, j + 1);
                var tr = VertexIndex(i + 1, j + 1);

                //bottom
                triangles[trInd++] = bl;
                triangles[trInd++] = tl;
                triangles[trInd++] = br;
                //top
                triangles[trInd++] = tl;
                triangles[trInd++] = tr;
                triangles[trInd++] = br;
            }
        }
    }

    private int VertexIndex(int x, int y)
    {
        return x + y * Height;
    }

    public void UpdateMesh(Mesh mesh, MeshCollider meshCollider)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        /*meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;*/
    }

    public void ChangeHeight(int x, int y, float change)
    {
        vertices[VertexIndex(x, y)].y += change;
    }
    public void SetHeight(int x, int y, float change)
    {
        vertices[VertexIndex(x, y)].y = change;
    }
}

abstract class Area
{
    /// <summary>
    /// Returns points relative to the area origin (0,0).
    /// </summary>
    public abstract IEnumerable<Point> GetPoints();

    /// <summary>
    /// Maps points from area space to absolute world space.
    /// </summary>
    public abstract Point ToAbsolute(Point newOrigin, Point p);
}

class Rectangle : Area
{
    private int left;
    private int bottom;
    private int right;
    private int top;

    public Rectangle(int left, int bottom, int right, int top)
    {
        this.left = left;
        this.bottom = bottom;
        this.right = right;
        this.top = top;
    }

    public override IEnumerable<Point> GetPoints()
    {
        for(int i= left; i<= right; i++)
        {
            for(int j= bottom; j <= top; j++)
            {
                yield return new Point(i, j);
            }
        }
    }

    public override Point ToAbsolute(Point newOrigin, Point p)
    {
        return new Point(newOrigin.x + p.x, newOrigin.y + p.y);
    }
}

class Circle : Area
{
    float radius;
    Rectangle rect;

    public Circle(float radius)
    {
        this.radius = radius;
        int r = (int)Math.Ceiling(radius);
        this.rect = new Rectangle(-r, -r, r, r);
    }

    public override IEnumerable<Point> GetPoints()
    {
        return rect.GetPoints().Where(p => p.LengthSqr < radius * radius);
    }

    public override Point ToAbsolute(Point newOrigin, Point p)
    {
        return new Point(newOrigin.x + p.x, newOrigin.y + p.y);
    }
}