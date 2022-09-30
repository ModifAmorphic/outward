namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public interface ICooldown
    {
        bool HasCooldown { get; }
        bool GetIsInCooldown();
        float GetProgress();
        float GetSecondsRemaining();
    }
}
