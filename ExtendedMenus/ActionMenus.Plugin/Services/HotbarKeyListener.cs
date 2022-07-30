using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class HotbarKeyListener : IHotbarRequestActions
    {
        private readonly Player _player;
        public int HotbarIndexRequested => throw new NotImplementedException();

        public HotbarKeyListener(Player player) => _player = player;

        public bool IsNextRequested() =>_player.GetButtonDown(RewiredConstants.ActionSlots.NextHotbarAction);

        public bool IsPreviousRequested() => _player.GetButtonDown(RewiredConstants.ActionSlots.PreviousHotbarAction);

        public bool IsSelectRequested() => false;
    }
}
