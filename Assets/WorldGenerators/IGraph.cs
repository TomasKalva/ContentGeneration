﻿using SD.Tools.Algorithmia.PriorityQueues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Edge<VertexT> : IEdge<VertexT> where VertexT : class
{
    public VertexT From { get; }

    public VertexT To { get; }

    public Edge(VertexT from, VertexT to)
    {
        From = from;
        To = to;
    }

    public bool Connects(VertexT from, VertexT to)
    {
        return (from == From && to == To) || (from == To && to == From);
    }

    public bool Contains(VertexT vert)
    {
        return vert == From || vert == To;
    }

    public VertexT Other(VertexT vert)
    {
        return vert == From ? To : From;
    }
}

public delegate IEnumerable<VertexT> Neighbors<VertexT>(VertexT v) where VertexT : class;

public class ImplicitGraph<VertexT> : IGraph<VertexT, Edge<VertexT>> where VertexT : class
{
    Neighbors<VertexT> neighbors;

    public ImplicitGraph(Neighbors<VertexT> neighbors)
    {
        this.neighbors = neighbors;
    }

    public bool AreConnected(VertexT from, VertexT to)
    {
        return neighbors(from).Contains(to);
    }

    public IEnumerable<Edge<VertexT>> EdgesFrom(VertexT vert)
    {
        return neighbors(vert).Select(neighbor => new Edge<VertexT>(vert, neighbor));
    }

    public IEnumerable<VertexT> Neighbors(VertexT vert)
    {
        return neighbors(vert);
    }

    public Graph<VertexT> ToGraph(List<VertexT> vertices)
    {
        var edges = vertices.SelectMany(v => Neighbors(v).Select(neighbor => new Edge<VertexT>(v, neighbor))).ToList();
        return new Graph<VertexT>(vertices, edges);
    }
}

public interface IEdge<VertexT>
{
    public VertexT From { get; }
    public VertexT To { get; }

    bool Connects(VertexT from, VertexT to);

    public bool Contains(VertexT vert);

    public VertexT Other(VertexT vert);
}

public interface IGraph<VertexT, EdgeT> where VertexT : class where EdgeT : IEdge<VertexT>
{
    /*public IEnumerable<VertexT> Vertices { get; }
    public IEnumerable<EdgeT> Edges { get; }*/
    bool AreConnected(VertexT from, VertexT to);
    IEnumerable<VertexT> Neighbors(VertexT vert);
    IEnumerable<EdgeT> EdgesFrom(VertexT vert);
}

public class GraphAlgorithms<VertexT, EdgeT, GraphT> where VertexT : class where EdgeT : IEdge<VertexT> where GraphT : IGraph<VertexT, EdgeT>
{
    IGraph<VertexT, EdgeT> graph;

    public GraphAlgorithms(IGraph<VertexT, EdgeT> graph)
    {
        this.graph = graph;
    }

    /// <summary>
    /// Only for symmetrical graphs!
    /// </summary>
    public IEnumerable<IEnumerable<VertexT>> ConnectedComponentsSymm(IEnumerable<VertexT> rootVertices)
    {
        var components = new List<IEnumerable<VertexT>>();
        var found = new HashSet<VertexT>();

        foreach (var root in rootVertices)
        {
            var fringe = new Stack<VertexT>();
            var current = new List<VertexT>();
            fringe.Push(root);
            if (found.Contains(root))
            {
                continue;
            }

            while (fringe.Any())
            {
                var v = fringe.Pop();
                if (found.Contains(v))
                {
                    continue;
                }

                found.Add(v);
                current.Add(v);
                foreach (var neighbor in graph.Neighbors(v))
                {
                    fringe.Push(neighbor);
                }
            }

            components.Add(current);
        }

        return components;
    }

    public IEnumerable<VertexT> DFS(VertexT start)
    {
        var found = new HashSet<VertexT>();
        var fringe = new Stack<VertexT>();
        fringe.Push(start);
        while (fringe.Any())
        {
            var v = fringe.Pop();
            if (found.Contains(v))
            {
                continue;
            }

            yield return v;
            found.Add(v);
            foreach (var neighbor in graph.Neighbors(v))
            {
                fringe.Push(neighbor);
            }
        }
    }

    public IEnumerable<EdgeT> EdgeDFS(VertexT start)
    {
        var found = new HashSet<EdgeT>();
        var fringe = new Stack<EdgeT>();

        foreach (var edge in graph.EdgesFrom(start))
        {
            fringe.Push(edge);
        }
        while (fringe.Any())
        {
            var edge = fringe.Pop();
            yield return edge;
            if (found.Any(e => e.Connects(edge.From, edge.To)))
            {
                continue;
            }

            found.Add(edge);
            foreach (var neighborEdge in graph.EdgesFrom(edge.To))
            {
                fringe.Push(neighborEdge);
            }
        }
    }

    /// <summary>
    /// Has O(|V|^2) complpexity.
    /// </summary>
    public int Distance(VertexT from, VertexT to)
    {
        var closed = new HashSet<VertexT>();
        var distance = new Dictionary<VertexT, int>();
        var fringe = new HashSet<VertexT>();
        fringe.Add(from);
        distance.Add(from, 0);

        while (fringe.Any())
        {
            var v = fringe.ArgMin(u => distance.Get(u, int.MaxValue));
            fringe.Remove(v);

            if(v == to)
            {
                return distance[v];
            }

            if (closed.Contains(v))
            {
                continue;
            }
            closed.Add(v);

            foreach (var neighbor in graph.Neighbors(v))
            {
                var bestDist = Math.Min(distance.Get(neighbor, int.MaxValue), distance[v] + 1);
                distance.TryAdd(neighbor, bestDist);
                fringe.Add(neighbor);
            }
        }
        return int.MaxValue;
    }

    public bool Path(VertexT from, VertexT to)
    {
        return DFS(from).Contains(to);
    }
}