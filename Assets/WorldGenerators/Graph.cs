using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Graph<VertexT> : IGraph<VertexT, Edge<VertexT>> where VertexT : class
{
    public List<VertexT> Vertices { get; }
    public List<Edge<VertexT>> Edges { get; }

    public Graph()
    {
        Vertices = new List<VertexT>();
        Edges = new List<Edge<VertexT>>();
    }

    public Graph(List<VertexT> vertices, List<Edge<VertexT>> edges)
    {
        Vertices = vertices;
        Edges = edges;
    }

    public void AddArea(VertexT area)
    {
        Vertices.Add(area);
    }

    public void Connect(VertexT from, VertexT to)
    {
        Edges.Add(new Edge<VertexT>(from, to));
    }

    public bool AreConnected(VertexT from, VertexT to)
    {
        return Edges.Any(edge => edge.Connects(from, to));
    }

    public IEnumerable<VertexT> Neighbors(VertexT vert)
    {
        return Edges.Where(edge => edge.Contains(vert)).Select(edge => edge.Other(vert));
    }

    public IEnumerable<Edge<VertexT>> EdgesFrom(VertexT vert)
    {
        return Edges.Where(edge => edge.Contains(vert));
    }
}