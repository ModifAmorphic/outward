using System;

namespace ModifAmorphic.Outward.StashPacks.Settings
{
    public class StashPackHostSettings
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

        public static StashPackHostSettings Deserialize(object[] data)
        {
            return new StashPackHostSettings()
            {
                AllScenesEnabled = Convert.ToBoolean((byte)data[0]),
                DisableBagScalingRotation = Convert.ToBoolean((byte)data[1])
            };

        }

        public static object[] Serialize(StashPackHostSettings hostSettings)
        {
            return new object[] {
                Convert.ToByte(hostSettings.AllScenesEnabled),
                Convert.ToByte(hostSettings.DisableBagScalingRotation)
            };
        }

        
    }
}
