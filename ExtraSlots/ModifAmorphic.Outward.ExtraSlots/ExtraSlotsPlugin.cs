using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;

namespace ModifAmorphic.Outward.ExtraSlots
{
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("modifamorphic.outward.shared", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, ModVersion)]
    public class ExtraSlotsPlugin : BaseUnityPlugin
    {
        public const string ModId = "modifamorphic.outward.extraquickslots";
        public const string ModName = "Extra QuickSlots";
        public const string ModVersion = "0.2.0";
        //string modVersion = AssemblyName.GetAssemblyName(System.Reflection.Assembly.GetExecutingAssembly().Location).Version.ToString();

        internal void Awake()
        {
            var harmony = new Harmony(ModId);
            try
            {
                UnityEngine.Debug.Log($"[{ModName}] - Starting...");
                harmony.PatchAll();

                var extraSlots = new ExtraSlots();
                extraSlots.Enable(this);
            }
            catch
            {
                UnityEngine.Debug.LogError($"Failed to enable {ModId} {ModName}.");
                harmony.UnpatchSelf();
                throw;
            }
        }
    }
}
