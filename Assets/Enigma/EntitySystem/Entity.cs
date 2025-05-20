using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma
{
    public class Entity : MonoBehaviour
    {
        [Header("Entity")]

        [SerializeField]
        private string currentState = "Null";
        private string previousState;
        [System.NonSerialized]
        public bool actionLock = false;
        [SerializeField]
        private List<Action> actions = new List<Action>();

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void LateUpdate()
        {
            previousState = currentState;
        }

        public void ChangeState(
            string state, string requiredState = null, bool overrideLock = false)
        {
            if(actionLock == true && overrideLock == false)
                return;
            if(requiredState != GetState() && requiredState != null)
                return;
            currentState = state;

        }

        public string GetState()
        {
            return currentState;
        }

        public string GetPreviousSate()
        {
            return previousState;
        }
    }
}