using System;

namespace ModifAmorphic.Outward.Unity.ActionUI
{
    public class MenuNavigationActions
    {
        public Func<bool> MoveUp { get; set; }
        public Func<bool> MoveDown { get; set; }
        public Func<bool> MoveLeft { get; set; }
        public Func<bool> MoveRight { get; set; }
        public Func<bool> Cancel { get; set; }
    }
}
