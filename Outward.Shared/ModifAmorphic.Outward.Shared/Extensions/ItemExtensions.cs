using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Extensions
{
    public static class ItemExtensions
    {
        public static Item SetNames(this Item item, string name)
        {
            item.SetPrivateField("m_name", name);
            item.SetPrivateField("m_localizedName", name);
            item.gameObject.name = $"{item.ItemID}_{item.Name.Replace(" ", "")}";
            return item;
        }
        public static Item SetDescription(this Item item, string description)
        {
            item.SetPrivateField("m_localizedDescription", description);
            return item;
        }
        public static Item ClearEffects(this Item item)
        {
            var effectsHolder = item.transform.Find("Effects");
            var effects = effectsHolder.GetComponents<Effect>();

            foreach (var eff in effects)
            {
                UnityEngine.Object.DestroyImmediate(eff);
            }
            return item;
        }
        public static T AddEffect<T>(this Item item) where T : Effect
        {
            var effectsHolder = item.transform.Find("Effects");
            return effectsHolder.GetOrAddComponent<T>();
        }
    }
}
