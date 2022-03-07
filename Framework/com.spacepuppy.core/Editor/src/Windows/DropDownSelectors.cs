using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using System.Runtime.CompilerServices;
using Codice.CM.Client.Differences.Graphic;

namespace com.spacepuppyeditor.Windows
{

    public class TypeDropDownWindowSelector : ISearchDropDownSelector
    {

        public const int DEFAULT_MAXCOUNT = 100;

        #region Fields

        private System.Type _defaultType = null;
        private System.Type _baseType = typeof(object);
        private TypeDropDownListingStyle _listStyle;
        private System.Predicate<System.Type> _enumeratePredicate;
        private System.Func<System.Type, string, bool> _searchFilter;

        #endregion

        #region ISelector Interface

        public int MaxCount { get; set; } = DEFAULT_MAXCOUNT;

        public new bool Equals(object x, object y)
        {
            return (x as System.Type) == (y as System.Type);
        }

        public int GetHashCode(object obj)
        {
            return (obj as System.Type)?.GetHashCode() ?? 0;
        }

        public GUIContent GetLabel(object element)
        {
            var tp = element as System.Type;
            if (tp == null)
            {
                return new GUIContent("Nothing...");
            }
            else
            {
                switch (_listStyle)
                {
                    case TypeDropDownListingStyle.Namespace:
                        return new GUIContent(tp.Namespace + "/" + tp.Name, tp.FullName);
                    case TypeDropDownListingStyle.ComponentMenu:
                    case TypeDropDownListingStyle.Flat:
                    default:
                        return new GUIContent(tp.Name, tp.FullName);
                }
            }
        }

        public IEnumerable<SearchDropDownElement> GetElements(GenericSearchDropDownWindow window)
        {
            if (_defaultType == null)
            {
                yield return new SearchDropDownElement()
                {
                    Content = GetLabel(null)
                };
            }

            var match = window.CurrentSearch ?? string.Empty;
            IEnumerable<SearchDropDownElement> results;
            if (_searchFilter != null)
            {
                window.Header = (_baseType == typeof(object) || _baseType == null) ? "Types" : _baseType.Name + "/s";

                results = TypeUtil.GetTypes(_enumeratePredicate)
                                .Where(tp => _searchFilter(tp, match))
                                .Select(tp => new SearchDropDownElement()
                                {
                                    Content = GetLabel(tp),
                                    Element = tp
                                });
            }
            else if (string.IsNullOrEmpty(window.CurrentSearch))
            {
                window.Header = (_baseType == typeof(object) || _baseType == null) ? "Types" : _baseType.Name + "/s";

                results = TypeUtil.GetTypes(_enumeratePredicate)
                                .Select(tp => new SearchDropDownElement()
                                {
                                    Content = GetLabel(tp),
                                    Element = tp
                                });
            }
            else
            {
                window.Header = (_baseType == typeof(object) || _baseType == null) ? "Filtered Types" : "Filtered " + _baseType.Name + "/s";

                results = TypeUtil.GetTypes(_enumeratePredicate)
                                .Where(tp => tp.Name.IndexOf(match, 0, System.StringComparison.OrdinalIgnoreCase) >= 0)
                                .Select(tp => new SearchDropDownElement()
                                {
                                    Content = GetLabel(tp),
                                    Element = tp
                                });
            }

            foreach (var se in results)
            {
                yield return se;
            }
        }

        #endregion

        #region Static Factory

        public static System.Type Popup(Rect position, GUIContent label,
                                        System.Type selectedType,
                                        System.Predicate<System.Type> enumeratePredicate,
                                        System.Type baseType = null,
                                        System.Type defaultType = null,
                                        TypeDropDownListingStyle listType = TypeDropDownListingStyle.Flat,
                                        System.Func<System.Type, string, bool> searchFilter = null,
                                        int maxVisibleCount = DEFAULT_MAXCOUNT)
        {
            return GenericSearchDropDownWindow.Popup(position, label, selectedType, new TypeDropDownWindowSelector()
            {
                _baseType = baseType,
                _enumeratePredicate = enumeratePredicate,
                _defaultType = defaultType,
                _listStyle = listType,
                _searchFilter = searchFilter,
                MaxCount = maxVisibleCount,
            }) as System.Type;
        }

