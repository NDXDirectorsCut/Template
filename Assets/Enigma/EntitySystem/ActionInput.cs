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
    }
}
