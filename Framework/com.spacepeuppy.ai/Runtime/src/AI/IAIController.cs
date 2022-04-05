using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.AI
{
    public interface IAIController
    {

        AIVariableCollection Variables { get; }

    }

    public static class IAIControllerExtensions
    {

        /// <summary>
        /// Retrieves a variable from an IAIController. It first searches the IAIController.Variables and if not found it'll check public members of the IAIController. Variables is always prioritized.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetVariable<T>(this IAIController controller, string name)
        {
            if (string.IsNullOrEmpty(name)) return default(T);

            if (controller.Variables.HasMember(name))
            {
                return ConvertUtil.Coerce<T>(controller.Variables[name]);
            }
            else
            {
                object result;
                if (DynamicUtil.TryGetValue(controller, name, out result))
                {
                    return ConvertUtil.Coerce<T>(result);
                }
                else
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// Sets a variable for an IAIController. It first will overwrite a member of IAIController.Variables if one exists, otherwise it'll look for a public member of IAIController, and if neither of those succeed it will create a new variable on IAIController.Variables for it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetVariable<T>(this IAIController controller, string name, T value)
        {
            if (string.IsNullOrEmpty(name)) return;

            try
            {
                if (value is ComplexTarget target)
                {
                    SetComplexTarget(controller, name, target);
                    return;
                }

                if (controller.Variables.HasMember(name))
                {
                    controller.Variables[name] = value;
                    return;
                }

                var member = DynamicUtil.GetMemberFromType(controller.GetType(), name, false, MemberTypes.Property | MemberTypes.Field);
                if (member != null)
                {
                    var rtp = DynamicUtil.GetReturnType(member);
                    controller.SetValue(member, ConvertUtil.Coerce(value, rtp));
                }
                else
                {
                    controller.Variables[name] = value;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }

        /// <summary>
        /// Just like GetVariable, but specifically for ComplexTarget and resolves the ComplexTarget internal type.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ComplexTarget GetComplexTarget(this IAIController controller, string name)
        {
            if (string.IsNullOrEmpty(name)) return ComplexTarget.Null;

            if (controller.Variables.HasMember(name))
            {
                return controller.Variables.GetAsComplexTarget(name);
            }
            else
            {
                try
                {
                    var member = DynamicUtil.GetMemberFromType(controller.GetType(), name, false, MemberTypes.Property | MemberTypes.Field);
                    if (member != null)
                    {
                        var rtp = DynamicUtil.GetReturnType(member);
                        if (rtp == typeof(ComplexTarget))
                        {
                            return controller.GetValue<ComplexTarget>(member);
                        }
                        else
                        {
                            switch (VariantReference.GetVariantType(rtp))
                            {
                                case VariantType.Vector2:
                                    return new ComplexTarget(controller.GetValue<Vector2>(member));
                                case VariantType.Vector3:
                                case VariantType.Vector4:
                                    return new ComplexTarget(controller.GetValue<Vector3>(member));
                                case VariantType.Object:
                                case VariantType.GameObject:
                                case VariantType.Component:
                                case VariantType.Ref:
                                    return ComplexTarget.FromObject(controller.GetValue(member));
                                default:
                                    return ComplexTarget.Null;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }

                return ComplexTarget.Null;
            }
        }

        /// <summary>
        /// Just like SetVariable, but specifically for ComplexTarget and resolve the ComplexTarget internal type.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="name"></param>
        /// <param name="target"></param>
        public static void SetComplexTarget(this IAIController controller, string name, ComplexTarget target)
        {
            if (string.IsNullOrEmpty(name)) return;

            try
            {
                if (controller.Variables.HasMember(name))
                {
                    controller.Variables.SetAsComplexTarget(name, target);
                    return;
                }

                var member = DynamicUtil.GetMemberFromType(controller.GetType(), name, false, MemberTypes.Property | MemberTypes.Field);
                if (member != null)
                {
                    var rtp = DynamicUtil.GetReturnType(member);
                    if (rtp == typeof(ComplexTarget))
                    {
                        controller.SetValue(member, target);
                        return;
                    }
                    else
                    {
                        switch (VariantReference.GetVariantType(rtp))
                        {
                            case VariantType.Vector2:
                                controller.SetValue<Vector2>(member, target.Position2D);
                                return;
                            case VariantType.Vector3:
                                controller.SetValue<Vector3>(member, target.Position);
                                return;
                            case VariantType.Vector4:
                                controller.SetValue<Vector4>(member, (Vector4)target.Position);
                                return;
                            case VariantType.GameObject:
                                controller.SetValue(member, (object)target.Transform?.gameObject);
                                return;
                            case VariantType.Component:
                                controller.SetValue(member, (object)target.Transform?.gameObject.GetComponent(rtp));
                                return;
                            default:
                                //do nothing
                                return;
                        }
                    }
                }

                controller.Variables.SetAsComplexTarget(name, target);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return;
            }

        }

    }

}
