using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Merchants
{
    public class MerchantModule : IModifModule
    {
        private readonly Func<IModifLogger> _loggerFactory;
        private IModifLogger Logger => _loggerFactory.Invoke();

        //scene, <merchantPath, CustomDropTables>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CustomDropTables>> _customDropTables =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, CustomDropTables>>();

        public HashSet<Type> PatchDependencies => new HashSet<Type>() {
            typeof(MerchantPatches)
        };

        public HashSet<Type> EventSubscriptions => new HashSet<Type>() {
            typeof(MerchantPatches)
        };

        public HashSet<Type> DepsWithMultiLogger => new HashSet<Type>() {
            typeof(MerchantPatches)
        };

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal MerchantModule(Func<IModifLogger> loggerFactory)
        {
            this._loggerFactory = loggerFactory;
            MerchantPatches.InitDropTableGameObjectAfter += AppendCustomDrops;
        }

        private void AppendCustomDrops((Merchant Merchant, UnityEngine.Transform MerchantInventoryTablePrefab, Dropable DropableInventory) args)
        {
#if DEBUG
            Logger.LogDebug($"{nameof(MerchantModule)}::{nameof(AppendCustomDrops)}:" +
                $" Looking for custom drop tables for shop '{args.Merchant.ShopName}'" +
                $"\n\tScene: {AreaManager.Instance.CurrentArea.SceneName}" +
                $"\n\tPath: {args.Merchant.gameObject.GetPath()}");
#endif
            if (!_customDropTables.TryGetValue(AreaManager.Instance.CurrentArea.SceneName, out var merchantTables))
                return;

            if (!merchantTables.TryGetValue(args.Merchant.gameObject.GetPath(), out var customDrops))
                return;

            //var gDrops = args.DropableInventory.GetComponentsInChildren<GuaranteedDrop>();
            //var guaranteedDrop = args.DropableInventory.gameObject.AddComponent<GuaranteedDrop>();
            foreach(var d in customDrops.GuaranteedDrops)
            {
                var guaranteedDrop = args.DropableInventory.gameObject.AddComponent<GuaranteedDrop>();
                guaranteedDrop.ItemGenatorName = d.ItemGenatorName;
                //guaranteedDrop.name = args.DropableInventory.gameObject.name;
                guaranteedDrop.SetItemDrops(d.CustomGuaranteedDrops.ToItemDrops());
                //var guaranteedDrops = args.DropableInventory.GetAllGuaranteedDrops();
                //guaranteedDrops.Add(guaranteedDrop);
                Logger.LogDebug($"{nameof(MerchantModule)}::{nameof(AppendCustomDrops)}:" +
                $" Added {d.CustomGuaranteedDrops.Count} guaranteed custom drops to merchant shop '{args.Merchant.ShopName}'.");
            }
        }

        public void AddGuaranteedDrops(string sceneName, string merchantPath, IEnumerable<CustomGuaranteedDrop> guaranteedDrops, string itemGenName, string gameObjectName = "InventoryTable")
        {
            var merchantTables = _customDropTables.GetOrAdd(sceneName, new ConcurrentDictionary<string, CustomDropTables>());
            var dropTables = merchantTables.GetOrAdd(merchantPath, new CustomDropTables());

            dropTables.GuaranteedDrops.Add(new GuaranteedDropTable()
            {
                CustomGuaranteedDrops = guaranteedDrops.ToList(),
                ItemGenatorName = itemGenName,
                GameObjectName = gameObjectName
            });
        }
        
    }
}
