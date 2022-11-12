using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ModifAmorphic.Outward.RespecPotions.Settings
{
    internal static class RespecConstants
    {

        public const int ItemStartID = -13010000;

        public readonly static List<(string SceneName, string VendorPath)> VendorPaths = new List<(string SceneName, string VendorPath)>()
        {
            { ("Harmattan", "/Interactions/NPCs/NPC_Minor/MarketTradersHighPrices/UNPC_DLC_Market_Alchemy/NPC/MerchantSettings") },
            { ("Harmattan", "/Interactions/NPCs/NPC_Minor/MarketTradersLowPrices/UNPC_DLC_Market_Alchemy/NPC/MerchantSettings") },
            { ("CierzoNewTerrain", "/_SNPC/_UNPC/NoLongerBlock/HumanSNPC_Alchemist/MerchantTemplate/NPC/MerchantSettings") },
            { ("Monsoon", "/_SNPC/CharactersToDesactivate/_Merchants/UNPC_LaineAberforthA/MerchantTemplate/NPC/MerchantSettings") },
            { ("Levant", "/_SNPC/DisablingForHMQ4/_MerchantsLowPrices/HumanSNPC_CounterAlchemist/MerchantTemplate/NPC/MerchantSettings") },
            { ("Levant", "/_SNPC/DisablingForHMQ4/_MerchantsHighPrices/HumanSNPC_CounterAlchemist/MerchantTemplate/NPC/MerchantSettings") },
            { ("Berg", "/_SNPC/_Merchants/HumanSNPC_CounterAlchemist/MerchantTemplate/NPC/MerchantSettings") },
            { ("NewSirocco", "/BuildingVisualPool/9400171_AlchemistShop_v/Building/BuildingVisual/prf_env_bld_BaseBuild_Alchemist_A/3-Finished/Interactables-Finished/UNPC_DLC2_AlchemyStore/AlchemyMerchant_Dialogue/NPC/MerchantSettings") },
            { ("NewSirocco", "/BuildingVisualPool/9400171_AlchemistShop_v/Building/BuildingVisual/prf_env_bld_BaseBuild_Alchemist_A/4-Upgrade_A/Interactables-Upgrade_A/UNPC_DLC2_AlchemyStore/AlchemyMerchant_Dialogue/NPC/MerchantSettings") },
            { ("NewSirocco", "/BuildingVisualPool/9400171_AlchemistShop_v/Building/BuildingVisual/prf_env_bld_BaseBuild_Alchemist_A/5-Upgrade_B/Interactables-Upgrade_B/UNPC_DLC2_AlchemyStore/AlchemyMerchant_Dialogue/NPC/MerchantSettings") },
        };

        public static string IconPath = Path.Combine("icons", "ForgetPotions");
        public const string PotionNameFormat = "Blackout - {SchoolName}";
        public const string PotionDescFormat = "<b><color=#d3aa30ff>{SchoolName}</color></b>\nA potent concoction known to cause extreme blackouts. Consuming this potion will cause you to forget all of your training for this school.";
        public const string PotionDuplicateFormat = "<b><color=#ff4500>Custom Class '{SchoolName}' has been added multiple times. It is unlikely the '{SchoolName}' mod author did this intentionally. Drinking this potion may have unintended consequences, such as forgetting skills from other schools.</color></b>";

        /// <summary>
        /// Dictionary collection of Icon File Names, indexed by School Name.
        /// Dictionary string = SkillSchool.name,
        ///            string = IconFileName
        /// </summary>
        public readonly static Dictionary<string, string> CustomSchoolIcons = new Dictionary<string, string>()
        {
            {  "Default", "Default.png" },
            {  "AbrassarMercenary", "Mercenary.png" },
            {  "AbrassarRogue", "Rogue Engineer.png" },
            {  "ChersoneseEto", "Kazite Spellblade.png" },
            {  "ChersoneseHermit", "Cabal Hermit.png" },
            {  "EmmerkarSage", "Rune Sage.png" },
            {  "EmmerkarHunter", "Wild Hunter.png" },
            {  "HallowedMarshPhilosopher", "Philosopher.png" },
            {  "HallowedMarshWarriorMonk", "Warrior Monk.png" },
            {  "HarmattanSpeedster", "The Speedster.png" },
            {  "HarmattanHexMage", "Hex Mage.png" },
            {  "CalderaThePrimalRitualist", "Primal Ritualist.png" },
            {  "CalderaWeaponMaster", "Weapon Master.png" },
            {  "CalderaSpecialist", "Specialist.png" }
        };
    }
}
