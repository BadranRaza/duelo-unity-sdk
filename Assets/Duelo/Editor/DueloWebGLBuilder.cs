#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class DueloWebGLBuilder
{
    [MenuItem("DUELO/Build WebGL")]
    public static void BuildWebGLFromMenu()
    {
        BuildWebGLInternal(exitOnFinish: false);
    }

    public static void BuildWebGL()
    {
        BuildWebGLInternal(exitOnFinish: true);
    }

    private static void BuildWebGLInternal(bool exitOnFinish)
    {
        string outputDir = Environment.GetEnvironmentVariable("DUELO_BUILD_OUTPUT");
        if (string.IsNullOrEmpty(outputDir))
        {
            outputDir = Path.Combine(Path.GetTempPath(), "DueloWebGLBuild");
        }

        string locationPathName = Path.Combine(outputDir, "DueloWebGL");

        if (Directory.Exists(locationPathName))
        {
            Directory.Delete(locationPathName, true);
        }
        Directory.CreateDirectory(locationPathName);

        DueloWebGLSettings.ApplyRequiredSettings(logResult: true);

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new Exception("No enabled scenes in EditorBuildSettings.");
        }

        Debug.Log($"[DueloBuilder] Building {scenes.Length} scene(s) to {locationPathName}");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = locationPathName,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"[DueloBuilder] Result: {summary.result}, totalSize={summary.totalSize}, output={summary.outputPath}");

        if (summary.result != BuildResult.Succeeded)
        {
            if (exitOnFinish) EditorApplication.Exit(1);
            return;
        }

        WriteBuildInfo(locationPathName);

        if (exitOnFinish) EditorApplication.Exit(0);
        else EditorUtility.RevealInFinder(locationPathName);
    }

    private static void WriteBuildInfo(string locationPathName)
    {
        string commit = RunGit("rev-parse HEAD");
        string branch = RunGit("rev-parse --abbrev-ref HEAD");
        string porcelain = RunGit("status --porcelain");

        // Fail LOUD, not clean: an unreadable git state must never mint "dirty: false".
        bool gitUnreadable = commit == "unknown" || branch == "unknown" || porcelain == "unknown";
        bool dirty = gitUnreadable || !string.IsNullOrEmpty(porcelain);

        static string Esc(string v) => (v ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");

        var json =
            "{\n" +
            $"  \"commit\": \"{Esc(commit)}\",\n" +
            $"  \"branch\": \"{Esc(branch)}\",\n" +
            $"  \"dirty\": {(dirty ? "true" : "false")},\n" +
            $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
            $"  \"builtAtUtc\": \"{DateTime.UtcNow:o}\",\n" +
            $"  \"productName\": \"{Esc(Application.productName)}\"\n" +
            "}\n";

        File.WriteAllText(Path.Combine(locationPathName, "build-info.json"), json);
        Debug.Log($"[DueloBuilder] build-info.json commit={commit} branch={branch} dirty={dirty}");

        if (dirty)
            Debug.LogWarning("[DueloBuilder] Working tree is DIRTY — this build is not reproducible.");
    }

    private static string RunGit(string args)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", args)
            {
                WorkingDirectory = Path.GetDirectoryName(Application.dataPath),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.RedirectStandardError = true;
            using var p = System.Diagnostics.Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd().Trim();
            if (!p.WaitForExit(5000)) { p.Kill(); return "unknown"; }
            if (p.ExitCode != 0) return "unknown";
            return output;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DueloBuilder] git {args} failed: {e.Message}");
            return "unknown";
        }
    }
}
#endif
