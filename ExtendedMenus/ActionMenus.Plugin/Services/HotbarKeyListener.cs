using ModifAmorphic.Outward.ActionMenus.Settings;
using ModifAmorphic.Outward.Unity.ActionMenus;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionMenus.Services
{
    internal class HotbarKeyListener : IHotbarNavActions
    {
        private readonly Player _player;

        public HotbarKeyListener(Player player) => _player = player;

        public bool IsNextRequested() => _player.GetButtonDown(RewiredConstants.ActionSlots.NextHotbarAction.id);

        public bool IsPreviousRequested() => _player.GetButtonDown(RewiredConstants.ActionSlots.PreviousHotbarAction.id);

        public bool IsHotbarRequested(out int hotbarIndex)
        {
            hotbarIndex = -1;
            if (!_player.GetAnyButtonDown())
                return false;

            for (int i = 0; i < RewiredConstants.ActionSlots.HotbarNavActions.Count; i++)
            {
                if (_player.GetButtonDown(RewiredConstants.ActionSlots.HotbarNavActions[i].id))
                {
                    hotbarIndex = i;
                    return true;
                }
            }
            return false;
        }
    }
}
