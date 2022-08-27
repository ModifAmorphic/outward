using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Unity.ActionMenus.Data
{
    public interface IHotbarProfileService
    {
        event Action<IHotbarProfile> OnProfileChanged;
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
}
