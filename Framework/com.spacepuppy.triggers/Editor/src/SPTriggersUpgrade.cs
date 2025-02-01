using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using UnityEditor;

namespace com.spacepuppyeditor
{

    public static class SPTriggersUpgrade
    {


        public static void UpgradeAssetYAML()
        {
            SPUpgrade.UpgradeAssetYAML(typeof(TriggerStateMachineMirror).Assembly);
        }

        [SPUpgrade.YamlUpgradeCallback(typeof(TriggerStateMachineMirror))]
        static bool UpgradeAssetYAML_TriggerStateMachineMirror(string assetPath)
        {
            bool edited = false;
            using (var reader = new AssetRawTextReader(assetPath))
            {
                AssetRawTextReader.SerailizedFieldResult field;
                string ln;
                while ((ln = reader.ReadUntilScript(typeof(TriggerStateMachineMirror))) != null)
                {
                    edited = true;

                    if (reader.SeekUntilSerializedField("_targetStateMachine", out field, true) && !string.IsNullOrEmpty(field.fieldData)) //the data is completely empty if and only if we've previously updated
                    {
                        reader.DeleteLine();
                        reader.InsertLine("  _targetStateMachine:");
                        reader.InsertLine("    _obj:" + field.fieldData);
                        reader.SeekToBeginningOfCurrentObject();
                    }
                    if (reader.SeekUntilSerializedField("_sourceStateMachine", out field, true) && !string.IsNullOrEmpty(field.fieldData)) //the data is completely empty if and only if we've previously updated
                    {
                        reader.DeleteLine();
                        reader.InsertLine("  _sourceStateMachine:");
                        reader.InsertLine("    _obj:" + field.fieldData);
                        reader.SeekToNextObject();
                    }
                }

                if (edited)
                {
                    reader.WriteAllLines(assetPath);
                }
            }

            return edited;
        }

        [SPUpgrade.YamlUpgradeCallback(typeof(TriggerStateMachineProxyLink))]
        static bool UpgradeAssetYAML_TriggerStateMachineProxyLink(string assetPath)
        {
            bool edited = false;
            using (var reader = new AssetRawTextReader(assetPath))
            {
                AssetRawTextReader.SerailizedFieldResult field;
                string ln;
                while ((ln = reader.ReadUntilScript(typeof(TriggerStateMachineProxyLink))) != null)
                {
                    edited = true;

                    if (reader.SeekUntilSerializedField("_stateMachine", out field, true)) //the field is no longer named this so this is our test for re-processing
                    {
                        reader.DeleteLine();
                        reader.InsertLine("  _stateMachine:");
                        reader.InsertLine("    _obj:" + field.fieldData);
                        reader.SeekToNextObject();
                    }
                }

                if (edited)
                {
                    reader.WriteAllLines(assetPath);
                }
            }

            return edited;
        }


    }

}
