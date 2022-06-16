using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Render
{
    public static class MaterialUtil
    {

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

        public static System.Type GetPropertyType(this MaterialPropertyValueType valueType)
        {
            switch(valueType)
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

        #endregion

        #region Material Source

        public static bool IsMaterialSource(object obj)
        {
            return obj is Material
                   || obj is Renderer
                   || obj is UnityEngine.UI.Graphic
                   || obj is MaterialSource;
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
                case MaterialSource src:
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
                case MaterialSource src:
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
            return source ? source.GetUniqueMaterial() : null;
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

    }
}
