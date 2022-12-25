using System;
using System.Runtime.CompilerServices;

namespace MyEcs
{

    public class MyList<T>
    {
        public int Count;
        public T[] _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MyList(in int size)
        {
            _data = new T[size];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Add(T item)
        {
            if (Count + 1 >= _data.Length)
                Array.Resize(ref _data, _data.Length << 1);

            _data[Count] = item;
            return Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Count = 0;
        }

        /// <summary>
        /// вызывает аллокацию.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            T[] output = new T[Count];
            Array.Copy(_data, 0, output, 0, Count);
            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyToArray(ref T[] outputArr)
        {
            while (outputArr.Length < Count)
            {
                Array.Resize(ref outputArr, outputArr.Length << 1);
            }
            Array.Copy(_data, 0, outputArr, 0, Count);
        }

        /// <summary>
        /// Удаляет указанный ID и перемещает на его место элемент из конца листа
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteReplaced(int id)
        {
            _data[id] = _data[Count - 1];
            Count--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contain(in T item, out int id)
        {
            id = -1;
            for (int i = 0; i < Count; i++)
            {
                if (_data[i].GetHashCode() == item.GetHashCode())
                {
                    id = i;
                    return true;
                }
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contain(in T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_data[i].GetHashCode() == item.GetHashCode())
                {
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            return string.Format("{0} ({1}/{2})",
            typeof(T).Name,
            Count,
            _data.Length);
        }
#endif

    }
}
