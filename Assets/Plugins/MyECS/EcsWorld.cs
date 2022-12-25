using System;
using System.Runtime.CompilerServices;

namespace MyEcs
{
    public class EcsWorld
    {

        public void Dispose()
        {
#if UNITY_EDITOR
            _debugWorld = null;
#endif
        }
#if UNITY_EDITOR
        static public EcsWorld _debugWorld = null;



        public EntityData[] DBG_GetEntityData => entityData;
        public int DBG_GetEntityCount => entityCount;
        public MyList<int> DBG_GetFreeEntity => freeEntity;
        public IComponentPool[] DBG_GetPool => pool;
#endif
        //TODO ускорить поиск компонентов, добавив ссылки на сущности по компонентам
        // типа список сущностей содержащий этот компонент
        // а затем использовать его в фильтрах, чтобы не перебирать все сущности


        //FIXME сделать команду, которая будет уменьшать Component-массивы. и вызывать её раз в ~10 цыклов у системы

        public const int PoolSize = 8;

        /// <summary>
        /// энтити начинаются с 1
        /// </summary>
        internal EntityData[] entityData;

        /// <summary>
        /// Нужно прибавлять + 1, если перебираете entityData
        /// </summary>
        internal int entityCount;
        internal MyList<int> freeEntity;

        internal IComponentPool[] pool;


        public EcsWorld()
        {
#if UNITY_EDITOR
            _debugWorld = this;
#endif

            entityData = new EntityData[PoolSize];
            freeEntity = new MyList<int>(PoolSize);
            pool = new IComponentPool[PoolSize]; //это массив интерфейсов
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetChangesVer<T>() where T : struct
        {
            var pool = GetPool<T>();
            return pool.changes;
        }

        public Entity NewEntity()
        {
            Entity ent;
            if (freeEntity.Count > 0)
            {
                var entID = freeEntity._data[freeEntity.Count - 1];
                freeEntity.DeleteReplaced(freeEntity.Count - 1);
                ent = new Entity(entID, entityData[entID].Gen, this);
            }
            else
            {
                var freeID = ++entityCount;
                if (entityData.Length <= freeID)
                {
                    Array.Resize(ref entityData, entityData.Length << 1);
                }

                ref var entData = ref entityData[freeID];
                entData._id = new MyList<int>(8);
                entData._types = new MyList<int>(8);
                entData.Gen = 1;

                ent = new Entity(freeID, entData.Gen, this);
            }
            return ent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Recycle(Entity ent)
        {
            ref var entData = ref entityData[ent.ID];

#if DEBUG
            if (entData._types.Count > 0) throw new Exception("Entity contain data");
            if (entData.Gen != ent.gen) throw new Exception($"Entity Wrong Generation Data:{entData.Gen} Ent:{ent.gen} ID:{ent.ID}");
#endif
            entData.Gen++;
            freeEntity.Add(ent.ID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref EntityData GetEntityData(Entity ent)
        {
            return ref entityData[ent.ID];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ComponentPool<T> GetPool<T>() where T : struct
        {
            var typeID = EcsComponentType<T>.TypeIndex;
            if (pool.Length <= typeID)
            {
                var len = pool.Length << 1;
                while (len <= typeID)
                {
                    len <<= 1;
                }
                Array.Resize(ref pool, len);

            }
            if (pool[typeID] == null)
            {
                pool[typeID] = new ComponentPool<T>(PoolSize, this) as IComponentPool;
            }

            return (ComponentPool<T>)pool[typeID];
        }

    }

    public struct EntityData
    {
        internal int Gen;
        internal MyList<int> _types;
        internal MyList<int> _id;

#if DEBUG
        public int DBG_GetGen => Gen;
        public MyList<int> DBG_GetTypes => _types;
        public MyList<int> DBG_GetId => _id;
#endif
    }
}