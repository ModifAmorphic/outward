using UnityEngine.Events;

namespace ModifAmorphic.Outward.Unity.ActionUI.Data
{
    public interface IPositionsProfileService
    {
        PositionsProfile GetProfile();
        UnityEvent<PositionsProfile> OnProfileChanged { get; }
        void Save();
        void SaveNew(PositionsProfile positionsProfile);
        void AddOrUpdate(UIPositions position);
        void Remove(UIPositions position);
    }
}
