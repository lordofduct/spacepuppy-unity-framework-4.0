using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.DataBinding
{

    public class ColorBinder : ContentBinder
    {

        #region Fields

        [SerializeField]
        private Component _target;

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            TrySetColor(_target, context.GetBoundValue(source, this.Key));
        }

        #endregion

        #region Utils

        public static bool HasColorMember(object target)
        {
            if (target is UnityEngine.UI.Image) return true;

            var mi = DynamicUtil.GetMember(target, "color", false);
            System.Type rtp;

            if(mi != null)
            {
                rtp = DynamicUtil.GetReturnType(mi);
                if (rtp == typeof(Color))
                {
                    return true;
                }
                else if (rtp == typeof(Color32))
                {
                    return true;
                }
            }

            mi = DynamicUtil.GetMember(target, "Color", false);
            if (mi != null)
            {
                rtp = DynamicUtil.GetReturnType(mi);
                if (rtp == typeof(Color))
                {
                    return true;
                }
                else if (rtp == typeof(Color32))
                {
                    return true;
                }
            }

            return false;
        }

        public static void TrySetColor(object target, object value)
        {
            switch (target)
            {
                case UnityEngine.UI.Image img:
                    img.color = ConvertUtil.ToColor(value);
                    break;
                default:
                    {
                        var mi = DynamicUtil.GetMember(target, "color", false);
                        System.Type rtp;
                        if (mi != null)
                        {
                            rtp = DynamicUtil.GetReturnType(mi);
                            if (rtp == typeof(Color))
                            {
                                DynamicUtil.SetValue(target, "color", ConvertUtil.ToColor(value));
                            }
                            else if (rtp == typeof(Color32))
                            {
                                DynamicUtil.SetValue(target, "color", ConvertUtil.ToColor32(value));
                            }
                            return;
                        }

                        mi = DynamicUtil.GetMember(target, "Color", false);
                        if (mi != null)
                        {
                            rtp = DynamicUtil.GetReturnType(mi);
                            if (rtp == typeof(Color))
                            {
                                DynamicUtil.SetValue(target, "Color", ConvertUtil.ToColor(value));
                            }
                            else if (rtp == typeof(Color32))
                            {
                                DynamicUtil.SetValue(target, "Color", ConvertUtil.ToColor32(value));
                            }
                            return;
                        }
                    }
                    break;
            }
        }

        #endregion

    }

}
