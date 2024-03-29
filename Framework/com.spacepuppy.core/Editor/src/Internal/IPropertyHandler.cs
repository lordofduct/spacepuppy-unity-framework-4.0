﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.spacepuppyeditor.Internal
{
    public interface IPropertyHandler
    {

        float GetHeight(SerializedProperty property, GUIContent label, bool includeChildren);

        bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren);

        bool OnGUILayout(SerializedProperty property, GUIContent label, bool includeChildren, GUILayoutOption[] options);

        void OnValidate(SerializedProperty property);

    }

}
