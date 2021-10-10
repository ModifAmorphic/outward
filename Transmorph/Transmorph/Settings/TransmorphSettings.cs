using System;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    public class TransmorphSettings
    {
        public bool AllScenesEnabled { get; set; }

        public event Action<bool> DisableBagScalingRotationChanged;

        private bool _disableBagScalingRotation;
        public bool DisableBagScalingRotation
        {
            get => _disableBagScalingRotation;
            set
            {
                _disableBagScalingRotation = value;
                DisableBagScalingRotationChanged?.Invoke(value);
            }
        }        
    }
}
