using ModifAmorphic.Outward.ActionUI.Settings;
using ModifAmorphic.Outward.Unity.ActionUI;
using Rewired;
using UnityEngine;

namespace ModifAmorphic.Outward.ActionUI.Services
{
    internal class HotbarKeyListener : IHotbarNavActions
    {
        private readonly Player _player;

        public HotbarKeyListener(Player player) => _player = player;

        public bool IsNextRequested() => _player.GetButtonDown(RewiredConstants.ActionSlots.NextHotbarAction.id) || !Mathf.Approximately(_player.GetAxis(RewiredConstants.ActionSlots.NextHotbarAxisAction.id), 0f);

        public bool IsPreviousRequested() => _player.GetButtonDown(RewiredConstants.ActionSlots.PreviousHotbarAction.id) || !Mathf.Approximately(_player.GetAxis(RewiredConstants.ActionSlots.PreviousHotbarAxisAction.id), 0f);

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
