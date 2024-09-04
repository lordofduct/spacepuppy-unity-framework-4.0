using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using com.spacepuppy;
using com.spacepuppy.UI;
using com.spacepuppy.Utils;

namespace com.spacepuppy.debugconsole
{

    public struct CommandInput
    {
        public string Input;
        public string Command;
        public string Args;

        public IEnumerable<string> SplitArgs()
        {
            if (string.IsNullOrEmpty(Args)) yield break;

            var m = Regex.Match(Args, @"(""(?<argq>[^""]+)""|(?<arg>[^\s""]+))\s*");
            if (!m.Success) yield break;

            do
            {
                if (m.Groups["argq"].Success)
                {
                    yield return m.Groups["argq"].Value;
                }
                else
                {
                    yield return m.Groups["arg"].Value;
                }

            }
            while ((m = m.NextMatch()).Success);
        }

        public static bool TryParseCommand(string input, out CommandInput command)
        {
            if (string.IsNullOrEmpty(input) || !input.StartsWith("/"))
            {
                command = default;
                return false;
            }

            var m = Regex.Match(input, @"\/(?<arg>\w+)\s*");
            if (!m.Success)
            {
                command = default;
                return false;
            }

            command = new CommandInput()
            {
                Input = input,
                Command = m.Groups["arg"].Value,
                Args = input.Substring(m.Length),
            };
            return true;
        }

    }

    public interface ICommand
    {
        string Id { get; }
        string Description { get; }
        int HelpOrder { get; }
        void Invoke(SPDebugConsole console, CommandInput input);
    }

    public class Command : ICommand
    {

        public Command(string id, System.Action<SPDebugConsole, CommandInput> callback)
        {
            this.Id = id;
            this.Callback = callback;
        }

        public Command(string id, string desc, System.Action<SPDebugConsole, CommandInput> callback)
        {
            this.Id = id;
            this.Description = desc;
            this.Callback = callback;
        }

        public string Id { get; private set; }
        public string Description { get; set; }
        public int HelpOrder { get; set; }
        public System.Action<SPDebugConsole, CommandInput> Callback { get; set; }

        public void Invoke(SPDebugConsole console, CommandInput input)
        {
            this.Callback?.Invoke(console, input);
        }

    }

    public class PrintCommand : ICommand
    {
        public static readonly PrintCommand Default = new PrintCommand();

        public PrintCommand() { }

        public string Id => "print";
        public string Description => "prints message to output";
        public int HelpOrder { get; } = HelpCommand.HELP_COMMAND_ORDER + 1;
        public void Invoke(SPDebugConsole console, CommandInput input)
        {
            console?.Print(input.Args);
        }
    }

    public class HelpCommand : ICommand
    {
        public const int HELP_COMMAND_ORDER = -100000;
        public static readonly HelpCommand Default = new HelpCommand();

        public HelpCommand()
        {
            this.Header = "Debug Console: allows monitoring log and issuing commands.";
        }
        public HelpCommand(string customHeader)
        {
            this.Header = customHeader;
        }

        public string Id => "help";
        public string Description => "list available commands";
        public int HelpOrder { get; } = HelpCommand.HELP_COMMAND_ORDER;
        public string Header { get; set; }
        public void Invoke(SPDebugConsole console, CommandInput input)
        {
            if (console == null) return;

            if (!string.IsNullOrEmpty(this.Header))
            {
                console.Print(this.Header);
            }

            foreach (var c in console.EnumerateCommands().OrderBy(o => o.HelpOrder))
            {
                console.Print($"/{c.Id} - {c.Description}", 1000);
            }
        }
    }

    public class ClearCommand : ICommand
    {
        public static readonly ClearCommand Default = new ClearCommand();

        public ClearCommand() { }

        public string Id => "clear";
        public string Description => "clear the output";
        public int HelpOrder { get; } = HelpCommand.HELP_COMMAND_ORDER + 2;
        public void Invoke(SPDebugConsole console, CommandInput input)
        {
            console?.Clear();
        }
    }

    public class CloseCommand : ICommand
    {
        public static readonly CloseCommand Default = new CloseCommand();

        public CloseCommand() { }

        public string Id => "close";
        public string Description => "close the console";
        public int HelpOrder { get; } = HelpCommand.HELP_COMMAND_ORDER + 3;
        public void Invoke(SPDebugConsole console, CommandInput input)
        {
            console?.Close();
        }
    }

