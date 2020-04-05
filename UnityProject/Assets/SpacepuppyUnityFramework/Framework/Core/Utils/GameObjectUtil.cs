using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Utils
{

    public static class GameObjectUtil
    {


        #region Get*FromSource

        public static bool IsGameObjectSource(object obj)
        {
            return (obj is GameObject || obj is Component || obj is IGameObjectSource);
        }

        public static bool IsGameObjectSource(object obj, bool respectProxy)
        {
            if (respectProxy && obj is IProxy)
            {
                obj = (obj as IProxy).GetTarget();
                if (obj == null) return false;
            }

            return (obj is GameObject || obj is Component || obj is IGameObjectSource);
        }

        public static GameObject GetGameObjectFromSource(object obj, bool respectProxy = false)
        {
            if (obj == null) return null;

            if (respectProxy && obj is IProxy)
            {
                obj = (obj as IProxy).GetTarget();
                if (obj == null) return null;
            }

            if (obj is GameObject)
                return obj as GameObject;
            if (obj is Component)
                return ObjUtil.IsObjectAlive(obj as Component) ? (obj as Component).gameObject : null;
            if (obj is IGameObjectSource)
                return obj.IsNullOrDestroyed() ? null : (obj as IGameObjectSource).gameObject;

            return null;
        }


        #endregion

        #region Find Root

        /// <summary>
        /// Attempts to find a parent with a tag of 'Root', if none is found, null is returned.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static GameObject FindTrueRoot(this GameObject go)
        {
            if (go == null) return null;

            var entity = SPEntity.Pool.GetFromSource(go);
            if (entity != null)
                return entity.gameObject;
            else
                return FindParentWithTag(go.transform, SPConstants.TAG_ROOT);
        }

        public static GameObject FindTrueRoot(this Component c)
        {
            if (c == null) return null;

            var entity = SPEntity.Pool.GetFromSource(c);
            if (entity != null)
                return entity.gameObject;
            else
                return FindParentWithTag(c.transform, SPConstants.TAG_ROOT);
        }

        /// <summary>
        /// Attempts to find a parent with a tag of 'Root', if none is found, self is returned.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static GameObject FindRoot(this GameObject go)
        {
            if (go == null) return null;

            var entity = SPEntity.Pool.GetFromSource(go);
            if (entity != null)
                return entity.gameObject;
            else
            {
                var root = FindParentWithTag(go.transform, SPConstants.TAG_ROOT);
                return (root != null) ? root : go; //we return self if no root was found...
            }
        }

        public static GameObject FindRoot(this Component c)
        {
            if (c == null) return null;

            var entity = SPEntity.Pool.GetFromSource(c);
            if (entity != null)
                return entity.gameObject;
            else
            {
                var root = FindParentWithTag(c.transform, SPConstants.TAG_ROOT);
                return (root != null) ? root : c.gameObject; //we return self if no root was found...
            }
        }

        #endregion

        #region Find By Tags

        /**
         * Find
         */

        public static GameObject[] FindGameObjectsWithMultiTag(string tag)
        {
            if (tag == SPConstants.TAG_MULTITAG)
            {
                return GameObject.FindGameObjectsWithTag(SPConstants.TAG_MULTITAG);
            }
            else
            {
                using (var tmp = TempList<GameObject>.GetList())
                {
                    foreach (var go in GameObject.FindGameObjectsWithTag(tag)) tmp.Add(go);

                    MultiTag.FindAll(tag, tmp);

                    return tmp.ToArray();
                }
            }
        }

        public static int FindGameObjectsWithMultiTag(string tag, ICollection<UnityEngine.GameObject> coll)
        {
            if (coll == null) throw new System.ArgumentNullException("coll");

            int cnt = coll.Count;
            if (tag == SPConstants.TAG_MULTITAG)
            {
                coll.AddRange(GameObject.FindGameObjectsWithTag(SPConstants.TAG_MULTITAG));
            }
            else
            {
                foreach (var go in GameObject.FindGameObjectsWithTag(tag)) coll.Add(go);

                MultiTag.FindAll(tag, coll);
            }
            return coll.Count - cnt;
        }

        public static GameObject FindWithMultiTag(string tag)
        {
            if (tag == SPConstants.TAG_MULTITAG)
            {
                return GameObject.FindWithTag(SPConstants.TAG_MULTITAG);
            }
            else
            {
                var directHit = GameObject.FindWithTag(tag);
                if (directHit != null) return directHit;

                var comp = MultiTag.Find(tag);
                return (comp != null) ? comp.gameObject : null;
            }
        }

        public static GameObject FindWithMultiTag(this GameObject go, string tag)
        {
            if (MultiTagHelper.HasTag(go, tag)) return go;

            foreach (var child in go.transform.IterateAllChildren())
            {
                if (MultiTagHelper.HasTag(child.gameObject, tag)) return child.gameObject;
            }

            return null;
        }

        public static IEnumerable<GameObject> FindAllWithMultiTag(this GameObject go, string tag)
        {
            if (MultiTagHelper.HasTag(go)) yield return go;

            foreach (var child in go.transform.IterateAllChildren())
            {
                if (MultiTagHelper.HasTag(child.gameObject, tag)) yield return child.gameObject;
            }
        }

        /**
         * FindParentWithTag
         */

        public static GameObject FindParentWithTag(this GameObject go, string stag)
        {
            if (go == null) return null;
            return FindParentWithTag(go.transform, stag);
        }

        public static GameObject FindParentWithTag(this Component c, string stag)
        {
            if (c == null) return null;
            return FindParentWithTag(c.transform, stag);
        }

        public static GameObject FindParentWithTag(this Transform t, string stag)
        {
            while (t != null)
            {
                if (MultiTagHelper.HasTag(t, stag)) return t.gameObject;
                t = t.parent;
            }

            return null;
        }

        #endregion

        #region Parenting

        public static IEnumerable<Transform> IterateAllChildren(this Transform trans)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                yield return trans.GetChild(i);
            }

            for (int i = 0; i < trans.childCount; i++)
            {
                foreach (var c in IterateAllChildren(trans.GetChild(i)))
                    yield return c;
            }
        }

        // ##############
        // Is Parent
        // ##########

        public static bool IsParentOf(this GameObject parent, GameObject possibleChild)
        {
            if (parent == null || possibleChild == null) return false;
            return possibleChild.transform.IsChildOf(parent.transform);
        }

        public static bool IsParentOf(this Transform parent, GameObject possibleChild)
        {
            if (parent == null || possibleChild == null) return false;
            return possibleChild.transform.IsChildOf(parent);
        }

        public static bool IsParentOf(this GameObject parent, Transform possibleChild)
        {
            if (parent == null || possibleChild == null) return false;
            return possibleChild.IsChildOf(parent.transform);
        }

        public static bool IsParentOf(this Transform parent, Transform possibleChild)
        {
            if (parent == null || possibleChild == null) return false;
            /*
             * Since implementation of this, Unity has since added 'IsChildOf' that is far superior in efficiency
             * 
            while (possibleChild != null)
            {
                if (parent == possibleChild.parent) return true;
                possibleChild = possibleChild.parent;
            }
            return false;
            */

            return possibleChild.IsChildOf(parent);
        }

        #endregion

    }

}