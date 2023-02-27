﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace com.spacepuppyeditor
{

    public static class SpacepuppySettings
    {

        private const string SETTING_STORESETTINGSLOCAL = "Spacepuppy.StoreSettingsLocal";

        private const string SETTING_SPEDITOR_ISDEFAULT_ACTIVE = "UseSPEditor.IsDefault.Active";
        private const string SETTING_ADVANCEDANIMINSPECTOR_ACTIVE = "AdvancedAnimationInspector.Active";
        private const string SETTING_HIERARCHYDRAWER_ACTIVE = "EditorHierarchyEvents.Active";
        private const string SETTING_HIEARCHYALTERNATECONTEXTMENU_ACTIVE = "EditorHierarchyAlternateContextMenu.Active";
        private const string SETTING_SIGNALIVALIDATERECEIVER_ACTIVE = "SignalValidateReceiver.Active";

        private const string SETTING_MODELIMPORT_USE = "ModelImportManager.UseSpacepuppyModelImportSettings";
        private const string SETTING_MODELIMPORT_SETMATERIALSEARCH = "ModelImportManager.SetMaterialSearch";
        private const string SETTING_MODELIMPORT_MATERIALSEARCH = "ModelImportManager.MaterialSearch";
        private const string SETTING_MODELIMPORT_SETANIMSETTINGS = "ModelImportManager.SetAnimationSettings";
        private const string SETTING_MODELIMPORT_ANIMRIGTYPE = "ModelImportManager.AnimRigType";


        public static bool StoreSettingsLocal
        {
            get
            {
                return EditorProjectPrefs.Local.GetBool(SETTING_STORESETTINGSLOCAL, false);
            }
            set
            {
                EditorProjectPrefs.Local.SetBool(SETTING_STORESETTINGSLOCAL, value);
            }
        }

        /*
         * EDITOR SETTINGS
         */

        public static EditorProjectPrefs.ISettings ProjectPrefs => StoreSettingsLocal ? EditorProjectPrefs.Local : EditorProjectPrefs.Group;

        public static bool UseSPEditorAsDefaultEditor
        {
            get => ProjectPrefs.GetBool(SETTING_SPEDITOR_ISDEFAULT_ACTIVE, true);
            set => ProjectPrefs.SetBool(SETTING_SPEDITOR_ISDEFAULT_ACTIVE, value);
        }

        public static bool UseAdvancedAnimationInspector
        {
            get => ProjectPrefs.GetBool(SETTING_ADVANCEDANIMINSPECTOR_ACTIVE, true);
            set => ProjectPrefs.SetBool(SETTING_ADVANCEDANIMINSPECTOR_ACTIVE, value);
        }

        public static bool UseHierarchDrawer
        {
            get => ProjectPrefs.GetBool(SETTING_HIERARCHYDRAWER_ACTIVE, true);
            set => ProjectPrefs.SetBool(SETTING_HIERARCHYDRAWER_ACTIVE, value);
        }

        public static bool UseHierarchyAlternateContextMenu
        {
            get => ProjectPrefs.GetBool(SETTING_HIEARCHYALTERNATECONTEXTMENU_ACTIVE, true);
            set => ProjectPrefs.SetBool(SETTING_HIEARCHYALTERNATECONTEXTMENU_ACTIVE, value);
        }

        public static bool SignalValidateReceiver
        {
            get => ProjectPrefs.GetBool(SETTING_SIGNALIVALIDATERECEIVER_ACTIVE, true);
            set => ProjectPrefs.SetBool(SETTING_SIGNALIVALIDATERECEIVER_ACTIVE, value);
        }

        /*
         * MODELIMPORT SETTINGS
         */

        //Material Import Settings

        public static bool SetMaterialSearchOnImport
        {
            get => ProjectPrefs.GetBool(SETTING_MODELIMPORT_SETMATERIALSEARCH, true);
            set => ProjectPrefs.SetBool(SETTING_MODELIMPORT_SETMATERIALSEARCH, value);
        }

        public static ModelImporterMaterialSearch MaterialSearch
        {
            get => ProjectPrefs.GetEnum(SETTING_MODELIMPORT_MATERIALSEARCH, ModelImporterMaterialSearch.Everywhere);
            set => ProjectPrefs.SetEnum(SETTING_MODELIMPORT_MATERIALSEARCH, value);
        }

        //Animation Import Settings

        public static bool SetAnimationSettingsOnImport
        {
            get => ProjectPrefs.GetBool(SETTING_MODELIMPORT_SETANIMSETTINGS, true);
            set => ProjectPrefs.SetBool(SETTING_MODELIMPORT_SETANIMSETTINGS, value);
        }

        public static ModelImporterAnimationType ImportAnimRigType
        {
            get => ProjectPrefs.GetEnum(SETTING_MODELIMPORT_ANIMRIGTYPE, ModelImporterAnimationType.Legacy);
            set => ProjectPrefs.SetInt(SETTING_MODELIMPORT_ANIMRIGTYPE, (int)value);
        }

    }

}