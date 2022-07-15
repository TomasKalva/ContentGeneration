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
        private int simulationSpeed;
        List<Agent2> agents;
        List<Spawner> spawners;

        public Agents(int simulationSpeed)
        {
            this.agents = new List<Agent2>();
            this.spawners = new List<Spawner>();
            this.simulationSpeed = simulationSpeed;
        }

        public void AddAgent(Agent2 agent)
        {
            agents.Add(agent);
        }

        public void AddSpawner(Spawner spawner)
        {
            spawners.Add(spawner);
        }

        public IEnumerator Step(TerrainMap map, Mesh mesh, MeshCollider meshCollider)
        {
            while (true)
            {
                for (int i = 0; i < simulationSpeed; i++)
                {
                    if (spawners.Any())
                    {
                        var s = spawners.First();
                        s.Step(this, map);
                        if (s.Finished)
                            spawners.Remove(s);
                    }
                    foreach (var a in agents)
                    {
                        a.Step(map);
                    }
                    agents.RemoveAll(a => a.Finished);
                }
                map.UpdateMesh(mesh, meshCollider);
                yield return new WaitForSeconds(0.01f);
            }
        }

        public IEnumerable WaitUntilAgentsFinish()
        {
            while (agents.Any())
            {
                yield return null;
            }
        }
    }

    abstract class Agent2
    {
        public Vector2 Position { get; protected set; }
        protected int Steps { get; set; }
        private Brush brush;
        public Agent2(Brush brush)
        {
            Steps = int.MaxValue;
            SetPosition(new Vector2(50f, 50f));
            this.brush = brush;
        }
        public Agent2 SetPosition(Vector2 pos)
        {
            Position = pos;
            return this;
        }
        public Agent2 SetSteps(int steps)
        {
            Steps = steps;
            return this;
        }
        public void Step(TerrainMap map)
        {
            StepLogic(map);
            brush.Stroke(map, Position);
            Steps--;
        }
        protected void Move(float dx, float dy, TerrainMap map)
        {
            Position = new Vector2((Position.x + dx + map.Width) % map.Width, (Position.y + dy + map.Height) % map.Height);
        }
        public abstract void StepLogic(TerrainMap map);
        public bool Finished => Steps <= 0;
    }
}

class BasicAgent : Assets.Agent2
{
    public BasicAgent(Brush brush):base(brush)
    {
    }

    public override void StepLogic(TerrainMap map)
    {
        //Move(Random.Range(0, 3) - 1, Random.Range(0, 3) - 1, map);
        var angle = Random.value * 2 * Mathf.PI;
        Move(Mathf.Cos(angle), Mathf.Sin(angle), map);
    }
}

class RepulsedAgent : Assets.Agent2
{
    Vector2 repulsor;

    public RepulsedAgent(Brush brush, Vector2 repulsor) : base(brush)
    {
        this.repulsor = repulsor;
    }

    private float InverseSmoothstep(float y)
    {
        return 0.5f - Mathf.Sin(Mathf.Asin(1.0f - 2.0f * y) / 3.0f);
    }

    public override void StepLogic(TerrainMap map)
    {
        var diff = repulsor - Position;
        var aToRepulsor = Mathf.Atan2(diff.y, diff.x);
        var rand = Random.value;
        var angle = aToRepulsor + 2 * Mathf.PI * InverseSmoothstep(rand);
        Move(Mathf.Cos(angle), Mathf.Sin(angle), map);
    }
}

class ClimbinAgent : Assets.Agent2
{
    public ClimbinAgent(Brush brush) : base(brush)
    {
    }

    public override void StepLogic(TerrainMap map)
    {
        var newPoint = map.GetNeighbors((Point)Position).ArgMax(p => map[p]);
        var d = newPoint - Position;
        Move(d.x, d.y, map);
    }
}

class DescendingAgent : Assets.Agent2
{
    float? height;

    public DescendingAgent(Brush brush) : base(brush)
    {
    }

    public override void StepLogic(TerrainMap map)
    {
        var newPoint = map.GetNeighbors((Point)Position).ArgMin(p => map[p]);
        var newHeight = map[newPoint];
        // end if height can't be changed anymore
        if (height != null && height.Value - newHeight < 0.0001f)
        {
            Steps = 0;
        }
        height = newHeight;
        var d = newPoint - Position;
        Move(d.x, d.y, map);
    }
}