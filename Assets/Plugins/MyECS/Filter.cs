using System;
using System.Runtime.CompilerServices;

namespace MyEcs
{
    public abstract class Filter
    {
        public const int LINKS_SIZE = 8;


        internal EcsWorld _world;
        internal MyList<int> filter = new MyList<int>(8);
        internal MyList<int> tempFilter2 = new MyList<int>(8);
        public int Count { get; internal set; }
        protected Object key = new Object();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity GetEntity(in int i)
        {
            var id = filter._data[i];
            return new Entity(id, _world.entityData[id].Gen, _world);
        }

        public void Init(EcsWorld world)
        {
            _world = world;
            Init();
        }
        internal virtual void Init()
        {
            GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Enumerator GetEnumerator();

        public struct Enumerator
        {
            int curr;
            int count;
            public Enumerator(Filter filter)
            {
                count = filter.Count;
                curr = -1;
            }
            public int Current => curr;
            public bool MoveNext()
            {
                return ++curr < count;
            }
            public void Reset()
            {
                curr = -1;
            }
        }

        //TODO добавить теги на NullChecks ArrayBoundsChecks
        /// <summary>
        /// Возвращает entityIDs с этим компонентом
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SelectEntityWith_1<T>() where T : struct
        {
            var typeID = EcsComponentType<T>.TypeIndex;
            var entComponents = _world.pool[typeID] as ComponentPool<T>;
            var filter_entID = entComponents.entID;
            var filterCount = entComponents.Count;
            filter.Count = 0;

            for (int i = 0, iMax = filterCount; i < iMax; i++)
            {
                if (filter_entID[i] != 0)
                    filter.Add(filter_entID[i]);
            }
        }
        /// <summary>
        /// Возвращает entityIDs с этим компонентом
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SelectEntityWith_2<T>() where T : struct
        {
            var typeID = EcsComponentType<T>.TypeIndex;
            var entDataArr = _world.entityData;

            int i = filter.Count;
            bool skip;
            while (--i >= 0)
            {
                skip = false;
                ref var entData = ref entDataArr[filter._data[i]];

                for (int x = 0, xMax = entData._types.Count; x < xMax; x++)
                {
                    if (entData._types._data[x] == typeID)
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip) continue;
                filter.DeleteReplaced(i);
            }
        }


        /// <summary>
        /// Возвращает entityIDs с этим компонентом
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ExcludeEntityWith<T>() where T : struct
        {
            var typeID = EcsComponentType<T>.TypeIndex;
            var entDataArr = _world.entityData;

            int i = filter.Count;
            while (--i >= 0)
            {
                ref var entData = ref entDataArr[filter._data[i]];

                for (int x = 0, xMax = entData._types.Count; x < xMax; x++)
                {
                    if (entData._types._data[x] == typeID)
                    {
                        filter.DeleteReplaced(i);
                        break;
                    }
                }
            }
        }



        /// <summary>
        /// Возвращает ID данного компонента из списка entityID
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MyList<int> GetDataIdFromEntitys<T>(in MyList<int> filter) where T : struct
        {
            var typeID = EcsComponentType<T>.TypeIndex;
            var entArr = _world.entityData;
            tempFilter2.Count = 0;

            for (int i = 0, iMax = filter.Count; i < iMax; i++)
            {
                ref var entData = ref entArr[filter._data[i]];

                for (int x = 0, xMax = entData._types.Count; x < xMax; x++)
                {
                    if (entData._types._data[x] == typeID)
                    {
                        tempFilter2.Add(entData._id._data[x]);
                        break;
                    }
                }
            }

            return tempFilter2;
        }
    }

