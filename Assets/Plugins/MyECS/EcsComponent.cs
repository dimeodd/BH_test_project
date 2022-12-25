using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace MyEcs
{
    public static class EcsComponentType<T> where T : struct
    {
        // ReSharper disable StaticMemberInGenericType
        public static readonly int TypeIndex;
        public static readonly Type Type;

        static EcsComponentType()
        {
            TypeIndex = Interlocked.Increment(ref EcsComponentPool.ComponentTypesCount);
            Type = typeof(T);
        }
    }
    public sealed class EcsComponentPool
    {
        /// <summary>
        /// Global component type counter.
        /// First component will be "1" for correct filters updating (add component on positive and remove on negative).
        /// </summary>
        internal static int ComponentTypesCount;
    }

    public class ComponentPool<T> : IComponentPool where T : struct
    {
#if DEBUG
        public MyList<int> DBG_GetFreeData => freeData;

        public int GetChanges() => changes;
        public int GetCount() => Count;
#endif


        EcsWorld _world;
        T[] data;
        public int[] entID;

        public int Count;
        public int changes;
        MyList<int> freeData;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int[] GetEntID() => entID;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentPool(int size, EcsWorld world)
        {
            data = new T[size];
            entID = new int[size];
            freeData = new MyList<int>(EcsWorld.PoolSize);
            _world = world;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index >= Count) throw new Exception(string.Format(
                        "out of pool range index:{0} Count:{1}",
                        index,
                        Count)
                    );
#if DEBUG
                if (freeData.Contain(index))
                    throw new Exception(string.Format(
                            "component is free index:{0}",
                            index)
                        );
#endif
                return ref data[index];
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int New(int ownerID)
        {
            changes++;
            if (changes == Int32.MaxValue) changes = 0;

            int id;
            if (freeData.Count > 0)
            {
                id = freeData._data[freeData.Count - 1];
                freeData.DeleteReplaced(freeData.Count - 1);
            }
            else
            {
                id = Count++;
                if (data.Length <= Count)
                {
                    Array.Resize(ref data, data.Length << 1);
                    Array.Resize(ref entID, entID.Length << 1);
                }
            }
            entID[id] = ownerID;
            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Recycle(int compId)
        {
            changes++;
            entID[compId] = 0;
            data[compId] = default(T);
            freeData.Add(compId);
        }
    }

    public interface IComponentPool
    {
#if DEBUG
        MyList<int> DBG_GetFreeData { get; }
        int GetChanges();
        int GetCount();
#endif

        int[] GetEntID();
        int New(int id);
        void Recycle(int id);
    }
}