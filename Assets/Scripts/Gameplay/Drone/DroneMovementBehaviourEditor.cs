#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Gameplay.Drone
{
    [CustomEditor(typeof(DroneMovementBehaviour))]
    public class DroneControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            var drone = (DroneMovementBehaviour) target;
            var strategy = drone.MovementStrategy;
            
            if (strategy is not BezierMovementStrategy bezierStrategy) return;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bezier Strategy Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add Control Point"))
            {
                bezierStrategy.AddControlPoint(drone.transform);
                EditorUtility.SetDirty(target);
            }
            
            if (GUILayout.Button("Clear Points"))
            {
                bezierStrategy.ClearPoints(drone.transform);
                EditorUtility.SetDirty(target);
            }
        }
    }
}
#endif
