namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    public interface ICooldown
    {
        bool HasCooldown { get; }
        bool GetIsInCooldown();
        float GetProgress();
        float GetSecondsRemaining();
    }
}
