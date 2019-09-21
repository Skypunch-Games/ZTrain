using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2.Localization
{
    [System.Serializable]
    public class LocalizedString
    {
        /// <summary>
        /// Gets the message in the currently selected language.
        /// </summary>
        [IgnoreCustomSerialization]
        public string message
        {
            get
            {
                return LocalizationManager.GetString(_key);
            }
            set
            {
                var c = LocalizationManager.currentDatabase;
                if (c != null)
                {
                    if (_key == LocalizationManager.NoKeyConstant)
                    {
                        _key = System.Guid.NewGuid().ToString();
                    }
                    
                    c.SetString(_key, value);
                }
            }
        }

        [SerializeField]
        private string _key = LocalizationManager.NoKeyConstant;

        public LocalizedString()
        {

        }
        
        public LocalizedString(string key)
        {
            _key = key;
        }

        public override string ToString()
        {
            return message;
        }
    }
}