        public static void ShowAndCallbackOnSelect(int controlId, Rect positionUnder, System.Type selectedType,
                                                   System.Predicate<System.Type> enumeratePredicate,
                                                   System.Action<System.Type> callback,
                                                   System.Type baseType = null,
                                                   System.Type defaultType = null,
                                                   TypeDropDownListingStyle listType = TypeDropDownListingStyle.Flat,
                                                   System.Func<System.Type, string, bool> searchFilter = null,
                                                   int maxVisibleCount = DEFAULT_MAXCOUNT)
        {
            GenericSearchDropDownWindow.ShowAndCallbackOnSelect(controlId, positionUnder, null, (o) => callback(o as System.Type), new TypeDropDownWindowSelector()
            {
                _baseType = baseType,
                _enumeratePredicate = enumeratePredicate,
                _defaultType = defaultType,
                _listStyle = listType,
                _searchFilter = searchFilter,
                MaxCount = maxVisibleCount,
            });
        }

        public static System.Predicate<System.Type> CreateEnumeratePredicate(System.Type baseType, bool allowAbstractTypes = false, bool allowInterfaces = false, bool allowGeneric = false, System.Type[] excludedTypes = null)
        {
            return new System.Predicate<System.Type>(tp =>
            {
                if (tp == null) return false;

                if (!TypeUtil.IsType(tp, baseType)) return false;
                if (!allowInterfaces && tp.IsInterface) return false;
                if (!allowAbstractTypes && tp.IsAbstract && !tp.IsInterface) return false;
                if (!allowGeneric && tp.IsGenericType) return false;
                if (excludedTypes != null && excludedTypes.IndexOf(tp) >= 0) return false;

                return true;
            });
        }

        public static System.Predicate<System.Type> CreateEnumeratePredicate(System.Type[] baseTypes, bool allowAbstractTypes = false, bool allowInterfaces = false, bool allowGeneric = false, System.Type[] excludedTypes = null)
        {
            return new System.Predicate<System.Type>(tp =>
            {
                if (tp == null) return false;

                if (!TypeUtil.IsType(tp, baseTypes)) return false;
                if (!allowInterfaces && tp.IsInterface) return false;
                if (!allowAbstractTypes && tp.IsAbstract && !tp.IsInterface) return false;
                if (!allowGeneric && tp.IsGenericType) return false;
                if (excludedTypes != null && excludedTypes.IndexOf(tp) >= 0) return false;

                return true;
            });
        }

        #endregion

    }

    public class UnityObjectDropDownWindowSelector : ISearchDropDownSelector
    {

        public const int DEFAULT_MAXCOUNT = 100;

        private System.Func<IEnumerable<UnityEngine.Object>> _retrieveObjectsCallback;

        #region Properties

        public virtual System.Type TargetType => typeof(UnityEngine.Object);

        #endregion

        #region ISelector Interface

        public int MaxCount { get; set; } = DEFAULT_MAXCOUNT;

        public virtual new bool Equals(object x, object y)
        {
            return (x as UnityEngine.Object) == (y as UnityEngine.Object);
        }

        public int GetHashCode(object obj)
        {
            return (obj as UnityEngine.Object)?.GetHashCode() ?? 0;
        }

        public GUIContent GetLabel(object element)
        {
            var obj = element as UnityEngine.Object;

            if (obj == null)
            {
                return new GUIContent("Nothing...");
            }
            else
            {
                if (AssetDatabase.Contains(obj))
                {
                    return new GUIContent("asset: " + obj.name);
                }
                else
                {
                    return new GUIContent(obj.name);
                }
            }
        }

