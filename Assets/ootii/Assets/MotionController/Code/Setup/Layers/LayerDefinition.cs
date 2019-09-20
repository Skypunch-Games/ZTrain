using System;
using UnityEngine;

namespace com.ootii.Setup
{
    // CDL 06/30/2018 - represents a Unity layer
    [Serializable]
    public class LayerDefinition
    {
        [Tooltip("The layer's index")]
        public int _Index = 0;
        public int Index
        {
            get { return _Index; }
            set { _Index = value; }
        }

        [Tooltip("The layer's display name")]
        public string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
    }
}

