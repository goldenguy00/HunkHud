using System.Reflection;
using UnityEngine;

namespace MaterialHud
{
    internal static class Utils
    {
        public static T TryAddComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var component))
            {
                return component;
            }
            return go.AddComponent<T>();
        }

        public static void RemoveComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var component))
            {
                Component.Destroy(component);
            }
        }

        public static void RemoveComponents<T>(this GameObject go) where T : Component
        {
            var coms = go.GetComponents<T>();
            for (var i = 0; i < coms.Length; i++)
            {
                Component.Destroy(coms[i]);
            }
        }

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            var type = comp.GetType();
            if (type != other.GetType())
                return null; // type mis-match

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            var finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T GetCopyOf<T>(this ScriptableObject comp, T other) where T : ScriptableObject
        {
            var type = comp.GetType();
            if (type != other.GetType())
                return null; // type mis-match

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            var pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            var finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T AddComponentCopy<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd);
        }
    }
}
