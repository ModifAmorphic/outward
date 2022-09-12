#if DEBUG
using HarmonyLib;
using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorphic.Patches
{
	[HarmonyPatch(typeof(ItemManager))]
	internal static class ItemManagerPatches
	{
		private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

		public static event Action BeforeResourcesDoneLoading;

#pragma warning disable IDE0051 // Remove unused private members
		[HarmonyPatch(nameof(ItemManager.ResourcesDoneLoading))]
		[HarmonyPrefix]
		private static void ResourcesDoneLoadingPrefix()
		{
			try
			{

				Logger.LogDebug($"{nameof(ItemManagerPatches)}::{nameof(ResourcesDoneLoadingPrefix)}:.");
				BeforeResourcesDoneLoading?.Invoke();
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(ResourcesDoneLoadingPrefix)}(): Exception.", ex);
			}
		}

        public static event Action<Item> AfterRequestItemInitialization;

        [HarmonyPatch(nameof(ItemManager.RequestItemInitialization))]
        [HarmonyPostfix]
        private static void RequestItemInitializationPostfix(Item _item)
        {
            try
            {
                Logger.LogTrace($"{nameof(ItemManagerPatches)}::{nameof(RequestItemInitializationPostfix)}: Invoking {nameof(AfterRequestItemInitialization)}.");
                AfterRequestItemInitialization?.Invoke(_item);
            }
            catch (Exception ex)
            {
                Logger.LogException($"{nameof(ItemManagerPatches)}::{nameof(RequestItemInitializationPostfix)}(): Exception invoking {nameof(AfterRequestItemInitialization)}.", ex);
            }
        }

		public static event Action<ItemManager, bool> AfterIsAllItemSynced;

#pragma warning disable IDE0051 // Remove unused private members
		[HarmonyPatch(nameof(ItemManager.IsAllItemSynced), MethodType.Getter)]
		[HarmonyPostfix]
		private static void IsAllItemSyncedPostfix(ItemManager __instance, bool __result)
		{
			try
			{
				if (!__result)
					return;

				Logger.LogDebug($"{nameof(EnchantItemManagerPatches)}::{nameof(IsAllItemSyncedPostfix)}: Invoking {nameof(AfterIsAllItemSynced)}().");
				AfterIsAllItemSynced?.Invoke(__instance, __result);
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(EnchantItemManagerPatches)}::{nameof(IsAllItemSyncedPostfix)}(): Exception Invoking {nameof(AfterIsAllItemSynced)}().", ex);
			}
		}
	}

	[HarmonyPatch(typeof(Item))]
	internal static class ItemPatches
	{
		private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

		//public static event Action<ItemManager, bool> AfterIsAllItemSynced;

#pragma warning disable IDE0051 // Remove unused private members
		[HarmonyPatch("OnAwake")]
		[HarmonyPrefix]
		private static void OnAwakePrefix(Item __instance)
		{
			try
			{

				Logger.LogDebug($"{nameof(ItemPatches)}::{nameof(OnAwakePrefix)}: {__instance.name} ({__instance.UID}) Awaking.");
				//AfterIsAllItemSynced?.Invoke(__instance, __result);
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(ItemPatches)}::{nameof(OnAwakePrefix)}(): Exception.", ex);
			}
		}
	}

	[HarmonyPatch(typeof(ResourcesPrefabManager))]
	static class ResourcesPrefabManagerPatches
	{
		private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

		

		//[HarmonyPatch("GenerateItem", MethodType.Normal)]
		//[HarmonyPrefix]
		//static void GenerateItemPrefix(string _itemIDString)
		//{
		//	try
		//	{
		//		Logger.LogDebug($"{nameof(ResourcesPrefabManagerPatches)}:{nameof(GenerateItemPrefix)}:: _itemIDString == {_itemIDString}");
		//	}
		//	catch (Exception ex)
		//	{
		//		Logger.LogException($"{nameof(ResourcesPrefabManagerPatches)}:{nameof(GenerateItemPrefix)}:: Exception _itemIDString == {_itemIDString}.", ex);
		//	}
		//}

		public static event Action AfterLoad;

		[HarmonyPatch("Load", MethodType.Normal)]
		[HarmonyPostfix]
		static void LoadPostfix()
		{
			try
			{
				Logger.LogDebug($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(LoadPostfix)}: Invoking {nameof(AfterLoad)}");
				AfterLoad?.Invoke();
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(ResourcesPrefabManagerPatches)}::{nameof(LoadPostfix)}: Exception Invoking {nameof(AfterLoad)}.", ex);
			}
		}
	}

	[HarmonyPatch(typeof(ItemDetailsDisplay))]
	static class ItemDetailsDisplayPatches
    {
		private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);
		[HarmonyPatch("RefreshDetail", MethodType.Normal)]
		[HarmonyPrefix]
		static void RefreshDetailPrefix(int _rowIndex, ItemDetailsDisplay.DisplayedInfos _infoType)
		{
			try
			{
				Logger.LogDebug($"{nameof(ItemDetailsDisplayPatches)}:{nameof(RefreshDetailPrefix)}:: _rowIndex == {_rowIndex}, _infoType == {_infoType}");
			}
			catch (Exception ex)
			{
				Logger.LogException($"{nameof(ItemDetailsDisplayPatches)}:{nameof(RefreshDetailPrefix)}:: Exception.", ex);
			}
		}
	}
	[HarmonyPatch(typeof(Weapon))]
	static class WeaponPatches
    {
		private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);
		[HarmonyPatch("RefreshEnchantmentModifiers", MethodType.Normal)]
		[HarmonyPrefix]
		static bool RefreshEnchantmentModifiersPrefix(Weapon __instance)
		{

			try
            {
				RefreshEnchantmentModifiers(__instance);
				return true;
			}
			catch (Exception ex)
            {
				Logger.LogException("RefreshEnchantmentModifiersPrefix exception.", ex);
            }
			return false;
		}
		static void RefreshEnchantmentModifiers(Weapon __instance)
		{

			__instance.InvokePrivateMethod("RefreshEnchantmentModifiers");
			__instance.SetPrivateField("AttackSpeedModifier", 1f);
			__instance.SetPrivateField("m_attackSpeedBonus", 0f);
			__instance.SetPrivateField("m_impactBonus", 0f);
			__instance.SetPrivateField("m_enchantmentHealthAbsorbRatio", 0f);
			__instance.SetPrivateField("m_staminaAbsorbRatio", 0f);
			__instance.SetPrivateField("m_manaAbsorbRatio", 0f);

			var m_enchantmentDamageBonus = __instance.GetPrivateField<Weapon, DamageList>("m_enchantmentDamageBonus");
			if (m_enchantmentDamageBonus != null)
			{
				m_enchantmentDamageBonus.Clear();
			}
			else
			{
				m_enchantmentDamageBonus = new DamageList();
			}
			var m_activeEnchantments = __instance.GetPrivateField<Weapon, List<Enchantment>>("m_activeEnchantments");
			var m_baseDamage = __instance.GetPrivateField<Weapon, DamageList>("m_baseDamage");
			for (int i = 0; i < m_activeEnchantments.Count; i++)
			{
				__instance.SetPrivateField("AttackSpeedModifier", 1f + m_activeEnchantments[i].StatModifications.GetModifierValue(Enchantment.Stat.AttackSpeed) * 0.01f);
				__instance.SetPrivateField("m_attackSpeedBonus", m_activeEnchantments[i].StatModifications.GetBonusValue(Enchantment.Stat.AttackSpeed));
				__instance.SetPrivateField("m_impactBonus", m_activeEnchantments[i].StatModifications.GetBonusValue(Enchantment.Stat.Impact));
				Enchantment.AdditionalDamage[] additionalDamages = m_activeEnchantments[i].AdditionalDamages;
				for (int j = 0; j < additionalDamages.Length; j++)
				{
					Enchantment.AdditionalDamage additionalDamage = additionalDamages[j];
					if (m_baseDamage[additionalDamage.SourceDamageType] != null)
					{
						float damage = m_baseDamage[additionalDamage.SourceDamageType].Damage * additionalDamage.ConversionRatio;
						m_enchantmentDamageBonus.Add(new DamageType(additionalDamage.BonusDamageType, damage));
					}
				}
				foreach (DamageType item in m_activeEnchantments[i].DamageBonus.List)
				{
					m_enchantmentDamageBonus.Add(item);
				}
				__instance.SetPrivateField("m_enchantmentHealthAbsorbRatio", m_activeEnchantments[i].HealthAbsorbRatio);
				__instance.SetPrivateField("m_staminaAbsorbRatio", m_activeEnchantments[i].StaminaAbsorbRatio);
				__instance.SetPrivateField("m_manaAbsorbRatio", m_activeEnchantments[i].ManaAbsorbRatio);
			}
			var m_activeBaseDamage = __instance.GetPrivateField<Weapon, DamageList>("m_activeBaseDamage");
			m_activeBaseDamage.Clear();
			m_activeBaseDamage.Add(m_baseDamage);
			m_activeBaseDamage.Add(m_enchantmentDamageBonus);
			m_activeBaseDamage.IgnoreHalfResistances = __instance.IgnoreHalfResistances;
		}
	}
	[HarmonyPatch(typeof(EffectSynchronizer))]
	static class EffectSynchronizerPatches
	{
		private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

		[HarmonyPatch(nameof(EffectSynchronizer.RegisterEffect), MethodType.Normal)]
		[HarmonyPrefix]
		[HarmonyPatch(new Type[] { typeof(Effect), typeof(EffectActivationStack) })]
		public static bool RegisterEffect(EffectSynchronizer __instance, Effect _effect, EffectActivationStack _idStack, ref EffectActivationStack __result)
		{
			if (_effect == null || ModifEffectSynchronizer.IsEffectAlreadyRegistered(__instance, _effect))
			{
				__result = null;
				return true;
			}
			int num = -1;
			if (_effect.OverrideEffectCategory != 0)
			{
				num = (int)_effect.OverrideEffectCategory;
			}
			if (num == -1)
			{
				if (_effect.name.Contains("Normal") || _effect.name == "Effects" || _effect.name == "Effect" || _effect.name == "ExtraEffects" || _effect.name == "HiddenEffects")
				{
					num = 1;
				}
				else if (_effect.name.Contains("Activation") || _effect.name.Contains("Passive"))
				{
					num = 2;
				}
				else if (_effect.name.Contains("Restauration") || _effect.name.Contains("Restoration"))
				{
					num = 3;
				}
				else if (_effect.name.Contains("Hit"))
				{
					num = 4;
				}
				else if (_effect.name.Contains("Referenced"))
				{
					num = 5;
				}
				else if (_effect.name.Contains("Block"))
				{
					num = 6;
				}
			}
			if (num != -1)
			{
				__result = ModifEffectSynchronizer.ProcessEffectReference(__instance, _effect, _idStack, num);
				return true;
			}
			
			__result = null;
			return true;
		}
	}
    internal class ModifEffectSynchronizer : EffectSynchronizer
    {
		private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);
		public static bool IsEffectAlreadyRegistered(EffectSynchronizer effectSynchronizer, Effect _effect)
		{
			var m_effects = effectSynchronizer.GetPrivateField<EffectSynchronizer, Dictionary<string, EffectReference>>("m_effects");
			EffectReference value = null;
			if (!string.IsNullOrEmpty(_effect.SourceID) && m_effects.TryGetValue(_effect.SourceID, out value) && value.EffectSourceType != EffectSourceTypes.Reference)
			{
				if (_effect.gameObject.GetComponents<Effect>().Length == 1)
				{
					UnityEngine.Object.Destroy(_effect.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(_effect);
				}
			}
			return value != null;
		}
		public static EffectActivationStack ProcessEffectReference(EffectSynchronizer effectSynchronizer, Effect _effect, EffectActivationStack _idStack, int num)
        {
			Logger.LogDebug("ProcessEffectReference: 01");
			EffectReference effectReference = new EffectReference(_effect);
			effectReference.EffectSourceType = ((!string.IsNullOrEmpty(_idStack.BaseSource) && !(_idStack.BaseSource == effectSynchronizer.GetSourceID())) ? EffectSourceTypes.Copy : EffectSourceTypes.Original);
			Logger.LogDebug("ProcessEffectReference: 02");
			if (effectReference.EffectSourceType == EffectSourceTypes.Original)
			{
				Logger.LogDebug("ProcessEffectReference: 03");
				_idStack.BaseSource = effectSynchronizer.GetSourceID();
				_idStack.RootItemUID = effectSynchronizer.GetSourceUID();
			}
			Logger.LogDebug("ProcessEffectReference: 04");
			var m_categoryEffects = effectSynchronizer.GetPrivateField<EffectSynchronizer, List<string>[]>("m_categoryEffects");
			Logger.LogDebug("ProcessEffectReference: 05");
			int count = m_categoryEffects[num].Count;
			Logger.LogDebug("ProcessEffectReference: 06");
			_idStack.Push(num, count);
			Logger.LogDebug("ProcessEffectReference: 07");

			effectSynchronizer.InvokePrivateMethod("AddEffect", num, _idStack.ToSourceID(), effectReference);
			Logger.LogDebug("ProcessEffectReference: 08");
			if ((bool)effectSynchronizer.OwnerCharacter)
			{
				Logger.LogDebug("ProcessEffectReference: 09");
				effectSynchronizer.ProcessEffect(_effect);
			}
			Logger.LogDebug("ProcessEffectReference: 10");
			return _idStack;
		}
		public override string GetSourceID()
        {
            throw new NotImplementedException();
        }

        public override string GetSourceUID()
        {
            throw new NotImplementedException();
        }

        protected override void SendStopEffects(Character _affectedCharacter, string _stoppedEffects)
        {
            throw new NotImplementedException();
        }

        protected override void SendSyncEffects(Character _affectedCharacter, string _infos)
        {
            throw new NotImplementedException();
        }
    }
}
#endif