    public class Filter<T1> : Filter
        where T1 : struct
    {
        internal int changes_1;
        internal ComponentPool<T1> arr_1; // данные
        internal int[] id_1 = new int[LINKS_SIZE]; // cписок ссылок на данные

        internal override void Init()
        {
            arr_1 = _world.GetPool<T1>();
            base.Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T1 Get1(int i) => ref arr_1[id_1[i]];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Enumerator GetEnumerator()
        {
            lock (key)
            {
                var wChanges = arr_1.changes;

                if (changes_1 != wChanges)
                {
                    changes_1 = wChanges;

                    SelectEntityWith_1<T1>();

                    GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1); //FIXME сделать постепенное освобождение памяти, если она не использкется >10(?) вызовов
                    Count = filter.Count;
                }
            }
            return new Enumerator(this);
        }
        public class Exclude<E1> : Filter<T1>
            where E1 : struct
        {
            internal int changes_e1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Enumerator GetEnumerator()
            {
                lock (key)
                {
                    var chI1 = arr_1.changes;
                    var chE1 = _world.GetChangesVer<E1>();

                    if (changes_1 != chI1
                     || changes_e1 != chE1)
                    {
                        changes_1 = chI1;
                        changes_e1 = chE1;

                        SelectEntityWith_1<T1>();
                        ExcludeEntityWith<E1>();

                        GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                        Count = filter.Count;
                    }
                }
                return new Enumerator(this);
            }
        }

        public class Exclude<E1, E2> : Exclude<E1>
            where E1 : struct
            where E2 : struct
        {
            internal int changes_e2;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Enumerator GetEnumerator()
            {
                lock (key)
                {
                    var chI1 = arr_1.changes;
                    var chE1 = _world.GetChangesVer<E1>();
                    var chE2 = _world.GetChangesVer<E2>();

                    if (changes_1 != chI1
                     || changes_e1 != chE1
                     || changes_e2 != chE2)
                    {
                        changes_1 = chI1;
                        changes_e1 = chE1;
                        changes_e2 = chE2;

                        SelectEntityWith_1<T1>();
                        ExcludeEntityWith<E1>();
                        ExcludeEntityWith<E2>();

                        GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                        Count = filter.Count;
                    }
                }
                return new Enumerator(this);
            }
        }
    }


