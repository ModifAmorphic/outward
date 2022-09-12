using HarmonyLib;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Items.Patches
{
	[HarmonyPatch(typeof(ResourcesPrefabManager))]
	internal static class ResourcesPrefabManagerPatches
	{
		[MultiLogger]
		private static IModifLogger Logger { get; set; } = new NullLogger();

		public delegate bool TryGetItemPrefabDelegate(int itemID, out Item itemPrefab);
		public static List<TryGetItemPrefabDelegate> TryGetItemPrefabActions = new List<TryGetItemPrefabDelegate>();

		[HarmonyPatch(nameof(ResourcesPrefabManager.GetItemPrefab), MethodType.Normal)]
		[HarmonyPostfix]
		[HarmonyPatch(new Type[] { typeof(int) })]
		static void GetItemPrefabByInt(int _itemID, ref Item __result)
		{
			try
			{
				if (__result != null)
					return;

				Logger.LogTrace($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(GetItemPrefabByInt)}: Invoking {nameof(TryGetItemPrefabActions)}");
				GetItemPrefab(_itemID, ref __result);
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(GetItemPrefabByInt)}: Exception Invoking {nameof(TryGetItemPrefabActions)}.", ex);
			}
		}

		[HarmonyPatch(nameof(ResourcesPrefabManager.GetItemPrefab), MethodType.Normal)]
		[HarmonyPostfix]
		[HarmonyPatch(new Type[] { typeof(string) })]
		static void GetItemPrefabByString(string _itemIDString, ref Item __result)
		{
			try
			{
				if (__result != null || !int.TryParse(_itemIDString, out var itemID))
					return;

				Logger.LogTrace($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(GetItemPrefabByString)}: Invoking {nameof(TryGetItemPrefabActions)}");
				GetItemPrefab(itemID, ref __result);
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(GetItemPrefabByString)}: Exception Invoking {nameof(TryGetItemPrefabActions)}.", ex);
			}
		}

		private static void GetItemPrefab(int itemID, ref Item item)
        {
			for (int i = 0; i < TryGetItemPrefabActions.Count; i++)
			{
				if (TryGetItemPrefabActions[0].Invoke(itemID, out var prefab))
				{
					item = prefab;
					break;
				}
			}
		}
	}
}
