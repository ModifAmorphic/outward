using BepInEx;
using ModifAmorphic.Outward.Coroutines;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Modules.Items;
using ModifAmorphic.Outward.Transmorphic.Extensions;
using ModifAmorphic.Outward.Transmorphic.Patches;
using ModifAmorphic.Outward.Transmorphic.Settings;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Transmog
{
    class TransmogItemListener
    {
        private readonly TransmogSettings _settings;

        private IModifLogger Logger => _getLogger.Invoke();
        private readonly Func<IModifLogger> _getLogger;

        private readonly ItemVisualizer _itemVisualizer;
        private readonly Func<ItemManager> _itemManagerFactory;
        private ItemManager ItemManager => _itemManagerFactory.Invoke();

        private readonly LevelCoroutines _levelCoroutines;

        public TransmogItemListener(ItemVisualizer itemVisualizer, LevelCoroutines levelCoroutines, Func<ItemManager> itemManagerFactory, TransmogSettings settings, Func<IModifLogger> getLogger)
        {
            (_itemVisualizer, _levelCoroutines, _itemManagerFactory, _settings, _getLogger) =
                (itemVisualizer, levelCoroutines, itemManagerFactory, settings, getLogger);

            TmogItemManagerPatches.TransmogItemAdded += TryAddVisualization;
            TmogItemManagerPatches.TransmogItemAdded += TryRegisterIcon;

        }

        private void TryAddVisualization(Item item)
        {
            if (_itemVisualizer.IsItemVisualRegistered(item.UID))
                return;

            if (item.HolderUID.TryGetVisualItemID(out var visualItemID))
            {
                _itemVisualizer.RegisterItemVisual(visualItemID, item.UID);
                //return;
                //_visualizations.Enqueue((item.UID, visualItemID));
                //if (!coroutinesStarted)
                //    StartCoroutines();
                
            }
        }
        private void TryRegisterIcon(Item item)
        {
            if (!_itemVisualizer.IsAdditionalIconRegistered(item.UID, TransmogSettings.ItemIconName))
                _itemVisualizer.RegisterAdditionalIcon(item.UID, TransmogSettings.ItemIconName, TransmogSettings.ItemIconImageFilePath);
        }
#if DEBUG
        private readonly ConcurrentQueue<(string, int)> _visualizations = new ConcurrentQueue<(string, int)>();
        private bool coroutinesStarted = false;
        
        private void StartCoroutines()
        {
            coroutinesStarted = true;

            _levelCoroutines.InvokeAfterLevelAndPlayersLoaded(
                NetworkLevelLoader.Instance,
                StartProcessingVisualsQueue,
                300, 1f);
        }
        private void StartProcessingVisualsQueue()
        {
            _levelCoroutines.StartRoutine(
                _levelCoroutines.InvokeUntil(
                    () => !ItemManager.IsAllItemSynced, 
                    ProcessVisualsQueue, 
                    3, 
                    () => coroutinesStarted = false)
                );
        }
        private void ProcessVisualsQueue()
        {
            while (_visualizations.TryDequeue(out (string uid, int visualID) r))
            {
                _itemVisualizer.RegisterItemVisual(r.visualID, r.uid);
            }
        }
#endif
    }
}
