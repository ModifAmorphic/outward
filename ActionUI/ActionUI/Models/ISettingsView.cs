namespace ModifAmorphic.Outward.Unity.ActionUI
{
    internal interface ISettingsView
    {
        bool IsShowing { get; }
        void Show();
        void Hide();
    }
}
