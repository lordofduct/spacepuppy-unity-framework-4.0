using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Quality
{

    public enum AntiAliasingLevel
    {
        None = 0,
        MSAA2x = 2,
        MSAA4x = 4,
        MSAA8x = 8,
    }

    public enum ShadowCascades
    {
        None = 0,
        Split2 = 2,
        Split4 = 4,
    }

    public struct ShadowSettings
    {

        public ShadowQuality shadows;
        public ShadowmaskMode shadowmaskMode;
        public ShadowResolution shadowResolution;
        public ShadowProjection shadowProjection;
        public float shadowDistance;
        public float shadowNearPlaneOffset;

        private ShadowCascades m_shadowCascades;
        public ShadowCascades shadowCascades => m_shadowCascades;
        private Vector3 m_shadowCascadeSplitVector;
        public float shadowCascade2Split => m_shadowCascadeSplitVector.x;
        public Vector3 shadowCascade4Split => m_shadowCascadeSplitVector;

        public void SetShadowCascadeNone()
        {
            m_shadowCascades = ShadowCascades.None;
            m_shadowCascadeSplitVector = default;
        }

        public void SetShadowCascade2Split(float value)
        {
            m_shadowCascades = ShadowCascades.Split2;
            m_shadowCascadeSplitVector = new Vector3(value, 0f, 0f);
        }

        public void SetShadowCascade4Split(Vector3 value)
        {
            m_shadowCascades = ShadowCascades.Split4;
            m_shadowCascadeSplitVector = value;
        }

        public void WriteToCurrentQualitySettings()
        {
            QualitySettings.shadows = this.shadows;
            QualitySettings.shadowmaskMode = this.shadowmaskMode;
            QualitySettings.shadowResolution = this.shadowResolution;
            QualitySettings.shadowProjection = this.shadowProjection;
            QualitySettings.shadowDistance = this.shadowDistance;
            QualitySettings.shadowNearPlaneOffset = this.shadowNearPlaneOffset;

            switch ((ShadowCascades)QualitySettings.shadowCascades)
            {
                case ShadowCascades.None:
                    QualitySettings.shadowCascades = 0;
                    break;
                case ShadowCascades.Split2:
                    QualitySettings.shadowCascades = 2;
                    QualitySettings.shadowCascade2Split = m_shadowCascadeSplitVector.x;
                    break;
                case ShadowCascades.Split4:
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.shadowCascade4Split = m_shadowCascadeSplitVector;
                    break;
                default:
                    QualitySettings.shadowCascades = 0;
                    break;
            }
        }

        public static ShadowSettings ReadFromCurrentQualitySettings()
        {
            var result = new ShadowSettings()
            {
                shadows = QualitySettings.shadows,
                shadowmaskMode = QualitySettings.shadowmaskMode,
                shadowResolution = QualitySettings.shadowResolution,
                shadowProjection = QualitySettings.shadowProjection,
                shadowDistance = QualitySettings.shadowDistance,
                shadowNearPlaneOffset = QualitySettings.shadowNearPlaneOffset
            };
            switch ((ShadowCascades)QualitySettings.shadowCascades)
            {
                case ShadowCascades.None:
                    result.SetShadowCascadeNone();
                    break;
                case ShadowCascades.Split2:
                    result.SetShadowCascade2Split(QualitySettings.shadowCascade2Split);
                    break;
                case ShadowCascades.Split4:
                    result.SetShadowCascade4Split(QualitySettings.shadowCascade4Split);
                    break;
                default:
                    result.SetShadowCascadeNone();
                    break;
            }
            return result;
        }

    }

}
