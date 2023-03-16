using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Windows
{

    public interface ISearchDropDownSelector : IEqualityComparer<object>
    {
        int MaxCount { get; }

        GUIContent GetLabel(object element);
        IEnumerable<SearchDropDownElement> GetElements(GenericSearchDropDownWindow dropdown);
    }

    public struct SearchDropDownElement
    {
        public GUIContent Content;
        public object Element;
        public object Aux;
    }


    public class GenericSearchDropDownWindow : EditorWindow
    {

        private enum SearchStates
        {
            DelayedDirty = -1,
            Dirty = 0,
            Clean = 1,
        }

        #region Fields

        private Styles _style;

        private string _cntrl_name;
        private ISearchDropDownSelector _selector;


        private string _header;
        private string _search = string.Empty;
        private string _lastSearch = string.Empty;
        private List<SearchDropDownElement> _searchElements = new List<SearchDropDownElement>();
        private SearchStates _searchState;

        private Vector2 _scrollPosition;
        private int _selectedIndex = -1;
        private bool _scrollToSelected;

        private bool _searchHasEllipsis;

        #endregion

        #region CONSTRUCTOR

        protected virtual void Awake()
        {
            _style = new Styles();
        }

        protected virtual void OnEnable()
        {
            this.wantsMouseMove = true;
        }

        protected virtual void OnDestroy()
        {
            if (object.ReferenceEquals(this, _window)) _window = null;
        }

        #endregion

        #region Properties

        public string CurrentSearch
        {
            get => _search;
        }

        public string Header
        {
            get => _header;
            set => _header = value ?? string.Empty;
        }

        #endregion

        #region Private Redirect Properties

        private string _searchCacheId => _cntrl_name + "*SearchCache";

        private SearchDropDownElement? _activeElement
        {
            get
            {
                if (_selectedIndex < 0 || _selectedIndex >= _searchElements.Count) return null;
                return _searchElements[_selectedIndex];
            }
        }

        #endregion

        #region Methods

        protected virtual void OnGUI()
        {
            this.HandleKeyboard();

            GUI.Label(new Rect(0.0f, 0.0f, this.position.width, this.position.height), GUIContent.none, _style.background);
            GUILayout.Space(7f);
            EditorGUI.FocusTextInControl(_cntrl_name);

            //first draw search area and update search if needed
            var rect = GUILayoutUtility.GetRect(10f, 20f);
            rect.x += 8f;
            rect.width -= 16f;

            GUI.SetNextControlName(_cntrl_name);
            _search = SPEditorGUI.SearchField(rect, _search);
            switch (Event.current.type)
            {
                case EventType.Layout:
                    switch (_searchState)
                    {
                        case SearchStates.DelayedDirty:
                            EditorHelper.Invoke(() =>
                            {
                                if (_searchState <= SearchStates.Dirty)
                                {
                                    this.RebuildSearch();
                                    this.Repaint();
                                }
                            }, 0.1f);
                            break;
                        case SearchStates.Dirty:
                            this.RebuildSearch();
                            break;
                        default:
                            if (_search != _lastSearch)
                            {
                                this.RebuildSearch();
                            }
                            break;
                    }
                    break;
            }


            //next draw type header (display the name of the base type)
            rect = GUILayoutUtility.GetRect(10f, 25f);
            EditorGUI.LabelField(rect, _header, _style.header);

            //lastly list found types
            this.DrawList();
        }

        private void DrawList()
        {
            switch (_searchState)
            {
                case SearchStates.DelayedDirty:
                case SearchStates.Dirty:
                    {
                        Rect rect = GUILayoutUtility.GetRect(16f, 80f, new GUILayoutOption[1]
                        {
                                GUILayout.ExpandWidth(true)
                        });
                        EditorGUI.LabelField(rect, "Processing...", _style.processingLabel);
                    }
                    break;
                default:
                    {
                        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                        Event current = Event.current;
                        Rect selectedRect = new Rect();
                        EditorGUI.indentLevel++;
                        for (int index = 0; index < _searchElements.Count; index++)
                        {
                            var e = _searchElements[index];
                            Rect rect = GUILayoutUtility.GetRect(16f, 20f, new GUILayoutOption[1]
                            {
                                GUILayout.ExpandWidth(true)
                            });
                            rect.x += 2f;
                            rect.width -= 2f;

                            if ((current.type == EventType.MouseMove || current.type == EventType.MouseDown) && rect.Contains(current.mousePosition))
                            {
                                _selectedIndex = index;
                                this.Repaint();
                            }
                            bool selectedFlag = false;
                            if (index == _selectedIndex)
                            {
                                selectedFlag = true;
                                selectedRect = rect;
                            }
                            if (current.type == EventType.Repaint)
                            {
                                _style.componentButton.Draw(rect, e.Content, false, false, selectedFlag, selectedFlag);
                            }
                            if (current.type == EventType.MouseUp && rect.Contains(current.mousePosition))
                            {
                                current.Use();
                                if (!_searchHasEllipsis || index < (_searchElements.Count - 1))
                                {
                                    _selectedIndex = index;
                                    this.SelectElement(e);
                                }
                            }
                        }
                        EditorGUI.indentLevel--;

                        GUILayout.EndScrollView();



                        if (!_scrollToSelected || Event.current.type != EventType.Repaint)
                            return;
                        _scrollToSelected = false;
                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        if ((double)selectedRect.yMax - (double)lastRect.height > (double)_scrollPosition.y)
                        {
                            _scrollPosition.y = selectedRect.yMax - lastRect.height;
                            this.Repaint();
                        }
                        if ((double)selectedRect.y >= (double)_scrollPosition.y)
                            return;
                        _scrollPosition.y = selectedRect.y;
                        this.Repaint();
                    }
                    break;
            }
        }

        private void HandleKeyboard()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown)
                return;

            switch (current.keyCode)
            {
                case KeyCode.DownArrow:
                    ++_selectedIndex;
                    _selectedIndex = Mathf.Min(_selectedIndex, _searchElements.Count - 1);
                    this._scrollToSelected = true;
                    current.Use();
                    break;
                case KeyCode.UpArrow:
                    --_selectedIndex;
                    _selectedIndex = Mathf.Max(_selectedIndex, 0);
                    this._scrollToSelected = true;
                    current.Use();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    this.SelectElement(_activeElement);
                    current.Use();
                    break;
                case KeyCode.LeftArrow:
                case KeyCode.Backspace:
                    //if (this.hasSearch) return;
                    //this.GoToParent();
                    //current.Use();
                    break;
                case KeyCode.RightArrow:
                    //if (this.hasSearch) return;
                    //this.GoToChild(this.activeElement, false);
                    //current.Use();
                    break;
                case KeyCode.Escape:
                    this.Close();
                    current.Use();
                    break;
            }
        }

        private void SelectElement(SearchDropDownElement? el)
        {
            this.Close();
            if (this == _window)
            {
                _window = null;
                if (CallbackInfo.instance != null && el != null)
                {
                    CallbackInfo.instance.SignalChange(_selector, el.Value.Element);
                }
            }
        }



        private void RebuildSearch()
        {
            _lastSearch = _search;
            _searchState = SearchStates.Clean;
            _scrollPosition = Vector2.zero;
            _selectedIndex = -1;
            _searchHasEllipsis = false;

            var id = _searchCacheId;
            if (!string.IsNullOrEmpty(id)) EditorPrefs.SetString(id, _search);

            _searchElements.Clear();
            var e = _selector?.GetElements(this)?.Where(o => o.Content != null);
            if (e != null && _selector.MaxCount > 0)
            {
                _searchElements.AddRange(TakeWithElipsis(e, _selector.MaxCount));
            }
            else if (e != null)
            {
                _searchElements.AddRange(e);
            }
        }

        private IEnumerable<SearchDropDownElement> TakeWithElipsis(IEnumerable<SearchDropDownElement> coll, int count)
        {
            var e = coll.GetEnumerator();
            int i = -1;
            while (e.MoveNext())
            {
                i++;
                if (i < count)
                {
                    yield return e.Current;
                }
                else
                {
                    _searchHasEllipsis = true;
                    yield return new SearchDropDownElement()
                    {
                        Content = new GUIContent("..."),
                        Element = null
                    };
                    yield break;
                }
            }
        }

        #endregion

        #region Special Types

        private class Styles
        {
            public GUIStyle header;
            public GUIStyle componentButton = new GUIStyle((GUIStyle)"PR Label");
            public GUIStyle background = (GUIStyle)"grey_border";
            public GUIStyle previewBackground = (GUIStyle)"PopupCurveSwatchBackground";
            public GUIStyle previewHeader = new GUIStyle(EditorStyles.label);
            public GUIStyle previewText = new GUIStyle(EditorStyles.wordWrappedLabel);
            public GUIStyle rightArrow = (GUIStyle)"AC RightArrow";
            public GUIStyle leftArrow = (GUIStyle)"AC LeftArrow";
            public GUIStyle groupButton;
            public GUIStyle processingLabel = new GUIStyle(EditorStyles.label);

            public Styles()
            {
                header = SPEditorStyles.GetStyle("In BigTitle");
                this.header.font = EditorStyles.boldLabel.font;
                this.componentButton.alignment = TextAnchor.MiddleLeft;
                this.componentButton.padding.left -= 15;
                this.componentButton.fixedHeight = 20f;
                this.groupButton = new GUIStyle(this.componentButton);
                this.groupButton.padding.left += 17;
                this.previewText.padding.left += 3;
                this.previewText.padding.right += 3;
                ++this.previewHeader.padding.left;
                this.previewHeader.padding.right += 3;
                this.previewHeader.padding.top += 3;
                this.previewHeader.padding.bottom += 2;
                this.processingLabel.alignment = TextAnchor.MiddleCenter;
            }
        }

        #endregion

        #region Static Inspector Show Methods

        private static GenericSearchDropDownWindow _window;

        public static bool WindowActive => _window != null;

        public static object GetSelectedValueForControl(int controlID, object element)
        {
            return CallbackInfo.GetSelectedValueForControl(controlID, element);
        }

        /// <summary>
        /// Displays a standard 'popup' entry that can be clicked uncovering the selector dropdown. 
        /// The selected element's 'Content' id shown on the popup. 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="label"></param>
        /// <param name="currentElement"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static object Popup(Rect position, GUIContent label, object currentElement, ISearchDropDownSelector selector)
        {
            if (selector == null) throw new System.ArgumentNullException(nameof(selector));

            int controlID = GUIUtility.GetControlID(selector.GetType().GetHashCode(), FocusType.Passive, position);
            position = EditorGUI.PrefixLabel(position, controlID, label);

            currentElement = CallbackInfo.GetSelectedValueForControl(controlID, currentElement);

            var content = selector.GetLabel(currentElement);
            var current = Event.current;
            var type = current.type;
            switch (type)
            {
                case EventType.KeyDown:
                    {
                        //TODO?
                        //EditorStyles.popup.Draw(position, content, controlID);
                    }
                    break;
                case EventType.Repaint:
                    {
                        EditorStyles.popup.Draw(position, content, controlID);
                    }
                    break;
                case EventType.MouseDown:
                    {
                        if (current.button == 0 && position.Contains(current.mousePosition))
                        {
                            CallbackInfo.instance = new CallbackInfo(controlID, currentElement);
                            GenericSearchDropDownWindow.DisplayCustomMenu(position, label, selector);
                            GUIUtility.keyboardControl = controlID;
                            current.Use();
                        }
                    }
                    break;
            }

            return currentElement;
        }

        /// <summary>
        /// Display a dropdown window below some property. This should be called when the user 
        /// clicks on the region of the screen that causes the dropdown (caret? the entire popup? your decision). 
        /// </summary>
        /// <param name="positionUnder"></param>
        /// <param name="currentElement"></param>
        /// <param name="callback"></param>
        /// <param name="selector"></param>
        public static void ShowAndCallbackOnSelect(int controlId,
                                                   Rect positionUnder,
                                                   object currentElement,
                                                   System.Action<object> callback,
                                                   ISearchDropDownSelector selector)
        {
            if (selector == null) throw new System.ArgumentNullException(nameof(selector));

            CallbackInfo.instance = new CallbackInfo(controlId, currentElement, callback);
            DisplayCustomMenu(positionUnder, GUIContent.none, selector);
        }

        private static void DisplayCustomMenu(Rect position, GUIContent label, ISearchDropDownSelector selector)
        {
            if (_window != null)
            {
                _window.Close();
            }

            var pos = GUIUtility.GUIToScreenPoint(new Vector2(position.x, position.y));
            position.x = pos.x;
            position.y = pos.y;

            _window = EditorWindow.CreateInstance<GenericSearchDropDownWindow>();
            _window._cntrl_name = selector.GetType().Name;
            _window._search = EditorPrefs.GetString(_window._searchCacheId);
            _window._selector = selector;
            _window._searchState = SearchStates.DelayedDirty;

            _window.ShowAsDropDown(position, new Vector2(position.width, 320f));
            _window.Focus();
        }


        private class CallbackInfo
        {
            public const string CMND_MENUCHANGED = "GenericSearchPopupMenuChanged";
            public static CallbackInfo instance;

            public int controlID { get; private set; }
            public object selectedElement { get; private set; }
            public bool signaled { get; private set; }

            private com.spacepuppyeditor.Internal.GUIViewProxy _sourceView;
            private System.Action<object> _callback;

            public CallbackInfo(int controlId, object element)
            {
                controlID = controlId;
                selectedElement = element;
                _sourceView = com.spacepuppyeditor.Internal.GUIViewProxy.GetCurrent();
            }

            public CallbackInfo(int controlId, object element, System.Action<object> callback)
            {
                controlID = controlId;
                selectedElement = element;
                _sourceView = com.spacepuppyeditor.Internal.GUIViewProxy.GetCurrent();
                _callback = callback;
            }

            public void SignalChange(ISearchDropDownSelector selector, object element)
            {
                if (!signaled)
                {
                    signaled = true;
                    if (!selector.Equals(element, selectedElement))
                    {
                        selectedElement = element;
                        if (_sourceView != null)
                        {
                            _sourceView.SendEvent(EditorGUIUtility.CommandEvent(CMND_MENUCHANGED));
                        }
                        _callback?.Invoke(selectedElement);
                    }
                }
            }

            public static object GetSelectedValueForControl(int controlID, object element)
            {
                Event current = Event.current;
                if (current.type == EventType.ExecuteCommand && current.commandName == CMND_MENUCHANGED)
                {
                    if (instance != null && instance.controlID == controlID)
                    {
                        element = instance.selectedElement;
                        GUI.changed = true;
                        current.Use();
                    }
                }
                return element;
            }

        }

        #endregion


        #region Special Methods

        protected virtual void OnHierarchyChange()
        {
            _sceneObjects = null;
        }

        private static GameObject[] _sceneObjects;
        public static IEnumerable<T> GetSceneObjects<T>() where T : class
        {
            if (_sceneObjects == null) _sceneObjects = UnityEngine.Object.FindObjectsOfType<GameObject>(true);

            if (typeof(T) == typeof(UnityEngine.Object) || typeof(T) == typeof(GameObject))
            {
                return _sceneObjects.Cast<T>();
            }
            else if (ComponentUtil.IsAcceptableComponentType(typeof(T)))
            {
                return _sceneObjects.Select(o => ObjUtil.GetAsFromSource<T>(o)).Where(o => o != null) ?? Enumerable.Empty<T>();
            }
            else
            {
                return _sceneObjects.OfType<T>();
            }
        }

        #endregion

    }

    public class GenericSearchDropDownObjectFieldHelper<T> where T : class
    {
        public delegate void ShowDropDownCallbackDelegate(int controlId, Rect position, T asset);
        public delegate string ObjectBoxLabelFormatterDelegate(UnityEngine.Object oasset, System.Type objType);

        public static readonly ObjectBoxLabelFormatterDelegate DefaultObjectBoxLabelFormatter = (a, tp) => a != null ? a.name : string.Format("None ({0})", tp?.Name ?? typeof(T).Name);

        #region Properties

        /// <summary>
        /// A sub-type of T to further restrict by. This may be necessary because you want to allow dragging on of many types that are then filtered down (like IProxy). 
        /// Generally it can be left blank or as its default of typeof(T).
        /// </summary>
        public virtual System.Type ObjectType { get; set; } = typeof(T);

        public System.Predicate<T> Filter { get; set; }

        public virtual ShowDropDownCallbackDelegate ShowDropDownCallback { get; set; }

        public virtual bool AllowSceneObjects { get; set; }

        public virtual bool AllowProxy { get; set; }

        private ObjectBoxLabelFormatterDelegate _objectBoxLabelFormatter;
        public virtual ObjectBoxLabelFormatterDelegate ObjectBoxLabelFormatter
        {
            get => _objectBoxLabelFormatter ?? DefaultObjectBoxLabelFormatter;
            set => _objectBoxLabelFormatter = value ?? DefaultObjectBoxLabelFormatter;
        }

        #endregion

        #region Methods

        public virtual T DrawObjectField(Rect position, GUIContent label, T asset)
        {
            var objType = this.ObjectType ?? typeof(T);
            var labelFormatter = this.ObjectBoxLabelFormatter ?? DefaultObjectBoxLabelFormatter;
            int controlId = GUIUtility.GetControlID(label, FocusType.Passive, position);
            asset = GenericSearchDropDownWindow.GetSelectedValueForControl(controlId, asset) as T;

            bool isDragging = Event.current.type == EventType.DragUpdated && position.Contains(Event.current.mousePosition);
            bool isDropping = Event.current.type == EventType.DragPerform && position.Contains(Event.current.mousePosition);

            float pickerWidth = 20f;
            Rect pickerRect = position;
            pickerRect.width = pickerWidth;
            pickerRect.x = position.xMax - pickerWidth;

            bool isPickerPressed = Event.current.type == EventType.MouseDown && Event.current.button == 0 && pickerRect.Contains(Event.current.mousePosition);
            bool isEnterKeyPressed = Event.current.type == EventType.KeyDown && Event.current.isKey && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return);
            if (isPickerPressed || isDragging || isDropping || isEnterKeyPressed)
            {
                // To override ObjectField's default behavior
                Event.current.Use();
            }

            if (asset != null)
            {
                var oasset = asset as UnityEngine.Object;
                GameObject go;

                float iconHeight = EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing * 3;
                Vector2 iconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(iconHeight, iconHeight));

                var assetcontent = EditorHelper.ObjectContent(oasset, objType, true);
                assetcontent.text = labelFormatter(oasset, objType);
                UnityEngine.GUI.Box(position, assetcontent, EditorStyles.objectField);

                EditorGUIUtility.SetIconSize(iconSize);

                bool isFieldPressed = Event.current.type == EventType.MouseDown && Event.current.button == 0 && position.Contains(Event.current.mousePosition);
                if (isFieldPressed)
                {
                    if (Event.current.clickCount == 1)
                        EditorGUIUtility.PingObject(oasset);
                    if (Event.current.clickCount == 2)
                    {
                        AssetDatabase.OpenAsset(oasset);
                        GUIUtility.ExitGUI();
                    }
                }
            }
            else
            {
                position = EditorGUI.PrefixLabel(position, controlId, label);
                UnityEngine.GUI.Box(position, new GUIContent(labelFormatter(null, objType)), EditorStyles.objectField);
            }

