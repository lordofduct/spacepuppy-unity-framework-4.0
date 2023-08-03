#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Render
{

    /// <summary>
    /// A self-tracking Material source accessor. This allows you to track if a Material is using a shared material, or you had made it unique. 
    /// The default interface for accessing materials on a Renderer in Unity is rather confusing and it's hard to tell if it's shared or not. 
    /// This also leads to accidental memory consumption due to repeated access of the 'Renderer.material' method duplicates the material over 
    /// and over.
    /// 
    /// Instead with this, accessing the field 'Material' gives you the material assigned to the Renderer source. If you want to make that material 
    /// unique, you can call GetUniqueMaterial(). This will flag the MaterialSource as 'Unique'. If you happen to assign this material to another 
    /// Renderer, you can manually flag it back false implying it's shared (though not globally shared).
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public sealed class RendererMaterialSource : MaterialSource
    {

        #region Fields

        [SerializeField()]
        [DefaultFromSelf]
        [ForceFromSelf(EntityRelativity.Self)]
        private Renderer _renderer;
        [SerializeField()]
        private MaterialSourceUniquessModes _mode;

        [System.NonSerialized()]
        private Material[] _uniqueMaterials;
        [System.NonSerialized]
        private Material[] _sharedMaterials;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            if (object.ReferenceEquals(_renderer, null))
            {
                Debug.LogWarning("MaterialSource attached incorrectly to GameObject. Either attach MaterialSource at design time through editor, or call MaterialSource.GetMaterialSource.");
                UnityEngine.Object.Destroy(this);
            }
            else if (_renderer.gameObject != this.gameObject)
            {
                Debug.LogWarning("MaterialSource must be attached to the same GameObject as its Source Renderer.");
                UnityEngine.Object.Destroy(this);
            }

            if (_mode == MaterialSourceUniquessModes.MakeUniqueOnStart && !this.IsUnique)
            {
                this.GetUniqueMaterial();
            }

            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_uniqueMaterials != null)
            {
                for (int i = 0; i < _uniqueMaterials.Length; i++)
                {
                    if (_uniqueMaterials[i]) Destroy(_uniqueMaterials[i]);
                }
            }
        }

        #endregion

        #region Properties

        public Renderer Renderer
        {
            get { return _renderer; }
        }

        public MaterialSourceUniquessModes Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public override Material Material
        {
            get
            {
                if (!_renderer) return null;
#if UNITY_EDITOR
                if (!Application.isPlaying) return _renderer.sharedMaterial;
#endif

                switch (_mode)
                {
                    case MaterialSourceUniquessModes.UseSharedMaterial:
                        return _renderer.sharedMaterial;
                    case MaterialSourceUniquessModes.MakeUniqueOnStart:
                        if (!this.started)
                        {
                            return this.GetUniqueMaterial();
                        }
                        else
                        {
                            return _renderer.sharedMaterial;
                        }
                    case MaterialSourceUniquessModes.MakeUniqueOnAccess:
                        return this.GetUniqueMaterial();
                }
                return null;
            }
            set
            {
                this.SetMaterialAt(0, value);
            }
        }

        /// <summary>
        /// Does not reflect if the material is truly unique. But suggests the possibly of uniqueness if 
        /// 'GetUniqueMaterial' were called.
        /// </summary>
        public override bool IsUnique
        {
            get { return _uniqueMaterials != null; }
        }

        public int MaterialCount
        {
            get
            {
                if (!_renderer) return 0;
                if (_sharedMaterials == null) _sharedMaterials = _renderer.sharedMaterials;
                return _sharedMaterials.Length;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Replaces the material on the Renderer with a copy of itself so it's unique.
        /// </summary>
        /// <returns></returns>
        public override Material GetUniqueMaterial()
        {
            if (!_renderer) return null;

            if (_uniqueMaterials == null) _uniqueMaterials = _renderer.materials;
            _sharedMaterials = _uniqueMaterials;
            return _uniqueMaterials.Length > 0 ? _uniqueMaterials[0] : null;
        }

        public Material GetUniqueMaterialAt(int index)
        {
            if (!_renderer) return null;

            if (_uniqueMaterials == null) _uniqueMaterials = _renderer.materials;
            _sharedMaterials = _uniqueMaterials;
            return index >= 0 && index < _uniqueMaterials.Length ? _uniqueMaterials[index] : null;
        }

        public Material GetMaterialAt(int index)
        {
            if (!_renderer || index < 0) return null;
#if UNITY_EDITOR
            if (!Application.isPlaying) return _renderer.sharedMaterial;
#endif

            if (index == 0)
            {
                switch (_mode)
                {
                    case MaterialSourceUniquessModes.UseSharedMaterial:
                        return _renderer.sharedMaterial;
                    case MaterialSourceUniquessModes.MakeUniqueOnStart:
                        if (!this.started)
                        {
                            return this.GetUniqueMaterial();
                        }
                        else
                        {
                            return _renderer.sharedMaterial;
                        }
                    case MaterialSourceUniquessModes.MakeUniqueOnAccess:
                        return this.GetUniqueMaterial();
                }
            }
            else
            {
                switch (_mode)
                {
                    case MaterialSourceUniquessModes.UseSharedMaterial:
                        if (_sharedMaterials == null) _sharedMaterials = _renderer.sharedMaterials;
                        return index < _sharedMaterials.Length ? _sharedMaterials[index] : null;
                    case MaterialSourceUniquessModes.MakeUniqueOnStart:
                        if (!this.started)
                        {
                            return this.GetUniqueMaterialAt(index);
                        }
                        else
                        {
                            if (_sharedMaterials == null) _sharedMaterials = _renderer.sharedMaterials;
                            return index < _sharedMaterials.Length ? _sharedMaterials[index] : null;
                        }
                    case MaterialSourceUniquessModes.MakeUniqueOnAccess:
                        return this.GetUniqueMaterialAt(index);
                }
            }
            return null;
        }

        public void SetMaterialAt(int index, Material material)
        {
            if (!_renderer) return;

            if (index == 0)
            {
                if (_uniqueMaterials != null)
                {
                    _sharedMaterials = _uniqueMaterials;
                    if (_uniqueMaterials.Length > 0)
                    {
                        _uniqueMaterials[0] = material;
                        _renderer.materials = _uniqueMaterials;
                    }
                }
                else if (_sharedMaterials != null && _sharedMaterials.Length > 0)
                {
                    _sharedMaterials[0] = material;
                    _renderer.sharedMaterials = _sharedMaterials;
                }
                else
                {
                    _sharedMaterials = null;
                    _renderer.sharedMaterial = material;
                }
            }
            else if (index > 0)
            {
                if (_uniqueMaterials != null)
                {
                    _sharedMaterials = _uniqueMaterials;
                    if (index < _uniqueMaterials.Length)
                    {
                        _uniqueMaterials[index] = material;
                        _renderer.materials = _uniqueMaterials;
                    }
                }
                else if (_sharedMaterials != null && _sharedMaterials.Length > index)
                {
                    _sharedMaterials[index] = material;
                    _renderer.sharedMaterials = _sharedMaterials;
                }
                else
                {
                    _sharedMaterials = _renderer.sharedMaterials;
                    if (index < _sharedMaterials.Length)
                    {
                        _sharedMaterials[index] = material;
                        _renderer.sharedMaterials = _sharedMaterials;
                    }
                }
            }
        }

        #endregion

        #region Static Interface

        public static RendererMaterialSource GetMaterialSource(Renderer renderer, bool donotAddSourceIfNull = false)
        {
            if (!renderer) return null;

            using (var lst = TempCollection.GetList<RendererMaterialSource>())
            {
                renderer.GetComponents<RendererMaterialSource>(lst);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i]._renderer == renderer) return lst[i];
                }
            }

            if (donotAddSourceIfNull) return null;

            var source = renderer.gameObject.AddComponent<RendererMaterialSource>();
            source._renderer = renderer;
            return source;
        }

        #endregion

    }

}
