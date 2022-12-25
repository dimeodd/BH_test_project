using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MyEcs.GoPool;
using MyEcs;
using EcsStructs;

namespace MyEcs.Debugger
{
    [CustomEditor(typeof(EntityID))]
    public class EntityIDEditor : Editor
    {
        Entity entity;
        void OnEnable()
        {
            var field = target.GetType().GetField("entity", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            entity = (Entity)field.GetValue(target);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Label("ID:" + entity.GetID());
            GUILayout.Label("Gen:" + entity.GetGen());
            GUILayout.Label("CurrentGen:" + entity.GetWorld().DBG_GetEntityData[entity.GetID()].DBG_GetGen);

            if (GUILayout.Button("Select"))
            {
               var a = entity.Get<GoEntityProvider>();
               Selection.activeObject = a.provider;
            }

            //TODO сделать отображаение всех компонентов с возможностью редактировать значения
        }
    }

    public struct DestroyTag{}
}