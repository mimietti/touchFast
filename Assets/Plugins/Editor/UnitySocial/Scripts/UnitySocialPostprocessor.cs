using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnitySocialEditor.xCode;

public static class UnitySocialPostprocessor
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        try
        {
            UnitySocialSettings settings = UnitySocialSettingsEditor.LoadSettings();

            if (settings != null && settings.IsValid)
            {
                if (settings.iosSupportEnabled && target == kBuildTarget_iOS)
                {
                    if (pathToBuiltProject.StartsWith("./"))   // Fix two erroneous path cases on Unity 5.4f03
                    {
                        pathToBuiltProject = Path.Combine(Application.dataPath.Replace("Assets", ""), pathToBuiltProject.Replace("./", ""));
                    }
                    else if (pathToBuiltProject.Contains("./"))
                    {
                        pathToBuiltProject = pathToBuiltProject.Replace("./", "");
                    }

                    UpdateXcodeProject(pathToBuiltProject);
                    UpdatePlist(pathToBuiltProject, settings.clientId);

                    Debug.Log("Unity Social postprocess done.");
                }
            }
            else if (settings.androidSupportEnabled && target == BuildTarget.Android)
            {
                PluginImporter[] pluginImporters = PluginImporter.GetAllImporters();
                foreach (PluginImporter pluginImporter in pluginImporters)
                {
                    if (pluginImporter.assetPath.Contains("Plugins/UnitySocial/Native/Android"))
                    {
                        pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, settings.androidSupportEnabled);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Unity Social postprocess failed: " + e.ToString());
        }
    }

    private static void UpdateXcodeProject(string pathToBuiltProject)
    {
        if (pathToBuiltProject != null && pathToBuiltProject.Length > 0)
        {
            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

            if (projectPath != null && File.Exists(projectPath))
            {
                List<string> frameWorksToBeAdded = new List<string>();

                frameWorksToBeAdded.Add("JavaScriptCore.framework");
                frameWorksToBeAdded.Add("Security.framework");
                frameWorksToBeAdded.Add("MobileCoreServices.framework");

                List<string> otherLinkerFlagsToBeAdded = new List<string>();

                otherLinkerFlagsToBeAdded.Add("-lstdc++");
                otherLinkerFlagsToBeAdded.Add("-ObjC");

                List<string> filesToBeAdded = new List<string>();

                filesToBeAdded.Add("UnitySocial.h");
                filesToBeAdded.Add("UnitySocialNativeWrapper.mm");
                filesToBeAdded.Add("libUnitySocialUI.a");

                PBXProject project = new PBXProject();

                project.ReadFromString(File.ReadAllText(projectPath));

                string targetGuid = project.TargetGuidByName("Unity-iPhone");

                foreach (string framework in frameWorksToBeAdded)
                {
                    project.AddFrameworkToProject(targetGuid, framework, false);
                }

                foreach (string flag in otherLinkerFlagsToBeAdded)
                {
                    project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", flag);
                }

                project.AddBuildProperty(targetGuid, "LIBRARY_SEARCH_PATHS", "\"$(SRCROOT)/UnitySocial\"");
                project.AddBuildProperty(targetGuid, "FRAMEWORK_SEARCH_PATHS", "\"$(SRCROOT)/UnitySocial\"");

                string srcPath = Path.Combine(Application.dataPath, "Plugins/UnitySocial/Native/iOS");
                string dstPath = Path.Combine(pathToBuiltProject, "UnitySocial");

                if (CreateDirectory(dstPath))
                {
                    foreach (string file in filesToBeAdded)
                    {
                        string sourceFilePath = Path.Combine(srcPath, file);
                        string targetFilePath = Path.Combine(dstPath, file);
                        string relativeFilePath = targetFilePath.Replace(pathToBuiltProject, "").TrimStart('/');

                        // TODO: could do symlinks based on BuildOptions.SymlinkLibraries editor option
                        CopyFileOrDirectory(sourceFilePath, targetFilePath);

                        project.AddFileToBuild(targetGuid, project.AddFile(targetFilePath, relativeFilePath, PBXSourceTree.Source));
                    }
                }

                File.WriteAllText(projectPath, project.WriteToString());
            }
        }
    }

    private static void UpdatePlist(string pathToBuiltProject, string clientId)
    {
        if (pathToBuiltProject != null && pathToBuiltProject.Length > 0)
        {
            string plistFilePath = Path.Combine(pathToBuiltProject, "Info.plist");

            if (File.Exists(plistFilePath))
            {
                PlistDocument plistDocument = new PlistDocument();

                plistDocument.ReadFromFile(plistFilePath);

                // Add url schemes

                PlistElement urlTypesElement = plistDocument.root["CFBundleURLTypes"];

                if (urlTypesElement == null)
                {
                    plistDocument.root["CFBundleURLTypes"] = new PlistElementArray();
                }

                PlistElementArray urlTypes = plistDocument.root["CFBundleURLTypes"].AsArray();

                bool updated = false;

                if (urlTypes != null)
                {
                    PlistElementDict dict = urlTypes.AddDict();

                    PlistElementArray array = dict.CreateArray("CFBundleURLSchemes");

                    array.AddString("us" + clientId);
                    array.AddString("ep" + clientId);

                    updated = true;
                }

                // Add allow arbitrary loads

                PlistElement appTransportSecurityElement = plistDocument.root["NSAppTransportSecurity"];

                if (appTransportSecurityElement == null)
                {
                    plistDocument.root["NSAppTransportSecurity"] = new PlistElementDict();
                }

                PlistElementDict appTransportSecurityElementDict = plistDocument.root["NSAppTransportSecurity"].AsDict();

                if (appTransportSecurityElementDict != null)
                {
                    appTransportSecurityElementDict["NSAllowsArbitraryLoads"] = new PlistElementBoolean(true);
                }

                // Add camera usage description for iOS 10

                PlistElement cameraUsageDescriptionElement = plistDocument.root["NSCameraUsageDescription"];

                if (cameraUsageDescriptionElement == null)
                {
                    plistDocument.root["NSCameraUsageDescription"] = new PlistElementString("HeyPlay requires access to the camera");
                }

                // Add microphone usage description for iOS 10

                PlistElement microphoneUsageDescriptionElement = plistDocument.root["NSMicrophoneUsageDescription"];

                if (microphoneUsageDescriptionElement == null)
                {
                    plistDocument.root["NSMicrophoneUsageDescription"] = new PlistElementString("HeyPlay requires access to the microphone");
                }

                // Add photo library usage description for iOS 10

                PlistElement photoLibraryUsageDescriptionElement = plistDocument.root["NSPhotoLibraryUsageDescription"];

                if (photoLibraryUsageDescriptionElement == null)
                {
                    plistDocument.root["NSPhotoLibraryUsageDescription"] = new PlistElementString("HeyPlay requires access to the photo library");
                }

                // Add contact usage description for iOS 10

                PlistElement contactUsageDescriptionElement = plistDocument.root["NSContactsUsageDescription"];

                if (contactUsageDescriptionElement == null)
                {
                    plistDocument.root["NSContactsUsageDescription"] = new PlistElementString("HeyPlay requires access to the contacts");
                }

                if (updated)
                {
                    plistDocument.WriteToFile(plistFilePath);
                }
            }
        }
    }

    public static void ValidateState(UnitySocialSettings settings)
    {
        bool isValid = false;

        if (settings != null && settings.IsValid)
        {
            isValid = true;
        }

        foreach (BuildTargetGroup target in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (target == kBuildTargetGroup_iOS)
            {
                UnitySocialPostprocessor.SetScriptingDefineSymbolForTarget(kBuildTargetGroup_iOS, "UNITY_SOCIAL", isValid ? settings.iosSupportEnabled : false);
            }
            else if (target == BuildTargetGroup.Android)
            {
                UnitySocialPostprocessor.SetScriptingDefineSymbolForTarget(BuildTargetGroup.Android, "UNITY_SOCIAL", isValid ? settings.androidSupportEnabled : false);
            }
        }
    }

    private static void SetScriptingDefineSymbolForTarget(BuildTargetGroup target, string targetDefine, bool enabled)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

        defines = defines.Replace(targetDefine, "");
        defines = defines.Replace(";;", ";");

        if (enabled)
        {
            if (defines.Length > 0)
            {
                defines = targetDefine + ";" + defines;
            }
            else
            {
                defines = targetDefine;
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
    }

    private static bool CreateDirectory(string path)
    {
        if (path != null)
        {
            DeleteFileOrDirectory(path);

            if (Directory.CreateDirectory(path) != null)
            {
                return true;
            }
        }

        return false;
    }

    private static void DeleteFileOrDirectory(string path)
    {
        if (path != null)
        {
            if (Directory.Exists(path))
            {
                string[] folders = Directory.GetDirectories(path);

                foreach (string folder in folders)
                {
                    DeleteFileOrDirectory(folder);
                }

                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    File.Delete(Path.Combine(path, file));
                }

                Directory.Delete(path);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static bool CopyFileOrDirectory(string srcPath, string dstPath)
    {
        if (srcPath != null && dstPath != null)
        {
            if (File.Exists(srcPath) || Directory.Exists(srcPath))
            {
                FileAttributes srcPathAttributes = File.GetAttributes(srcPath);

                if ((srcPathAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    if (!Directory.Exists(dstPath))
                    {
                        Directory.CreateDirectory(dstPath);
                    }

                    string[] files = Directory.GetFiles(srcPath);

                    foreach (string fileNameWithPath in files)
                    {
                        string file = Path.GetFileName(fileNameWithPath);

                        if (!file.EndsWith(".meta"))
                        {
                            File.Copy(Path.Combine(srcPath, file), Path.Combine(dstPath, file));
                        }
                    }

                    string[] folders = Directory.GetDirectories(srcPath);

                    foreach (string folderWithPath in folders)
                    {
                        string folder = Path.GetFileName(folderWithPath);

                        CopyFileOrDirectory(Path.Combine(srcPath, folder), Path.Combine(dstPath, folder));
                    }
                }
                else if (!srcPath.EndsWith(".meta"))
                {
                    File.Copy(srcPath, dstPath);
                }
            }

            return true;
        }
        return false;
    }

    private const BuildTarget kBuildTarget_iOS = (BuildTarget) 9; // (enum changed from iPhone -> iOS)
    private const BuildTargetGroup kBuildTargetGroup_iOS = (BuildTargetGroup) 4; // (enum changed from iPhone -> iOS)
}
