﻿using UnityEngine;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor
{

    public static class Assertions
    {

        public static void Assert(string msg)
        {
            //throw new System.Exception(msg);
            Debug.LogWarning(msg);
        }

        public static void Assert(string msg, UnityEngine.Object context)
        {
            //throw new System.Exception(msg);
            Debug.LogWarning(msg, context);
        }

        /// <summary>
        /// Throws error message if obj is not null.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool AssertNull(object obj, string msg)
        {
            if (obj != null)
            {
                Assert(msg);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Throws error message if obj is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool AssertNotNull(object obj, string msg)
        {
            if (obj == null)
            {
                Assert(msg);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Throws error message if index is less than 0 or greater than len - 1.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool AssertInBounds(int index, int len, string msg = "index out of range.")
        {
            if (index < 0 || index >= len)
            {
                Assert(msg);
                return true;
            }

            return false;
        }

        #region HasLikeComponent

        public static bool AssertHasLikeComponent(GameObject go, System.Type tp)
        {
            if (!go.HasComponent(tp))
            {
                Assert(System.String.Format("(GameObject:{1}) GameObject requires a component of type {0}.", tp.Name, go.name), go);
                return true;
            }

            return false;
        }

        public static bool AssertRequireLikeComponentAttrib(Component comp, bool silent = false)
        {
            return AssertRequireLikeComponentAttrib(comp, out _, silent);
        }

        public static bool AssertRequireLikeComponentAttrib(Component comp, out System.Type missingCompType, bool silent = false)
        {
            if (comp == null) throw new System.ArgumentNullException("comp");
            missingCompType = null;

            var tp = comp.GetType();
            foreach (var obj in tp.GetCustomAttributes(typeof(RequireLikeComponentAttribute), true))
            {
                RequireLikeComponentAttribute attrib = obj as RequireLikeComponentAttribute;
                foreach (var reqType in attrib.Types)
                {
                    if (!comp.HasComponent(reqType))
                    {
                        missingCompType = reqType;
                        if (!silent) Assert(System.String.Format("(GameObject:{2}) Component type {0} requires the gameobject to also have a component of type {1}.", tp.Name, reqType.Name, comp.gameObject.name), comp);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AssertRequireComponentInEntityAttrib(Component comp, bool silent = false)
        {
            return AssertRequireComponentInEntityAttrib(comp, out _, silent);
        }

        public static bool AssertRequireComponentInEntityAttrib(Component comp, out System.Type missingCompType, bool silent = false)
        {
            if (comp == null) throw new System.ArgumentNullException("comp");
            missingCompType = null;

            var tp = comp.GetType();
            foreach (var obj in tp.GetCustomAttributes(typeof(RequireComponentInEntityAttribute), true))
            {
                RequireComponentInEntityAttribute attrib = obj as RequireComponentInEntityAttribute;
                foreach (var reqType in attrib.Types)
                {
                    if (!comp.EntityHasComponent(reqType))
                    {
                        missingCompType = reqType;
                        if (!silent) Assert(System.String.Format("(Entity:{2}) Component type {0} requires the entity to also have a component of type {1}.", tp.Name, reqType.Name, comp.FindRoot().name), comp);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AssertRequireComponentInParentAttrib(Component comp, bool silent = false)
        {
            return AssertRequireComponentInParentAttrib(comp, out _, silent);
        }

        public static bool AssertRequireComponentInParentAttrib(Component comp, out System.Type missingCompType, bool silent = false)
        {
            if (comp == null) throw new System.ArgumentNullException("comp");
            missingCompType = null;

            var tp = comp.GetType();
            foreach (var obj in tp.GetCustomAttributes(typeof(RequireComponentInParentAttribute), true))
            {
                RequireComponentInParentAttribute attrib = obj as RequireComponentInParentAttribute;
                foreach (var reqType in attrib.Types)
                {
                    if (comp.GetComponentInParent(reqType) == null)
                    {
                        missingCompType = reqType;
                        if (!silent) Assert(System.String.Format("(Object:{2}) Component type {0} requires a parent to also have a component of type {1}.", tp.Name, reqType.Name, comp.name), comp);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AssertUniqueToEntityAttrib(Component comp, bool silent = false)
        {
            var tp = comp.GetType();
            var attrib = tp.GetCustomAttributes(typeof(UniqueToEntityAttribute), false).FirstOrDefault() as UniqueToEntityAttribute;

            if (attrib.IgnoreInactive && !comp.gameObject.activeInHierarchy) return false;

            if (attrib != null)
            {
                if (attrib.MustBeAttachedToRoot)
                {
                    if (!comp.HasComponent<SPEntity>())
                    {
                        if (!silent) Assert(System.String.Format("(Entity:{1}) Component type {0} must be attached to the root gameObject.", tp.Name, comp.FindRoot().name), comp);
                        return true;
                    }
                }


                var root = comp.FindRoot();

                foreach (var c in root.GetComponentsInChildren(tp, !attrib.IgnoreInactive))
                {
                    if (c.gameObject != comp.gameObject)
                    {
                        if (!silent) Assert(System.String.Format("(Entity:{1}) Only one component of type {0} must be attached to a root or any of its children.", tp.Name, root.name), comp);
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

    }

}