        public IEnumerable<SearchDropDownElement> GetElements(GenericSearchDropDownWindow window)
        {
            window.Header = this.TargetType.Name ?? "Object";

            if (_retrieveObjectsCallback == null)
            {
                return Enumerable.Empty<SearchDropDownElement>().Prepend(new SearchDropDownElement()
                {
                    Content = GetLabel(null),
                    Element = null,
                });
            }

            if (string.IsNullOrEmpty(window.CurrentSearch))
            {
                return _retrieveObjectsCallback().Select(o => new SearchDropDownElement()
                {
                    Content = GetLabel(o),
                    Element = o
                }).Prepend(new SearchDropDownElement()
                {
                    Content = GetLabel(null),
                    Element = null,
                });
            }
            else
            {
                return _retrieveObjectsCallback()
                    .Where(o => (o is UnityEngine.Object uo) && uo.name.IndexOf(window.CurrentSearch, 0, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(o => new SearchDropDownElement()
                    {
                        Content = GetLabel(o),
                        Element = o
                    }).Prepend(new SearchDropDownElement()
                    {
                        Content = GetLabel(null),
                        Element = null,
                    });
            }
        }

        #endregion

        #region Static Factory

        public static T Popup<T>(Rect position, GUIContent label, T currentElement, System.Func<IEnumerable<T>> getobjectscallback, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            if (typeof(T) == typeof(UnityEngine.Object))
            {
                return GenericSearchDropDownWindow.Popup(position, label, currentElement, new UnityObjectDropDownWindowSelector()
                {
                    _retrieveObjectsCallback = (System.Func<IEnumerable<UnityEngine.Object>>)getobjectscallback,
                    MaxCount = maxVisibleCount,
                }) as T;
            }
            else
            {
                return GenericSearchDropDownWindow.Popup(position, label, currentElement, new UnityObjectDropDownWindowSelector()
                {
                    _retrieveObjectsCallback = () => getobjectscallback().OfType<UnityEngine.Object>(),
                    MaxCount = maxVisibleCount,
                }) as T;
            }
        }

        public static void ShowAndCallbackOnSelect<T>(int controlId, Rect positionUnder, T currentElement, System.Func<IEnumerable<T>> getobjectscallback, System.Action<T> callback, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            if (typeof(T) == typeof(UnityEngine.Object))
            {
                GenericSearchDropDownWindow.ShowAndCallbackOnSelect(controlId, positionUnder, currentElement, (o) => callback(o as T), new UnityObjectDropDownWindowSelector()
                {
                    _retrieveObjectsCallback = (System.Func<IEnumerable<UnityEngine.Object>>)getobjectscallback,
                    MaxCount = maxVisibleCount,
                });
            }
            else
            {
                GenericSearchDropDownWindow.ShowAndCallbackOnSelect(controlId, positionUnder, currentElement, (o) => callback(o as T), new UnityObjectDropDownWindowSelector()
                {
                    _retrieveObjectsCallback = () => getobjectscallback().OfType<UnityEngine.Object>(),
                    MaxCount = maxVisibleCount,
                });
            }
        }

        #endregion

        #region Special Object Field

        public static UnityEngine.Object ObjectField(Rect position, GUIContent label, UnityEngine.Object asset, System.Type objType, bool allowSceneObjects, bool allowProxy, System.Predicate<UnityEngine.Object> filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT)
        {
            if (objType == null) throw new System.ArgumentNullException(nameof(objType));
            if (!objType.IsInterface && !TypeUtil.IsType(objType, typeof(UnityEngine.Object))) throw new System.ArgumentException("Type must be an interface or UnityEngine.Object", nameof(objType));

            return DoObjectField<UnityEngine.Object>(position, label, asset, objType, allowSceneObjects, allowProxy, null, filter, maxVisibleCount);
        }

        public static T ObjectField<T>(Rect position, GUIContent label, SerializedProperty property, bool allowSceneObjects, System.Predicate<T> filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            return DoObjectField<T>(position, label, property.objectReferenceValue as T, typeof(T), allowSceneObjects, false, (o) =>
            {
                property.objectReferenceValue = o as UnityEngine.Object;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }, filter, maxVisibleCount);
        }

        public static T ObjectField<T>(Rect position, GUIContent label, T asset, bool allowSceneObjects, System.Predicate<T> filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            return DoObjectField<T>(position, label, asset, typeof(T), allowSceneObjects, false, null, filter, maxVisibleCount);
        }

        private static T DoObjectField<T>(Rect position, GUIContent label, T asset, System.Type objType, bool allowSceneObjects, bool allowProxy, System.Action<T> dropdownselectedcallback, System.Predicate<T> filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
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
                Texture2D assetIcon = null;
                if (AssetDatabase.Contains(oasset))
                {
                    assetIcon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(oasset)) as Texture2D;
                }
                else if (go = GameObjectUtil.GetGameObjectFromSource(oasset))
                {
                    assetIcon = PrefabUtility.GetIconForGameObject(go);
                }

                position = EditorGUI.PrefixLabel(position, controlId, label);
                UnityEngine.GUI.Box(position, new GUIContent(oasset?.name, assetIcon), EditorStyles.objectField);

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
                UnityEngine.GUI.Box(position, new GUIContent(string.Format("None ({0})", objType?.Name ?? "Object")), EditorStyles.objectField);
            }

#if UNITY_2019_1_OR_NEWER
            DrawCaret(pickerRect);
#endif

            if (isPickerPressed && !GenericSearchDropDownWindow.WindowActive)
            {
                System.Func<IEnumerable<UnityEngine.Object>> retrieveobjscallback;
                //if (allowSceneObjects)
                //{
                //    retrieveobjscallback = () =>
                //    {
                //        var results = GenericSearchDropDownWindow.GetSceneObjects<T>();
                //        if (objType != typeof(T)) results = results.Where(o => ObjUtil.GetAsFromSource(objType, o) != null);
                //        results = results.Union(AssetDatabase.FindAssets("a:assets")
                //                       .Select(s => ObjUtil.GetAsFromSource(objType, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(UnityEngine.Object))) as T)
                //                       .Where(o => o != null));
                //        if (filter != null) results = results.Where(o => filter(o));
                //        return results.Cast<UnityEngine.Object>();
                //    };
                //}
                //else
                //{
                //    retrieveobjscallback = () =>
                //    {
                //        IEnumerable<T> results = AssetDatabase.FindAssets("a:assets")
                //                                  .Select(s => ObjUtil.GetAsFromSource(objType, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(UnityEngine.Object))) as T)
                //                                  .Where(o => o != null);
                //        if (filter != null) results = results.Where(o => filter(o));
                //        return results.Cast<UnityEngine.Object>();
                //    };
                //}

                retrieveobjscallback = () =>
                {
                    var types = allowProxy ? new System.Type[] { objType, typeof(IProxy) } : null;
                    IEnumerable<UnityEngine.Object> sceneresults = null;
                    if (allowSceneObjects)
                    {
                        if (allowProxy)
                        {
                            sceneresults = GenericSearchDropDownWindow.GetSceneObjects<UnityEngine.Object>()
                                                                 .Where(o => ObjUtil.GetAsFromSource(types, o) != null);
                        }
                        else
                        {
                            sceneresults = GenericSearchDropDownWindow.GetSceneObjects<T>().Cast<UnityEngine.Object>();
                            if (objType != typeof(T)) sceneresults = sceneresults.Where(o => ObjUtil.GetAsFromSource(objType, o) != null);
                        }
                    }

                    IEnumerable<UnityEngine.Object> results;
                    if (allowProxy)
                    {

                        results = AssetDatabase.FindAssets("a:assets")
                                               .Select(s => ObjUtil.GetAsFromSource(types, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(UnityEngine.Object))) as UnityEngine.Object)
                                               .Where(o => o != null);
                    }
                    else
                    {
                        results = AssetDatabase.FindAssets("a:assets")
                                               .Select(s => ObjUtil.GetAsFromSource(objType, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(UnityEngine.Object))) as UnityEngine.Object)
                                               .Where(o => o != null);
                    }

                    if (sceneresults != null)
                    {
                        results = sceneresults.Union(results);
                    }

                    return results;
                };


