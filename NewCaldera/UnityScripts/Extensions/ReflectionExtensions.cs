using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.UnityScripts
{
    internal static class ReflectionExtensions
    {
        public static object GetFieldValue(this object instance, Type parentType, string fieldName)
        {
            var field = parentType.GetAnyField(fieldName);
            return field.GetValue(instance);
        }

        public static T GetFieldValue<T>(this object instance, Type parentType, string fieldName)
        {
            var field = parentType.GetAnyField(fieldName);
            return (T)field.GetValue(instance);
        }

        public static object GetPropertyValue(this object instance, Type parentType, string propertyName)
        {
            var property = parentType.GetAnyProperty(propertyName);
            return property.GetValue(instance);
        }

        public static T GetPropertyValue<T>(this object instance, Type parentType, string propertyName)
        {
            var property = parentType.GetAnyProperty(propertyName);
            return (T)property.GetValue(instance);
        }

        public static object GetStaticFieldValue(Type parentType, string fieldName)
        {
            var field = parentType.GetAnyField(fieldName);
            return field.GetValue(null);
        }
        public static T GetStaticFieldValue<T>(Type parentType, string fieldName) => (T)GetStaticFieldValue(parentType, fieldName);

        public static object GetStaticPropertyValue(Type parentType, string propertyName)
        {
            var property = parentType.GetAnyProperty(propertyName);
            return property.GetValue(null);
        }

        public static T GetStaticPropertyValue<T>(Type parentType, string fieldName) => (T)GetStaticPropertyValue(parentType, fieldName);

        public static void SetField<T>(this object instance, Type parentType, string fieldName, T value)
        {
            var field = parentType.GetAnyField(fieldName);
            if (field == null)
            {
                Debug.LogWarning($"Could not determine type for field {parentType.Name}.{fieldName}.");
            }
            field.SetValue(instance, value);
        }

        public static void SetField(this object instance, Type parentType, string fieldName, object value)
        {
            var field = parentType.GetAnyField(fieldName);
            if (field == null)
            {
                Debug.LogWarning($"Could not determine type for field {parentType.Name}.{fieldName}.");
            }
            field.SetValue(instance, value);
        }

        public static void SetProperty<T>(this object instance, Type parentType, string propertyName, T value)
        {
            var property = parentType.GetAnyProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Could not determine type for field {parentType.Name}.{propertyName}.");
            }
            property.SetValue(instance, value);
        }

        public static MethodInfo GetMethod(this object instance, Type instanceType, string methodName, params object[] orderedArgs) =>
            instanceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, orderedArgs.Select(a => a.GetType()).ToArray(), null);

        public static void InvokeMethod(this object instance, Type instanceType, string methodName)
        {
            InvokeMethod(instance, instanceType, methodName, new object[0]);
        }

        public static void InvokeMethod(this object instance, Type instanceType, string methodName, params object[] orderedArgs)
        {
            var method = instanceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, orderedArgs.Select(a => a.GetType()).ToArray(), null);
            method.Invoke(instance, orderedArgs);
        }

        public static object GetMethodResult(this object instance, Type instanceType, string methodName, params object[] orderedArgs)
        {
            var method = instanceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, orderedArgs.Select(a => a.GetType()).ToArray(), null);
            return method.Invoke(instance, orderedArgs);
        }

        public static IList CreateList(Type type)
        {
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(type);

            return (IList)Activator.CreateInstance(constructedListType);
        }

        public static FieldInfo GetAnyField(this Type t, string fieldName)
        {
            return GetAllFields(t).FirstOrDefault(f => f.Name == fieldName);
        }

        public static IEnumerable<FieldInfo> GetAllFields(this Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }

        public static PropertyInfo GetAnyProperty(this Type t, string propertyName)
        {
            return GetAllProperties(t).FirstOrDefault(f => f.Name == propertyName);
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type t)
        {
            if (t == null)
                return Enumerable.Empty<PropertyInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;
            return t.GetProperties(flags).Concat(GetAllProperties(t.BaseType));
        }


        //public static object ToNestedEnumValue(this Enum sourceEnum, string parentTypeName, string enumName)
        //{
        //    return Enum.ToObject(OutwardAssembly.GetNestedType(parentTypeName, enumName), sourceEnum);
        //}
        //public static object ToNestedEnumValue(this Enum sourceEnum, Type parentType, string enumName)
        //{
        //    return Enum.ToObject(OutwardAssembly.GetNestedType(parentType, enumName), sourceEnum);
        //}

        public static Enum ToOutwardEnumValue<T>(this T instance) where T : Enum
            => (Enum)Enum.ToObject(OutwardAssembly.GetOutwardEnumType<T>(), instance);

        public static T ToLocalEnumValue<T>(this Enum instance) where T : Enum
        {
            var outwardType = OutwardAssembly.GetOutwardEnumType<T>();
            
            var localType = OutwardAssembly.GetLocalEnumType<T>(instance.GetType());
            return (T)Enum.ToObject(localType, instance);

        }
            

        public static IDictionary<K,V> CastDictionary<K, V>(this IDictionary dictionary) =>
            Enumerate(dictionary).ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
        public static IEnumerable<DictionaryEntry> Enumerate(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }
    }
}
