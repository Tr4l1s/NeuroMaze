using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class PulseAndroidBuild
{
    static PulseAndroidBuild() { EditorApplication.update += RunQueuedBuild; }
    // A local one-shot request lets the connected assistant rebuild through the
    // already licensed Editor after imports finish, without launching another Editor.
    static void RunQueuedBuild()
    {
        const string request="Temp/PulseAndroidBuild.request";
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying || BuildPipeline.isBuildingPlayer || !File.Exists(request)) return;
        File.Delete(request);
        EditorApplication.delayCall += Build;
    }
    [MenuItem("Tools/NeuroMaze/Build Android Study APK")]
    public static void Build()
    {
        string path = Argument("-pulseOutput") ?? Path.GetFullPath("Builds/Android/NeuroMaze-Android.apk");
        try
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Önce Play modundan çıkın.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Android Build Support kurulu değil.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android &&
                !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Android platformuna geçilemedi.");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
            var identifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            if (string.IsNullOrEmpty(identifier) || identifier.Contains("DefaultCompany") || identifier.Contains("-"))
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.neuromaze.labirentmobile");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            AssetDatabase.SaveAssets();
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0 || scenes.Any(s => !File.Exists(s))) throw new InvalidOperationException("Build sahne listesi eksik.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = scenes, locationPathName = path, target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            File.WriteAllText(path + ".build-result.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nBytes: " + report.summary.totalSize);
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Android build başarısız: " + report.summary.result);
            Debug.Log("PULSE_BUILD_SUCCEEDED " + path);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
    }

    static string Argument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return null;
    }
}
