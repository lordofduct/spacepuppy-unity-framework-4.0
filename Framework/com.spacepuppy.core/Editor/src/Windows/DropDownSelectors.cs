using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Windows
{

    public class TypeDropDownWindowSelector : ISearchDropDownSelector
    {

        public const int DEFAULT_MAXCOUNT = 100;

        #region Fields

        private System.Type _defaultType = null;
        private System.Type _baseType = typeof(object);
        private TypeDropDownListingStyle _listStyle;
        private IEnumerable<System.Type> _typeEnumerator;
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
                    content = GetLabel(null)
                };
            }

            var match = window.CurrentSearch ?? string.Empty;
            IEnumerable<SearchDropDownElement> results;
            if (_searchFilter != null)
            {
                window.Header = (_baseType == typeof(object) || _baseType == null) ? "Types" : _baseType.Name + "/s";

                results = _typeEnumerator
                                .Where(tp => _searchFilter(tp, match))
                                .Select(tp => new SearchDropDownElement()
                                {
                                    content = GetLabel(tp),
                                    searchReference = new SearchReference(tp)
                                });
            }
            else if (string.IsNullOrEmpty(window.CurrentSearch))
            {
                window.Header = (_baseType == typeof(object) || _baseType == null) ? "Types" : _baseType.Name + "/s";

                results = _typeEnumerator
                                .Select(tp => new SearchDropDownElement()
                                {
                                    content = GetLabel(tp),
                                    searchReference = new SearchReference(tp)
                                });
            }
            else
            {
                window.Header = (_baseType == typeof(object) || _baseType == null) ? "Filtered Types" : "Filtered " + _baseType.Name + "/s";

                results = _typeEnumerator
                                .Where(tp => tp.Name.IndexOf(match, 0, System.StringComparison.OrdinalIgnoreCase) >= 0)
                                .Select(tp => new SearchDropDownElement()
                                {
                                    content = GetLabel(tp),
                                    searchReference = new SearchReference(tp)
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
                                        IEnumerable<System.Type> typeEnumerator,
                                        System.Type baseType = null,
                                        System.Type defaultType = null,
                                        TypeDropDownListingStyle listType = TypeDropDownListingStyle.Flat,
                                        System.Func<System.Type, string, bool> searchFilter = null,
                                        int maxVisibleCount = DEFAULT_MAXCOUNT)
        {
            return GenericSearchDropDownWindow.Popup(position, label, selectedType, new TypeDropDownWindowSelector()
            {
                _baseType = baseType,
                _typeEnumerator = typeEnumerator ?? TypeUtil.GetTypes(),
                _defaultType = defaultType,
                _listStyle = listType,
                _searchFilter = searchFilter,
                MaxCount = maxVisibleCount,
            }) as System.Type;
        }

        public static void ShowAndCallbackOnSelect(int controlId, Rect positionUnder, System.Type selectedType,
                                        IEnumerable<System.Type> typeEnumerator,
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
                _typeEnumerator = typeEnumerator ?? TypeUtil.GetTypes(),
                _defaultType = defaultType,
                _listStyle = listType,
                _searchFilter = searchFilter,
                MaxCount = maxVisibleCount,
            });
        }

        public static System.Type Popup(Rect position, GUIContent label,
                                        System.Type selectedType,
                                        System.Func<System.Type, bool> enumeratePredicate,
                                        System.Type baseType = null,
                                        System.Type defaultType = null,
                                        TypeDropDownListingStyle listType = TypeDropDownListingStyle.Flat,
                                        System.Func<System.Type, string, bool> searchFilter = null,
                                        int maxVisibleCount = DEFAULT_MAXCOUNT)
        {
            return GenericSearchDropDownWindow.Popup(position, label, selectedType, new TypeDropDownWindowSelector()
            {
                _baseType = baseType,
                _typeEnumerator = TypeUtil.GetTypes(enumeratePredicate),
                _defaultType = defaultType,
                _listStyle = listType,
                _searchFilter = searchFilter,
                MaxCount = maxVisibleCount,
            }) as System.Type;
        }

        public static void ShowAndCallbackOnSelect(int controlId, Rect positionUnder, System.Type selectedType,
                                                   System.Func<System.Type, bool> enumeratePredicate,
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
                _typeEnumerator = TypeUtil.GetTypes(enumeratePredicate),
                _defaultType = defaultType,
                _listStyle = listType,
                _searchFilter = searchFilter,
                MaxCount = maxVisibleCount,
            });
        }

        public static IEnumerable<System.Type> CreateTypeEnumerator(System.Type baseType, bool allowAbstractTypes = false, bool allowInterfaces = false, bool allowGeneric = false, System.Type[] excludedTypes = null)
        {
            return TypeUtil.GetTypes(tp =>
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

        public static IEnumerable<System.Type> CreateTypeEnumerator(System.Type[] baseTypes, bool allowAbstractTypes = false, bool allowInterfaces = false, bool allowGeneric = false, System.Type[] excludedTypes = null)
        {
            return TypeUtil.GetTypes(tp =>
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

        private System.Type _targetType;
        private System.Func<IEnumerable<SearchReference>> _retrieveSearchReferencesCallback;

        #region Properties

        public virtual System.Type TargetType => _targetType ?? typeof(UnityEngine.Object);

        #endregion

        #region ISelector Interface

        public int MaxCount { get; set; } = DEFAULT_MAXCOUNT;

        public virtual new bool Equals(object x, object y)
        {
            //return (x as UnityEngine.Object) == (y as UnityEngine.Object);
            return x?.Equals(y) ?? false;
        }

        public int GetHashCode(object obj)
        {
            //return (obj as UnityEngine.Object)?.GetHashCode() ?? 0;
            return obj?.GetHashCode() ?? 0;
        }

        public GUIContent GetLabel(object element)
        {
            switch (element)
            {
                case SearchReference sref:
                    if (sref.HasReference)
                    {
                        var content = EditorHelper.ObjectContent(null, sref.Type, true);
                        content.text = sref.Name;
                        return content;
                    }
                    else
                    {
                        return new GUIContent("Nothing...");
                    }
                case UnityEngine.Object uo:
                    return EditorHelper.ObjectContent(uo, this.TargetType, true);
                default:
                    return new GUIContent("Nothing...");
            }
        }

        public IEnumerable<SearchDropDownElement> GetElements(GenericSearchDropDownWindow window)
        {
            window.Header = this.TargetType.Name ?? "Object";

            if (_retrieveSearchReferencesCallback == null)
            {
                return Enumerable.Empty<SearchDropDownElement>().Prepend(new SearchDropDownElement()
                {
                    content = GetLabel(null),
                    searchReference = SearchReference.Empty
                });
            }

            var elements = _retrieveSearchReferencesCallback().Where(o => o.HasReference);
            if (!string.IsNullOrEmpty(window.CurrentSearch))
            {
                elements = elements.Where(o => o.Name.IndexOf(window.CurrentSearch, 0, System.StringComparison.OrdinalIgnoreCase) >= 0);
            }
            elements = elements.OrderBy(o => this.TargetType != null && TypeUtil.IsType(o.Type, this.TargetType) ? 0 : 1).ThenBy(o => o.Name);
            return elements.Select(o => new SearchDropDownElement()
            {
                content = GetLabel(o),
                searchReference = o
            }).Prepend(new SearchDropDownElement()
            {
                content = GetLabel(null),
                searchReference = SearchReference.Empty
            });
        }

        #endregion

        #region Static Factory

        public static T Popup<T>(Rect position, GUIContent label, T currentElement, System.Func<IEnumerable<T>> getobjectscallback, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            return GenericSearchDropDownWindow.Popup(position, label, currentElement, new UnityObjectDropDownWindowSelector()
            {
                _retrieveSearchReferencesCallback = () => getobjectscallback().OfType<UnityEngine.Object>().Select(o => new SearchReference(o)),
                MaxCount = maxVisibleCount,
            }) as T;
        }

        public static void ShowAndCallbackOnSelect<T>(int controlId, Rect positionUnder, T currentElement, System.Func<IEnumerable<T>> getobjectscallback, System.Action<T> callback, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {

            GenericSearchDropDownWindow.ShowAndCallbackOnSelect(controlId, positionUnder, currentElement, (o) => callback(o as T), new UnityObjectDropDownWindowSelector()
            {
                _retrieveSearchReferencesCallback = () => getobjectscallback().OfType<UnityEngine.Object>().Select(o => new SearchReference(o)),
                MaxCount = maxVisibleCount,
            });
        }

        #endregion

        #region Special Object Field

        public static UnityEngine.Object ObjectField(Rect position, GUIContent label, UnityEngine.Object asset, System.Type objType, bool allowSceneObjects, bool allowProxy, SearchFilter filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT)
        {
            if (objType == null) throw new System.ArgumentNullException(nameof(objType));
            if (!objType.IsInterface && !TypeUtil.IsType(objType, typeof(UnityEngine.Object))) throw new System.ArgumentException("Type must be an interface or UnityEngine.Object", nameof(objType));

            return DoObjectField<UnityEngine.Object>(position, label, asset, objType, allowSceneObjects, allowProxy, null, filter, maxVisibleCount);
        }

        public static T ObjectField<T>(Rect position, GUIContent label, SerializedProperty property, bool allowSceneObjects, SearchFilter filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            return DoObjectField<T>(position, label, property.objectReferenceValue as T, typeof(T), allowSceneObjects, false, (o) =>
            {
                property.objectReferenceValue = o as UnityEngine.Object;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }, filter, maxVisibleCount);
        }

        public static T ObjectField<T>(Rect position, GUIContent label, T asset, bool allowSceneObjects, SearchFilter filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            return DoObjectField<T>(position, label, asset, typeof(T), allowSceneObjects, false, null, filter, maxVisibleCount);
        }

        private static T DoObjectField<T>(Rect position, GUIContent label, T asset, System.Type objType, bool allowSceneObjects, bool allowProxy, System.Action<T> dropdownselectedcallback, SearchFilter filter = null, int maxVisibleCount = DEFAULT_MAXCOUNT) where T : class
        {
            var helper = new GenericSearchDropDownObjectFieldHelper<T>()
            {
                ObjectType = objType,
                Filter = filter,
                AllowSceneObjects = allowSceneObjects,
                AllowProxy = allowProxy,
                ShowDropDownCallback = (controlId, p, a) =>
                {
                    System.Func<IEnumerable<SearchReference>> retrieveobjscallback = () =>
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

                        var infos = SpaceuppyAssetDatabase.GetAssetInfos();
                        if (allowProxy)
                        {
                            infos = infos.Where(o => o.SupportsType(objType) || o.SupportsType(typeof(IProxy)));
                        }
                        else
                        {
                            infos = infos.Where(o => o.SupportsType(objType));
                        }

                        var results = infos.Select(o => new SearchReference(o));

                        if (sceneresults != null)
                        {
                            results = results.Union(sceneresults.Select(o => new SearchReference(o)));
                        }

                        if (filter != null)
                        {
                            results = results.SelectIf((SearchReference o, out SearchReference ot) =>
                            {
                                ot = o;
                                return filter(ref ot);
                            });
                        }
                        else
                        {
                            results = (from o in results
                                       where o.TryConvert(typeof(T))
                                       select o);
                        }

                        return results;
                    };


                    GenericSearchDropDownWindow.ShowAndCallbackOnSelect(controlId, p, a,
                                                                        (dropdownselectedcallback != null) ? (o) => dropdownselectedcallback(o as T) : (System.Action<object>)null,
                                                                        new UnityObjectDropDownWindowSelector()
                                                                        {
                                                                            _targetType = objType,
                                                                            _retrieveSearchReferencesCallback = retrieveobjscallback,
                                                                            MaxCount = maxVisibleCount,
                                                                        });
                },
            };
            return helper.DrawObjectField(position, label, asset);
        }

        #endregion

    }

    public class ComponentDropDownWindowSelector<T> : GenericSearchDropDownObjectFieldHelper<T>, ISearchDropDownSelector where T : class
    {

        public static readonly ObjectBoxLabelFormatterDelegate DefaultComponentBoxLabelFormatter = (a, tp) =>
        {
            switch (a)
            {
                case GameObject go:
                    return go.name;
                case Component c:
                    return string.Format("{0} : {1}", c.name, c.GetType().Name);
                case IProxy p:
                    return p is UnityEngine.Object uo ? string.Format("{0} (IProxy)", uo.name) : string.Format("{0} (IProxy)", p.GetType().Name);
                default:
                    return GenericSearchDropDownObjectFieldHelper<UnityEngine.Object>.DefaultObjectBoxLabelFormatter(null, tp);
            }
        };

        #region CONSTRUCTOR

        public ComponentDropDownWindowSelector()
        {
            this.ObjectBoxLabelFormatter = DefaultComponentBoxLabelFormatter;
            this.ShowDropDownCallback = (controlId, p, a) =>
            {
                GenericSearchDropDownWindow.ShowAndCallbackOnSelect(controlId, p, a, null, this);
            };
        }

        #endregion

        #region Properties/Fields

        private T _target;
        private GameObject _targetGameObject;

        public bool IncludeGameObject { get; set; }

        public System.Func<Component, bool> ComponentFilterPredicate { get; set; }

        public override ObjectBoxLabelFormatterDelegate ObjectBoxLabelFormatter
        {
            get => base.ObjectBoxLabelFormatter;
            set => base.ObjectBoxLabelFormatter = value ?? DefaultComponentBoxLabelFormatter;
        }

        #endregion

        #region Methods

        public override T DrawObjectField(Rect position, GUIContent label, T asset)
        {
            _target = asset;
            _targetGameObject = GameObjectUtil.GetGameObjectFromSource(_target);
            _target = base.DrawObjectField(position, label, _target);
            _targetGameObject = GameObjectUtil.GetGameObjectFromSource(_target);

            if (_targetGameObject == null && _target != null)
            {
                if (!this.AllowProxy || !(_target is IProxy))
                {
                    _target = null;
                }
            }
            return _target;
        }

        #endregion

        #region ISelector Interface

        public int MaxCount { get; set; } = 100;

        public virtual new bool Equals(object x, object y)
        {
            return (x as UnityEngine.Object) == (y as UnityEngine.Object);
        }

        public int GetHashCode(object obj)
        {
            return (obj as UnityEngine.Object)?.GetHashCode() ?? 0;
        }

        public IEnumerable<SearchDropDownElement> GetElements(GenericSearchDropDownWindow window)
        {
            yield return new SearchDropDownElement()
            {
                content = EditorHelper.TempContent("Nothing..."),
                searchReference = SearchReference.Empty
            };

            if (_targetGameObject == null)
            {
                if (this.AllowProxy && _target is IProxy && _target is UnityEngine.Object uo)
                {
                    yield return new SearchDropDownElement()
                    {
                        content = EditorHelper.TempContent(string.Format("{0} (IProxy)", uo.name)),
                        searchReference = new SearchReference(_target as UnityEngine.Object)
                    };
                    yield break;
                }
                else
                {
                    yield break;
                }
            }

            string sname = _targetGameObject.name;
            if (this.IncludeGameObject)
            {
                yield return new SearchDropDownElement()
                {
                    content = EditorHelper.TempContent(sname),
                    searchReference = new SearchReference(_targetGameObject)
                };
            }

            if (string.IsNullOrEmpty(window.CurrentSearch))
            {
                int i = 0;
                foreach (var c in _targetGameObject.GetComponents(typeof(Component)))
                {
                    if (this.ComponentFilterPredicate != null && !this.ComponentFilterPredicate(c)) continue;

                    i++;
                    yield return new SearchDropDownElement()
                    {
                        content = EditorHelper.TempContent(string.Format("{0} : {1} [{2}]", sname, c.GetType().Name, i)),
                        searchReference = new SearchReference(c)
                    };
                }
            }
            else
            {
                int i = 0;
                foreach (var c in _targetGameObject.GetComponents(typeof(Component)))
                {
                    if (this.ComponentFilterPredicate != null && !this.ComponentFilterPredicate(c)) continue;

                    i++;

                    if (c.GetType().Name.IndexOf(window.CurrentSearch, 0, System.StringComparison.OrdinalIgnoreCase) < 0) continue;

                    yield return new SearchDropDownElement()
                    {
                        content = EditorHelper.TempContent(string.Format("{0} : {1} [{2}]", sname, c.GetType().Name, i)),
                        searchReference = new SearchReference(c)
                    };
                }
            }
        }

        public GUIContent GetLabel(object element)
        {
            return null;
        }

        #endregion

    }

}
