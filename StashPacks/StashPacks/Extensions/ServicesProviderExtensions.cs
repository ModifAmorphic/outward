using ModifAmorphic.Outward.Logging;
using ModifAmorphic.Outward.StashPacks.Network;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.StashPacks.Extensions
{
    public static class ServicesProviderExtensions
    {
        public static ServicesProvider ConfigureStashPackNet(this ServicesProvider services)
        {
            var gameObject = new GameObject(nameof(StashPackNet))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var stashPackNet = gameObject.AddComponent<StashPackNet>();
            stashPackNet.LoggerFactory = services.GetService<IModifLogger>;

            return services.AddSingleton(stashPackNet);
        }
    }
}
