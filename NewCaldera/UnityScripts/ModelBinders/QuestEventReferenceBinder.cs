using System;
using System.Collections.Generic;
using System.Text;

namespace ModifAmorphic.Outward.UnityScripts
{
    [Serializable]
    public class QuestEventReferenceBinder : IBoundTypeConverter
    {
        public string UID;
        public string m_eventUID;

        public static Type GetBindingType() => OutwardAssembly.Types.QuestEventReference;
        public Type GetBoundType() => GetBindingType();

        public object ToBoundType()
        {
            var type = GetBoundType();
            var questEventReference = Activator.CreateInstance(type);

            questEventReference.SetField(type, "UID", UID);
            questEventReference.SetField(type, "m_eventUID", m_eventUID);

            return questEventReference;
        }
    }
}
