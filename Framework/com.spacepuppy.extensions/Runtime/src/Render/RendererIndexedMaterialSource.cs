#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Render
{

    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(RendererMaterialSource))]
    public class RendererIndexedMaterialSource : MaterialSource, IIdentifiableComponent
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf]
        [ForceFromSelf]
        private RendererMaterialSource _primarySource;

        [SerializeField]
        [DefaultFromSelf]
        [ForceFromSelf]
        private Renderer _renderer;

        [SerializeField]
        [Min(1)]
        private int _materialIndex = 1;

        #endregion

        #region Properties

        public Renderer Renderer
        {
            get { return _renderer; }
        }

        public override Material Material
        {
            get => _primarySource ? _primarySource.GetMaterialAt(_materialIndex) : null;
            set
            {
                if (_primarySource) _primarySource.SetMaterialAt(_materialIndex, value);
            }
        }

        public int MaterialIndex => _materialIndex;

        /// <summary>
        /// Does not reflect if the material is truly unique. But suggests the possibly of uniqueness if 
        /// 'GetUniqueMaterial' were called.
        /// </summary>
        public override bool IsUnique => _primarySource.IsUnique;

        #endregion

        #region Methods

        /// <summary>
        /// Replaces the material on the Renderer with a copy of itself so it's unique.
        /// </summary>
        /// <returns></returns>
        public override Material GetUniqueMaterial()
        {
            if (!_primarySource) return null;

            return _primarySource.GetUniqueMaterialAt(_materialIndex);
        }

        #endregion

        #region IIdentifiableComponent Interface

        private string _id;
        string IIdentifiableComponent.Id => _id ?? (_id = string.Format("Material {0:0}", _materialIndex));

        #endregion

        #region Static Interface

        public static MaterialSource GetMaterialSource(Renderer renderer, int materialIndex, bool donotAddSourceIfNull = false)
        {
            var primarySource = RendererMaterialSource.GetMaterialSource(renderer, donotAddSourceIfNull);
            if (materialIndex <= 0) return primarySource;

            using (var lst = TempCollection.GetList<RendererIndexedMaterialSource>())
            {
                renderer.GetComponents<RendererIndexedMaterialSource>(lst);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i]._renderer == renderer) return lst[i];
                }
            }

            if (donotAddSourceIfNull) return null;

            var source = renderer.gameObject.AddComponent<RendererIndexedMaterialSource>();
            source._primarySource = primarySource;
            source._renderer = renderer;
            source._materialIndex = materialIndex;
            return source;
        }

        #endregion

    }

}
