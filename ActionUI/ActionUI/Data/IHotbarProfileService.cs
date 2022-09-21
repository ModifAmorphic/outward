using ModifAmorphic.Outward.Unity.ActionMenus;
using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IHotbarProfileService
    {
        UnityEvent<IHotbarProfile, HotbarProfileChangeTypes> OnProfileChanged { get; }
        IHotbarProfile GetProfile();
        void Save();
        void SaveNew(IHotbarProfile hotbarProfile);
        void Update(HotbarsContainer hotbar);
        IHotbarProfile AddHotbar();
        IHotbarProfile RemoveHotbar();
        IHotbarProfile AddRow();
        IHotbarProfile RemoveRow();
        IHotbarProfile AddSlot();
        IHotbarProfile RemoveSlot();
        IHotbarProfile SetCooldownTimer(bool showTimer, bool preciseTime);
        IHotbarProfile SetCombatMode(bool combatMode);
        IHotbarProfile SetEmptySlotView(EmptySlotOptions option);
    }

    public enum HotbarProfileChangeTypes
    {
        ProfileRefreshed,
        HotbarAdded,
        HotbarRemoved,
        RowAdded,
        RowRemoved,
        SlotAdded,
        SlotRemoved,
        CooldownTimer,
        CombatMode,
        EmptySlotView
    }
}
