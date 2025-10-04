#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Render
{

    [RequireComponent(typeof(UnityEngine.UI.Graphic))]
    public sealed class GraphicMaterialSource : MaterialSource
    {

        #region Fields

        [SerializeField()]
        [DefaultFromSelf]
        [ForceFromSelf(EntityRelativity.Self)]
        private UnityEngine.UI.Graphic _graphics;
        [SerializeField()]
        private MaterialSourceUniquessModes _mode;

        [System.NonSerialized()]
        private bool _unique;
        [System.NonSerialized]
        private Material _uniqueMaterial;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            if (object.ReferenceEquals(_graphics, null))
            {
                Debug.LogWarning("MaterialSource attached incorrectly to GameObject. Either attach MaterialSource at design time through editor, or call MaterialSource.GetMaterialSource.");
                UnityEngine.Object.Destroy(this);
            }
            else if (_graphics.gameObject != this.gameObject)
            {
                Debug.LogWarning("MaterialSource must be attached to the same GameObject as its Source Renderer.");
                UnityEngine.Object.Destroy(this);
            }

            if (_mode == MaterialSourceUniquessModes.MakeUniqueOnStart && !_unique)
            {
                this.GetUniqueMaterial();
            }

            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_uniqueMaterial)
            {
                UnityEngine.Object.Destroy(_uniqueMaterial);
            }
        }

        #endregion

        #region Properties

        public UnityEngine.UI.Graphic Graphics
        {
            get { return _graphics; }
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
                if (!_graphics) return null;
#if UNITY_EDITOR
                if (!Application.isPlaying) return _graphics.material ?? _graphics.defaultMaterial;
#endif

                switch (_mode)
                {
                    case MaterialSourceUniquessModes.UseSharedMaterial:
                        return _graphics.material ?? _graphics.defaultMaterial;
                    case MaterialSourceUniquessModes.MakeUniqueOnStart:
                    case MaterialSourceUniquessModes.MakeUniqueOnAccess:
                        return this.GetUniqueMaterial();
                }
                return null;
            }
            set
            {
                //note this doesn't necessarily destroy any 'unique material'. That material is preserved until the next time
                //'Material getter' or 'GetUniqueMaterial' is accessed or this is destroyed just in case the user is in the middle of configuring
                if (_graphics) _graphics.material = value;
            }
        }

        /// <summary>
        /// Does not reflect if the material is truly unique. But suggests the possibly of uniqueness if 
        /// 'GetUniqueMaterial' were called.
        /// </summary>
        public override bool IsUnique
        {
            get { return _unique; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Replaces the material on the Renderer with a copy of itself so it's unique.
        /// </summary>
        /// <returns></returns>
        public override Material GetUniqueMaterial()
        {
            if (!_graphics) return null;

            var mat = _graphics.material ?? _graphics.defaultMaterial;
            if (_uniqueMaterial == mat) return _uniqueMaterial; //if it already exists, return it

            if (_uniqueMaterial) Destroy(_uniqueMaterial); //if this exists that means something else change the material of the Grahics and so we should destroy the old one since it's no longer used
            _unique = true;
            _uniqueMaterial = new Material(mat);
            _graphics.material = _uniqueMaterial;
            return _uniqueMaterial;
        }

        #endregion

        #region Static Interface

        public static GraphicMaterialSource GetMaterialSource(UnityEngine.UI.Graphic graphic, bool donotAddSourceIfNull = false)
        {
            if (!graphic) return null;

            using (var lst = TempCollection.GetList<GraphicMaterialSource>())
            {
                graphic.GetComponents<GraphicMaterialSource>(lst);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i]._graphics == graphic) return lst[i];
                }
            }

            if (donotAddSourceIfNull) return null;

            var source = graphic.gameObject.AddComponent<GraphicMaterialSource>();
            source._graphics = graphic;
            return source;
        }

        #endregion

    }

}
