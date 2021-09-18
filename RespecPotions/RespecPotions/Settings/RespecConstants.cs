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
        public const string PotionNameFormat = "Forget {SchoolName}";
        public const string PotionDescFormat = "Consuming this potion will cause you to forget all of your {SchoolName} training.";

        /// <summary>
        /// Dictionary collection of Icon File Names, indexed by School Name.
        /// Dictionary string = SchoolName,
        ///            string = IconFileName
        /// </summary>
        public readonly static Dictionary<string, string> CustomSchoolIcons = new Dictionary<string, string>()
        {
            {  "Default", "Default.png" },
            {  "Mercenary", "Mercenary.png" },
            {  "Rogue Engineer", "Rogue Engineer.png" },
            {  "Kazite Spellblade", "Kazite Spellblade.png" },
            {  "Cabal Hermit", "Cabal Hermit.png" },
            {  "Rune Sage", "Rune Sage.png" },
            {  "Wild Hunter", "Wild Hunter.png" },
            {  "Philosopher", "Philosopher.png" },
            {  "Warrior Monk", "Warrior Monk.png" },
            //{  "The Speedster", "The Speedster.png" },
            //{  "Hex Mage", "Hex Mage.png" },
            //{  "Primal Ritualist", "Primal Ritualist.png" },
            //{  "Weapon Master", "Weapon Master.png" },
            //{  "Specialist", "Specialist.png" }
        };
    }
}
