using ModifAmorphic.Outward.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Transmorph.Menu
{
    internal class TransmorphMenuTab : UIMenuTab
    {

        protected void Awake()
        {
            var charUI = gameObject.FindInParents<CharacterUI>();
            LoggerFactory.GetLogger(ModInfo.ModId).LogDebug($"TransmorphMenuTab::Awake(): CharacterUI: {charUI?.name}, UI TargetCharacter: {charUI?.TargetCharacter?.Name}");
        }
        protected override void StartInit()
        {
            base.StartInit();
            var charUI = gameObject.FindInParents<CharacterUI>();
            LoggerFactory.GetLogger(ModInfo.ModId).LogDebug($"TransmorphMenuTab::StartInit(): CharacterUI: {charUI?.name}, UI TargetCharacter: {charUI?.TargetCharacter?.Name}");
        }
        protected new void Start()
        {
            base.Start();
            var charUI = gameObject.FindInParents<CharacterUI>();
            LoggerFactory.GetLogger(ModInfo.ModId).LogDebug($"TransmorphMenuTab::Start(): CharacterUI: {charUI?.name}, UI TargetCharacter: {charUI?.TargetCharacter?.Name}");
        }
    }
}
