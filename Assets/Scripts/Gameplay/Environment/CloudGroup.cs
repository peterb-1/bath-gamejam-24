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
        private bool isBackground;
        
        [SerializeField, ShowIf(nameof(isBackground))] 
        private bool isDistance;
        
        [SerializeField] 
        private int cloudCount;
        
        [SerializeField] 
        private float horizontalSpeed;

        [SerializeField] 
        private float leftEnd;

        [SerializeField] 
        private float rightEnd;

        [SerializeField, ReadOnly] 
        private Cloud[] clouds;

        public float BaseSpeed => horizontalSpeed;
        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            CurrentSpeed = BaseSpeed;
        }

        public void SetSpeed(float speed)
        {
            foreach (var cloud in clouds)
            {
                cloud.Configure(speed, leftEnd, rightEnd);
            }

            CurrentSpeed = speed;
        }

#if UNITY_EDITOR
        [Button("Setup Clouds")]
        private void SetupClouds()
        {
            var span = rightEnd - leftEnd;
            var interCloudDistance = span / cloudCount;

            // stupid hack, can't be bothered - will need changing if we add more prefabs
            var cloudPrefabGuid = isBackground 
                ? isDistance 
                    ? AssetDatabase.FindAssets("Cloud t:GameObject")[6] 
                    : AssetDatabase.FindAssets("Cloud t:GameObject")[0]
                : AssetDatabase.FindAssets("Cloud t:GameObject")[2];
            var cloudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(cloudPrefabGuid));

            clouds = new Cloud[cloudCount];

            for (var i = 0; i < cloudCount; i++)
            {
                var cloudGameObject = (GameObject) PrefabUtility.InstantiatePrefab(cloudPrefab);
                var cloud = cloudGameObject.GetComponent<Cloud>();

                cloudGameObject.transform.parent = transform;
                cloud.transform.localPosition = Vector3.right * (leftEnd + (i + 0.5f) * interCloudDistance);

                cloud.Configure(horizontalSpeed, leftEnd, rightEnd);

                clouds[i] = cloud;
            }
        }

        [Button("Clear Clouds")]
        private void ClearClouds()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            clouds = Array.Empty<Cloud>();
        }
#endif
    }
}
