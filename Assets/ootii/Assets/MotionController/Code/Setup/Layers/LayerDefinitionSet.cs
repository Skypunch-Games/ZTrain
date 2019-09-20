using System.Collections.Generic;
using UnityEngine;

namespace com.ootii.Setup
{
    // CDL 06/30/2018 - a layer definition holds a collection of layers
    // and can be used to quickly set layers in Unity (via the inspector)
    [CreateAssetMenu(menuName = "ootii/Game/Layer Definition Set")]
    public class LayerDefinitionSet : ScriptableObject
    {                
        [TextArea(3, 5)]
        [Tooltip("HelpBox description text.")]
        public string _Description;
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        
        [Tooltip("The layer definitions belonging to this set.")]
        public List<LayerDefinition> _LayerDefinitions = new List<LayerDefinition>();
        public List<LayerDefinition> LayerDefinitions
        {
            get { return _LayerDefinitions; }
            set { _LayerDefinitions = value; }
        }

        [Tooltip("Overwrite existing layer names.")]
        public bool _OverwriteExistingLayers = false;
        public bool OverwriteExistingLayers
        {
            get { return _OverwriteExistingLayers; }
            set { _OverwriteExistingLayers = value; }
        }        
    }
}
