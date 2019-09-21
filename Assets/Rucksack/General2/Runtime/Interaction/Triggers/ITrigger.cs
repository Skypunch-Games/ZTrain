using UnityEngine;

namespace Devdog.General2
{
    public interface ITrigger
    {
        Transform transform { get; }
        ITriggerRangeHandler rangeHandler { get; }
        
        bool Toggle(Character character);
        
        bool CanUse(Character character);
        bool Use(Character character);
        void Server_Use(Character character);
        
        bool CanUnUse(Character character);
        bool UnUse(Character character);
        void Server_UnUse(Character character);



    }
}