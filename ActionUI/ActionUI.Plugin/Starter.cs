﻿using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.ActionUI
{

    internal static class Starter
    {
        private static IModifLogger Logger => LoggerFactory.GetLogger(ModInfo.ModId);

        public static bool TryStart(IStartable startable)
        {
            try
            {
                Logger.LogDebug($"Starting {startable.GetType().Name}.");
                startable.Start();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException($"Failed to start {startable.GetType().Name}.", ex);
            }
            return false;
        }
    }
}
