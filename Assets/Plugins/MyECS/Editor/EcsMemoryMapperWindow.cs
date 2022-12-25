using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MyEcs.Debugger
{
    public class EcsMemoryMapperWindow : EditorWindow
    {
        //TODO записывать значения в папку
        internal static bool _showData = false, _showEntity = false, _showAll = false;

        MyList<DataWindow> _data = new MyList<DataWindow>(8);
        MyList<bool> _showData2 = new MyList<bool>(8);
        MyList<bool> _showEntity2 = new MyList<bool>(8);
        EcsWorld _world;

        Vector2 _scrollpos;

        [MenuItem("Debug/Ecs Debugger")]
        private static void ShowWindow()
        {
            var window = GetWindow<EcsMemoryMapperWindow>();
            window.titleContent = new GUIContent("Ecs Memory");
            window.Show();
        }
        private void OnGUI()
        {
            //TODO сохранение настроек в текстовый файлик, прям как в CopyBlendTree
            EditorGUILayout.HelpBox("Это окно несчадно жрёт ЦП", MessageType.Warning);
            _world = EcsWorld._debugWorld;

            _showAll = GUILayout.Toggle(_showAll, "show hided");

            _scrollpos = GUILayout.BeginScrollView(_scrollpos, false, true);
            if (_world == null || _world.DBG_GetEntityCount == 0)
            {
                GUILayout.Label("World not exist!");
            }
            else
            {
                if (_data.Count != _world.DBG_GetPool.Length)
                    CheckCache();

                _showData = EditorGUILayout.Foldout(_showData, $"Data {_data.Count}/{_world.DBG_GetPool.Length}");
                if (_showData) DrawMemory();

                _showEntity = EditorGUILayout.Foldout(_showEntity, string.Format(
                    "Entity {0}/{1}",
                    _world.DBG_GetEntityCount - _world.DBG_GetFreeEntity.Count,
                    _world.DBG_GetEntityData.Length
                ));
                if (_showEntity) DrawEntity();
            }

            GUILayout.EndScrollView();

            this.Repaint();
        }

        void CheckCache()
        {
            Debug.Log("CheckCache");
            _data = new MyList<DataWindow>(8);
            for (int i = 0; i < _world.DBG_GetPool.Length; i++)
            {
                _data.Add(new DataWindow(i, _world));
            }

            while (_data.Count > _showData2.Count)
            {
                _showData2.Add(false);
            }
        }

        void DrawMemory()
        {
            var temp = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            while (_world.DBG_GetPool.Length > _showData2.Count)
            {
                _showData2.Add(false);
            }

            for (int i = 1; i < _world.DBG_GetPool.Length; i++)
            {
                var pool = _world.DBG_GetPool[i];
                if (pool == null)
                {
                    EditorGUILayout.LabelField("null");
                    continue;
                }

                var t = pool.GetType().GenericTypeArguments[0];

                GUILayout.BeginHorizontal();

                _showData2._data[i] = EditorGUILayout.Foldout(_showData2._data[i], string.Format(
                    "{0}: {2} Gen {3}\t{1}",
                    i,
                    t.Name,
                    pool.GetCount(),
                    pool.GetChanges()
                ));

                GUILayout.EndHorizontal();

                if (_showData2._data[i])
                {
                    _data._data[i].OnGUI();
                }

            }

            EditorGUI.indentLevel = temp;
        }

        void DrawEntity()
        {
            while (_world.DBG_GetEntityCount > _showEntity2.Count)
            {
                _showEntity2.Add(false);
            }

            var temp = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            for (int j = 1; j < _world.DBG_GetEntityCount; j++)
            {
                var entData = _world.DBG_GetEntityData[j];

                if (entData.DBG_GetTypes.Count == 0) continue;

                _showEntity2._data[j] = EditorGUILayout.Foldout(_showEntity2._data[j],
                                        string.Format("Entity ID:{0}", j));

                if (_showEntity2._data[j]) //Draw Entity Components
                {
                    GUILayout.BeginHorizontal();
                    // GUILayout.Space(50);
                    for (int i = 0; i < entData.DBG_GetTypes.Count; i++)
                    {
                        var DBG_type = entData.DBG_GetTypes._data[i];
                        var DBG_id = entData.DBG_GetId._data[i];

                        var pool = _world.DBG_GetPool[DBG_type];
                        var fields = pool.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        var array = fields[1].GetValue(pool) as Array;

                        var poolName = pool.GetType().GenericTypeArguments[0].Name;
                        var structData = DataWindow.StructToString(array.GetValue(DBG_id));

                        GUILayout.Label(string.Format(
                            "[{0}]: {1}\n{2}",
                             poolName,
                             DBG_id,
                             structData
                        ));
                    }
                    GUILayout.EndHorizontal();
                }

            }

            EditorGUI.indentLevel = temp;
        }
    }
}
