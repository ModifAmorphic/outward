using ModifAmorphic.Outward.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModifAmorphic.Outward.Internal
{
    static class ReflectUtil
    {
        static readonly SafeDictionary<Type, SafeDictionary<string, FieldInfo>> _fieldCache = new SafeDictionary<Type, SafeDictionary<string, FieldInfo>>();
        public static TField GetReflectedPrivateField<TField, TOwner>(string propertyName, TOwner propertyOwner)
        {
            var field = GetCachedFieldInfo(propertyName, typeof(TOwner), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(propertyOwner) ?? default(TField);
            return (TField)field;
        }
        public static void SetReflectedPrivateField<TOwner>(object value, string propertyName, TOwner propertyOwner)
        {
            GetCachedFieldInfo(propertyName, typeof(TOwner), BindingFlags.Instance | BindingFlags.NonPublic).SetValue(propertyOwner, value);
        }
        public static TField GetReflectedPrivateStaticField<TField, TOwner>(string propertyName, TOwner propertyOwner)
        {
            var field = GetCachedFieldInfo(propertyName, typeof(TOwner), BindingFlags.Static | BindingFlags.NonPublic).GetValue(propertyOwner) ?? default(TField);
            return (TField)field;
        }
        public static void SetReflectedPrivateStaticField<TOwner>(object value, string propertyName, TOwner owner)
        {
            GetCachedFieldInfo(propertyName, typeof(TOwner), BindingFlags.Static | BindingFlags.NonPublic).SetValue(owner, value);
        }
        static FieldInfo GetCachedFieldInfo(string fieldName, Type ownerType, BindingFlags bindingFlags)
        {
            FieldInfo fieldInfo;
            if (_fieldCache.TryGetValue(ownerType, out SafeDictionary<string, FieldInfo> fields))
            {
                if (fields.TryGetValue(fieldName, out fieldInfo))
                {
                    return fieldInfo;
                }
                else
                {
                    fieldInfo = ownerType.GetField(fieldName, bindingFlags);
                    fields.AddOrUpdate(fieldName, fieldInfo);
                }
            }
            else
            {
                fieldInfo = ownerType.GetField(fieldName, bindingFlags);
                var newFieldDict = new SafeDictionary<string, FieldInfo>();
                newFieldDict.AddOrUpdate(fieldName, fieldInfo);
                _fieldCache.AddOrUpdate(ownerType, newFieldDict);
            }
            return fieldInfo;
        }
    }
}
