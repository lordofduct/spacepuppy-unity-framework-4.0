using UnityEngine;
using System.Collections.Generic;
using System.Text;

using com.spacepuppy;
using com.spacepuppy.UI;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DebugConsole
{

    public class SPDebugConsole : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private RectTransform _container;

        [SerializeField]
        private TextFieldTarget _output = new TextFieldTarget();

        [SerializeField]
        private TextInputFieldTarget _input = new TextInputFieldTarget();

        [SerializeField]
        private bool _monitorDebugLogOnEnable;

        [System.NonSerialized]
        private static readonly StringBuffer _outputbuffer = new StringBuffer();

        [System.NonSerialized]
        private string _lastinput;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (_monitorDebugLogOnEnable) this.MonitorDebugLog();
            _output.text = _outputbuffer.ToString();
            if (_input.HasTarget)
            {
                _input.text = string.Empty;
                _input.OnSubmit_AddListener(_input_OnSubmit);
                _input.SelectUIElement();
                _input.ActivateInputField();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.StopMonitorDebugLog();
            if (_input.HasTarget)
            {
                _input.OnSubmit_RemoveListener(_input_OnSubmit);
            }
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public void Clear()
        {
            _outputbuffer.Clear();
        }

        public void MonitorDebugLog()
        {
            Application.logMessageReceived -= Application_logMessageReceived;
            Application.logMessageReceived += Application_logMessageReceived;
        }

        public void StopMonitorDebugLog()
        {
            Application.logMessageReceived -= Application_logMessageReceived;
        }

        public void Print(string stxt, int maxlen = 80)
        {
            if (stxt == null) stxt = string.Empty;
            else if (maxlen >= 0 && stxt.Length > maxlen) stxt = stxt.Substring(0, maxlen);

            _outputbuffer.AppendLine(stxt);
        }

        private void _input_OnSubmit(string stxt)
        {
            if (string.IsNullOrEmpty(stxt)) return;

            bool reselectinput = true;
            if (!this.HandleInput(stxt)) this.Print(stxt);
            _lastinput = stxt;
            if (_outputbuffer.Changed)
            {
                _output.text = _outputbuffer.GetStringAndResetFlag();
            }

            _input.text = string.Empty;
            if (reselectinput)
            {
                _input.SelectUIElement();
                _input.ActivateInputField();
            }
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            try
            {
                switch (type)
                {
                    case LogType.Exception:
                        _outputbuffer.AppendLine(condition);
                        _outputbuffer.AppendLine(stackTrace);
                        break;
                    default:
                        _outputbuffer.AppendLine(condition);
                        _output.text = _outputbuffer.ToString();
                        break;
                }
            }
            catch (System.Exception)
            {

            }
        }

        private void Update()
        {
            if (_outputbuffer.Changed)
            {
                _output.text = _outputbuffer.GetStringAndResetFlag();
            }

            if (_container)
            {
                if (TouchScreenKeyboard.visible)
                {
                    _container.anchorMin = new Vector2(0f, Mathf.Clamp01(TouchScreenKeyboard.area.height / Screen.height));
                    _container.anchorMax = Vector2.one;
                }
                else
                {
                    _container.anchorMin = Vector2.zero;
                    _container.anchorMax = Vector2.one;
                }
            }
        }

        protected virtual bool HandleInput(string stxt)
        {
            return false;
        }

        #endregion

        #region Special Types

        private class StringBuffer
        {

            private StringBuilder _builder = new StringBuilder();
            private Queue<int> _linecounts = new Queue<int>();

            public int MaxLineCount { get; set; } = 120;
            public int CurrentLineCount => _linecounts.Count;
            public bool Changed { get; private set; }

            public string GetStringAndResetFlag()
            {
                this.Changed = false;
                return _builder.ToString();
            }

            public override string ToString() => _builder.ToString();

            public void Clear()
            {
                _builder.Clear();
                _linecounts.Clear();
                this.Changed = true;
            }

            public void AppendLine(string line)
            {
                int len = _builder.Length;
                _builder.AppendLine(line);
                _linecounts.Enqueue(_builder.Length - len);
                while (_linecounts.Count > this.MaxLineCount)
                {
                    len = _linecounts.Dequeue();
                    _builder.Remove(0, len);
                }
                this.Changed = true;
            }

        }

        #endregion

    }

}
