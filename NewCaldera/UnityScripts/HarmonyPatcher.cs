using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts
{
    internal class HarmonyPatcher
    {
        private readonly Harmony _harmony;
        public Harmony Harmony => _harmony;

        private static HarmonyPatcher _instance;
        public static HarmonyPatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HarmonyPatcher();
                }
                return _instance;
            }
        }
        
        public HarmonyPatcher()
        {
            _harmony = new Harmony(ModInfo.ModId);
        }

    }
}
