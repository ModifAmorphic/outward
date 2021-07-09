using ModifAmorphic.Outward.Extensions;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.KeyBindings
{
    internal class MoreQuickslotsLocalizationListener : ILocalizeListener
    {
        private readonly Dictionary<string, string> qsLocalizations;
        private readonly Logging.Logger logger;
        /// <summary>
        /// Listener that adds quick slot descriptions to the localization array that are later used when displaying the keybindings in the setup menu.
        /// </summary>
        public MoreQuickslotsLocalizationListener(Dictionary<string, string> qsLocalizations, Logging.Logger logger)
        {
            this.logger = logger;
            this.qsLocalizations = qsLocalizations;
        }
        public void Localize()
        {
            logger.LogInfo($"{nameof(MoreQuickslotsLocalizationListener)} Adding {qsLocalizations.Count} new localizations for quickslots.");
            var localizations = LocalizationManager.Instance.GetGeneralLocalizations();
            StringBuilder sb = new StringBuilder();
            foreach (var qs in qsLocalizations)
            {
                localizations.Add(qs.Key, qs.Value);
                sb.AppendLine($"\tname: {qs.Key}, desc: {qs.Value}");
            }
            logger.LogDebug($"{nameof(MoreQuickslotsLocalizationListener)} Localizations Added:\n{sb}.");
            LocalizationManager.Instance.SetGeneralLocalizations(localizations);
        }
    }
}
