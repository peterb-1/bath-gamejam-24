using System;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Environment
{
    public class CloudGroup : MonoBehaviour
    {
        [SerializeField] 
        private int cloudCount;
        
        [SerializeField, Min(0f)] 
        private float horizontalSpeed;

        [SerializeField] 
        private float despawnPosition;

        [SerializeField] 
        private float respawnPosition;

#if UNITY_EDITOR
        [Button("Setup Clouds")]
        private void SetupClouds()
        {
            var span = Mathf.Abs(respawnPosition - despawnPosition);
            var start = Mathf.Min(respawnPosition, despawnPosition);
            var interCloudDistance = span / cloudCount;
            var direction = despawnPosition > respawnPosition ? 1f : -1f;

            var cloudPrefabGuid = AssetDatabase.FindAssets("Cloud t:GameObject")[0];
            var cloudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(cloudPrefabGuid));

            for (var i = 0; i < cloudCount; i++)
            {
                var cloudGameObject = (GameObject) PrefabUtility.InstantiatePrefab(cloudPrefab);
                var cloud = cloudGameObject.GetComponent<Cloud>();

                cloudGameObject.transform.parent = transform;
                cloud.transform.localPosition = Vector3.right * (start + (i + 0.5f) * interCloudDistance);

                cloud.Configure(horizontalSpeed * direction, despawnPosition, respawnPosition);
            }
        }

        [Button("Clear Clouds")]
        private void ClearClouds()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
#endif
    }
}
