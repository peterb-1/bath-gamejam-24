#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    [CustomEditor(typeof(Button), true)]
    [CanEditMultipleObjects]
    public class LevelSelectButtonEditor : ButtonEditor
    {
        private SerializedProperty textProperty;
        private SerializedProperty sceneConfigProperty;
        private SerializedProperty animatorProperty;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            textProperty = serializedObject.FindProperty("levelNumberText");
            sceneConfigProperty = serializedObject.FindProperty("sceneConfig");
            animatorProperty = serializedObject.FindProperty("borderAnimator");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (Application.isPlaying)
            {
                return;
            }
            
            EditorGUILayout.Space();

            serializedObject.Update();
            
            EditorGUILayout.PropertyField(textProperty);
            EditorGUILayout.PropertyField(sceneConfigProperty);
            EditorGUILayout.PropertyField(animatorProperty);
            
            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed)
            {
                foreach (var targetObject in targets)
                {
                    EditorUtility.SetDirty(targetObject);
                }
            }
        }
    }
}

#endif