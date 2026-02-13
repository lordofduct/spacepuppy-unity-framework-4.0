using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Render
{

    public static class MaterialUtil
    {

        public static System.Type GetPropertyType(this MaterialPropertyValueType valueType)
        {
            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return typeof(float);
                case MaterialPropertyValueType.Color:
                    return typeof(Color);
                case MaterialPropertyValueType.Vector:
                    return typeof(Vector4);
                case MaterialPropertyValueType.Texture:
                    return typeof(Texture);
                default:
                    return null;
            }
        }

        public static IMaterialPropertySetter GetMaterialPropertySetter(object target, bool useMaterialPropertyBlockIfAvailable = false)
        {
            switch (target)
            {
                case Material mat:
                    return MaterialAccessor.GetTemp(mat);
                case MaterialPropertyBlock block:
                    return MaterialPropertyBlockAccessor.GetTemp(block);
                case IMaterialSource matsrc:
                    if (matsrc.UsePropertyBlock || (useMaterialPropertyBlockIfAvailable && matsrc.CanUsePropertyBlock))
                    {
                        return MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(matsrc, true);
                    }
                    else
                    {
                        return MaterialAccessor.GetTemp(matsrc.Material);
                    }
                case Renderer rend:
                    {
                        var src = RendererMaterialSource.GetMaterialSource(rend, true);
                        if (src && src.UsePropertyBlock)
                        {
                            return MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(src, true);
                        }
                        else if (useMaterialPropertyBlockIfAvailable)
                        {
                            return Renderer_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(rend, true);
                        }
                        else
                        {
                            return MaterialAccessor.GetTemp(rend.sharedMaterial);
                        }
                    }
                case UnityEngine.UI.Graphic gr:
                    {
                        var src = GraphicMaterialSource.GetMaterialSource(gr, true);
                        if (src)
                        {
                            if (src.UsePropertyBlock || (useMaterialPropertyBlockIfAvailable && src.CanUsePropertyBlock))
                            {
                                return MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(src, true);
                            }
                            else
                            {
                                return MaterialAccessor.GetTemp(src.Material);
                            }
                        }
                        else
                        {
                            return MaterialAccessor.GetTemp(gr.material != null ? gr.material : gr.defaultMaterial);
                        }
                    }
                default:
                    return NullAccessor.Default;

            }
        }

        public static IMaterialPropertyGetter GetMaterialPropertyGetter(object target, bool useMaterialPropertyBlockIfAvailable = false)
        {
            switch (target)
            {
                case Material mat:
                    return MaterialAccessor.GetTemp(mat);
                case MaterialPropertyBlock block:
                    return MaterialPropertyBlockAccessor.GetTemp(block);
                case IMaterialSource matsrc:
                    if (matsrc.UsePropertyBlock || (useMaterialPropertyBlockIfAvailable && matsrc.CanUsePropertyBlock))
                    {
                        return MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(matsrc, false);
                    }
                    else
                    {
                        return MaterialAccessor.GetTemp(matsrc.Material);
                    }
                case Renderer rend:
                    {
                        var src = RendererMaterialSource.GetMaterialSource(rend, true);
                        if (src && (src.UsePropertyBlock || (useMaterialPropertyBlockIfAvailable && src.CanUsePropertyBlock)))
                        {
                            return MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(src, false);
                        }
                        else if (useMaterialPropertyBlockIfAvailable)
                        {
                            return Renderer_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(rend, false);
                        }
                        else
                        {
                            return MaterialAccessor.GetTemp(rend.sharedMaterial);
                        }
                    }
                case UnityEngine.UI.Graphic gr:
                    {
                        var src = GraphicMaterialSource.GetMaterialSource(gr, true);
                        if (src)
                        {
                            if (src.UsePropertyBlock || (useMaterialPropertyBlockIfAvailable && src.CanUsePropertyBlock))
                            {
                                return MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor.GetTemp(src, false);
                            }
                            else
                            {
                                return MaterialAccessor.GetTemp(src.Material);
                            }
                        }
                        else
                        {
                            return MaterialAccessor.GetTemp(gr.material != null ? gr.material : gr.defaultMaterial);
                        }
                    }
                default:
                    return NullAccessor.Default;

            }
        }

        #region Material Properties

        public static bool SetProperty(this Material mat, string propertyName, MaterialPropertyValueType valueType, object value)
        {
            if (!mat.HasProperty(propertyName)) return false;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        mat.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        mat.SetColor(propertyName, ConvertUtil.ToColor(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        mat.SetVector(propertyName, ConvertUtil.ToVector4(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        mat.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public static bool SetProperty(this Material mat, string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member, object value)
        {
            if (!mat.HasProperty(propertyName)) return false;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        mat.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                mat.SetColor(propertyName, ConvertUtil.ToColor(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var c = mat.GetColor(propertyName);
                                    c.r = ConvertUtil.ToSingle(value);
                                    mat.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var c = mat.GetColor(propertyName);
                                    c.g = ConvertUtil.ToSingle(value);
                                    mat.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var c = mat.GetColor(propertyName);
                                    c.b = ConvertUtil.ToSingle(value);
                                    mat.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var c = mat.GetColor(propertyName);
                                    c.a = ConvertUtil.ToSingle(value);
                                    mat.SetColor(propertyName, c);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                mat.SetVector(propertyName, ConvertUtil.ToVector4(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var v = mat.GetVector(propertyName);
                                    v.x = ConvertUtil.ToSingle(value);
                                    mat.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var v = mat.GetVector(propertyName);
                                    v.y = ConvertUtil.ToSingle(value);
                                    mat.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var v = mat.GetVector(propertyName);
                                    v.z = ConvertUtil.ToSingle(value);
                                    mat.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var v = mat.GetVector(propertyName);
                                    v.w = ConvertUtil.ToSingle(value);
                                    mat.SetVector(propertyName, v);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        mat.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public static object GetProperty(this Material mat, string propertyName, MaterialPropertyValueType valueType)
        {
            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return mat.GetFloat(propertyName);
                case MaterialPropertyValueType.Color:
                    return mat.GetColor(propertyName);
                case MaterialPropertyValueType.Vector:
                    return mat.GetVector(propertyName);
                case MaterialPropertyValueType.Texture:
                    return mat.GetTexture(propertyName);
                default:
                    return null;
            }
        }

        public static object GetProperty(this Material mat, string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member)
        {
            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return mat.GetFloat(propertyName);
                case MaterialPropertyValueType.Color:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return mat.GetColor(propertyName);
                        case MaterialPropertyValueTypeMember.X:
                            return mat.GetColor(propertyName).r;
                        case MaterialPropertyValueTypeMember.Y:
                            return mat.GetColor(propertyName).g;
                        case MaterialPropertyValueTypeMember.Z:
                            return mat.GetColor(propertyName).b;
                        case MaterialPropertyValueTypeMember.W:
                            return mat.GetColor(propertyName).a;
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Vector:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return mat.GetVector(propertyName);
                        case MaterialPropertyValueTypeMember.X:
                            return mat.GetVector(propertyName).x;
                        case MaterialPropertyValueTypeMember.Y:
                            return mat.GetVector(propertyName).y;
                        case MaterialPropertyValueTypeMember.Z:
                            return mat.GetVector(propertyName).z;
                        case MaterialPropertyValueTypeMember.W:
                            return mat.GetVector(propertyName).w;
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Texture:
                    return mat.GetTexture(propertyName);
                default:
                    return null;
            }
        }

        #endregion

        #region MaterialPropertyBlock Properties

        public static bool SetProperty(this MaterialPropertyBlock block, string propertyName, MaterialPropertyValueType valueType, object value)
        {
            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        block.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        block.SetColor(propertyName, ConvertUtil.ToColor(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        block.SetVector(propertyName, ConvertUtil.ToVector4(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        block.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public static bool SetProperty(this MaterialPropertyBlock block, string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member, object value)
        {
            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        block.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                block.SetColor(propertyName, ConvertUtil.ToColor(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var c = block.GetColor(propertyName);
                                    c.r = ConvertUtil.ToSingle(value);
                                    block.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var c = block.GetColor(propertyName);
                                    c.g = ConvertUtil.ToSingle(value);
                                    block.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var c = block.GetColor(propertyName);
                                    c.b = ConvertUtil.ToSingle(value);
                                    block.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var c = block.GetColor(propertyName);
                                    c.a = ConvertUtil.ToSingle(value);
                                    block.SetColor(propertyName, c);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                block.SetVector(propertyName, ConvertUtil.ToVector4(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var v = block.GetVector(propertyName);
                                    v.x = ConvertUtil.ToSingle(value);
                                    block.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var v = block.GetVector(propertyName);
                                    v.y = ConvertUtil.ToSingle(value);
                                    block.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var v = block.GetVector(propertyName);
                                    v.z = ConvertUtil.ToSingle(value);
                                    block.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var v = block.GetVector(propertyName);
                                    v.w = ConvertUtil.ToSingle(value);
                                    block.SetVector(propertyName, v);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        block.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public static object GetProperty(this MaterialPropertyBlock block, string propertyName, MaterialPropertyValueType valueType)
        {
            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return block.GetFloat(propertyName);
                case MaterialPropertyValueType.Color:
                    return block.GetColor(propertyName);
                case MaterialPropertyValueType.Vector:
                    return block.GetVector(propertyName);
                case MaterialPropertyValueType.Texture:
                    return block.GetTexture(propertyName);
                default:
                    return null;
            }
        }

        public static object GetProperty(this MaterialPropertyBlock block, string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member)
        {
            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return block.GetFloat(propertyName);
                case MaterialPropertyValueType.Color:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return block.GetColor(propertyName);
                        case MaterialPropertyValueTypeMember.X:
                            return block.GetColor(propertyName).r;
                        case MaterialPropertyValueTypeMember.Y:
                            return block.GetColor(propertyName).g;
                        case MaterialPropertyValueTypeMember.Z:
                            return block.GetColor(propertyName).b;
                        case MaterialPropertyValueTypeMember.W:
                            return block.GetColor(propertyName).a;
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Vector:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return block.GetVector(propertyName);
                        case MaterialPropertyValueTypeMember.X:
                            return block.GetVector(propertyName).x;
                        case MaterialPropertyValueTypeMember.Y:
                            return block.GetVector(propertyName).y;
                        case MaterialPropertyValueTypeMember.Z:
                            return block.GetVector(propertyName).z;
                        case MaterialPropertyValueTypeMember.W:
                            return block.GetVector(propertyName).w;
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Texture:
                    return block.GetTexture(propertyName);
                default:
                    return null;
            }
        }

        #endregion

        #region Material Source

        public static bool IsMaterialSource(object obj)
        {
            return obj is Material
                   || obj is Renderer
                   || obj is UnityEngine.UI.Graphic
                   || obj is MaterialSource;
        }

        public static void SetMaterialBySource(object obj, Material material)
        {
            switch (obj)
            {
                case IMaterialSource src:
                    src.Material = material;
                    break;
                case Renderer rend:
                    {
                        var src = RendererMaterialSource.GetMaterialSource(rend, true);
                        if (src)
                            src.Material = material;
                        else
                            rend.sharedMaterial = material;
                    }
                    break;
                case UnityEngine.UI.Graphic gr:
                    {
                        var src = GraphicMaterialSource.GetMaterialSource(gr, true);
                        if (src)
                            src.Material = material;
                        else
                            gr.material = material;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Reduces obj to a Material source type (Material, Renderer, UI.Graphics), and returns the material used by it. 
        /// Uses the sharedMaterial by default.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Material GetMaterialFromSource(object obj)
        {
            switch (obj)
            {
                case Material mat:
                    return mat;
                case IMaterialSource src:
                    return src.Material;
                case Renderer rend:
                    {
                        var src = RendererMaterialSource.GetMaterialSource(rend, true);
                        if (src)
                            return src.Material;
                        else
                            return rend.sharedMaterial;
                    }
                case UnityEngine.UI.Graphic gr:
                    {
                        var src = GraphicMaterialSource.GetMaterialSource(gr, true);
                        if (src)
                            return src.Material;
                        else
                            return gr.material != null ? gr.material : gr.defaultMaterial;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reduces obj to a Material source type (Material, Renderer, UI.Graphics), and returns the material used by it. 
        /// Uses the sharedMaterial by default.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="reduceFromGameObjectSource">If the object is a GameObjectSource, when true attempts to retrieve a Renderer or UI.Graphics from said source.</param>
        /// <returns></returns>
        public static Material GetMaterialFromSource(object obj, bool reduceFromGameObjectSource)
        {
            var result = GetMaterialFromSource(obj);

            if (!result && reduceFromGameObjectSource)
            {
                var go = com.spacepuppy.Utils.GameObjectUtil.GetGameObjectFromSource(obj);
                if (!go) return null;

                using (var lst = TempCollection.GetList<Component>())
                {
                    go.GetComponents<Component>(lst);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        result = GetMaterialFromSource(lst[i]);
                        if (result) break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Reduces obj to a Material source type (Material, Renderer, UI.Graphics), and returns the material used by it. 
        /// Uses the sharedMaterial by default.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="index">Index of the material if multiple.</param>
        /// <returns></returns>
        public static Material GetMaterialFromSource(object obj, int index)
        {
            if (index < 0) return null;

            switch (obj)
            {
                case Material mat:
                    return index == 0 ? mat : null;
                case IMaterialSource src:
                    return index == 0 ? src.Material : null;
                case Renderer rend:
                    if (index == 0)
                    {
                        var src = RendererMaterialSource.GetMaterialSource(rend, true);
                        if (src)
                            return src.Material;
                        else
                            return rend.sharedMaterial;
                    }
                    else
                    {
                        var src = RendererIndexedMaterialSource.GetMaterialSource(rend, index, true);
                        if (src)
                            return src.Material;
                        else
                            return rend.sharedMaterial;
                    }
                case UnityEngine.UI.Graphic gr:
                    if (index == 0)
                    {
                        var src = GraphicMaterialSource.GetMaterialSource(gr, true);
                        if (src)
                            return src.Material;
                        else
                            return gr.material != null ? gr.material : gr.defaultMaterial;
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reduces obj to a Material source type (Material, Renderer, UI.Graphics), and returns the material used by it. 
        /// Uses the sharedMaterial by default.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="index">Index of the material if multiple.</param>
        /// <param name="reduceFromGameObjectSource">If the object is a GameObjectSource, when true attempts to retrieve a Renderer or UI.Graphics from said source.</param>
        /// <returns></returns>
        public static Material GetMaterialFromSource(object obj, int index, bool reduceFromGameObjectSource)
        {
            if (index < 0) return null;
            var result = GetMaterialFromSource(obj, index);

            if (!result && reduceFromGameObjectSource)
            {
                var go = com.spacepuppy.Utils.GameObjectUtil.GetGameObjectFromSource(obj);
                if (!go) return null;

                using (var lst = TempCollection.GetList<Component>())
                {
                    go.GetComponents<Component>(lst);
                    for (int i = 0; i < lst.Count; i++)
                    {
                        result = GetMaterialFromSource(lst[i], index);
                        if (result) break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Reduces obj to source type, and returns a copy of the material on it. 
        /// Works like Renderer.material, but also for UI.Graphics.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="reduceFromGameObjectSource"></param>
        /// <returns></returns>
        public static Material CopyMaterialFromSource(object obj, bool reduceFromGameObjectSource = false)
        {
            var source = MaterialSource.GetMaterialSource(obj, reduceFromGameObjectSource, false);
            return source?.GetUniqueMaterial();
        }

        public static Material GetUniqueMaterial(this Renderer renderer)
        {
            if (renderer == null) throw new System.ArgumentNullException(nameof(renderer));

            var source = RendererMaterialSource.GetMaterialSource(renderer);
            return source ? source.GetUniqueMaterial() : null;
        }

        public static Material GetUniqueMaterial(this UnityEngine.UI.Graphic graphic)
        {
            if (graphic == null) throw new System.ArgumentNullException(nameof(graphic));

            var source = GraphicMaterialSource.GetMaterialSource(graphic);
            return source ? source.GetUniqueMaterial() : null;
        }

        #endregion

        #region Reusable MaterialPropertyBlock

        private static MaterialPropertyBlock REUSABLE_PROPERTY_BLOCK;
        internal static DisposableMaterialPropertyBlockToken GetTempPropertyBlock()
        {
            if (REUSABLE_PROPERTY_BLOCK != null)
            {
                var result = new DisposableMaterialPropertyBlockToken()
                {
                    block = REUSABLE_PROPERTY_BLOCK
                };
                REUSABLE_PROPERTY_BLOCK = null;
                return result;
            }
            return new DisposableMaterialPropertyBlockToken()
            {
                block = new MaterialPropertyBlock(),
            };
        }
        internal static void ReleaseTempPropertyBlock(MaterialPropertyBlock block)
        {
            if (REUSABLE_PROPERTY_BLOCK == null && block != null)
            {
                block.Clear();
                REUSABLE_PROPERTY_BLOCK = block;
            }
        }

        internal struct DisposableMaterialPropertyBlockToken : System.IDisposable
        {
            public MaterialPropertyBlock block;

            public void Dispose() => ReleaseTempPropertyBlock(block);
        }

        #endregion

    }

    public interface IMaterialPropertyGetter : System.IDisposable
    {
        object GetProperty(string propertyName, MaterialPropertyValueType valueType);
        object GetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member);
    }

    public interface IMaterialPropertySetter : IMaterialPropertyGetter
    {
        bool SetProperty(string propertyName, MaterialPropertyValueType valueType, object value);
        bool SetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member, object value);

        void SetFloat(string propertyName, float value);
        void SetInteger(string propertyName, int value);
        void SetColor(string propertyName, Color value);
        void SetVector(string propertyName, Vector4 value);
        void SetTexture(string propertyName, Texture value);
    }

    class NullAccessor : IMaterialPropertySetter
    {

        public static readonly NullAccessor Default = new();

        public void Dispose()
        {
        }

        public object GetProperty(string propertyName, MaterialPropertyValueType valueType)
        {
            return null;
        }

        public object GetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member)
        {
            return null;
        }

        public void SetColor(string propertyName, Color value)
        {
        }

        public void SetFloat(string propertyName, float value)
        {
        }

        public void SetInteger(string propertyName, int value)
        {
        }

        public bool SetProperty(string propertyName, MaterialPropertyValueType valueType, object value)
        {
            return false;
        }

        public bool SetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member, object value)
        {
            return false;
        }

        public void SetTexture(string propertyName, Texture value)
        {

        }

        public void SetVector(string propertyName, Vector4 value)
        {

        }
    }

    class MaterialAccessor : IMaterialPropertySetter
    {

        private static MaterialAccessor _cache;
        public static MaterialAccessor GetTemp(Material mat)
        {
            if (_cache != null)
            {
                var result = _cache;
                _cache = null;
                result._target = mat;
                return result;
            }
            return new MaterialAccessor()
            {
                _target = mat,
            };
        }

        public void Dispose()
        {
            _target = null;
            if (_cache == null)
            {
                _cache = this;
            }
        }

        private Material _target;

        public object GetProperty(string propertyName, MaterialPropertyValueType valueType)
        {
            if (!_target) return null;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return _target.GetFloat(propertyName);
                case MaterialPropertyValueType.Color:
                    return _target.GetColor(propertyName);
                case MaterialPropertyValueType.Vector:
                    return _target.GetVector(propertyName);
                case MaterialPropertyValueType.Texture:
                    return _target.GetTexture(propertyName);
                default:
                    return null;
            }
        }

        public object GetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member)
        {
            if (!_target) return null;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return _target.GetFloat(propertyName);
                case MaterialPropertyValueType.Color:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return _target.GetColor(propertyName);
                        case MaterialPropertyValueTypeMember.X:
                            return _target.GetColor(propertyName).r;
                        case MaterialPropertyValueTypeMember.Y:
                            return _target.GetColor(propertyName).g;
                        case MaterialPropertyValueTypeMember.Z:
                            return _target.GetColor(propertyName).b;
                        case MaterialPropertyValueTypeMember.W:
                            return _target.GetColor(propertyName).a;
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Vector:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return _target.GetVector(propertyName);
                        case MaterialPropertyValueTypeMember.X:
                            return _target.GetVector(propertyName).x;
                        case MaterialPropertyValueTypeMember.Y:
                            return _target.GetVector(propertyName).y;
                        case MaterialPropertyValueTypeMember.Z:
                            return _target.GetVector(propertyName).z;
                        case MaterialPropertyValueTypeMember.W:
                            return _target.GetVector(propertyName).w;
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Texture:
                    return _target.GetTexture(propertyName);
                default:
                    return null;
            }
        }

        public bool SetProperty(string propertyName, MaterialPropertyValueType valueType, object value)
        {
            if (!_target || !_target.HasProperty(propertyName)) return false;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        _target.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        _target.SetColor(propertyName, ConvertUtil.ToColor(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        _target.SetVector(propertyName, ConvertUtil.ToVector4(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        _target.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public bool SetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member, object value)
        {
            if (!_target || !_target.HasProperty(propertyName)) return false;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        _target.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                _target.SetColor(propertyName, ConvertUtil.ToColor(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var c = _target.GetColor(propertyName);
                                    c.r = ConvertUtil.ToSingle(value);
                                    _target.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var c = _target.GetColor(propertyName);
                                    c.g = ConvertUtil.ToSingle(value);
                                    _target.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var c = _target.GetColor(propertyName);
                                    c.b = ConvertUtil.ToSingle(value);
                                    _target.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var c = _target.GetColor(propertyName);
                                    c.a = ConvertUtil.ToSingle(value);
                                    _target.SetColor(propertyName, c);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                _target.SetVector(propertyName, ConvertUtil.ToVector4(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var v = _target.GetVector(propertyName);
                                    v.x = ConvertUtil.ToSingle(value);
                                    _target.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var v = _target.GetVector(propertyName);
                                    v.y = ConvertUtil.ToSingle(value);
                                    _target.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var v = _target.GetVector(propertyName);
                                    v.z = ConvertUtil.ToSingle(value);
                                    _target.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var v = _target.GetVector(propertyName);
                                    v.w = ConvertUtil.ToSingle(value);
                                    _target.SetVector(propertyName, v);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        _target.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public void SetFloat(string propertyName, float value)
        {
            if (_target)
            {
                _target.SetFloat(propertyName, value);
            }
        }

        public void SetInteger(string propertyName, int value)
        {
            if (_target)
            {
                _target.SetInteger(propertyName, value);
            }
        }

        public void SetColor(string propertyName, Color value)
        {
            if (_target)
            {
                _target.SetColor(propertyName, value);
            }
        }

        public void SetVector(string propertyName, Vector4 value)
        {
            if (_target)
            {
                _target.SetVector(propertyName, value);
            }
        }

        public void SetTexture(string propertyName, Texture value)
        {
            if (_target)
            {
                _target.SetTexture(propertyName, value);
            }
        }

    }

    class MaterialPropertyBlockAccessor : IMaterialPropertySetter
    {

        private static MaterialPropertyBlockAccessor _cache;
        public static MaterialPropertyBlockAccessor GetTemp(MaterialPropertyBlock block)
        {
            if (_cache != null)
            {
                var result = _cache;
                _cache = null;
                result._block = block;
                return result;
            }
            return new MaterialPropertyBlockAccessor()
            {
                _block = block,
            };
        }
        public static MaterialPropertyBlockAccessor GetTemp(MaterialPropertyBlock block, Material targetMaterial)
        {
            if (_cache != null)
            {
                var result = _cache;
                _cache = null;
                result._block = block;
                result._targetMaterial = targetMaterial;
                return result;
            }
            return new MaterialPropertyBlockAccessor()
            {
                _block = block,
                _targetMaterial = targetMaterial,
            };
        }

        public virtual void Dispose()
        {
            _targetMaterial = null;
            _block = null;
            if (_cache == null)
            {
                _cache = this;
            }
        }

        protected Material _targetMaterial;
        protected MaterialPropertyBlock _block;

        public object GetProperty(string propertyName, MaterialPropertyValueType valueType)
        {
            if (_block == null) return null;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return _block.HasProperty(propertyName) ? _block.GetFloat(propertyName) : ((_targetMaterial) ? _targetMaterial.GetFloat(propertyName) : null);
                case MaterialPropertyValueType.Color:
                    return _block.HasProperty(propertyName) ? _block.GetColor(propertyName) : ((_targetMaterial) ? _targetMaterial.GetColor(propertyName) : null);
                case MaterialPropertyValueType.Vector:
                    return _block.HasProperty(propertyName) ? _block.GetVector(propertyName) : ((_targetMaterial) ? _targetMaterial.GetVector(propertyName) : null);
                case MaterialPropertyValueType.Texture:
                    return _block.HasProperty(propertyName) ? _block.GetTexture(propertyName) : ((_targetMaterial) ? _targetMaterial.GetTexture(propertyName) : null);
                default:
                    return null;
            }
        }

        public object GetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member)
        {
            if (_block == null) return null;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    return _block.HasProperty(propertyName) ? _block.GetFloat(propertyName) : ((_targetMaterial) ? _targetMaterial.GetFloat(propertyName) : null);
                case MaterialPropertyValueType.Color:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return _block.HasProperty(propertyName) ? _block.GetColor(propertyName) : ((_targetMaterial) ? _targetMaterial.GetColor(propertyName) : null);
                        case MaterialPropertyValueTypeMember.X:
                            return _block.HasProperty(propertyName) ? _block.GetColor(propertyName).r : ((_targetMaterial) ? _targetMaterial.GetColor(propertyName).r : null);
                        case MaterialPropertyValueTypeMember.Y:
                            return _block.HasProperty(propertyName) ? _block.GetColor(propertyName).g : ((_targetMaterial) ? _targetMaterial.GetColor(propertyName).g : null);
                        case MaterialPropertyValueTypeMember.Z:
                            return _block.HasProperty(propertyName) ? _block.GetColor(propertyName).b : ((_targetMaterial) ? _targetMaterial.GetColor(propertyName).b : null);
                        case MaterialPropertyValueTypeMember.W:
                            return _block.HasProperty(propertyName) ? _block.GetColor(propertyName).a : ((_targetMaterial) ? _targetMaterial.GetColor(propertyName).a : null);
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Vector:
                    switch (member)
                    {
                        case MaterialPropertyValueTypeMember.None:
                            return _block.HasProperty(propertyName) ? _block.GetVector(propertyName) : ((_targetMaterial) ? _targetMaterial.GetVector(propertyName) : null);
                        case MaterialPropertyValueTypeMember.X:
                            return _block.HasProperty(propertyName) ? _block.GetVector(propertyName).x : ((_targetMaterial) ? _targetMaterial.GetVector(propertyName).x : null);
                        case MaterialPropertyValueTypeMember.Y:
                            return _block.HasProperty(propertyName) ? _block.GetVector(propertyName).y : ((_targetMaterial) ? _targetMaterial.GetVector(propertyName).y : null);
                        case MaterialPropertyValueTypeMember.Z:
                            return _block.HasProperty(propertyName) ? _block.GetVector(propertyName).z : ((_targetMaterial) ? _targetMaterial.GetVector(propertyName).z : null);
                        case MaterialPropertyValueTypeMember.W:
                            return _block.HasProperty(propertyName) ? _block.GetVector(propertyName).w : ((_targetMaterial) ? _targetMaterial.GetVector(propertyName).w : null);
                        default:
                            return 0f;
                    }
                case MaterialPropertyValueType.Texture:
                    return _block.HasProperty(propertyName) ? _block.GetTexture(propertyName) : ((_targetMaterial) ? _targetMaterial.GetTexture(propertyName) : null);
                default:
                    return null;
            }
        }

        public bool SetProperty(string propertyName, MaterialPropertyValueType valueType, object value)
        {
            if (_block == null || (_targetMaterial && !_targetMaterial.HasProperty(propertyName))) return false;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        _block.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        _block.SetColor(propertyName, ConvertUtil.ToColor(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        _block.SetVector(propertyName, ConvertUtil.ToVector4(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        _block.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public bool SetProperty(string propertyName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member, object value)
        {
            if (_block == null || (_targetMaterial && !_targetMaterial.HasProperty(propertyName))) return false;

            switch (valueType)
            {
                case MaterialPropertyValueType.Float:
                    try
                    {
                        _block.SetFloat(propertyName, ConvertUtil.ToSingle(value));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Color:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                _block.SetColor(propertyName, ConvertUtil.ToColor(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var c = _block.HasProperty(propertyName) ? _block.GetColor(propertyName) : _targetMaterial.GetColor(propertyName);
                                    c.r = ConvertUtil.ToSingle(value);
                                    _block.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var c = _block.HasProperty(propertyName) ? _block.GetColor(propertyName) : _targetMaterial.GetColor(propertyName);
                                    c.g = ConvertUtil.ToSingle(value);
                                    _block.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var c = _block.HasProperty(propertyName) ? _block.GetColor(propertyName) : _targetMaterial.GetColor(propertyName);
                                    c.b = ConvertUtil.ToSingle(value);
                                    _block.SetColor(propertyName, c);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var c = _block.HasProperty(propertyName) ? _block.GetColor(propertyName) : _targetMaterial.GetColor(propertyName);
                                    c.a = ConvertUtil.ToSingle(value);
                                    _block.SetColor(propertyName, c);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Vector:
                    try
                    {
                        switch (member)
                        {
                            case MaterialPropertyValueTypeMember.None:
                                _block.SetVector(propertyName, ConvertUtil.ToVector4(value));
                                break;
                            case MaterialPropertyValueTypeMember.X:
                                {
                                    var v = _block.HasProperty(propertyName) ? _block.GetVector(propertyName) : _targetMaterial.GetVector(propertyName);
                                    v.x = ConvertUtil.ToSingle(value);
                                    _block.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Y:
                                {
                                    var v = _block.HasProperty(propertyName) ? _block.GetVector(propertyName) : _targetMaterial.GetVector(propertyName);
                                    v.y = ConvertUtil.ToSingle(value);
                                    _block.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.Z:
                                {
                                    var v = _block.HasProperty(propertyName) ? _block.GetVector(propertyName) : _targetMaterial.GetVector(propertyName);
                                    v.z = ConvertUtil.ToSingle(value);
                                    _block.SetVector(propertyName, v);
                                }
                                break;
                            case MaterialPropertyValueTypeMember.W:
                                {
                                    var v = _block.HasProperty(propertyName) ? _block.GetVector(propertyName) : _targetMaterial.GetVector(propertyName);
                                    v.w = ConvertUtil.ToSingle(value);
                                    _block.SetVector(propertyName, v);
                                }
                                break;
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                case MaterialPropertyValueType.Texture:
                    try
                    {
                        _block.SetTexture(propertyName, value as Texture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
            }

            return false;
        }

        public void SetFloat(string propertyName, float value)
        {
            _block?.SetFloat(propertyName, value);
        }

        public void SetInteger(string propertyName, int value)
        {
            _block?.SetInteger(propertyName, value);
        }

        public void SetColor(string propertyName, Color value)
        {
            _block?.SetColor(propertyName, value);
        }

        public void SetVector(string propertyName, Vector4 value)
        {
            _block?.SetVector(propertyName, value);
        }

        public void SetTexture(string propertyName, Texture value)
        {
            _block?.SetTexture(propertyName, value);
        }

    }

    /// <summary>
    /// Writes to a MaterialPropertyBlock and sets it to the target IMaterialSource on Dispose.
    /// </summary>
    class MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor : MaterialPropertyBlockAccessor
    {

        private MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor() { }

        private static MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor _cache;
        public static MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor GetTemp(IMaterialSource matsrc, bool commitsOnDispose)
        {
            MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor result;
            if (_cache != null)
            {
                result = _cache;
                _cache = null;
                result._commitsOnDispose = commitsOnDispose;
                result._matsrc = matsrc;
                result._targetMaterial = matsrc.Material;
                if (result._block == null)
                {
                    result._block = new();
                }
                else
                {
                    result._block.Clear();
                }
            }
            else
            {
                result = new MaterialSource_SelfCommiting_MaterialPropertyBlockAccessor()
                {
                    _commitsOnDispose = commitsOnDispose,
                    _matsrc = matsrc,
                    _targetMaterial = matsrc.Material,
                    _block = new(),
                };
            }

            result._matsrc.GetPropertyBlock(result._block);
            return result;
        }

        public override void Dispose()
        {
            if (_commitsOnDispose && _matsrc != null) _matsrc.SetPropertyBlock(_block);

            _commitsOnDispose = false;
            _matsrc = null;
            _targetMaterial = null;
            _block?.Clear();
            if (_cache == null)
            {
                _cache = this;
            }
        }

        private IMaterialSource _matsrc;
        private bool _commitsOnDispose;

    }

    /// <summary>
    /// Writes to a MaterialPropertyBlock and sets it to the target Renderer on Dispose.
    /// </summary>
    class Renderer_SelfCommiting_MaterialPropertyBlockAccessor : MaterialPropertyBlockAccessor
    {

        private Renderer_SelfCommiting_MaterialPropertyBlockAccessor() { }

        private static Renderer_SelfCommiting_MaterialPropertyBlockAccessor _cache;
        public static Renderer_SelfCommiting_MaterialPropertyBlockAccessor GetTemp(Renderer rend, bool commitsOnDispose)
        {
            Renderer_SelfCommiting_MaterialPropertyBlockAccessor result;
            if (_cache != null)
            {
                result = _cache;
                _cache = null;
                result._commitsOnDispose = commitsOnDispose;
                result._rend = rend;
                result._targetMaterial = rend.sharedMaterial;
                if (result._block == null)
                {
                    result._block = new();
                }
                else
                {
                    result._block.Clear();
                }
            }
            else
            {
                result = new Renderer_SelfCommiting_MaterialPropertyBlockAccessor()
                {
                    _commitsOnDispose = commitsOnDispose,
                    _rend = rend,
                    _targetMaterial = rend.sharedMaterial,
                    _block = new(),
                };
            }

            result._rend.GetPropertyBlock(result._block);
            return result;
        }

        public override void Dispose()
        {
            if (_commitsOnDispose && _rend) _rend.SetPropertyBlock(_block);

            _commitsOnDispose = false;
            _rend = null;
            _targetMaterial = null;
            _block?.Clear();
            if (_cache == null)
            {
                _cache = this;
            }
        }

        private Renderer _rend;
        private bool _commitsOnDispose;

    }

}
