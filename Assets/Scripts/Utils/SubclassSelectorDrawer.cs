#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
        
            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);

            var currentType = property.managedReferenceValue?.GetType();
            var currentTypeName = currentType != null ? currentType.Name : "None";
            var dropdownRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 
                position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(currentTypeName), FocusType.Keyboard))
            {
                ShowTypeMenu(property);
            }
        
            if (property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;

                var fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, 
                    position.width, position.height - EditorGUIUtility.singleLineHeight - 2);
                var iterator = property.Copy();
                var endProperty = iterator.GetEndProperty();

                iterator.NextVisible(true);
            
                var yOffset = fieldRect.y;
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    var height = EditorGUI.GetPropertyHeight(iterator, true);
                    var childRect = new Rect(fieldRect.x, yOffset, fieldRect.width, height);
                    
                    EditorGUI.PropertyField(childRect, iterator, true);
                    yOffset += height + EditorGUIUtility.standardVerticalSpacing;
                
                    if (!iterator.NextVisible(false)) break;
                }
            
                EditorGUI.indentLevel--;
            }
        
            EditorGUI.EndProperty();
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
        
            if (property.managedReferenceValue != null)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
            
                var iterator = property.Copy();
                var endProperty = iterator.GetEndProperty();

                iterator.NextVisible(true);
            
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    
                    if (!iterator.NextVisible(false)) break;
                }
            }

            return height;
        }

        private static void ShowTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("None"), false, () => {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            var baseType = GetFieldType(property);
            if (baseType == null) return;

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        private static Type GetFieldType(SerializedProperty property)
        {
            var parentType = property.serializedObject.targetObject.GetType();
            var fieldInfo = parentType.GetField(property.propertyPath,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
    
            return fieldInfo?.FieldType;
        }
    }
}
#endif
