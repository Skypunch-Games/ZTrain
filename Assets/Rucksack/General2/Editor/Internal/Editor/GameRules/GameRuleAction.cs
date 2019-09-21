using System;
using System.Collections.Generic;

namespace Devdog.General2.Editors.GameRules
{
    public class GameRuleAction
    {
        public System.Action action;
        public string name;

        public GameRuleAction(string name, System.Action action)
        {
            this.name = name;
            this.action = action;
        }
    }
}
