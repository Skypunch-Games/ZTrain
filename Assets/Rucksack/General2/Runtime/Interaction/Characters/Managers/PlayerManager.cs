using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devdog.General2
{
    public static partial class PlayerManager
    {
        public static event Action<Player, Player> OnPlayerChanged;

        private static Player _currentPlayer;
        public static Player currentPlayer
        {
            get { return _currentPlayer; }
            set
            {
                var before = _currentPlayer;
                _currentPlayer = value;

                if (before != _currentPlayer)
                {
                    if (OnPlayerChanged != null)
                    {
                        OnPlayerChanged(before, _currentPlayer);
                    }
                }
            }
        }
    }
}
