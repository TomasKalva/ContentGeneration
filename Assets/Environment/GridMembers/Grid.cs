using OurFramework.Environment.StylingAreas;
using OurFramework.Gameplay.State;
using OurFramework.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace OurFramework.Environment.GridMembers
{
    /// <summary>
    /// Grid made of cubes. Stores all information about geometry.
    /// </summary>
    public class Grid<T> : IEnumerable<T>
    {
        Vector3Int sizes;

        Dictionary<Vector3Int, T[,,]> chunks;

        public IEnumerable<T[,,]> Chunks() => chunks.Values;

        public int Width => sizes.x;
        public int Height => sizes.y;
        public int Depth => sizes.z;

        public bool ValidCoords(Vector3Int coords) => chunks.ContainsKey(GetChunkCoords(coords));

        public Vector3Int Sizes => sizes;

        public delegate T ItemConstructor(Grid<T> grid, Vector3Int position);
        ItemConstructor Constr { get; }

        public T this[int x, int y, int z]
        {
            get => GetItem(new Vector3Int(x, y, z));
            set => SetItem(new Vector3Int(x, y, z), value);
        }

        public T this[Vector3Int coords]
        {
            get => GetItem(coords);
            set => SetItem(coords, value);
        }

        Vector3Int TryCreateChunk(Vector3Int coords)
        {
            var chunkCoords = GetChunkCoords(coords);
            if (!ValidCoords(coords))
            {
                CreateChunk(chunkCoords);
            }
            return chunkCoords;
        }

        T GetItem(Vector3Int coords)
        {
            var chunkCoords = TryCreateChunk(coords);
            var localCoords = coords.Mod(sizes);
            var cube = chunks[chunkCoords][localCoords.x, localCoords.y, localCoords.z];
            return cube;
        }

        void SetItem(Vector3Int coords, T item)
        {
            var chunkCoords = TryCreateChunk(coords);
            var localCoords = coords.Mod(sizes);
            chunks[chunkCoords][localCoords.x, localCoords.y, localCoords.z] = item;
        }

        Vector3Int GetChunkCoords(Vector3Int coords) => coords.Div(sizes);

        void CreateChunk(Vector3Int chunkCoords)
        {
            var chunk = new T[sizes.x, sizes.y, sizes.z];
            var lbb = chunkCoords * sizes;
            foreach (var c in new Box3Int(Vector3Int.zero, sizes))
            {
                chunk[c.x, c.y, c.z] = Constr(this, lbb + c);
            }
            chunks.Add(chunkCoords, chunk);
        }


        public Grid(Vector3Int sizes, ItemConstructor constructor)
        {
            this.sizes = sizes;
            chunks = new Dictionary<Vector3Int, T[,,]>();
            Constr = constructor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var chunk in chunks.Values)
            {
                foreach (var item in chunk)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            foreach(var kvp in chunks)
            {
                var lbb = kvp.Key * sizes;
                var chunk = kvp.Value;
                foreach (var c in new Box3Int(Vector3Int.zero, sizes))
                {
                    chunk[c.x, c.y, c.z] = Constr(this, lbb + c);
                }
            }
        }
    }

    /// <summary>
    /// Used for placing the grid geometry.
    /// </summary>
    public interface IGridGeometryOwner
    {
        void AddArchitectureElement(Transform el);
        WorldGeometry WorldGeometry { get; }

        Transform ArchitectureParent { get; }

        void AddInteractivePersistentObject(InteractiveObjectState interactivePersistentObject);
    }

    /// <summary>
    /// Represents size and position of the world geometry.
    /// </summary>
    public class WorldGeometry
    {
        public Transform WorldParent { get; }
        public Vector3 ParentPosition { get; }
        public float WorldScale { get; }

        public WorldGeometry(Transform worldParent, float worldScale)
        {
            this.WorldParent = worldParent;
            this.WorldScale = worldScale;
            ParentPosition = worldParent.position;
        }

        public Vector3 GridToWorld(Vector3 gridPos)
        {
            return ParentPosition + WorldScale * gridPos;
        }

        public Vector3 WorldToGrid(Vector3 worldPos)
        {
            return (worldPos - ParentPosition) / WorldScale;
        }
    }
}