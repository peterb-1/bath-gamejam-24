using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ShaderUnscaledTimeProvider : MonoBehaviour
    {
        [SerializeField] 
        private Graphic targetGraphic;

        private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");

        private void Awake()
        {
            if (targetGraphic.material == null)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            targetGraphic.material.SetFloat(UnscaledTime, Time.unscaledTime);
        }
    }
}