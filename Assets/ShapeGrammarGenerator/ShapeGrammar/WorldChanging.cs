using Assets.ShapeGrammarGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.WorldState;

namespace ShapeGrammar
{
    public class WorldChanging
    {
        public LevelDevelopmentKit ldk;

        public WorldChanging(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }

        public ChangeWorld AddNearXZ(Func<LevelElement> elementF)
        {
            return (worldState) =>
            {
                var element = elementF();
                var possibleMoves = element.DontIntersect(element.MovesNearXZ(worldState.Last).Ms.Where(m => m.y == 0), worldState.Added.LevelElements).Ms;
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()) : null;
                return worldState.TryPush(movedElement);
            };
        }

        public ChangeWorld AddNotIntersecting(Func<LevelElement> elementF)
        {
            return (worldState) =>
            {
                var element = elementF();
                var possibleMoves = element.DontIntersect(element.MovesNearXZ(worldState.Last).Ms.Where(m => m.y == 0), worldState.Added.LevelElements).Ms;
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()) : null;
                return worldState.TryPush(movedElement);
            };
        }

        public ChangeWorld AddRemoveOverlap(Func<LevelElement> elementF)
        {
            return (worldState) =>
            {
                var element = elementF();
                var possibleMoves = element.DontIntersect(element.MovesToPartlyIntersectXZ(worldState.Last.Where(le => le.AreaStyle != AreaStyles.Path())).Ms.Where(m => m.y == 0), worldState.Added.LevelElements.Others(worldState.Last)).Ms;
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()).Minus(worldState.Last) : null;
                return worldState.TryPush(movedElement);
            };
        }

        public ChangeWorld PathTo(Func<LevelElement> elementF)
        {
            return (worldState) =>
            {
                var element = elementF();
                var possibleMoves = element.DontIntersect(element.MovesInDistanceXZ(worldState.Last, 5).Ms.Where(m => m.y == 0), worldState.Added.LevelElements).Ms;
                if (!possibleMoves.Any())
                    return worldState;

                var area = element.MoveBy(possibleMoves.GetRandom());
                var start = worldState.Last.WhereGeom(le => le.AreaStyle != AreaStyles.Path());
                var pathCG = ldk.paths.PathH(start, area, 2, worldState.Added).CG();
                var path = pathCG.ExtrudeVer(Vector3Int.up, 2).Merge(pathCG).LE(AreaStyles.Platform());
                var newElement = new LevelGroupElement(worldState.Last.Grid, AreaStyles.None(), path, area);
                return worldState.TryPush(newElement);
            };
        }

        public ChangeWorld SubdivideRoom()
        {
            return (worldState) =>
            {
                var room = worldState.Added.Nonterminals(AreaStyles.Room())
                    .Where(room => room.CG().Extents().AtLeast(new Vector3Int(3, 2, 3)))
                    .FirstOrDefault();
                if (room == null)
                    return worldState;

                var changedAdded = worldState.Added.ReplaceLeafsGrp(room, _ => ldk.tr.SubdivideRoom(room, ExtensionMethods.HorizontalDirections().GetRandom(), 0.3f));
                return worldState.ChangeAdded(changedAdded);
            };
        }

        public ChangeWorld SplitToFloors()
        {
            return (worldState) =>
            {
                var room = worldState.Added.Nonterminals(AreaStyles.Room())
                    .Where(room => room.CG().Extents().AtLeast(new Vector3Int(5, 5, 5)))
                    .FirstOrDefault();
                if (room == null)
                    return worldState;

                var changedAdded = worldState.Added.ReplaceLeafsGrp(room, _ => ldk.tr.FloorHouse(room, ldk.tr.BrokenFloor, 3, 6, 9));
                return worldState.ChangeAdded(changedAdded);
            };
        }

        /*
        public ChangeWorld ConnectTwoUnconnected(Graph<LevelGeometryElement> connectednessGraph, LevelGroupElement levelGeometry)
        {
            // find two areas with floor next to each other
            // calculation is done eagerly and in advance, so it doesn't react on changes of geometry
            var elementsWithFloor = levelGeometry.Leafs().Where(le => AreaStyle.CanBeConnectedByStairs(le.AreaStyle) && le.CG().BottomLayer().Cubes.Any()).ToList();
            var closeElementsWithFloor = elementsWithFloor
                .Select2Distinct((el1, el2) => new { el1, el2 })
                .Where(pair => pair.el1.CG().ExtrudeAll().Intersects(pair.el2.CG())).ToList();

            return (worldState) =>
            {
                var closePair = closeElementsWithFloor.Where(pair => !connectednessGraph.PathExists(pair.el1, pair.el2)).GetRandom();

                if (closePair == null)
                    return worldState;

                var searchSpace = new CubeGroup(ldk.grid, closePair.el1.CG().Merge(closePair.el2.CG()).Cubes);
                Neighbors<PathNode> neighborNodes = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var newPath = ldk.paths.ConnectByPath(closePair.el1.CG().BottomLayer(), closePair.el2.CG().BottomLayer(), neighborNodes);

                if (newPath == null)
                    return worldState;

                connectednessGraph.Connect(closePair.el1, closePair.el2);
                return worldState.TryPushIntersecting(newPath.LE(AreaStyles.Path()));
            };
        }
        */
    }
}
