using System.Collections.Generic;
using System.Linq;

namespace OurFramework.Util
{
    /// <summary>
    /// Graph with user defined vertices and edges.
    /// </summary>
    public class Graph<VertexT> : GraphAlgorithms<VertexT, Edge<VertexT>, Graph<VertexT>>, IGraph<VertexT, Edge<VertexT>> where VertexT : class
    {
        public List<VertexT> Vertices { get; }
        public List<Edge<VertexT>> Edges { get; }

        public Graph() : base(null)
        {
            Vertices = new List<VertexT>();
            Edges = new List<Edge<VertexT>>();
            graph = this;
        }

        public Graph(List<VertexT> vertices, List<Edge<VertexT>> edges) : base(null)
        {
            Vertices = vertices;
            Edges = edges;
            graph = this;
        }

        public void AddVertex(VertexT area)
        {
            Vertices.Add(area);
        }

        public void AddEdge(Edge<VertexT> edge)
        {
            Edges.Add(edge);
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
}
