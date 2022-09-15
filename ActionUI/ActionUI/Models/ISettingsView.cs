namespace ModifAmorphic.Outward.Unity.ActionMenus
{
    internal interface ISettingsView
    {
        bool IsShowing { get; }
        void Show();
        void Hide();
    }
}
