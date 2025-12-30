#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Drone
{
    [CustomPropertyDrawer(typeof(IDroneMovementStrategy), true)]
    public class StrategyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
        
            // Draw the field label
            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, label);
        
            // Get current type
            var currentType = property.managedReferenceValue?.GetType();
            var currentTypeName = currentType != null ? currentType.Name : "None";
        
            // Type dropdown (positioned after the label)
            var dropdownRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 
                position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(currentTypeName), FocusType.Keyboard))
            {
                ShowTypeMenu(property);
            }
        
            // Draw fields of selected type
            if (property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                var fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, 
                    position.width, position.height - EditorGUIUtility.singleLineHeight - 2);
            
                // Draw children without the main property label
                var iterator = property.Copy();
                var endProperty = iterator.GetEndProperty();
                iterator.NextVisible(true); // Enter children
            
                float yOffset = fieldRect.y;
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    float height = EditorGUI.GetPropertyHeight(iterator, true);
                    var childRect = new Rect(fieldRect.x, yOffset, fieldRect.width, height);
                    EditorGUI.PropertyField(childRect, iterator, true);
                    yOffset += height + EditorGUIUtility.standardVerticalSpacing;
                
                    if (!iterator.NextVisible(false))
                        break;
                }
            
                EditorGUI.indentLevel--;
            }
        
            EditorGUI.EndProperty();
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
        
            if (property.managedReferenceValue != null)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
            
                var iterator = property.Copy();
                var endProperty = iterator.GetEndProperty();
                iterator.NextVisible(true);
            
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    if (!iterator.NextVisible(false))
                        break;
                }
            }
        
            return height;
        }
    
        private void ShowTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), false, () => {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });
        
            // Find all derived types
            var baseType = typeof(IDroneMovementStrategy);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));
        
            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
        
            menu.ShowAsContext();
        }
    }
}
#endif