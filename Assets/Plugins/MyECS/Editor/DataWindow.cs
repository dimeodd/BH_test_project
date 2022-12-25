using System;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace MyEcs.Debugger
{
    public class DataWindow
    {
        internal Type _type = null;
        internal object _myList = null;
        internal Vector2 _scrollpos;
        EcsWorld _world;
        int typeID;

        public DataWindow(int typeID, EcsWorld world)
        {
            this.typeID = typeID;
            _world = world;
        }

        public void OnGUI()
        {
            var pool = _world.DBG_GetPool[typeID];

            if (pool.GetCount() == 0)
            {
                GUILayout.Label("NULL");
                return;
            }

            var fields = pool.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var array = (fields[1].GetValue(pool) as Array);

            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            GUILayout.BeginVertical();


            GUILayout.BeginHorizontal();
            GUILayout.Label($"ID\n" + GetStructNames(array.GetValue(0)));

            _scrollpos = GUILayout.BeginScrollView(_scrollpos, true, false);
            GUILayout.BeginHorizontal();

            if (EcsMemoryMapperWindow._showAll)
            {
                for (int i = 0, iMax = array.Length; i < iMax; i++)
                {
                    GUILayout.Label($"[{i}]:\n" + GetStructValues(array.GetValue(i)));
                }
            }
            else
            {
                for (int i = 0, iMax = pool.GetCount(); i < iMax; i++)
                {
                    if (!pool.DBG_GetFreeData.Contain(i))
                        GUILayout.Label($"[{i}]:\n" + GetStructValues(array.GetValue(i)));
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public static string StructToString(object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var sb = new StringBuilder();
            foreach (var field in fields)
            {
                sb.AppendFormat("{0}: {1}\n",
                field.Name,
                field.GetValue(obj)
                );
            }

            return sb.ToString();
        }
        private static string GetStructValues(object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var sb = new StringBuilder();
            foreach (var field in fields)
            {
                sb.AppendFormat("{0}\n",
                field.GetValue(obj)
                );
            }

            return sb.ToString();
        }
        private static string GetStructNames(object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var sb = new StringBuilder();
            foreach (var field in fields)
            {
                sb.AppendFormat("{0}:\n",
                field.Name
                );
            }

            return sb.ToString();
        }
    }

}
