using System;
using System.Runtime.CompilerServices;

namespace MyEcs
{

    public struct Entity
    {
        internal readonly int ID;
        internal readonly int gen;
        internal readonly EcsWorld world;

        public static readonly Entity NULL = new Entity();

        public Entity(int ID, int gen, EcsWorld world)
        {
            this.ID = ID;
            this.gen = gen;
            this.world = world;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Entity a, Entity b)
        {
            return a.ID == b.ID & a.gen == b.gen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            //TODO переделать так, чтобы для ID от 1 до 10000 и любом gen не было коллюзий
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = (ID * 397) ^ gen.GetHashCode();
                hashCode = (hashCode * 397) ^ (world != null ? world.GetHashCode() : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }
    }

    public static class EntityExtention
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(this Entity ent) where T : struct
        {
#if DEBUG
            if (ent.IsDestroyed()) throw new Exception("Try 'Get' destroyed entity ID:" + ent.ID);
#endif
            ref var entData = ref ent.world.GetEntityData(ent);

            var pool = ent.world.GetPool<T>();
            if (!ent.Contain<T>(out var id))
            {
                id = pool.New(ent.ID);
                entData._id.Add(id);
                entData._types.Add(EcsComponentType<T>.TypeIndex);
            }
            return ref pool[id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contain<T>(this Entity ent) where T : struct
        {
#if DEBUG
            if (ent.IsDestroyed()) throw new Exception("Try 'Contain' destroyed entity ID:" + ent.ID);
#endif
            var typeID = EcsComponentType<T>.TypeIndex;
            ref var entData = ref ent.world.GetEntityData(ent);

            for (int i = 0, iMax = entData._types.Count; i < iMax; i++)
            {
                if (entData._types._data[i] == typeID)
                {
                    return true;
                }
            }

            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetID(this Entity ent)
        {
            return ent.ID;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDestroyed(this Entity ent)
        {
            return ent.ID == 0 || ent.gen != ent.GetRealGen();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetGen(this Entity ent)
        {
            return ent.gen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRealGen(this Entity ent)
        {
            return ent.world.GetEntityData(ent).Gen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EcsWorld GetWorld(this Entity ent)
        {
            return ent.world;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Contain<T>(this Entity ent, out int id) where T : struct
        {
            var typeID = EcsComponentType<T>.TypeIndex;
            ref var entData = ref ent.world.GetEntityData(ent);

            id = -1;
            for (int i = 0, iMax = entData._types.Count; i < iMax; i++)
            {
                if (entData._types._data[i] == typeID)
                {
                    id = entData._id._data[i];
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Del<T>(this Entity ent) where T : struct
        {
#if DEBUG
            if (ent.IsDestroyed()) throw new Exception("Try 'Del' destroyed entity ID:" + ent.ID);
#endif
            var world = ent.world;
            var typeID = EcsComponentType<T>.TypeIndex;
            ref var entData = ref world.GetEntityData(ent);

            for (int i = 0, iMax = entData._types.Count; i < iMax; i++)
            {
                if (entData._types._data[i] == typeID)
                {
                    world.pool[typeID].Recycle(entData._id._data[i]);

                    entData._types.DeleteReplaced(i);
                    entData._id.DeleteReplaced(i);

                    break;
                }
            }

            if (entData._types.Count == 0)
            {
                ent.Destroy();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Entity ent)
        {
#if DEBUG
            if (ent.IsDestroyed()) throw new Exception("Try 'Destroy' destroyed entity ID:" + ent.ID);
#endif
            var world = ent.world;
            ref var entData = ref world.GetEntityData(ent);

            for (int i = 0, iMax = entData._types.Count; i < iMax; i++)
            {
                var typeID = entData._types._data[i];
                world.pool[typeID].Recycle(entData._id._data[i]);
            }
            entData._id = new MyList<int>(EcsWorld.PoolSize);
            entData._types = new MyList<int>(EcsWorld.PoolSize);

            world.Recycle(ent);
        }
    }

}
