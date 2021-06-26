using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using SBR.Persistence;

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

using UnityEngine;

namespace SBR.Editor.Persistence {
    [CustomEditor(typeof(PersistedGameObject))]
    public class PersistedObjectEditor : UnityEditor.Editor {
        private static readonly string[] ExcludeProps = {"m_Script", "_dynamicPrefabID", "_instanceID"};
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            var prefabIDProp = serializedObject.FindProperty("_dynamicPrefabID");
            var instanceIDProp = serializedObject.FindProperty("_instanceID");

            PersistedGameObject targetObj = (PersistedGameObject) target;


            var stage = PrefabStageUtility.GetPrefabStage(targetObj.gameObject);
            var type = PrefabUtility.GetPrefabAssetType(targetObj);
            var status = PrefabUtility.GetPrefabInstanceStatus(targetObj);


            bool isPrefabAsset = (stage != null && targetObj.transform.parent == null) ||
                                 ((type == PrefabAssetType.Regular || type == PrefabAssetType.Variant) &&
                                  status == PrefabInstanceStatus.NotAPrefab);

            var id = GlobalObjectId.GetGlobalObjectIdSlow(targetObj);
            
            if (isPrefabAsset) {
                instanceIDProp.longValue = 0;
                prefabIDProp.intValue =
                    stage != null
                        ? Math.Abs(AssetDatabase.GUIDFromAssetPath(stage.assetPath).GetHashCode())
                        : Math.Abs(id.assetGUID.GetHashCode());
            } else {
                instanceIDProp.longValue = (long)(id.targetObjectId ^ id.targetPrefabId);
            }
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LongField(instanceIDProp.displayName, instanceIDProp.longValue);
            EditorGUILayout.PropertyField(prefabIDProp);
            EditorGUI.EndDisabledGroup();

            if (isPrefabAsset) {
                bool isIncluded = DynamicObjectManifest.Instance.GetEntry(targetObj.DynamicPrefabID) != null;
                bool newIsIncluded = EditorGUILayout.Toggle("Include in Manifest", isIncluded);
                if (newIsIncluded && !isIncluded) {
                    string path = stage != null ? EditorUtil.GetResourcePath(stage.assetPath) : EditorUtil.GetResourcePath(targetObj);
                    DynamicObjectManifest.Instance.AddObject(targetObj.DynamicPrefabID, path);
                } else if (!newIsIncluded && isIncluded) {
                    DynamicObjectManifest.Instance.RemoveObject(targetObj.DynamicPrefabID);
                }
            }
            
            DrawPropertiesExcluding(serializedObject, ExcludeProps);

            serializedObject.ApplyModifiedProperties();
        }
    }
}