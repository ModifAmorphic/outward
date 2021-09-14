using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.Modules.Save
{
    public interface IModifSave<T>
    {
        string SaveName { get; }

        string Serialize();
        T Deserialize();
    }
}
