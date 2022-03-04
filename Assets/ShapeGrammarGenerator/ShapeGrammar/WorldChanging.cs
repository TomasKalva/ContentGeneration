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
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesNearXZ(addingState.Current).Where(m => m.y == 0), addingState.Added.LevelElements);
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()) : null;
                return addingState.TryPush(movedElement);
            };
        }

        public ChangeWorld AddRemoveOverlap(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesToPartlyIntersectXZ(addingState.Current.Where(le => le.AreaType != AreaType.Path)).Where(m => m.y == 0), addingState.Added.LevelElements.Others(addingState.Current));
                var movedElement = possibleMoves.Any() ? element.MoveBy(possibleMoves.GetRandom()).Minus(addingState.Current) : null;
                return addingState.TryPush(movedElement);
            };
        }

        public ChangeWorld PathTo(Func<LevelElement> elementF)
        {
            return (addingState) =>
            {
                var element = elementF();
                var possibleMoves = element.Moves(element.MovesInDistanceXZ(addingState.Current, 5).Where(m => m.y == 0), addingState.Added.LevelElements);
                if (!possibleMoves.Any())
                    return addingState;

                var area = element.MoveBy(possibleMoves.GetRandom());
                var start = addingState.Current.WhereGeom(le => le.AreaType != AreaType.Path);
                var pathCG = ldk.paths.PathH(start, area, 2, addingState.Added).CubeGroup();
                var path = pathCG.ExtrudeVer(Vector3Int.up, 2).Merge(pathCG).LevelElement(AreaType.Platform);
                var newElement = new LevelGroupElement(addingState.Current.Grid, AreaType.None, path, area);
                return addingState.TryPush(newElement);
            };
        }

        public ChangeWorld SubdivideRoom()
        {
            return (addingState) =>
            {
                var room = addingState.Added.Nonterminals(AreaType.Room)
                    .Where(room => room.CubeGroup().Extents().AtLeast(new Vector3Int(3, 2, 3)))
                    .FirstOrDefault();
                if (room == null)
                    return addingState;

                var changedAdded = addingState.Added.ReplaceLeafsGrp(room, _ => ldk.tr.SubdivideRoom(room, ExtensionMethods.HorizontalDirections().GetRandom(), 0.3f));
                return addingState.ChangeAdded(changedAdded);
            };
        }

        public ChangeWorld SplitToFloors()
        {
            return (addingState) =>
            {
                var room = addingState.Added.Nonterminals(AreaType.Room)
                    .Where(room => room.CubeGroup().Extents().AtLeast(new Vector3Int(5, 5, 5)))
                    .FirstOrDefault();
                if (room == null)
                    return addingState;

                var changedAdded = addingState.Added.ReplaceLeafsGrp(room, _ => ldk.tr.FloorHouse(room, ldk.tr.BrokenFloor, 3, 6, 9));
                return addingState.ChangeAdded(changedAdded);
            };
        }


        public ChangeWorld ConnectTwoUnconnected(Graph<LevelGeometryElement> connectednessGraph, LevelGroupElement levelGeometry)
        {
            // find two areas with floor next to each other
            // calculation is done eagerly and in advance, so it doesn't react on changes of geometry
            var elementsWithFloor = levelGeometry.Leafs().Where(le => AreaType.CanBeConnectedByStairs(le.AreaType) && le.CubeGroup().WithFloor().Cubes.Any()).ToList();
            var closeElementsWithFloor = elementsWithFloor
                .Select2Distinct((el1, el2) => new { el1, el2 })
                .Where(pair => pair.el1.CubeGroup().ExtrudeAll().Intersects(pair.el2.CubeGroup())).ToList();

            return (addingState) =>
            {
                var closePair = closeElementsWithFloor.Where(pair => !connectednessGraph.PathExists(pair.el1, pair.el2)).GetRandom();

                if (closePair == null)
                    return addingState;

                var searchSpace = new CubeGroup(ldk.grid, closePair.el1.CubeGroup().Merge(closePair.el2.CubeGroup()).Cubes);
                Neighbors<PathNode> neighborNodes = PathNode.BoundedBy(PathNode.StairsNeighbors(), searchSpace);
                var newPath = ldk.paths.ConnectByPath(closePair.el1.CubeGroup().WithFloor(), closePair.el2.CubeGroup().WithFloor(), neighborNodes);

                if (newPath == null)
                    return addingState;

                connectednessGraph.Connect(closePair.el1, closePair.el2);
                return addingState.TryPushIntersecting(newPath.LevelElement(AreaType.Path));
            };
        }
    }
}
