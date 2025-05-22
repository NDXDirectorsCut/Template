using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Enigma
{
    public enum InputType
    {
        Fixed,
        Analog,
        AI
    }

    [Serializable]
    public class ActionInput
    {
        public string name;
        public InputType inputType;
        public float GetInput()
        {
            if(inputType == InputType.Analog)
            {
                return Input.GetAxis(name);
            }
            else
            {
                return Input.GetButton(name) ? 1 : 0;
            }
            return 0;
        }
    }

}
