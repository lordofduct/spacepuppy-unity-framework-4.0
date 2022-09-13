#if SP_UNITYIAP
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEditor;
using UnityEditor.Purchasing;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppyeditor;

using com.spacepuppy.IAP.Events;

namespace com.vivariumeditor.Events
{

    [CustomEditor(typeof(i_PerformIAPPurchase))]
    public class i_PerformIAPPurchaseInspector : SPEditor
    {

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);

            if (this.serializedObject.FindProperty(i_PerformIAPPurchase.PROP_ACTIONTYPE).GetEnumValue<i_PerformIAPPurchase.ActionTypes>() == i_PerformIAPPurchase.ActionTypes.Purchase)
            {
                this.DrawPropertyField(i_PerformIAPPurchase.PROP_PRODUCTID);
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, i_PerformIAPPurchase.PROP_PRODUCTID);

            this.serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif
