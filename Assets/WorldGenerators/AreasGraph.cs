using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AreasGraph : IGraph<Area, AreasConnection>
{
    public List<Area> Vertices { get; }
    public List<AreasConnection> Edges { get; }

    public AreasGraph()
    {
        Vertices = new List<Area>();
        Edges = new List<AreasConnection>();
    }

    public void AddArea(Area area)
    {
        Vertices.Add(area);
    }

    public void Connect(Area from, Area to)
    {
        Edges.Add(new AreasConnection(from, to));
    }

    public bool AreConnected(Area from, Area to)
    {
        return Edges.Any(edge => edge.Connects(from, to));
    }

    public IEnumerable<Area> Neighbors(Area vert)
    {
        return Edges.Where(edge => edge.Contains(vert)).Select(edge => edge.Other(vert));
    }
}

public struct AreasConnection : Edge<Area>
{
    public Area From { get; }
    public Area To { get; }

    public AreasConnection(Area from, Area to)
    {
        From = from;
        To = to;
    }

    public bool Connects(Area from, Area to)
    {
        return (From == from && To == to) || (From == to && To == from);
    }

    public bool Contains(Area vert)
    {
        return vert == From || vert == To;
    }

    public Area Other(Area vert)
    {
        return vert == From ? To : From;
    }
}

public class NoEdge<VertexT> : Edge<VertexT> where VertexT : class
{
    public VertexT From => throw new NotImplementedException();

    public VertexT To => throw new NotImplementedException();

    public bool Connects(Area from, Area to)
    {
        throw new NotImplementedException();
    }
}

public delegate IEnumerable<VertexT> Neighbors<VertexT>(VertexT v) where VertexT : class;

public class ImplicitGraph<VertexT> : IGraph<VertexT, NoEdge<VertexT>> where VertexT : class
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

    public IEnumerable<VertexT> Neighbors(VertexT vert)
    {
        return neighbors(vert);
    }
}

public interface Edge<VertexT>
{
    public VertexT From { get; }
    public VertexT To { get; }

    bool Connects(Area from, Area to);
}

public interface IGraph<VertexT, EdgeT> where VertexT : class where EdgeT : Edge<VertexT>
{
    /*public IEnumerable<VertexT> Vertices { get; }
    public IEnumerable<EdgeT> Edges { get; }*/
    bool AreConnected(VertexT from, VertexT to);
    IEnumerable<VertexT> Neighbors(VertexT vert);
}

public class GraphAlgorithms<VertexT, EdgeT, GraphT> where VertexT : class where EdgeT : Edge<VertexT> where GraphT : IGraph<VertexT, EdgeT>
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
        var found = new List<VertexT>();

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
}