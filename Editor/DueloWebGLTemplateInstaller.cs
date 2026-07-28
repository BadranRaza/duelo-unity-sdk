#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum DueloWebGLTemplateInstallStatus
{
    Created,
    Updated,
    AlreadyCurrent,
    ConflictingTemplate,
    MissingCanonicalTemplate,
    InvalidCanonicalTemplate,
    Failed
}

public readonly struct DueloWebGLTemplateInstallPlan
{
    public DueloWebGLTemplateInstallPlan(
        DueloWebGLTemplateInstallStatus status,
        bool success,
        bool shouldWriteTemplate,
        string message
    )
    {
        Status = status;
        Success = success;
        ShouldWriteTemplate = shouldWriteTemplate;
        Message = message;
    }

    public DueloWebGLTemplateInstallStatus Status { get; }
    public bool Success { get; }
    public bool ShouldWriteTemplate { get; }
    public string Message { get; }
}

public static class DueloWebGLTemplateInstaller
{
    public const string TemplateVersion = "2";
    public const string VersionMarker = "<!-- DUELO_TEMPLATE_VERSION: 2 -->";
    public const string CanonicalTemplateGuid = "33cae6bc94eee48c8be61ee10364ad63";
    public static string CanonicalTemplateSourcePath =>
        AssetDatabase.GUIDToAssetPath(CanonicalTemplateGuid);
    public const string InstalledTemplatePath = "Assets/WebGLTemplates/Duelo/index.html";

    private const string AutoInstallSessionKey = "Duelo.WebGLTemplate.AutoInstallComplete";

    [MenuItem("DUELO/Setup Project")]
    public static void SetupProjectFromMenu()
    {
        DueloWebGLTemplateInstallPlan install = InstallTemplate(
            allowOverwriteUnmanaged: false,
            backupUnmanaged: false,
            logResult: true
        );

        if (install.Status == DueloWebGLTemplateInstallStatus.ConflictingTemplate)
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "DUELO WebGL Template",
                "Assets/WebGLTemplates/Duelo/index.html already exists and is not marked as a DUELO-managed template. DUELO will not replace it silently.",
                "Back Up + Replace",
                "Leave Existing",
                "Cancel"
            );

            if (choice != 0)
            {
                Debug.LogWarning(
                    "DUELO setup stopped because the existing WebGL template was left unchanged."
                );
                return;
            }