                GenericSearchDropDownWindow.ShowAndCallbackOnSelect(controlId, position, asset,
                                                                    (dropdownselectedcallback != null) ? (o) => dropdownselectedcallback(o as T) : (System.Action<object>)null,
                                                                    new UnityObjectDropDownWindowSelector()
                                                                    {
                                                                        _retrieveObjectsCallback = retrieveobjscallback,
                                                                        MaxCount = maxVisibleCount,
                                                                    });
            }

            var newobj = HandleDragAndDrop(isDragging, isDropping, asset as UnityEngine.Object, objType, allowSceneObjects, allowProxy) as T;
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

        private static UnityEngine.Object HandleDragAndDrop(bool isDragging, bool isDropping, UnityEngine.Object asset, System.Type objType, bool allowSceneObjects, bool allowProxy)
        {
            if (!isDragging && !isDropping) return asset;

            var types = allowProxy ? ArrayUtil.Temp<System.Type>(objType, typeof(IProxy)) : ArrayUtil.Temp<System.Type>(objType);
            try
            {
                bool validDrag;
                if (allowSceneObjects)
                {
                    validDrag = DragAndDrop.objectReferences.Any(o => ObjUtil.GetAsFromSource(types, o) != null);
                }
                else
                {
                    validDrag = DragAndDrop.objectReferences.Any(o => ObjUtil.GetAsFromSource(types, o) != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)));
                }

                if (isDragging)
                {
                    DragAndDrop.visualMode = !validDrag ? DragAndDropVisualMode.Rejected : DragAndDropVisualMode.Copy;
                }

                if (validDrag && isDropping)
                {
                    var entry = DragAndDrop.objectReferences.Select(o => ObjUtil.GetAsFromSource(types, o) as UnityEngine.Object).FirstOrDefault(o => o != null);
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

        #region Special Types

        #endregion

    }

}
