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
}

public delegate bool EdgeRelation<VertexT>(VertexT u, VertexT v) where VertexT : class;

public class ImplicitGraph<VertexT, EdgeT> : IGraph<VertexT, EdgeT> where VertexT : class where EdgeT : Edge<VertexT>
{
    EdgeRelation<VertexT> edgeRel;

    public ImplicitGraph(EdgeRelation<VertexT> edgeRel)
    {
        this.edgeRel = edgeRel;
    }

    public bool AreConnected(VertexT from, VertexT to)
    {
        return edgeRel(from, to);
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

}