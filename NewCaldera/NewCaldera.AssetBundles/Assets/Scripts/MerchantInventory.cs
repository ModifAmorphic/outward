using Assets.Scripts.Models;
using ModifAmorphic.Outward.UnityScripts;
using ModifAmorphic.Outward.UnityScripts.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class MerchantInventory : MonoBehaviour
{

    [JsonProperty]
    public AreaEnum Area;

    [JsonProperty]
    public NpcSearchTypes NpcSearchType;

    [Tooltip("Value of the Merchant monobehaviour's m_uid field.\n\tField: Merchant.m_uid")]
    [JsonProperty]
    public List<string> MerchantUIDs;

    [Tooltip("Value of the Merchant monobehaviour's ShopName property.\n\tProperty: Merchant.ShopName")]
    [JsonProperty]
    public List<string> ShopNames = new List<string>();

    [Tooltip("The name of the gameobject with the SPNC component attached.\n\tField: SNPC.name")]
    [JsonProperty]
    public List<string> ShopNpcNames = new List<string>();

    public List<ItemTable> SellableItems = new List<ItemTable>();
    [System.NonSerialized]
    [JsonProperty]
    public List<ItemAmounts> GuaranteedItems = new List<ItemAmounts>();

    public AdditonalMerchantInventory ToAdditonalMerchantInventory() =>
        new AdditonalMerchantInventory()
        {
            Area = Area,
            NpcSearchType = NpcSearchType,
            MerchantUIDs = MerchantUIDs,
            ShopNames = ShopNames,
            ShopNpcNames = ShopNpcNames,
            GuaranteedItems = GuaranteedItems
        };
    

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (SellableItems == null && !SellableItems.Any())
        {
            GuaranteedItems.Clear();
            return;
        }

        var sellItems = SellableItems.Where(x => x != null && x.Item != null);

        if (!sellItems.Any())
        {
            GuaranteedItems.Clear();
            return;
        }
        GuaranteedItems.Clear();
        GuaranteedItems
            .AddRange(sellItems.Select(s => new ItemAmounts() { ItemID = s.Item.ItemID, MinQuantity = s.Quantity, MaxQuantity = s.Quantity }));
    }

#endif
}