    public class LogMonitorCommand : ICommand
    {
        public static readonly LogMonitorCommand Default = new LogMonitorCommand();

        public LogMonitorCommand() { }

        public string Id => "monitor";
        public string Description => "monitors debug.log";
        public int HelpOrder { get; } = HelpCommand.HELP_COMMAND_ORDER + 4;
        public void Invoke(SPDebugConsole console, CommandInput input)
        {
            console?.MonitorDebugLog();
        }
    }

    public class KillMCommand : ICommand
    {
        public static readonly KillMCommand Default = new KillMCommand();

        public KillMCommand() { }

        public string Id => "killm";
        public string Description => "kills monitor of debug.log";
        public int HelpOrder { get; } = HelpCommand.HELP_COMMAND_ORDER + 5;
        public void Invoke(SPDebugConsole console, CommandInput input)
        {
            console?.StopMonitorDebugLog();
        }
    }


    [Infobox("This expects to be stored in the 'Resources' folder and named 'DebugConsole' to work with ShowDebugConsole.")]
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
        private bool _dontDestroyOnLoadWhenShown;
        [SerializeField]
        private bool _monitorDebugLogOnEnable;
        [SerializeField]
        private bool _printUnhandledInputs;

        [System.NonSerialized]
        private string _lastinput;
        private StringBuffer _outputbuffer = new StringBuffer();
        private Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(System.StringComparer.OrdinalIgnoreCase);
        private LogMessageReceiverListenerToken _logMessageReceiverToken;

        #endregion

        #region CONSTRUCTOR

