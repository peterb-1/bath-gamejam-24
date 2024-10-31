using System;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public struct GameObjectStateSetter
    {
        [SerializeField] 
        private GameObjectActiveState[] gameObjectActiveStates;
        
        public void SetState()
        {
            foreach (var gameObjectState in gameObjectActiveStates)
            {
                gameObjectState.SetState();
            }
        }

        public void SetInverseState()
        {
            foreach (var gameObjectState in gameObjectActiveStates)
            {
                gameObjectState.SetInverseState();
            }
        }
        
        [Serializable]
        private struct GameObjectActiveState
        {
            [SerializeField] 
            private GameObject gameObject;

            [SerializeField]
            private bool isActive;

            public void SetState()
            {
                gameObject.SetActive(isActive);
            }

            public void SetInverseState()
            {
                gameObject.SetActive(!isActive);
            }
        }
    }
}
