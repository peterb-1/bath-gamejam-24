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
        private SerializedProperty lineRendererProperty;
        private SerializedProperty leftConnectionProperty;
        private SerializedProperty rightConnectionProperty;
        private SerializedProperty hiddenConnectionProperty;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            textProperty = serializedObject.FindProperty("levelNumberText");
            sceneConfigProperty = serializedObject.FindProperty("sceneConfig");
            animatorProperty = serializedObject.FindProperty("borderAnimator");
            lineRendererProperty = serializedObject.FindProperty("lineRenderer");
            leftConnectionProperty = serializedObject.FindProperty("leftConnectionAnchor");
            rightConnectionProperty = serializedObject.FindProperty("rightConnectionAnchor");
            hiddenConnectionProperty = serializedObject.FindProperty("hiddenConnectionAnchor");
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
            EditorGUILayout.PropertyField(lineRendererProperty);
            EditorGUILayout.PropertyField(leftConnectionProperty);
            EditorGUILayout.PropertyField(rightConnectionProperty);
            EditorGUILayout.PropertyField(hiddenConnectionProperty);
            
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