        public SPDebugConsole()
        {
            this.AddCommand(HelpCommand.Default);
            this.AddCommand(PrintCommand.Default);
            this.AddCommand(ClearCommand.Default);
            this.AddCommand(CloseCommand.Default);
            this.AddCommand(LogMonitorCommand.Default);
            this.AddCommand(KillMCommand.Default);
        }

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            if (_monitorDebugLogOnEnable) this.MonitorDebugLog();
            _output.text = _outputbuffer.ToString();
            if (_input.HasTarget)
            {
                _input.text = string.Empty;
                _input.OnSubmit -= _input_OnSubmit;
                _input.OnSubmit += _input_OnSubmit;
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
                _input.OnSubmit -= _input_OnSubmit;
            }
        }

        #endregion

        #region Properties

        public bool MonitorDebugLogOnEnable
        {
            get => _monitorDebugLogOnEnable;
            set => _monitorDebugLogOnEnable = value;
        }

        public bool PrintUnhandledInputs
        {
            get => _printUnhandledInputs;
            set => _printUnhandledInputs = value;
        }

        /// <summary>
        /// While handling an input command this can be set to false to stop the input from being reselected after processing the command.
        /// </summary>
        public bool ReselectInput { get; set; }

        public bool LogMonitorActive => _logMessageReceiverToken.IsActive;

        /// <summary>
        /// Returns true if the window is considered showing (via the 'Show' method). This doesn't necessarily mean it's visible at the moment 
        /// as it could be disabled in the hierarchy or off camera.
        /// </summary>
        public virtual bool IsShowing => this.gameObject.activeSelf;

        #endregion

        #region Methods

        public virtual void Close()
        {
            if (object.ReferenceEquals(this, ActiveConsole))
            {
                ActiveConsole = null;
            }
            if (this) Destroy(this.gameObject);
        }

        public virtual void Show()
        {
            if (this)
            {
                this.gameObject.SetActive(true);
                if (_dontDestroyOnLoadWhenShown) DontDestroyOnLoad(this.gameObject);
            }
        }

        public void Clear()
        {
            _outputbuffer.Clear();
        }

        public void MonitorDebugLog()
        {
            if (!_logMessageReceiverToken.IsActive)
            {
                _logMessageReceiverToken = LogMessageReceiverListenerToken.Register(Application_logMessageReceived);
            }
        }

        public void StopMonitorDebugLog()
        {
            _logMessageReceiverToken.Dispose();
        }

        public void Print(string stxt, int maxlen = 80)
        {
            if (stxt == null) stxt = string.Empty;
            else if (maxlen >= 0 && stxt.Length > maxlen) stxt = stxt.Substring(0, maxlen);

            _outputbuffer.AppendLine(stxt);
        }

        public void AddCommand(ICommand cmd)
        {
            if (!string.IsNullOrEmpty(cmd?.Id))
            {
                _commands[cmd.Id] = cmd;
            }
        }

        public bool RemoveCommand(string id)
        {
            return !string.IsNullOrEmpty(id) && _commands.Remove(id);
        }

        public bool RemoveCommand(ICommand cmd)
        {
            if (!string.IsNullOrEmpty(cmd?.Id) && _commands.TryGetValue(cmd.Id, out ICommand other) && object.ReferenceEquals(other, cmd))
            {
                return _commands.Remove(cmd.Id);
            }
            return false;
        }

        public ICommand GetCommand(string id)
        {
            if (!string.IsNullOrEmpty(id) && _commands.TryGetValue(id, out ICommand cmd))
            {
                return cmd;
            }
            return null;
        }

        public bool TryGetCommand(string id, out ICommand cmd)
        {
            if (!string.IsNullOrEmpty(id) && _commands.TryGetValue(id, out cmd))
            {
                return true;
            }
            cmd = null;
            return false;
        }

        public IEnumerable<ICommand> EnumerateCommands() => _commands.Values;

        private void _input_OnSubmit(object sender, TempEventArgs e)
        {
            string stxt = e?.Value as string;
            if (string.IsNullOrEmpty(stxt)) return;

            this.ReselectInput = true;
            this.HandleInput(stxt);
            _lastinput = stxt;
            if (_outputbuffer.Changed)
            {
                _output.text = _outputbuffer.GetStringAndResetFlag();
            }

            _input.text = string.Empty;
            if (this.ReselectInput)
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

        protected virtual bool HandleInput(string input)
        {
            if (CommandInput.TryParseCommand(input, out CommandInput cmd) && this.HandleCommand(cmd))
            {
                return true;
            }
            else if (_printUnhandledInputs && !string.IsNullOrEmpty(input))
            {
                this.Print(input);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool HandleCommand(CommandInput input)
        {
            if (!string.IsNullOrEmpty(input.Command) && _commands.TryGetValue(input.Command, out ICommand cmd))
            {
                cmd?.Invoke(this, input);
                return true;
            }
            else
            {
                return false;
            }
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

        struct LogMessageReceiverListenerToken : System.IDisposable
        {

            private Application.LogCallback callback;
            public bool IsActive => callback != null;

            public void Dispose()
            {
                if (callback != null)
                {
                    Application.logMessageReceived -= callback;
                    callback = null;
                }
            }

            public static LogMessageReceiverListenerToken Register(Application.LogCallback callback)
            {
                if (callback == null) return default;
                Application.logMessageReceived += callback;
                return new LogMessageReceiverListenerToken() { callback = callback };
            }

        }

        #endregion

        #region Factory

        private static LogMessageReceiverListenerToken _showOnNextDebugLogListener;
        public static SPDebugConsole ActiveConsole { get; private set; }

        public static System.Func<SPDebugConsole> CreateConsoleCallback { get; set; } = () =>
        {
            var asset = Resources.Load<SPDebugConsole>("DebugConsole");
            return asset ? Instantiate(asset) : null;
        };

        public static SPDebugConsole ShowDebugConsole()
        {
            if (ActiveConsole)
            {
                ActiveConsole.Show();
                return ActiveConsole;
            }

            ActiveConsole = CreateConsoleCallback?.Invoke();
            if (ActiveConsole) ActiveConsole.Show();
            return ActiveConsole;
        }

        public static void ToggleDebugConsole()
        {
            if (ActiveConsole && ActiveConsole.IsShowing)
            {
                ActiveConsole.Close();
            }
            else
            {
                ShowDebugConsole();
            }
        }

        public static void ShowConsoleOnNextDebugLog()
        {
            if (_showOnNextDebugLogListener.IsActive) return;

            _showOnNextDebugLogListener = LogMessageReceiverListenerToken.Register((c, s, t) =>
            {
                _showOnNextDebugLogListener.Dispose();
                if (ActiveConsole && ActiveConsole.IsShowing) return;
                var console = ShowDebugConsole();
                if (console) console.Application_logMessageReceived(c, s, t);
            });
        }

        #endregion

    }

}
