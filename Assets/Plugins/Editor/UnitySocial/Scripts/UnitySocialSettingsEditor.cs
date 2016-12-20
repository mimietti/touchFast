using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;

[CustomEditor(typeof(UnitySocialSettings))]
public class UnitySocialSettingsEditor : Editor
{
    public const string settingsFile = "UnitySocialSettings";
    public const string settingsFileExtension = ".asset";

    private static GUIContent labelClientId = new GUIContent("Project ID");
    private static GUIContent labelSupport_iOS = new GUIContent("iOS enabled [?]", "Check to enable Unity Social on iOS devices");
    private static GUIContent labelSupport_Android = new GUIContent("Android enabled [?]", "Check to enable Unity Social on Android devices");

    private UnitySocialSettings currentSettings = null;
    private bool iosSupportEnabled;
    private bool androidSupportEnabled;

    [MenuItem("Edit/Unity Social/Settings")]
    public static void ShowSettings()
    {
        UnitySocialSettings settingsInstance = LoadSettings();

        if (settingsInstance == null)
        {
            settingsInstance = CreateSettings();
        }

        if (settingsInstance != null)
        {
            Selection.activeObject = settingsInstance;
        }
    }

    public override void OnInspectorGUI()
    {
        try
        {
            // Might be null when this gui is open and this file is being reimported
            if (target == null)
            {
                Selection.activeObject = null;
                return;
            }

            currentSettings = (UnitySocialSettings) target;

            if (currentSettings != null)
            {
                bool settingsValid = currentSettings.IsValid;

                EditorGUILayout.HelpBox("1) Enter your game credentials", MessageType.None);

                if (!currentSettings.IsValid)
                {
                    EditorGUILayout.HelpBox("Invalid or missing game credentials, Unity Social disabled.", MessageType.Error);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelClientId, GUILayout.Width(108), GUILayout.Height(18));
                currentSettings.clientId = TrimmedText(EditorGUILayout.TextField(currentSettings.clientId));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("2) Enable Unity Social on these platforms", MessageType.None);

                EditorGUILayout.BeginVertical();

                bool validityChanged = currentSettings.IsValid != settingsValid;
                bool selectedPlatformsChanged = false;

                iosSupportEnabled = EditorGUILayout.Toggle(labelSupport_iOS, currentSettings.iosSupportEnabled);
                androidSupportEnabled = EditorGUILayout.Toggle(labelSupport_Android, currentSettings.androidSupportEnabled);

                if (iosSupportEnabled != currentSettings.iosSupportEnabled)
                {
                    selectedPlatformsChanged = true;
                    currentSettings.iosSupportEnabled = iosSupportEnabled;

                    if (iosSupportEnabled)
                    {
                        iOSTargetOSVersion v = PlayerSettings.iOS.targetOSVersion;

                        if (v == iOSTargetOSVersion.iOS_4_0 || v == iOSTargetOSVersion.iOS_4_1 || v == iOSTargetOSVersion.iOS_4_2 ||
                            v == iOSTargetOSVersion.iOS_4_3 || v == iOSTargetOSVersion.iOS_5_0 || v == iOSTargetOSVersion.iOS_5_1 ||
                            v == iOSTargetOSVersion.iOS_6_0 || v == iOSTargetOSVersion.Unknown)
                        {
                            PlayerSettings.iOS.targetOSVersion = iOSTargetOSVersion.iOS_7_0;
                            Debug.Log("Unity Social requires minimum iOS 7.0. Target OS version updated to 7.0");
                        }
                    }

                    EditorUtility.SetDirty(currentSettings);
                }
                else if (androidSupportEnabled != currentSettings.androidSupportEnabled)
                {
                    EnableAndroidSupport(androidSupportEnabled);
                    selectedPlatformsChanged = true;
                    currentSettings.androidSupportEnabled = androidSupportEnabled;

                    if (androidSupportEnabled)
                    {
                        AndroidSdkVersions sdkVer = PlayerSettings.Android.minSdkVersion;

                        if (sdkVer < AndroidSdkVersions.AndroidApiLevel16)
                        {
                            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel16;
                            Debug.Log("Unity Social requires minimum Android API level 16. API minSdkVersion updated to 16.");
                        }
                    }

                    EditorUtility.SetDirty(currentSettings);
                }

                if (validityChanged || selectedPlatformsChanged)
                {
                    UnitySocialPostprocessor.ValidateState(currentSettings);
                }

                EditorGUILayout.EndVertical();
            }
        }
        catch (Exception e)
        {
            if (e != null)
            {
            }
        }
    }

    private static string TrimmedText(string txt)
    {
        if (txt != null)
        {
            return txt.Trim();
        }
        return "";
    }

    private static UnitySocialSettings CreateSettings()
    {
        UnitySocialSettings everyplaySettings = (UnitySocialSettings) ScriptableObject.CreateInstance(typeof(UnitySocialSettings));

        if (everyplaySettings != null)
        {
            if (!Directory.Exists(System.IO.Path.Combine(Application.dataPath, "Plugins/UnitySocial/Resources")))
            {
                AssetDatabase.CreateFolder("Assets/Plugins/UnitySocial", "Resources");
            }

            AssetDatabase.CreateAsset(everyplaySettings, "Assets/Plugins/UnitySocial/Resources/" + settingsFile + settingsFileExtension);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return everyplaySettings;
        }

        return null;
    }

    public static UnitySocialSettings LoadSettings()
    {
        return (UnitySocialSettings) Resources.Load(settingsFile);
    }

    void OnDisable()
    {
        if (currentSettings != null)
        {
            EditorUtility.SetDirty(currentSettings);
            currentSettings = null;
        }
    }

    private static void EnableAndroidSupport(bool enabled)
    {
        PluginImporter[] pluginImporters = PluginImporter.GetAllImporters();
        foreach (PluginImporter pluginImporter in pluginImporters)
        {
            if (pluginImporter.assetPath.Contains("Plugins/UnitySocial/Native/Android"))
            {
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, enabled);
            }
        }
    }
}
