using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
        public static void SetPrivateField<T, V>(this T parent, string fieldName, V value)
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
                if (!sourceFields[i].FieldType.IsValueType)
                    continue;
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
        public static void DeepCloneTo<T>(this T source, T target, Transform parentTransform)
        {
            var sourceFields = typeof(T).GetFields(BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            for (int i = 0; i < sourceFields.Length; i++)
            {
                //statics but no constants
                if (sourceFields[i].IsStatic && !(sourceFields[i].IsLiteral && !sourceFields[i].IsInitOnly))
                {
                    var value = sourceFields[i].GetValue(null);
                    sourceFields[i].SetValue(source.GetType(), value, BindingFlags.FlattenHierarchy, null, null);
                }
                else if (!sourceFields[i].IsStatic)
                {
                    var fieldType = sourceFields[i].FieldType;
                    if (fieldType.IsValueType)
                    {
                        var value = sourceFields[i].GetValue(source);
                        sourceFields[i].SetValue(target, value, BindingFlags.FlattenHierarchy, null, null);
                    }
                    else if (fieldType.IsAssignableFrom(typeof(UnityEngine.Object)))
                    {
                        var value = (UnityEngine.Object)sourceFields[i].GetValue(source);
                        if (value.TryCloneValue(parentTransform, out var tValue))
                            sourceFields[i].SetValue(target, tValue, BindingFlags.FlattenHierarchy, null, null);
                    }
                    else if (fieldType.IsAssignableFrom(typeof(IEnumerable)))
                    {
                        var sourceValues = (ICollection)sourceFields[i].GetValue(source);
                        if (fieldType.IsArray)
                        {
                            var sArray = (Array)sourceValues;
                            var tValues = Array.CreateInstance(fieldType, sArray.Length);
                            for (int si = 0; si < sArray.Length; si++)
                            {
                                var sourceValue = sArray.GetValue(si);
                                if (sourceValue.TryCloneValue(parentTransform, out var targetValue))
                                    tValues.SetValue(targetValue, si);
                            }
                            sourceFields[i].SetValue(target, tValues, BindingFlags.FlattenHierarchy, null, null);
                        }
                        else if (fieldType.IsGenericList())
                        {
                            var sList = (IList)sourceValues;
                            if (TryCreateGenericList(fieldType, out var tList))
                            {
                                for (int si = 0; si < sList.Count; si++)
                                {
                                    var sourceValue = sList[si];
                                    if (sourceValue.TryCloneValue(parentTransform, out var targetValue))
                                        tList[si] = targetValue;
                                }
                            }
                            sourceFields[i].SetValue(target, tList, BindingFlags.FlattenHierarchy, null, null);
                        }
                    }
                }
            }
        }
        public static bool TryCreateGenericList(this Type type, out IList list)
        {
            if (!type.IsGenericList())
            {
                list = null;
                return false;
            }

            var listType = typeof(List<>);
            var genericArgs = type.GetGenericArguments();
            var concreteType = listType.MakeGenericType(genericArgs);
            list = (IList)Activator.CreateInstance(concreteType);
            return true;
        }
        public static bool IsGenericList(this object potentialList)
        {
            var type = potentialList.GetType();
            return type.IsGenericList();
        }
        public static bool IsGenericList(this Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));
        }
        private static bool TryCloneValue<T>(this T sourceValue, Transform parentTransform, out T clone)
        {
            if (typeof(T).IsValueType)
            {
                clone = sourceValue;
                return true;
            }
            else if (sourceValue is UnityEngine.Object sourceUO)
            {
                var cloneUO = UnityEngine.Object.Instantiate(sourceUO, parentTransform);
                if (cloneUO is GameObject go)
                    go.DeCloneNames();
                if (sourceUO is T clonedValue)
                {
                    clone = clonedValue;
                    return true;
                }
            }
            clone = default;
            return false;
        }

        private static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetGenericArguments()[0];
        }
        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }
    }
}