            install = InstallTemplate(
                allowOverwriteUnmanaged: true,
                backupUnmanaged: true,
                logResult: true
            );
        }

        if (!install.Success)
        {
            Debug.LogError("DUELO setup failed: " + install.Message);
            return;
        }

        DueloWebGLSettings.ApplyRequiredSettings(logResult: true);
    }

    public static DueloWebGLTemplateInstallPlan InstallTemplate(
        bool allowOverwriteUnmanaged,
        bool logResult
    )
    {
        return InstallTemplate(
            allowOverwriteUnmanaged,
            backupUnmanaged: false,
            logResult: logResult
        );
    }

    internal static DueloWebGLTemplateInstallPlan InstallTemplate(
        bool allowOverwriteUnmanaged,
        bool backupUnmanaged,
        bool logResult
    )
    {
        try
        {
            string canonicalPath = ToAbsolutePath(CanonicalTemplateSourcePath);
            if (!File.Exists(canonicalPath))
            {
                return LogResult(
                    new DueloWebGLTemplateInstallPlan(
                        DueloWebGLTemplateInstallStatus.MissingCanonicalTemplate,
                        success: false,
                        shouldWriteTemplate: false,
                        message: $"canonical template is missing at {CanonicalTemplateSourcePath}"
                    ),
                    logResult
                );
            }

            string canonicalTemplate = File.ReadAllText(canonicalPath);
            string installedPath = ToAbsolutePath(InstalledTemplatePath);
            string existingTemplate = File.Exists(installedPath)
                ? File.ReadAllText(installedPath)
                : null;

            DueloWebGLTemplateInstallPlan plan = GetInstallPlan(
                canonicalTemplate,
                existingTemplate,
                allowOverwriteUnmanaged
            );

            if (!plan.ShouldWriteTemplate)
            {
                return LogResult(plan, logResult);
            }

            if (
                backupUnmanaged
                && existingTemplate != null
                && !IsDueloManagedTemplate(existingTemplate)
            )
            {
                string backupPath =
                    installedPath + ".duelo-backup-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                File.Copy(installedPath, backupPath, overwrite: false);
            }

            string installedDirectory = Path.GetDirectoryName(installedPath);
            if (!string.IsNullOrEmpty(installedDirectory))
            {
                Directory.CreateDirectory(installedDirectory);
            }

            File.WriteAllText(installedPath, canonicalTemplate);
            AssetDatabase.ImportAsset(InstalledTemplatePath);
            AssetDatabase.SaveAssets();

            return LogResult(plan, logResult);
        }
        catch (Exception ex)
        {
            return LogResult(
                new DueloWebGLTemplateInstallPlan(
                    DueloWebGLTemplateInstallStatus.Failed,
                    success: false,
                    shouldWriteTemplate: false,
                    message: ex.Message
                ),
                logResult
            );
        }
    }

    public static DueloWebGLTemplateInstallPlan GetInstallPlan(
        string canonicalTemplate,
        string existingTemplate,
        bool allowOverwriteUnmanaged
    )
    {
        if (string.IsNullOrWhiteSpace(canonicalTemplate))
        {
            return new DueloWebGLTemplateInstallPlan(
                DueloWebGLTemplateInstallStatus.InvalidCanonicalTemplate,
                success: false,
                shouldWriteTemplate: false,
                message: "canonical DUELO WebGL template is empty"
            );
        }

        if (!canonicalTemplate.Contains(VersionMarker))
        {
            return new DueloWebGLTemplateInstallPlan(
                DueloWebGLTemplateInstallStatus.InvalidCanonicalTemplate,
                success: false,
                shouldWriteTemplate: false,
                message: "canonical DUELO WebGL template is missing the version marker"
            );
        }

        if (existingTemplate == null)
        {
            return new DueloWebGLTemplateInstallPlan(
                DueloWebGLTemplateInstallStatus.Created,
                success: true,
                shouldWriteTemplate: true,
                message: $"created {InstalledTemplatePath}"
            );
        }

        if (existingTemplate == canonicalTemplate)
        {
            return new DueloWebGLTemplateInstallPlan(
                DueloWebGLTemplateInstallStatus.AlreadyCurrent,
                success: true,
                shouldWriteTemplate: false,
                message: $"{InstalledTemplatePath} is already current"
            );
        }

        if (IsDueloManagedTemplate(existingTemplate) || allowOverwriteUnmanaged)
        {
            return new DueloWebGLTemplateInstallPlan(
                DueloWebGLTemplateInstallStatus.Updated,
                success: true,
                shouldWriteTemplate: true,
                message: $"updated {InstalledTemplatePath}"
            );
        }

        return new DueloWebGLTemplateInstallPlan(
            DueloWebGLTemplateInstallStatus.ConflictingTemplate,
            success: false,
            shouldWriteTemplate: false,
            message: $"{InstalledTemplatePath} exists but is not DUELO-managed"
        );
    }

    public static bool IsDueloManagedTemplate(string template)
    {
        return !string.IsNullOrEmpty(template) && template.Contains("DUELO_TEMPLATE_VERSION:");
    }

    public static bool InstalledTemplateExists()
    {
        return File.Exists(ToAbsolutePath(InstalledTemplatePath));
    }

    public static bool InstalledTemplateIsManaged()
    {
        string path = ToAbsolutePath(InstalledTemplatePath);
        return File.Exists(path) && IsDueloManagedTemplate(File.ReadAllText(path));
    }

    public static bool InstalledTemplateIsCurrent()
    {
        string canonicalPath = ToAbsolutePath(CanonicalTemplateSourcePath);
        string installedPath = ToAbsolutePath(InstalledTemplatePath);

        return File.Exists(canonicalPath)
            && File.Exists(installedPath)
            && File.ReadAllText(canonicalPath) == File.ReadAllText(installedPath);
    }

    internal static void AutoInstallIfMissing()
    {
        if (SessionState.GetBool(AutoInstallSessionKey, false))
        {
            return;
        }

        SessionState.SetBool(AutoInstallSessionKey, true);

        DueloWebGLTemplateInstallPlan install = InstallTemplate(
            allowOverwriteUnmanaged: false,
            backupUnmanaged: false,
            logResult: false
        );

        if (install.Status == DueloWebGLTemplateInstallStatus.ConflictingTemplate)
        {
            Debug.LogWarning(
                "DUELO WebGL template was not auto-installed because an unmanaged template already exists. Use DUELO > Setup Project to back up and replace it."
            );
        }
        else if (
            !install.Success
            && install.Status != DueloWebGLTemplateInstallStatus.MissingCanonicalTemplate
        )
        {
            Debug.LogWarning("DUELO WebGL template auto-install failed: " + install.Message);
        }
    }

    private static DueloWebGLTemplateInstallPlan LogResult(
        DueloWebGLTemplateInstallPlan plan,
        bool logResult
    )
    {
        if (!logResult)
        {
            return plan;
        }

        if (plan.Success)
        {
            Debug.Log("DUELO WebGL template: " + plan.Message);
        }
        else
        {
            Debug.LogWarning("DUELO WebGL template: " + plan.Message);
        }

        return plan;
    }

    private static string ToAbsolutePath(string assetPath)
    {
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
    }
}

[InitializeOnLoad]
internal static class DueloWebGLTemplateAutoInstaller
{
    static DueloWebGLTemplateAutoInstaller()
    {
        EditorApplication.delayCall += DueloWebGLTemplateInstaller.AutoInstallIfMissing;
    }
}
#endif