#if UNITY_2019_1_OR_NEWER
            DrawCaret(pickerRect);
#endif

            if (isPickerPressed && !GenericSearchDropDownWindow.WindowActive)
            {
                ShowDropDownCallback?.Invoke(controlId, position, asset);
            }

            var newobj = HandleDragAndDrop(isDragging, isDropping, asset as UnityEngine.Object, objType, this.Filter, this.AllowSceneObjects, this.AllowProxy) as T;
            if (newobj != asset)
            {
                asset = newobj;
                GUI.changed = true;
            }
            return asset;
        }

        private static Texture2D m_CaretTexture;
        private static void DrawCaret(Rect pickerRect)
        {
#if UNITY_2019_1_OR_NEWER
            if (m_CaretTexture == null)
            {
                string caretIconPath = EditorGUIUtility.isProSkin
                    ? @"Packages\com.unity.addressables\Editor\Icons\PickerDropArrow-Pro.png"
                    : @"Packages\com.unity.addressables\Editor\Icons\PickerDropArrow-Personal.png";

                if (System.IO.File.Exists(caretIconPath))
                {
                    m_CaretTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(caretIconPath, typeof(Texture2D));
                }
            }

            if (m_CaretTexture != null)
            {
                UnityEngine.GUI.DrawTexture(pickerRect, m_CaretTexture, ScaleMode.ScaleToFit);
            }
#endif
        }

        private static UnityEngine.Object HandleDragAndDrop(bool isDragging, bool isDropping, UnityEngine.Object asset, System.Type objType, System.Predicate<T> filter, bool allowSceneObjects, bool allowProxy)
        {
            if (!isDragging && !isDropping) return asset;

            var types = allowProxy ? ArrayUtil.Temp<System.Type>(objType, typeof(IProxy)) : ArrayUtil.Temp<System.Type>(objType);
            try
            {
                bool validDrag;
                if (allowSceneObjects)
                {
                    validDrag = DragAndDrop.objectReferences.Any(o => ObjUtil.GetAsFromSource(types, o) != null &&
                                                                      (filter == null || (o is T ot && filter(ot))));
                }
                else
                {
                    validDrag = DragAndDrop.objectReferences.Any(o => ObjUtil.GetAsFromSource(types, o) != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)) &&
                                                                      (filter == null || (o is T ot && filter(ot))));
                }

                if (isDragging)
                {
                    DragAndDrop.visualMode = !validDrag ? DragAndDropVisualMode.Rejected : DragAndDropVisualMode.Copy;
                }

                if (validDrag && isDropping)
                {
                    UnityEngine.Object entry;
                    if (filter == null)
                    {
                        entry = DragAndDrop.objectReferences.Select(o => ObjUtil.GetAsFromSource(types, o) as UnityEngine.Object).FirstOrDefault(o => o != null);
                    }
                    else
                    {
                        entry = DragAndDrop.objectReferences.Select(o =>
                        {
                            o = ObjUtil.GetAsFromSource(types, o) as UnityEngine.Object;
                            return (o is T ot && filter(ot)) ? o : null;
                        }).FirstOrDefault(o => o != null);
                    }

                    if (entry != null && !object.ReferenceEquals(asset, entry))
                    {
                        asset = entry;
                    }
                }
                return asset;
            }
            finally
            {
                ArrayUtil.ReleaseTemp(types);
            }
        }

        #endregion

    }

}
