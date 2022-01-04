using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using com.spacepuppy.Utils;
using System.Runtime.CompilerServices;
using com.spacepuppy.Project;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.UI
{

    [Infobox("This allows easily accessing to configure a UI element's text/graphics easily ambiguous of how it's configured underneath (unity Text vs TextMeshPro vs custom).\r\n\r\nLeave the mode NULL to allow the decorator to automatically configure itself on enable.")]
    public sealed class TextConfigurationDecorator : MonoBehaviour, TextConfigurationDecorator.ITextDecoratorMode, IProxy
    {

        #region Fields

        [SerializeReference]
        [SerializeRefPicker(typeof(ITextDecoratorMode), AllowNull = true, AlwaysExpanded = true)]
        private ITextDecoratorMode _mode;

        #endregion

        #region CONSTRUCTOR

        void OnEnable()
        {
            if (_mode != null) return;

            _mode = (GetTempTextDecorator(this) as System.ICloneable)?.Clone() as ITextDecoratorMode;
            ReleaseTempTextDecorators();
        }

        #endregion

        #region Properties

        public ITextDecoratorMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public string text
        {
            get => _mode?.text ?? string.Empty;
            set
            {
                if (_mode != null) _mode.text = value;
            }
        }

        public Sprite icon
        {
            get => _mode?.icon;
            set
            {
                if (_mode != null) _mode.icon = value;
            }
        }

        #endregion

        #region Methods

        public System.Type GetTargetType() => _mode?.GetTargetType() ?? typeof(object);

        public object GetTarget() => _mode?.GetTarget();

        #endregion

        #region IProxy Interface

        bool IProxy.QueriesTarget => false;

        object IProxy.GetTarget(object arg) => _mode?.GetTarget();
        object IProxy.GetTargetAs(System.Type tp) => ObjUtil.GetAsFromSource(tp, _mode?.GetTarget());
        object IProxy.GetTargetAs(System.Type tp, object arg) => ObjUtil.GetAsFromSource(tp, _mode?.GetTarget());

        #endregion

        #region Static Helpers

        /// <summary>
        /// Attempts to find the text element of a target and set its text value via the same rules as adding this component to a GameObject without configuring its mode.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TrySetText(object target, string value)
        {
            if(target is ITextDecoratorMode m)
            {
                m.text = value;
                return true;
            }
            else
            {
                var targ = GetTempTextDecorator(target);
                if(targ != null)
                {
                    targ.text = value;
                    ReleaseTempTextDecorators();
                    return true;
                }
            }
            
            return false;
        }

        public static string TryGetText(object target)
        {
            string result = null;
            if (target is ITextDecoratorMode m)
            {
                result = m.text;
            }
            else
            {
                var targ = GetTempTextDecorator(target);
                if (targ != null)
                {
                    result = targ.text;
                    ReleaseTempTextDecorators();
                }
            }

            return result;
        }


        private static UnityTextMode _tempUnityTextMode;
#if SP_TMPRO
        private static TextMeshProMode _tempTMProTextMode;
#endif
        private static ITextDecoratorMode GetTempTextDecorator(object target)
        {
            var txt = target is Text ? target as Text : GameObjectUtil.GetGameObjectFromSource(target, true)?.GetComponentInChildren<Text>();
            if (txt)
            {
                if (_tempUnityTextMode == null) _tempUnityTextMode = new UnityTextMode();
                _tempUnityTextMode.TextElement = txt;
                _tempUnityTextMode.IconElement = null;
                return _tempUnityTextMode;
            }

#if SP_TMPRO
            var tmp = target is TMPro.TMP_Text ? target as TMPro.TMP_Text : GameObjectUtil.GetGameObjectFromSource(target, true)?.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp)
            {
                if (_tempTMProTextMode == null) _tempTMProTextMode = new TextMeshProMode();
                _tempTMProTextMode.TextElement = tmp;
                _tempTMProTextMode.IconElement = null;
                return _tempTMProTextMode;
            }
#endif

            return null;
        }

        private static void ReleaseTempTextDecorators()
        {
            if (_tempUnityTextMode != null) _tempUnityTextMode.TextElement = null;
#if SP_TMPRO
            if (_tempTMProTextMode != null) _tempTMProTextMode.TextElement = null;
#endif
        }

        #endregion

        #region Special Types

        public interface ITextDecoratorMode
        {
            string text { get; set; }
            Sprite icon { get; set; }

            System.Type GetTargetType();
            object GetTarget();

        }

        [System.Serializable]
        public class UnityTextMode : ITextDecoratorMode, System.ICloneable
        {

            public Text TextElement;
            public Image IconElement;

            public string text
            {
                get => this.TextElement?.text;
                set { if (this.TextElement) this.TextElement.text = value; }
            }

            public Sprite icon
            {
                get => this.IconElement?.sprite;
                set { if (this.IconElement) this.IconElement.sprite = value; }
            }

            public System.Type GetTargetType() => typeof(Text);

            public object GetTarget() => TextElement;

            public UnityTextMode Clone() => this.MemberwiseClone() as UnityTextMode;
            object System.ICloneable.Clone() => this.Clone();

        }

#if SP_TMPRO
        [System.Serializable]
        public class TextMeshProMode : ITextDecoratorMode, System.ICloneable
        {

            public TMPro.TMP_Text TextElement;
            public Image IconElement;

            public string text
            {
                get => this.TextElement?.text;
                set { if (this.TextElement) this.TextElement.text = value; }
            }

            public Sprite icon
            {
                get => this.IconElement?.sprite;
                set { if (this.IconElement) this.IconElement.sprite = value; }
            }

            public System.Type GetTargetType() => typeof(Text);

            public object GetTarget() => TextElement;

            public TextMeshProMode Clone() => this.MemberwiseClone() as TextMeshProMode;
            object System.ICloneable.Clone() => this.Clone();

        }
#endif

        [System.Serializable]
        public class TextDecoratorModeRef : SerializableInterfaceRef<ITextDecoratorMode>
        {

        }

        #endregion

    }

}
