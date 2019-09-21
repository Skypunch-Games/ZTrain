using System;
using System.Collections.Generic;
using Devdog.General2.Serialization;
using UnityEngine;

namespace Devdog.General2
{
    public class BetterScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private SerializationData serializationData;
        
        
        
        #region Full Serializer data
        
        /// <summary>
        /// Used to store the state of this scriptable object. 
        /// Fetched through reflection to avoid people messing with it.
        /// </summary>
        [SerializeField]
        [IgnoreCustomSerialization] // Ignore in custom serializer - Let unity seriarlize this.
        private string _serializedJsonString = "{}";

        [SerializeField]
        [IgnoreCustomSerialization] // Ignore in custom serializer - Let unity seriarlize this.
        private List<UnityEngine.Object> _objectReferences;
        
        [NonSerialized]
        private readonly BetterSerializationModel _serializer = new BetterSerializationModel();
        
        [NonSerialized]
        private bool _loadedFullSerializerData = false;
        
        #endregion
        
        public void LoadFullSerializerData()
        {
            _serializer.Load(ref _objectReferences, ref _serializedJsonString, this);
        }
        
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(_serializedJsonString) == false && _serializedJsonString != "{}")
            {
                LoadFullSerializerData();
                _serializedJsonString = "";
                _objectReferences = new List<UnityEngine.Object>();
                
                _loadedFullSerializerData = true;
            }
            
            UnitySerializationUtility.DeserializeUnityObject((UnityEngine.Object) this, ref this.serializationData, (DeserializationContext) null);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject((UnityEngine.Object) this, ref this.serializationData, false, (SerializationContext) null);
        }
        
        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (_loadedFullSerializerData && Application.isPlaying == false)
            {  
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}
