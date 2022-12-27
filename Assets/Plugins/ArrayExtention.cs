using System;
using System.Collections;
using System.Collections.Generic;

public static class ArrayExtention<T>
{
    public static List<T> ConvertToList(ref T[] arr)
    {
        var list = new List<T>();

        for (int i = 0, iMax = arr.Length; i < iMax; i++)
        {
            list.Add(arr[i]);
        }

        return list;
    }
}