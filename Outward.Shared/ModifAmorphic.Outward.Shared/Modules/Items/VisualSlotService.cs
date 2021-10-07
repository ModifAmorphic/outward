//using ModifAmorphic.Outward.Logging;
//using ModifAmorphic.Outward.Modules.Items.Models;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;

//namespace ModifAmorphic.Outward.Modules.Items
//{
//    internal class VisualSlotService
//    {
//        private readonly Func<IModifLogger> _loggerFactory;
//        private IModifLogger Logger => _loggerFactory.Invoke();
//        private readonly Func<ResourcesPrefabManager> _prefabManagerFactory;
//        private ResourcesPrefabManager PrefabManager => _prefabManagerFactory.Invoke();
//        private readonly Func<ItemManager> _itemManagerFactory;
//        private ItemManager ItemManager => _itemManagerFactory.Invoke();

//        internal VisualSlotService(Func<ResourcesPrefabManager> prefabManagerFactory, Func<ItemManager> itemManagerFactory, Func<IModifLogger> loggerFactory)
//        {
//            _loggerFactory = loggerFactory;
//            _prefabManagerFactory = prefabManagerFactory;
//            _itemManagerFactory = itemManagerFactory;

//            //ItemManagerPatches.GetVisualsByItemOverride += GetVisualsByItemOverride;
//            //ItemManagerPatches.GetSpecialVisualsByItemOverride += GetSpecialVisualsByItemOverride;
//            VisualSlotPatches.PositionVisualsOverride += PositionVisualsOverride;
//        }

//        private bool PositionVisualsOverride(VisualSlot visualSlot, Item input, out Item output)
//        {
//			var vs = VisualSlotWrapper.Wrap(visualSlot);

//			if (!Application.isPlaying)
//			{
//				vs.m_currentItem = input;
//			}
//			else
//			{
//				if (vs.m_editorCurrentVisuals != null)
//				{
//					vs.m_currentVisual = vs.m_editorCurrentVisuals;
//				}
//				if (input == vs.m_currentItem)
//				{
//					if (!input.HasSpecialVisualPrefab)
//					{
//						input.LinkVisuals(vs.m_currentVisual, _setParent: false);
//					}
//					vs.PositionVisuals();
//					return;
//				}
//				if (vs.m_currentItem != null)
//				{
//					visualSlot.PutBackVisuals();
//				}
//				vs.m_currentItem = input;
//			}
//			if (vs.m_currentVisual == null)
//			{
//				if (!input.HasSpecialVisualPrefab)
//				{
//					vs.m_currentVisual = input.LoadedVisual;
//					vs.m_editorCurrentVisuals = vs.m_currentVisual;
//				}
//				else
//				{
//					vs.m_currentVisual = ItemManager.GetSpecialVisuals(input);
//					vs.m_editorCurrentVisuals = vs.m_currentVisual;
//					if ((bool)vs.m_currentVisual)
//					{
//						vs.m_currentVisual.Show();
//					}
//					if (input is Equipment)
//					{
//						(input as Equipment).SetSpecialVisuals(vs.m_currentVisual);
//					}
//				}
//			}
//			if (Application.isPlaying)
//			{
//				vs.m_editorCurrentVisuals = vs.m_currentVisual;
//			}
//			else if (vs.m_currentItem != null)
//			{
//				vs.m_editorCurrentVisuals = vs.m_currentVisual;
//			}
//			vs.PositionVisuals();
//		}
//    }
//}
