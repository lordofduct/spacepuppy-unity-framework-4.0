using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace com.spacepuppy.UI
{

    [Infobox("This allows easily accessing to configure a UI element's text/graphics easily ambiguous of how it's configured underneath (unity Text vs TextMeshPro vs custom).\r\n\r\nLeave the mode NULL to allow the decorator to automatically configure itself on enable.")]
    public sealed class TextConfigurationDecorator : MonoBehaviour, TextConfigurationDecorator.ITextDecoratorMode
    {

        #region Fields

        [SerializeReference]
        [SerializeRefPicker(typeof(ITextDecoratorMode), AllowNull = true)]
        private ITextDecoratorMode _mode;

        #endregion

        #region CONSTRUCTOR

        void OnEnable()
        {
            if (_mode != null) return;

            var txt = this.GetComponentInChildren<Text>();
            if(txt)
            {
                _mode = new UnityTextMode()
                {
                    TextElement = txt,
                    IconElement = null,
                };
                return;
            }

            var tmp = this.GetComponentInChildren<TMPro.TMP_Text>();
            if(tmp)
            {
                _mode = new TextMeshProMode()
                {
                    TextElement = tmp,
                    IconElement = null,
                };
                return;
            }
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
            get => _mode?.text;
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

        #region Special Types

        public interface ITextDecoratorMode
        {

            string text { get; set; }
            Sprite icon { get; set; }

        }

        [System.Serializable]
        public class UnityTextMode : ITextDecoratorMode
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

        }

        //COMMENT THIS OUT IF YOU DON'T HAVE TextMeshPro
        [System.Serializable]
        public class TextMeshProMode : ITextDecoratorMode
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

        }

        #endregion

    }

}