    public class Filter<T1, T2> : Filter<T1>
        where T1 : struct
        where T2 : struct
    {
        internal int changes_2;
        internal ComponentPool<T2> arr_2; // данные
        internal int[] id_2 = new int[LINKS_SIZE];  // cписок ссылок на данные

        internal override void Init()
        {
            arr_2 = _world.GetPool<T2>();
            base.Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T2 Get2(int i) => ref arr_2[id_2[i]];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Enumerator GetEnumerator()
        {
            lock (key)
            {
                var chI1 = arr_1.changes;
                var chI2 = arr_2.changes;

                if (changes_1 != chI1
                 || changes_2 != chI2)
                {
                    changes_1 = chI1;
                    changes_2 = chI2;

                    SelectEntityWith_1<T1>();
                    SelectEntityWith_2<T2>();

                    GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                    GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                    Count = filter.Count;
                }
            }
            return new Enumerator(this);
        }
        public new class Exclude<E1> : Filter<T1, T2>
            where E1 : struct
        {
            internal int changes_e1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Enumerator GetEnumerator()
            {
                lock (key)
                {
                    var chI1 = arr_1.changes;
                    var chI2 = arr_2.changes;
                    var chE1 = _world.GetChangesVer<E1>();

                    if (changes_1 != chI1
                     || changes_2 != chI2
                     || changes_e1 != chE1)
                    {
                        changes_1 = chI1;
                        changes_2 = chI2;
                        changes_e1 = chE1;

                        SelectEntityWith_1<T1>();
                        SelectEntityWith_2<T2>();
                        ExcludeEntityWith<E1>();

                        GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                        GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                        Count = filter.Count;
                    }
                }
                return new Enumerator(this);
            }
        }
        public new class Exclude<E1, E2> : Exclude<E1>
           where E1 : struct
           where E2 : struct
        {
            internal int changes_e2;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Enumerator GetEnumerator()
            {
                lock (key)
                {
                    var chI1 = arr_1.changes;
                    var chI2 = arr_2.changes;
                    var chE1 = _world.GetChangesVer<E1>();
                    var chE2 = _world.GetChangesVer<E2>();

                    if (changes_1 != chI1
                     || changes_2 != chI2
                     || changes_e1 != chE1
                     || changes_e2 != chE2)
                    {
                        changes_1 = chI1;
                        changes_2 = chI2;
                        changes_e1 = chE1;
                        changes_e2 = chE2;

                        SelectEntityWith_1<T1>();
                        SelectEntityWith_2<T2>();
                        ExcludeEntityWith<E1>();
                        ExcludeEntityWith<E2>();

                        GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                        GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                        Count = filter.Count;
                    }
                }
                return new Enumerator(this);
            }
        }
    }

    public class Filter<T1, T2, T3> : Filter<T1, T2>
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        internal int changes_3;
        internal ComponentPool<T3> arr_3; // данные
        internal int[] id_3 = new int[LINKS_SIZE];  // cписок ссылок на данные


        internal override void Init()
        {
            arr_3 = _world.GetPool<T3>();
            base.Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T3 Get3(int i) => ref arr_3[id_3[i]];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Enumerator GetEnumerator()
        {
            lock (key)
            {
                var chI1 = arr_1.changes;
                var chI2 = arr_2.changes;
                var chI3 = arr_3.changes;

                if (changes_1 != chI1
                 || changes_2 != chI2
                 || changes_3 != chI3)
                {
                    changes_1 = chI1;
                    changes_2 = chI2;
                    changes_3 = chI3;

                    SelectEntityWith_1<T1>();
                    SelectEntityWith_2<T2>();
                    SelectEntityWith_2<T3>();

                    GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                    GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                    GetDataIdFromEntitys<T3>(filter).CopyToArray(ref id_3);
                    Count = filter.Count;
                }
            }
            return new Enumerator(this);
        }
        public new class Exclude<E1> : Filter<T1, T2, T3>
            where E1 : struct
        {
            internal int changes_e1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Enumerator GetEnumerator()
            {
                lock (key)
                {
                    var chI1 = arr_1.changes;
                    var chI2 = arr_2.changes;
                    var chI3 = arr_3.changes;
                    var chE1 = _world.GetChangesVer<E1>();

                    if (changes_1 != chI1
                     || changes_2 != chI2
                     || changes_3 != chI3
                     || changes_e1 != chE1)
                    {
                        changes_1 = chI1;
                        changes_2 = chI2;
                        changes_3 = chI3;
                        changes_e1 = chE1;

                        SelectEntityWith_1<T1>();
                        SelectEntityWith_2<T2>();
                        SelectEntityWith_2<T3>();
                        ExcludeEntityWith<E1>();

                        GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                        GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                        GetDataIdFromEntitys<T3>(filter).CopyToArray(ref id_3);
                        Count = filter.Count;
                    }
                }
                return new Enumerator(this);
            }
        }
    }


    public class Filter<T1, T2, T3, T4> : Filter<T1, T2, T3>
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        internal int changes_4;
        internal ComponentPool<T4> arr_4; // данные
        internal int[] id_4 = new int[LINKS_SIZE];  // cписок ссылок на данные

        internal override void Init()
        {
            arr_4 = _world.GetPool<T4>();
            base.Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T4 Get4(int i) => ref arr_4[id_4[i]];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Enumerator GetEnumerator()
        {
            lock (key)
            {
                var chI1 = arr_1.changes;
                var chI2 = arr_2.changes;
                var chI3 = arr_3.changes;
                var chI4 = arr_4.changes;

                if (changes_1 != chI1
                 || changes_2 != chI2
                 || changes_3 != chI3
                 || changes_4 != chI4)
                {
                    changes_1 = chI1;
                    changes_2 = chI2;
                    changes_3 = chI3;
                    changes_4 = chI4;

                    SelectEntityWith_1<T1>();
                    SelectEntityWith_2<T2>();
                    SelectEntityWith_2<T3>();
                    SelectEntityWith_2<T4>();

                    GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                    GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                    GetDataIdFromEntitys<T3>(filter).CopyToArray(ref id_3);
                    GetDataIdFromEntitys<T4>(filter).CopyToArray(ref id_4);
                    Count = filter.Count;
                }
            }
            return new Enumerator(this);
        }
        public new class Exclude<E1> : Filter<T1, T2, T3, T4>
            where E1 : struct
        {
            internal int changes_e1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Enumerator GetEnumerator()
            {
                lock (key)
                {
                    var chI1 = arr_1.changes;
                    var chI2 = arr_2.changes;
                    var chI3 = arr_3.changes;
                    var chI4 = arr_4.changes;
                    var chE1 = _world.GetChangesVer<E1>();

                    if (changes_1 != chI1
                     || changes_2 != chI2
                     || changes_3 != chI3
                     || changes_4 != chI4
                     || changes_e1 != chE1)
                    {
                        changes_1 = chI1;
                        changes_2 = chI2;
                        changes_3 = chI3;
                        changes_4 = chI4;
                        changes_e1 = chE1;

                        SelectEntityWith_1<T1>();
                        SelectEntityWith_2<T2>();
                        SelectEntityWith_2<T3>();
                        SelectEntityWith_2<T4>();
                        ExcludeEntityWith<E1>();

                        GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                        GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                        GetDataIdFromEntitys<T3>(filter).CopyToArray(ref id_3);
                        GetDataIdFromEntitys<T4>(filter).CopyToArray(ref id_4);
                        Count = filter.Count;
                    }
                }
                return new Enumerator(this);
            }
        }
    }

    public class Filter<T1, T2, T3, T4, T5> : Filter<T1, T2, T3, T4>
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
    {
        internal int changes_5;
        internal ComponentPool<T5> arr_5; // данные
        internal int[] id_5 = new int[LINKS_SIZE];  // cписок ссылок на данные

        internal override void Init()
        {
            arr_5 = _world.GetPool<T5>();
            base.Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T5 Get5(int i) => ref arr_5[id_5[i]];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Enumerator GetEnumerator()
        {
            lock (key)
            {
                var chI1 = arr_1.changes;
                var chI2 = arr_2.changes;
                var chI3 = arr_3.changes;
                var chI4 = arr_4.changes;
                var chI5 = arr_5.changes;

                if (changes_1 != chI1
                 || changes_2 != chI2
                 || changes_3 != chI3
                 || changes_4 != chI4
                 || changes_5 != chI5)
                {
                    changes_1 = chI1;
                    changes_2 = chI2;
                    changes_3 = chI3;
                    changes_4 = chI4;
                    changes_5 = chI5;

                    SelectEntityWith_1<T1>();
                    SelectEntityWith_2<T2>();
                    SelectEntityWith_2<T3>();
                    SelectEntityWith_2<T4>();
                    SelectEntityWith_2<T5>();

                    GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                    GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                    GetDataIdFromEntitys<T3>(filter).CopyToArray(ref id_3);
                    GetDataIdFromEntitys<T4>(filter).CopyToArray(ref id_4);
                    GetDataIdFromEntitys<T5>(filter).CopyToArray(ref id_5);
                    Count = filter.Count;
                }
            }
            return new Enumerator(this);
        }
        public new class Exclude<E1> : Filter<T1, T2, T3, T4, T5>
            where E1 : struct
        {
            internal int changes_e1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Enumerator GetEnumerator()
            {
                lock (key)
                {
                    var chI1 = arr_1.changes;
                    var chI2 = arr_2.changes;
                    var chI3 = arr_3.changes;
                    var chI4 = arr_4.changes;
                    var chI5 = arr_5.changes;
                    var chE1 = _world.GetChangesVer<E1>();

                    if (changes_1 != chI1
                     || changes_2 != chI2
                     || changes_3 != chI3
                     || changes_4 != chI4
                     || changes_5 != chI5
                     || changes_e1 != chE1)
                    {
                        changes_1 = chI1;
                        changes_2 = chI2;
                        changes_3 = chI3;
                        changes_4 = chI4;
                        changes_5 = chI5;
                        changes_e1 = chE1;

                        SelectEntityWith_1<T1>();
                        SelectEntityWith_2<T2>();
                        SelectEntityWith_2<T3>();
                        SelectEntityWith_2<T4>();
                        SelectEntityWith_2<T5>();
                        ExcludeEntityWith<E1>();

                        GetDataIdFromEntitys<T1>(filter).CopyToArray(ref id_1);
                        GetDataIdFromEntitys<T2>(filter).CopyToArray(ref id_2);
                        GetDataIdFromEntitys<T3>(filter).CopyToArray(ref id_3);
                        GetDataIdFromEntitys<T4>(filter).CopyToArray(ref id_4);
                        GetDataIdFromEntitys<T5>(filter).CopyToArray(ref id_5);
                        Count = filter.Count;
                    }
                }
                return new Enumerator(this);
            }
        }
    }
}
