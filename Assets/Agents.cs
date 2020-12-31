using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Region;
using Random = UnityEngine.Random;

namespace Assets
{

    class Agents
    {
        List<Agent> agents;

        public Agents()
        {
            this.agents = new List<Agent>();
        }

        public void AddAgent(Agent agent)
        {
            agents.Add(agent);
        }

        public IEnumerator Step(TerrainMap map, Mesh mesh, MeshCollider meshCollider)
        {
            while (agents.Any())
            {
                foreach (var a in agents)
                {
                    for(int i=0;i<100;i++)
                        a.Step(map);
                }
                map.UpdateMesh(mesh, meshCollider);
                agents.RemoveAll(a => a.Finished);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    abstract class Agent
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }
        protected int Steps { get; set; }
        public Agent()
        {
            Steps = int.MaxValue;
            X = 50;
            Y = 50;
        }
        public Agent SetX(int x)
        {
            X = x;
            return this;
        }
        public Agent SetY(int y)
        {
            Y = y;
            return this;
        }
        public Agent SetSteps(int steps)
        {
            Steps = steps;
            return this;
        }
        public void Step(TerrainMap map)
        {
            StepLogic(map);
            Move(Random.Range(0, 3) - 1, Random.Range(0, 3) - 1, map);
            Steps--;
        }
        public abstract void StepLogic(TerrainMap map);
        public bool Finished => Steps <= 0;
        private void Move(int dx, int dy, TerrainMap map)
        {
            X = (X + dx + map.Width) % map.Width;
            Y = (Y + dy + map.Height) % map.Height;
        }
    }
}

class IncreasingAgent : Agent
{
    public IncreasingAgent()
    {
    }

    public override void StepLogic(TerrainMap map)
    {
        map.ChangeHeight(X, Y, Random.value * 0.1f);
    }
}

class BrushAgent : Agent
{
    Brush brush;

    public BrushAgent(Brush brush)
    {
        this.brush = brush;
    }

    public override void StepLogic(TerrainMap map)
    {
        brush.Stroke(map, X, Y);
    }
}

class AveragerAgent : Agent
{
    private Area area;
    private Function function;
    float averagingSpeed;

    public AveragerAgent(Area area, Function function, float averagingSpeed)
    {
        this.area = area;
        this.function = function;
        this.averagingSpeed = averagingSpeed;
    }

    public override void StepLogic(TerrainMap map)
    {
        float totalChange = 0f;
        foreach (var p in area.GetPoints())
        {
            var coords = area.ToAbsolute(new Point(X,Y), p);
            totalChange += function.Apply(p, map[coords]);
        }
        foreach(var p in area.GetPoints())
        {
            var coords = area.ToAbsolute(new Point(X, Y), p);
            map[coords] = map[coords] * (1 - averagingSpeed) + totalChange * averagingSpeed;
        }
        //map.SetHeight(X, Y, totalChange);
    }
}

class AreaChangerAgent : Agent
{
    private Area area;
    private Function function;

    public AreaChangerAgent(Area area, Function function)
    {
        this.area = area;
        this.function = function;
    }

    public override void StepLogic(TerrainMap map)
    {
        foreach (var p in area.GetPoints())
        {
            var coords = area.ToAbsolute(new Point(X, Y), p);
            map[coords] += function.Apply(p, map[coords]);
        }
    }
}

abstract class Brush
{
    public abstract void Stroke(TerrainMap map, int x, int y);
}

class AreaChangerBrush : Brush
{
    private Area area;
    private Function function;

    public AreaChangerBrush(Area area, Function function)
    {
        this.area = area;
        this.function = function;
    }
    public override void Stroke(TerrainMap map, int x, int y)
    {
        foreach (var p in area.GetPoints())
        {
            var coords = area.ToAbsolute(new Point(x, y), p);
            map[coords] += function.Apply(p, map[coords]);
        }
    }
}

class AveragerBrush : Brush
{
    Area area;
    Function function;
    float averagingSpeed;

    public AveragerBrush(Area area, Function function, float averagingSpeed)
    {
        this.area = area;
        this.function = function;
        this.averagingSpeed = averagingSpeed;
    }

    public override void Stroke(TerrainMap map, int x, int y)
    {
        float totalChange = 0f;
        foreach (var p in area.GetPoints())
        {
            var coords = area.ToAbsolute(new Point(x, y), p);
            totalChange += function.Apply(p, map[coords]);
        }
        foreach (var p in area.GetPoints())
        {
            var coords = area.ToAbsolute(new Point(x, y), p);
            map[coords] = map[coords] * (1 - averagingSpeed) + totalChange * averagingSpeed;
        }
    }
}
