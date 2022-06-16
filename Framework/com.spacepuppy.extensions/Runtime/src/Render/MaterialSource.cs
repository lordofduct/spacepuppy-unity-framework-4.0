#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic;
using System.Reflection;

namespace com.spacepuppy.Render
{

    public interface IMaterialSource
    {

        Material Material { get; set; }
        
        bool IsUnique { get; }

        Material GetUniqueMaterial();

    }

    public abstract class MaterialSource : SPComponent, IMaterialSource, IDynamic
    {

        #region Fields

        [SerializeField]
        private ForwardedMaterialProperty[] _forwardedMaterialProps;

        #endregion

        #region IMaterialSource Interface

        public abstract bool IsUnique { get; }
        public abstract Material Material { get; set; }

        public abstract Material GetUniqueMaterial();

        #endregion

        #region IDynamic Interface

        bool IDynamic.SetValue(string sMemberName, object value, params object[] index)
        {
            if(_forwardedMaterialProps?.Length > 0)
            {
                for(int i = 0; i < _forwardedMaterialProps.Length; i++)
                {
                    if (_forwardedMaterialProps[i].Name == sMemberName)
                    {
                        try
                        {
                            var mat = this.Material;
                            if (mat == null || !mat.HasProperty(_forwardedMaterialProps[i].Name)) return false;
                            MaterialUtil.SetProperty(mat, _forwardedMaterialProps[i].Name, _forwardedMaterialProps[i].ValueType, value);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }

            return DynamicUtil.SetValueDirect(this, sMemberName, value, index);
        }

        bool IDynamic.TryGetValue(string sMemberName, out object result, params object[] args)
        {
            if (_forwardedMaterialProps?.Length > 0)
            {
                for (int i = 0; i < _forwardedMaterialProps.Length; i++)
                {
                    if (_forwardedMaterialProps[i].Name == sMemberName)
                    {
                        result = null;
                        try
                        {
                            var mat = this.Material;
                            if (mat == null || !mat.HasProperty(_forwardedMaterialProps[i].Name)) return false;
                            result = MaterialUtil.GetProperty(mat, _forwardedMaterialProps[i].Name, _forwardedMaterialProps[i].ValueType);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }

            return DynamicUtil.TryGetValueDirect(this, sMemberName, out result, args);
        }

        object IDynamic.InvokeMethod(string sMemberName, params object[] args)
        {
            return DynamicUtil.InvokeMethodDirect(this, sMemberName, args);
        }

        bool IDynamic.HasMember(string sMemberName, bool includeNonPublic)
        {
            if (_forwardedMaterialProps?.Length > 0)
            {
                for (int i = 0; i < _forwardedMaterialProps.Length; i++)
                {
                    if (_forwardedMaterialProps[i].Name == sMemberName)
                    {
                        var mat = this.Material;
                        return mat ? mat.HasProperty(_forwardedMaterialProps[i].Name) : false;
                    }
                }
            }

            return DynamicUtil.HasMemberDirect(this, sMemberName, includeNonPublic);
        }

        IEnumerable<string> IDynamic.GetMemberNames(bool includeNonPublic)
        {
            if (_forwardedMaterialProps?.Length > 0)
            {
                for (int i = 0; i < _forwardedMaterialProps.Length; i++)
                {
                    yield return _forwardedMaterialProps[i].Name;
                }
            }

            foreach(var sname in DynamicUtil.GetMemberNamesDirect(this, includeNonPublic))
            {
                yield return sname;
            }
        }

        IEnumerable<MemberInfo> IDynamic.GetMembers(bool includeNonPublic)
        {
            if (_forwardedMaterialProps?.Length > 0)
            {
                for (int i = 0; i < _forwardedMaterialProps.Length; i++)
                {
                    yield return new DynamicPropertyInfo(_forwardedMaterialProps[i].Name, typeof(MaterialSource), _forwardedMaterialProps[i].ValueType.GetPropertyType());
                }
            }

            foreach (var info in DynamicUtil.GetMembersDirect(this, includeNonPublic))
            {
                yield return info;
            }
        }

        MemberInfo IDynamic.GetMember(string sMemberName, bool includeNonPublic)
        {
            if (_forwardedMaterialProps?.Length > 0)
            {
                for (int i = 0; i < _forwardedMaterialProps.Length; i++)
                {
                    if (_forwardedMaterialProps[i].Name == sMemberName)
                    {
                        return new DynamicPropertyInfo(_forwardedMaterialProps[i].Name, typeof(MaterialSource), _forwardedMaterialProps[i].ValueType.GetPropertyType());
                    }
                }
            }

            return DynamicUtil.GetMemberDirect(this, sMemberName, includeNonPublic);
        }

        #endregion

        #region Static Interface

        public static MaterialSource GetMaterialSource(object src, bool reduceFromGameObjectSource = false, bool donotAddSourceIfNull = false)
        {
            switch (src)
            {
                case Renderer rend:
                    return RendererMaterialSource.GetMaterialSource(rend, donotAddSourceIfNull);
                case UnityEngine.UI.Graphic gr:
                    return GraphicMaterialSource.GetMaterialSource(gr, donotAddSourceIfNull);
                default:
                    if (reduceFromGameObjectSource)
                    {
                        var go = com.spacepuppy.Utils.GameObjectUtil.GetGameObjectFromSource(src);
                        if (!go) return null;

                        using (var lst = TempCollection.GetList<Component>())
                        {
                            MaterialSource result;
                            go.GetComponents<Component>(lst);
                            for (int i = 0; i < lst.Count; i++)
                            {
                                switch (lst[i])
                                {
                                    case Renderer rend:
                                        result = RendererMaterialSource.GetMaterialSource(rend, donotAddSourceIfNull);
                                        if (result) return result;
                                        break;
                                    case UnityEngine.UI.Graphic gr:
                                        result = GraphicMaterialSource.GetMaterialSource(gr, donotAddSourceIfNull);
                                        if (result) return result;
                                        break;
                                }
                            }
                        }
                    }
                    break;
            }

            return null;
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public struct ForwardedMaterialProperty
        {
            public string Name;
            public MaterialPropertyValueType ValueType;

            public ForwardedMaterialProperty(string name, MaterialPropertyValueType valueType)
            {
                this.Name = name;
                this.ValueType = valueType;
            }
        }

        #endregion

    }

}
