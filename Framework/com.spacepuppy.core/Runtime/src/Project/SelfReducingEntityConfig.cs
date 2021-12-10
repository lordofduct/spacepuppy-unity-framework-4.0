using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Project
{

    public interface ISelfReducingEntityConfig<T> where T : ISelfReducingEntityConfig<T>
    {
        T Reduce();
    }

    public abstract class SelfReducingEntityConfigAsset<T, TConcrete> : ScriptableObject where TConcrete : T where T : ISelfReducingEntityConfig<T>
    {

        #region Fields

        [DisplayFlat]
        [SerializeField]
        protected TConcrete _config;

        #endregion

        #region T Interface

        public T Reduce()
        {
            return _config;
        }

        #endregion

    }

    public abstract class SelfReducingEntityConfigRef<T> : SerializableInterfaceRef<T> where T : class, ISelfReducingEntityConfig<T>
    {

        public enum Source
        {
            Default,
            Entity,
            Self
        }

        #region Methods

        public Source GetSourceType(SPEntity entity)
        {
            if (this.Value.SanitizeRef() != null) return Source.Self;
            if (entity != null && this.EntityHasDirectRef(entity)) return Source.Entity;
            return Source.Default;
        }

        /// <summary>
        /// Resolves which config to use in order:
        /// This if it's setup, else Game.DefaultPlayerConfi, else PlayerConfig.Default.
        /// </summary>
        /// <returns></returns>
        public T GetConfig()
        {
            return this.Value?.Reduce().SanitizeRef() ?? this.GetDefault();
        }

        /// <summary>
        /// Resolves which config to use in order:
        /// This if it's setup, else entity component, else Game.DefaultPlayerConfi, else PlayerConfig.Default.
        /// </summary>
        /// <returns></returns>
        public T GetConfig(SPEntity entity)
        {
            return this.Value?.Reduce().SanitizeRef() ?? this.GetFromEntity(entity) ?? this.GetDefault();
        }


        protected abstract bool EntityHasDirectRef(SPEntity entity);

        protected abstract T GetFromEntity(SPEntity entity);

        protected abstract T GetDefault();

        #endregion

    }

}
