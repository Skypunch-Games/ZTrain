using UnityEngine;
using UnityEditor;
using Devdog.General2.Localization;

namespace Devdog.General2.Editors.GameRules
{
    public class DefaultManagersRule : ManagersRuleBase
    {
        public override void UpdateIssue()
        {
            FindManagerOfTypeOrCreateIssue<AudioManager>();
            FindManagerOfTypeOrCreateIssue<GeneralSettingsManager>();
            FindManagerOfTypeOrCreateIssue<InputManager>();
            FindManagerOfTypeOrCreateIssue<LocalizationManager>();
        }
    }
}