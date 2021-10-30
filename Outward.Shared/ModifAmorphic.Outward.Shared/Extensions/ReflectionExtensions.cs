using System;
using System.Reflection;

namespace ModifAmorphic.Outward.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        //public static T GetPrivateField<T>(this object parent, string fieldName)
        //{
        //    var field = parent.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        //    return (T)field.GetValue(parent);
        //}
        public static V GetPrivateField<T, V>(this T parent, string fieldName)
        {
            var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (V)field.GetValue(parent);
        }
        public static void SetPrivateField<T,V>(this T parent, string fieldName, V value)
        {
            var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(parent, value);
        }
        public static T InvokePrivateMethod<T>(this object parent, string methodName)
        {
            var method = parent.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)method.Invoke(parent, null);
        }
        public static void InvokePrivateMethod<T>(this T parent, string methodName, params object[] args)
        {
            var method = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(parent, args);
        }
        public static R InvokePrivateMethod<T, R>(this T parent, string methodName, params object[] args)
        {
            var method = parent.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (R)method.Invoke(parent, args);
        }
        public static void CopyFieldsTo<T>(this T source, T target)
        {
            var sourceFields = typeof(T).GetFields(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);

            for (int i = 0; i < sourceFields.Length; i++)
            {
                object value;
                //statics but no constants
                if (sourceFields[i].IsStatic && !(sourceFields[i].IsLiteral && !sourceFields[i].IsInitOnly))
                {
                    value = sourceFields[i].GetValue(null);
                    sourceFields[i].SetValue(source.GetType(), value, System.Reflection.BindingFlags.FlattenHierarchy, null, null);
                }
                else if (!sourceFields[i].IsStatic)
                {
                    value = sourceFields[i].GetValue(source);
                    sourceFields[i].SetValue(target, value, System.Reflection.BindingFlags.FlattenHierarchy, null, null);
                }
                //Logger.LogTrace($"{sourceFields[i].Name} set to '{value?.GetType()}' value: '{value}'");
            }
        }
    }
}
