#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class DueloWebGLSettings
{
    private const string RequiredTemplate = "PROJECT:Duelo";
    public const string RequiredBridgePluginGuid = "14d216e126e6d124fb695dffd5752e90";
    public static string RequiredBridgePluginPath =>
        AssetDatabase.GUIDToAssetPath(RequiredBridgePluginGuid);

    [MenuItem("DUELO/Apply Compatible WebGL Settings")]
    public static void ApplyFromMenu()
    {
        ApplyRequiredSettings(logResult: true);
    }

    [MenuItem("DUELO/Validate Compatible WebGL Settings")]
    public static void ValidateFromMenu()
    {
        string[] issues = GetCompatibilityIssues();
        if (issues.Length == 0)
        {
            Debug.Log("DUELO WebGL settings are compatible.");
            return;
        }

        Debug.LogError("DUELO WebGL settings are not compatible:\n- " + string.Join("\n- ", issues));
    }

    internal static bool ApplyRequiredSettings(bool logResult)
    {
        DueloWebGLTemplateInstallPlan templateInstall = DueloWebGLTemplateInstaller.InstallTemplate(
            allowOverwriteUnmanaged: false,
            logResult: logResult
        );

        if (!templateInstall.Success)
        {
            if (logResult)
            {
                Debug.LogError("DUELO WebGL settings were not applied: " + templateInstall.Message);
            }

            return false;
        }

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        }

        PlayerSettings.WebGL.template = RequiredTemplate;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.showDiagnostics = false;

        AssetDatabase.SaveAssets();

        if (logResult)
        {
            Debug.Log(
                "DUELO WebGL settings applied: " +
                $"template={PlayerSettings.WebGL.template}, " +
                $"compression={PlayerSettings.WebGL.compressionFormat}, " +
                $"decompressionFallback={PlayerSettings.WebGL.decompressionFallback}"
            );
        }

        return true;
    }

    internal static string[] GetCompatibilityIssues()
    {
        var issues = new List<string>();

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            issues.Add("active build target must be WebGL");
        }

        if (PlayerSettings.WebGL.template != RequiredTemplate)
        {
            issues.Add($"WebGL template must be {RequiredTemplate}");
        }

        if (PlayerSettings.WebGL.compressionFormat != WebGLCompressionFormat.Brotli)
        {
            issues.Add("compression format must be Brotli");
        }

        if (PlayerSettings.WebGL.decompressionFallback)
        {
            issues.Add("decompression fallback must be disabled");
        }

        if (!DueloWebGLTemplateInstaller.InstalledTemplateExists())
        {
            issues.Add(
                $"WebGL template file is missing at {DueloWebGLTemplateInstaller.InstalledTemplatePath}"
            );
        }
        else if (!DueloWebGLTemplateInstaller.InstalledTemplateIsManaged())
        {
            issues.Add(
                $"WebGL template at {DueloWebGLTemplateInstaller.InstalledTemplatePath} is not DUELO-managed"
            );
        }
        else if (!DueloWebGLTemplateInstaller.InstalledTemplateIsCurrent())
        {
            issues.Add(
                $"WebGL template at {DueloWebGLTemplateInstaller.InstalledTemplatePath} is not current"
            );
        }

        if (
            string.IsNullOrEmpty(RequiredBridgePluginPath)
            || AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(RequiredBridgePluginPath) == null
        )
        {
            issues.Add(
                $"WebGL bridge plugin is missing for asset GUID {RequiredBridgePluginGuid}"
            );
        }

        return issues.ToArray();
    }
}

public sealed class DueloWebGLPrebuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
        {
            return;
        }

        DueloWebGLSettings.ApplyRequiredSettings(logResult: false);

        string[] issues = DueloWebGLSettings.GetCompatibilityIssues();
        if (issues.Length > 0)
        {
            throw new BuildFailedException(
                "DUELO WebGL compatibility settings could not be applied:\n- " + string.Join("\n- ", issues)
            );
        }

        Debug.Log("DUELO WebGL compatibility settings verified before build.");
    }
}
#endif
