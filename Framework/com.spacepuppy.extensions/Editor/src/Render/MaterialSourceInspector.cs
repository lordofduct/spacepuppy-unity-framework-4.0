using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Render;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Render
{

    [CustomEditor(typeof(MaterialSource), true)]
    [CanEditMultipleObjects]
    public class MaterialSourceInspector : SPEditor
    {

        public const string PROP_FORWARDEDMATPROPS = "_forwardedMaterialProps";

        private ReorderableList _lstDrawer;
        private string[] _options;
        private GUIContent[] _displayOptions;
        private MaterialPropertyValueType[] _valueTypes;

        protected override void OnEnable()
        {
            base.OnEnable();

            _lstDrawer = new ReorderableList(this.serializedObject, this.serializedObject.FindProperty(PROP_FORWARDEDMATPROPS))
            {
                draggable = true,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = _lstDrawer_DrawHeader,
                drawElementCallback = _lstDrawer_DrawElement
            };
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawDefaultInspectorExcept(PROP_FORWARDEDMATPROPS);
            this.DrawForwardedMaterialProps();

            this.serializedObject.ApplyModifiedProperties();
        }

        protected void DrawForwardedMaterialProps()
        {
            if(_lstDrawer != null)
            {
                try
                {
                    this.GetInfoAndContent();
                    _lstDrawer.DoLayoutList();
                }
                finally
                {
                    _valueTypes = null;
                    _options = null;
                    _displayOptions = null;
                }
            }
        }

        private void GetInfoAndContent()
        {
            if (this.serializedObject.isEditingMultipleObjects)
            {
                _valueTypes = ArrayUtil.Empty<MaterialPropertyValueType>();
                _options = ArrayUtil.Empty<string>();
                _displayOptions = new GUIContent[] { new GUIContent("Custom...") };
            }
            else
            {
                var mat = (this.target as MaterialSource)?.Material;
                _valueTypes = null;
                _options = null;
                _displayOptions = null;

                if (mat != null && mat.shader != null)
                {
                    int cnt = ShaderUtil.GetPropertyCount(mat.shader);
                    using (var infoLst = TempCollection.GetList<MaterialSource.ForwardedMaterialProperty>(cnt))
                    using (var contentLst = TempCollection.GetList<GUIContent>(cnt))
                    {
                        for (int i = 0; i < cnt; i++)
                        {
                            var nm = ShaderUtil.GetPropertyName(mat.shader, i);
                            var tp = ShaderUtil.GetPropertyType(mat.shader, i);

                            switch (tp)
                            {
                                case ShaderUtil.ShaderPropertyType.Float:
                                    {
                                        infoLst.Add(new MaterialSource.ForwardedMaterialProperty(nm, MaterialPropertyValueType.Float));
                                        contentLst.Add(EditorHelper.TempContent(nm + " (float)"));
                                    }
                                    break;
                                case ShaderUtil.ShaderPropertyType.Range:
                                    {
                                        infoLst.Add(new MaterialSource.ForwardedMaterialProperty(nm, MaterialPropertyValueType.Float));
                                        var min = ShaderUtil.GetRangeLimits(mat.shader, i, 1);
                                        var max = ShaderUtil.GetRangeLimits(mat.shader, i, 2);
                                        contentLst.Add(EditorHelper.TempContent(string.Format("{0} (Range [{1}, {2}]])", nm, min, max)));
                                    }
                                    break;
                                case ShaderUtil.ShaderPropertyType.Color:
                                    {
                                        infoLst.Add(new MaterialSource.ForwardedMaterialProperty(nm, MaterialPropertyValueType.Color));
                                        contentLst.Add(EditorHelper.TempContent(nm + " (color)"));
                                    }
                                    break;
                                case ShaderUtil.ShaderPropertyType.Vector:
                                    {
                                        infoLst.Add(new MaterialSource.ForwardedMaterialProperty(nm, MaterialPropertyValueType.Vector));
                                        contentLst.Add(EditorHelper.TempContent(nm + " (vector)"));
                                    }
                                    break;
                            }
                        }
                        _displayOptions = contentLst.Append(EditorHelper.TempContent("Custom...")).ToArray();
                        _options = infoLst.Select(o => o.Name).ToArray();
                        _valueTypes = infoLst.Select(o => o.ValueType).ToArray();
                    }
                }
            }
        }

        #region List Drawer Methods

        private void _lstDrawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Available Material Props");
        }

        private void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var el = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index);
            var nameProp = el.FindPropertyRelative(nameof(MaterialSource.ForwardedMaterialProperty.Name));
            var valueTypeProp = el.FindPropertyRelative(nameof(MaterialSource.ForwardedMaterialProperty.ValueType));

            int selectedIndex = _options?.IndexOf(nameProp.stringValue) ?? -1;
            var r0 = new Rect(area.xMin, area.yMin, area.width / 2f, area.height);
            var r1 = new Rect(r0.xMax, r0.yMin, r0.width, r0.height);

            EditorGUI.BeginChangeCheck();
            nameProp.stringValue = SPEditorGUI.OptionPopupWithCustom(r0, GUIContent.none, nameProp.stringValue, _options, _displayOptions);
            if (EditorGUI.EndChangeCheck())
            {
                selectedIndex = _options?.IndexOf(nameProp.stringValue) ?? -1;
                if(selectedIndex >= 0 && selectedIndex < _options.Length)
                {
                    valueTypeProp.SetEnumValue(_valueTypes[selectedIndex]);
                }
            }

            if (selectedIndex < 0)
            {
                var eval = SPEditorGUI.EnumPopup(r1, valueTypeProp.GetEnumValue<MaterialPropertyValueType>());
                valueTypeProp.SetEnumValue(eval);
            }
            else
            {
                var enumDisplayText = EnumUtil.GetFriendlyName(valueTypeProp.GetEnumValue<MaterialPropertyValueType>());
                var cache = SPGUI.Disable();
                EditorGUI.LabelField(r1, enumDisplayText, EditorStyles.textField);
                cache.Reset();
            }
        }

        #endregion

    }

    [CustomEditor(typeof(RendererMaterialSource))]
    [CanEditMultipleObjects]
    public class RendererMaterialSourceInspector : MaterialSourceInspector
    {

        public const string PROP_RENDERER = "_renderer";
        public const string PROP_MODE = "_mode";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            this.DrawDefaultMaterialSourceInspector();
            
            this.DrawForwardedMaterialProps();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_RENDERER, PROP_MODE, PROP_FORWARDEDMATPROPS);

            this.serializedObject.ApplyModifiedProperties();
        }

        protected void DrawDefaultMaterialSourceInspector()
        {
            if (this.serializedObject.isEditingMultipleObjects)
            {
                var cache = SPGUI.Disable();
                this.DrawPropertyField(PROP_RENDERER);
                cache.Reset();
                this.DrawPropertyField(PROP_MODE);
                return;
            }
            
            var prop = this.serializedObject.FindProperty(PROP_RENDERER);
            var source = this.target as RendererMaterialSource;
            var go = GameObjectUtil.GetGameObjectFromSource(source);
            if(go == null)
            {
                EditorGUILayout.HelpBox("MaterialSource can not find target GameObject it's attached to.", MessageType.Error);
                return;
            }

            if(Application.isPlaying)
            {
                var cache = SPGUI.Disable();
                EditorGUILayout.ObjectField(prop.displayName, prop.objectReferenceValue, typeof(Renderer), true);
                this.DrawPropertyField(PROP_MODE);
                EditorGUILayout.Toggle("Is Unique", source.IsUnique);
                cache.Reset();
            }
            else
            {
                if (prop.objectReferenceValue != null && prop.objectReferenceValue is Renderer)
                {
                    var renderer = prop.objectReferenceValue as Renderer;
                    if (renderer.gameObject != go)
                    {
                        prop.objectReferenceValue = null;
                    }
                }

                var renderers = go.GetComponents<Renderer>();
                if (renderers.Length == 0)
                {
                    EditorGUILayout.HelpBox("MaterialSource can not find a Renderer on that GameObject it's attached to.", MessageType.Error);
                    return;
                }
                else
                {
                    var sources = go.GetComponents<RendererMaterialSource>();
                    if (sources.Length > renderers.Length)
                    {
                        Debug.LogWarning("There are too many MaterialSources attached to this GameObject. Removing extra.", go);
                        UnityEngine.Object.DestroyImmediate(this.target);
                        return;
                    }

                    renderers = renderers.Except((from s in sources where s.Renderer != null && s.Renderer != source.Renderer select s.Renderer)).ToArray();
                    var names = (from r in renderers select EditorHelper.TempContent( r.GetType().Name)).ToArray();
                    int index = renderers.IndexOf(source.Renderer);

                    index = EditorGUILayout.Popup(EditorHelper.TempContent(prop.displayName), index, names);
                    if (index >= 0)
                        prop.objectReferenceValue = renderers[index];
                    else
                        prop.objectReferenceValue = renderers.FirstOrDefault();
                }

                this.DrawPropertyField(PROP_MODE);
            }
        }

    }

    [CustomEditor(typeof(GraphicMaterialSource))]
    [CanEditMultipleObjects]
    public class GraphicMaterialSourceInspector : MaterialSourceInspector
    {

        public const string PROP_GRAPHICS = "_graphics";
        public const string PROP_MODE = "_mode";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            this.DrawDefaultMaterialSourceInspector();
            this.DrawForwardedMaterialProps();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_GRAPHICS, PROP_MODE, PROP_FORWARDEDMATPROPS);

            this.serializedObject.ApplyModifiedProperties();
        }

        protected void DrawDefaultMaterialSourceInspector()
        {
            if (this.serializedObject.isEditingMultipleObjects)
            {
                var cache = SPGUI.Disable();
                this.DrawPropertyField(PROP_GRAPHICS);
                cache.Reset();
                this.DrawPropertyField(PROP_MODE);
                return;
            }

            var prop = this.serializedObject.FindProperty(PROP_GRAPHICS);
            var source = this.target as GraphicMaterialSource;
            var go = GameObjectUtil.GetGameObjectFromSource(source);
            if (go == null)
            {
                EditorGUILayout.HelpBox("MaterialSource can not find target GameObject it's attached to.", MessageType.Error);
                return;
            }

            if (Application.isPlaying)
            {
                var cache = SPGUI.Disable();
                EditorGUILayout.ObjectField(prop.displayName, prop.objectReferenceValue, typeof(UnityEngine.UI.Graphic), true);
                this.DrawPropertyField(PROP_MODE);
                EditorGUILayout.Toggle("Is Unique", source.IsUnique);
                cache.Reset();
            }
            else
            {
                if (prop.objectReferenceValue != null && prop.objectReferenceValue is UnityEngine.UI.Graphic)
                {
                    var renderer = prop.objectReferenceValue as UnityEngine.UI.Graphic;
                    if (renderer.gameObject != go)
                    {
                        prop.objectReferenceValue = null;
                    }
                }

                var graphics = go.GetComponents<UnityEngine.UI.Graphic>();
                if (graphics.Length == 0)
                {
                    EditorGUILayout.HelpBox("MaterialSource can not find a Renderer on that GameObject it's attached to.", MessageType.Error);
                    return;
                }
                else
                {
                    var sources = go.GetComponents<GraphicMaterialSource>();
                    if (sources.Length > graphics.Length)
                    {
                        Debug.LogWarning("There are too many MaterialSources attached to this GameObject. Removing extra.", go);
                        UnityEngine.Object.DestroyImmediate(this.target);
                        return;
                    }

                    graphics = graphics.Except((from s in sources where s.Graphics != null && s.Graphics != source.Graphics select s.Graphics)).ToArray();
                    var names = (from r in graphics select EditorHelper.TempContent(r.GetType().Name)).ToArray();
                    int index = graphics.IndexOf(source.Graphics);

                    index = EditorGUILayout.Popup(EditorHelper.TempContent(prop.displayName), index, names);
                    if (index >= 0)
                        prop.objectReferenceValue = graphics[index];
                    else
                        prop.objectReferenceValue = graphics.FirstOrDefault();
                }

                this.DrawPropertyField(PROP_MODE);
            }
        }

    }

}
