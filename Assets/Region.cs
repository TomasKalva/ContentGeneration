using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets
{
    class Region
    {
        private int[,] tiles;
        private int width;
        private int height;
        private int tileTypeCount;

        int this[int i, int j]
        {
            set => tiles[i, j] = value;
            get => tiles[i, j];
        }

        public Region(int width, int height)
        {
            this.width = width;
            this.height = height;
            tiles = new int[width, height];
            tileTypeCount = 2;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tiles[i, j] = Random.Range(0, tileTypeCount);
                }
            }
        }

        public void GenerateGeometry(List<GameObject> objects, Transform parent)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int index = tiles[i, j];
                    var obj = objects[index];
                    var tile = Object.Instantiate(obj, parent);
                    tile.transform.localPosition = new Vector3(i, 0, j);
                }
            }
        }

        public Region Mutate(float p)
        {
            var reg = new Region(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (Random.value < p)
                    {
                        reg[i, j] = Random.Range(0, tileTypeCount);
                    }
                    else
                    {
                        reg[i, j] = this[i, j];
                    }
                }
            }
            return reg;
        }

        public Region Clone()
        {
            var newReg = new Region(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    newReg[i, j] = tiles[i, j];
                }
            }
            return newReg;
        }

        public (Region, Region) Cross(Region r)
        {
            /*
            ++++
            ++++
            ++++

            oooo
            oooo
            oooo

            ->

            +ooo
            +ooo
            +ooo
             */

            int cutX = Random.Range(1, width - 1);
            int cutY = Random.Range(1, height - 1);
            var newRegion1 = new Region(width, height);
            var newRegion2 = new Region(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i < cutX)
                    {
                        if (j < cutY)
                        {
                            newRegion1[i, j] = tiles[i, j];
                            newRegion2[i, j] = r[i, j];
                        }
                        else
                        {
                            newRegion1[i, j] = r[i, j];
                            newRegion2[i, j] = tiles[i, j];
                        }
                        tiles[i, j] = Random.Range(0, tileTypeCount);
                    }
                    else
                    {
                        if (j < cutY)
                        {
                            newRegion1[i, j] = r[i, j];
                            newRegion2[i, j] = tiles[i, j];
                        }
                        else
                        {
                            newRegion1[i, j] = tiles[i, j];
                            newRegion2[i, j] = r[i, j];
                        }
                    }
                }
            }
            return (newRegion1, newRegion2);
        }

        private bool Blocked(int i, int j)
        {
            return tiles[i, j] == 1;
        }

        public struct Point
        {
            public int x;
            public int y;

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public Vector2 GetVector(Vector2 b1, Vector2 b2)
            {
                return x * b1 + y * b2;
            }

            public float LengthSqr => x * x + y * y;
        }

        public IEnumerable<Point> Neighbors(Point p)
        {
            if (p.x > 0)
            {
                yield return new Point(p.x - 1, p.y);
            }
            if (p.x < width - 1)
            {
                yield return new Point(p.x + 1, p.y);
            }
            if (p.y > 0)
            {
                yield return new Point(p.x, p.y - 1);
            }
            if (p.y < height - 1)
            {
                yield return new Point(p.x, p.y + 1);
            }
        }

        public class Component
        {
            public List<Point> Points { get; }
            public int ComponentType { get; }
            public int Count => Points.Count;

            public Component(int type)
            {
                Points = new List<Point>();
                ComponentType = type;
            }

            public void AddPoint(Point p)
            {
                Points.Add(p);
            }
        }

        public class ComponentFinder
        {
            bool[,] discovered;
            Region region;

            public ComponentFinder(Region region)
            {
                discovered = new bool[region.width, region.height];
                this.region = region;
            }

            public Component GetComponent(Point p)
            {
                var stack = new Stack<Point>();
                var type = region[p.x, p.y];
                var component = new Component(type);
                stack.Push(p);

                while (stack.Any())
                {
                    var current = stack.Pop();
                    int x = current.x;
                    int y = current.y;

                    if (discovered[x, y] || region[x, y] != type)
                        continue;

                    component.AddPoint(current);
                    discovered[x, y] = true;
                    foreach (var n in region.Neighbors(current))
                    {
                        stack.Push(n);
                    }
                }
                return component;
            }

            public List<Component> ConnectedComponents()
            {
                var components = new List<Component>();

                for (int i = 0; i < region.width; i++)
                {
                    for (int j = 0; j < region.height; j++)
                    {
                        if (!discovered[i, j])
                            components.Add(GetComponent(new Point(i, j)));
                    }
                }
                return components;
            }
        }


        public int WallsCount()
        {
            var wallsCount = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    wallsCount += Blocked(i, j) ? 1 : 0;
                }
            }
            return wallsCount;
        }
    }

}
