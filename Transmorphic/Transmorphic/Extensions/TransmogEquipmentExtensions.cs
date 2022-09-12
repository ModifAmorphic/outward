using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Extensions
{
    public static class TransmogEquipmentExtensions
    {
        public static bool IsTransmogIngredient(this Equipment equipment) => (equipment is Armor || equipment is Weapon || equipment is Bag
                                                                                    || equipment.HasTag(ItemTags.BackpackTag)
                                                                                    || equipment.HasTag(ItemTags.LanternTag)
                                                                                    || equipment.HasTag(ItemTags.LexiconTag))
                                                                              && !TransmogSettings.ExcludedItemIDs.Contains(equipment.ItemID)
                                                                              && !(equipment is Ammunition);
    }
}
