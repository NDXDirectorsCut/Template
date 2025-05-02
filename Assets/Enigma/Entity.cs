using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma
{
    public class Entity : MonoBehaviour
    {
        [Header("Entity")]

        [SerializeField]
        private string state = "Null";
        private string prevState;
        [System.NonSerialized]
        public bool stateLock = false;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public bool SetState(string newState)
        {
            if(stateLock == false)
            {
                state = newState;
            }
            return stateLock;
        }

        public string GetState()
        {
            return state;
        }
    }
}