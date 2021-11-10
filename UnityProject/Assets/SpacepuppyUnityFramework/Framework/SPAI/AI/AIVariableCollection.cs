using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace com.spacepuppy.AI
{

    [System.Serializable()]
    public class AIVariableCollection : VariantCollection
    {

        public AIVariableCollection()
        {

        }

        public new object this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (value is ComplexTarget target)
                {
                    this.SetAsComplexTarget(key, target);
                }
                else
                {
                    base[key] = value;
                }
            }
        }

        public new void SetValue(string key, object value)
        {
            if (value is ComplexTarget target)
            {
                this.SetAsComplexTarget(key, target);
            }
            else
            {
                base[key] = value;
            }
        }


        public ComplexTarget GetAsComplexTarget(string key)
        {
            var v = this.GetVariant(key);
            if (v == null) return ComplexTarget.Null;

            switch (v.ValueType)
            {
                case VariantType.Vector2:
                    return new ComplexTarget(v.Vector2Value);
                case VariantType.Vector3:
                case VariantType.Vector4:
                    return new ComplexTarget(v.Vector3Value);
                case VariantType.Object:
                case VariantType.GameObject:
                case VariantType.Component:
                    return ComplexTarget.FromObject(v.Value);
                case VariantType.Ref:
                    {
                        var obj = v.Value;
                        return obj is ComplexTarget ? (ComplexTarget)obj : ComplexTarget.FromObject(obj);
                    }
                default:
                    return ComplexTarget.Null;
            }
        }

        public void SetAsComplexTarget(string key, ComplexTarget target)
        {
            if(target.Aux != null)
            {
                this.GetVariant(key, true).Value = target;
            }
            else
            {
                switch (target.TargetType)
                {
                    case ComplexTargetType.Null:
                        this.GetVariant(key, true).Value = null;
                        break;
                    case ComplexTargetType.GameObjectSource:
                        this.GetVariant(key, true).Value = target.Target;
                        break;
                    case ComplexTargetType.Transform:
                        this.GetVariant(key, true).Value = target.Transform;
                        break;
                    case ComplexTargetType.Vector2:
                        this.GetVariant(key, true).Vector2Value = target.Position2D;
                        break;
                    case ComplexTargetType.Vector3:
                        this.GetVariant(key, true).Vector3Value = target.Position;
                        break;
                }
            }
        }

    }
}
