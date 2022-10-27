using System;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace Utilities.CustomAttribute
{
    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    public class SceneNameDrawer : PropertyDrawer
    {
        public int SceneIndex = -1;
        public GUIContent[] SceneNames;

        private readonly string[] _splitSeparators = { "/", ".unity" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorBuildSettings.scenes.Length is 0)
                return;

            if (SceneIndex is -1)
                GetSceneNameArray(property);

            var oldIndex = SceneIndex;
            /*
             * EditorGUI.Popup返回被选中的值的索引，第三个参数会在编辑器GUI上显示SceneNames指定索引的值
             * 必须让SceneIndex有所更改，否则编辑器GUI会一直显示原来的值
             */
            SceneIndex = EditorGUI.Popup(position, label, SceneIndex, SceneNames);

            if (oldIndex != SceneIndex)
                property.stringValue = SceneNames[SceneIndex].text;
        }
        
        

        private void GetSceneNameArray(SerializedProperty property)
        {
            var scenes = EditorBuildSettings.scenes;
            SceneNames = new GUIContent[scenes.Length];

            for (var i = 0; i < SceneNames.Length; i++)
            {
                //从场景路径分割出路径和场景名称
                var splitPath = scenes[i].path.Split(_splitSeparators, StringSplitOptions.RemoveEmptyEntries);
                //如果有场景名称就获取场景，如果场景被删除了就返回(Deleted Scene)
                SceneNames[i] = new GUIContent(splitPath.Length is > 0 ? splitPath[^1] : "(Deleted Scene)");
            }

            if (SceneNames.Length is 0)
                SceneNames = new[] { new GUIContent("Check Your Build Settings") };

            SceneIndex = 0;
            /*
             * 如果字段本来就有值，检测这个值在不在场景列表里，这个行为是为了检测场景名称有没有拼错
             * 如果字段没有值，将0索引的值赋值到字段中
             */
            if (!string.IsNullOrEmpty(property.stringValue))
            {
                for (var i = 0; i < SceneNames.Length; i++)
                {
                    if (!SceneNames[i].text.Equals(property.stringValue))
                        continue;
                    SceneIndex = i;
                    break;
                }
            }
            property.stringValue = SceneNames[SceneIndex].text;
        }
    }
}