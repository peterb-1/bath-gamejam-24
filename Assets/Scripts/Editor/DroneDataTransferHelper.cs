using System.Collections.Generic;
using Gameplay.Drone;
using Gameplay.Events;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor
{
    public static class DroneDataTransferHelper
    {
        [MenuItem("Tools/Transfer Drone Data")]
        private static void TransferData()
        {
            var transferCount = 0;
            
            foreach (var droneMovementBehaviour in Object.FindObjectsByType<DroneMovementBehaviour>(FindObjectsSortMode.None))
            {
                var strategy = droneMovementBehaviour.MovementStrategy;
                var serializedObject = new SerializedObject(droneMovementBehaviour);
                var strategyProperty = serializedObject.FindProperty("movementStrategy");

                if (strategy is PatrolMovementStrategy)
                {
                    strategyProperty.FindPropertyRelative("cycleTime").floatValue = serializedObject.FindProperty("cycleTime").floatValue;
                    strategyProperty.FindPropertyRelative("cycleOffset").floatValue = serializedObject.FindProperty("cycleOffset").floatValue;
                    strategyProperty.FindPropertyRelative("smoothEnds").boolValue = serializedObject.FindProperty("smoothEnds").boolValue;
                    strategyProperty.FindPropertyRelative("radius").floatValue = serializedObject.FindProperty("radius").floatValue;
                    strategyProperty.FindPropertyRelative("isClockwise").boolValue = serializedObject.FindProperty("isClockwise").boolValue;
                    
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(droneMovementBehaviour);
                    
                    transferCount++;
                }

                if (strategy is BezierMovementStrategy && droneMovementBehaviour.gameObject.TryGetComponent<DroneFlyInBehaviour>(out var droneFlyInBehaviour))
                {
                    var serializedFlyInBehaviour = new SerializedObject(droneFlyInBehaviour);
                    var sourceCurveProperty = serializedFlyInBehaviour.FindProperty("bezierCurve");
                    var targetCurveProperty = strategyProperty.FindPropertyRelative("bezierCurve");
    
                    CopySerializedProperty(sourceCurveProperty, targetCurveProperty);
                    
                    strategyProperty.FindPropertyRelative("segmentCount").intValue = serializedFlyInBehaviour.FindProperty("curveSegmentCount").intValue;
                    strategyProperty.FindPropertyRelative("duration").floatValue = serializedFlyInBehaviour.FindProperty("flyInTime").floatValue;
                    
                    var strategyOnFinishProperty = strategyProperty.FindPropertyRelative("strategyOnFinish");
                    strategyOnFinishProperty.managedReferenceValue ??= new PatrolMovementStrategy();
                    
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                    
                    strategyOnFinishProperty.FindPropertyRelative("cycleTime").floatValue = serializedObject.FindProperty("cycleTime").floatValue;
                    strategyOnFinishProperty.FindPropertyRelative("cycleOffset").floatValue = serializedObject.FindProperty("cycleOffset").floatValue;
                    strategyOnFinishProperty.FindPropertyRelative("smoothEnds").boolValue = serializedObject.FindProperty("smoothEnds").boolValue;
                    strategyOnFinishProperty.FindPropertyRelative("radius").floatValue = serializedObject.FindProperty("radius").floatValue;
                    strategyOnFinishProperty.FindPropertyRelative("isClockwise").boolValue = serializedObject.FindProperty("isClockwise").boolValue;
                    
                    serializedObject.ApplyModifiedProperties();
                    serializedFlyInBehaviour.ApplyModifiedProperties();
                    
                    EditorUtility.SetDirty(droneMovementBehaviour);
                    EditorUtility.SetDirty(droneFlyInBehaviour);
                    
                    transferCount++;
                }
            }
            
            var actionTransferCount = 0;
            
            foreach (var action in Object.FindObjectsByType<DroneActivateAction>(FindObjectsSortMode.None))
            {
                var serializedAction = new SerializedObject(action);
                var flyInDronesProperty = serializedAction.FindProperty("flyInDrones");
                var patrolDronesProperty = serializedAction.FindProperty("patrolDrones");
                
                var existingDrones = new HashSet<Object>();
                for (var i = 0; i < patrolDronesProperty.arraySize; i++)
                {
                    var drone = patrolDronesProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (drone != null)
                    {
                        existingDrones.Add(drone);
                    }
                }
                
                var addedCount = 0;
                for (var i = 0; i < flyInDronesProperty.arraySize; i++)
                {
                    var flyInDrone = flyInDronesProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (flyInDrone != null && !existingDrones.Contains(flyInDrone))
                    {
                        var newIndex = patrolDronesProperty.arraySize;
                        patrolDronesProperty.InsertArrayElementAtIndex(newIndex);
                        
                        var flyInBehaviour = flyInDrone as DroneFlyInBehaviour;
                        if (flyInBehaviour != null && flyInBehaviour.TryGetComponent<DroneMovementBehaviour>(out var movementBehaviour))
                        {
                            patrolDronesProperty.GetArrayElementAtIndex(newIndex).objectReferenceValue = movementBehaviour;
                            addedCount++;
                        }
                    }
                }
                
                if (addedCount > 0)
                {
                    serializedAction.ApplyModifiedProperties();
                    EditorUtility.SetDirty(action);
                    actionTransferCount++;
                }
            }
            
            GameLogger.Log($"Transferred data for {transferCount} drones!");
            GameLogger.Log($"Merged drone lists in {actionTransferCount} DroneActivateAction objects!");
            
            AssetDatabase.SaveAssets();
        }
        
        private static void CopySerializedProperty(SerializedProperty source, SerializedProperty target)
        {
            if (source == null || target == null) return;
            
            if (source.isArray && target.isArray)
            {
                CopyArray(source, target);
                return;
            }
            
            var sourceIterator = source.Copy();
            var sourceEndProperty = source.GetEndProperty();
            var enterChildren = true;
            
            while (sourceIterator.Next(enterChildren) && !SerializedProperty.EqualContents(sourceIterator, sourceEndProperty))
            {
                enterChildren = false;
                
                var relativePath = sourceIterator.propertyPath[(source.propertyPath.Length + 1)..];
                var targetProperty = target.FindPropertyRelative(relativePath);
                
                if (targetProperty == null) continue;
                
                if (sourceIterator.isArray && targetProperty.isArray)
                {
                    CopyArray(sourceIterator, targetProperty);
                }
                else
                {
                    CopyPropertyValue(sourceIterator, targetProperty);
                }
            }
        }

        private static void CopyArray(SerializedProperty source, SerializedProperty target)
        {
            target.arraySize = source.arraySize;
            
            for (var i = 0; i < source.arraySize; i++)
            {
                var sourceElement = source.GetArrayElementAtIndex(i);
                var targetElement = target.GetArrayElementAtIndex(i);
                
                CopyPropertyValue(sourceElement, targetElement);
            }
        }

        private static void CopyPropertyValue(SerializedProperty source, SerializedProperty target)
        {
            switch (source.propertyType)
            {
                case SerializedPropertyType.Integer:
                    target.intValue = source.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    target.boolValue = source.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    target.floatValue = source.floatValue;
                    break;
                case SerializedPropertyType.String:
                    target.stringValue = source.stringValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    target.objectReferenceValue = source.objectReferenceValue;
                    break;
                case SerializedPropertyType.Enum:
                    target.enumValueIndex = source.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    target.vector2Value = source.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    target.vector3Value = source.vector3Value;
                    break;
                case SerializedPropertyType.Color:
                    target.colorValue = source.colorValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    target.quaternionValue = source.quaternionValue;
                    break;
                case SerializedPropertyType.Vector4:
                    target.vector4Value = source.vector4Value;
                    break;
                case SerializedPropertyType.Generic:
                    CopySerializedProperty(source, target);
                    break;
            }
        }
    }
}
