using Gameplay.Drone;
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

                if (strategy is PatrolMovementStrategy)
                {
                    var serializedObject = new SerializedObject(droneMovementBehaviour);
                    var strategyProperty = serializedObject.FindProperty("movementStrategy");
                    
                    strategyProperty.FindPropertyRelative("cycleTime").floatValue = serializedObject.FindProperty("cycleTime").floatValue;
                    strategyProperty.FindPropertyRelative("cycleOffset").floatValue = serializedObject.FindProperty("cycleOffset").floatValue;
                    strategyProperty.FindPropertyRelative("smoothEnds").boolValue = serializedObject.FindProperty("smoothEnds").boolValue;
                    strategyProperty.FindPropertyRelative("radius").floatValue = serializedObject.FindProperty("radius").floatValue;
                    strategyProperty.FindPropertyRelative("isClockwise").boolValue = serializedObject.FindProperty("isClockwise").boolValue;
                    
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(droneMovementBehaviour);
                    
                    transferCount++;
                }
            }
            
            GameLogger.Log($"Transferred data for {transferCount} drones!");
            AssetDatabase.SaveAssets();
        }
    }
}