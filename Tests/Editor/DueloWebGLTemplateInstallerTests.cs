#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;

public sealed class DueloWebGLTemplateInstallerTests
{
    [Test]
    public void GetInstallPlanCreatesMissingTemplate()
    {
        DueloWebGLTemplateInstallPlan plan = DueloWebGLTemplateInstaller.GetInstallPlan(
            DueloWebGLTemplateInstaller.VersionMarker + "\n<html></html>",
            null,
            allowOverwriteUnmanaged: false
        );

        Assert.AreEqual(DueloWebGLTemplateInstallStatus.Created, plan.Status);
        Assert.IsTrue(plan.Success);
        Assert.IsTrue(plan.ShouldWriteTemplate);
    }

    [Test]
    public void GetInstallPlanUpdatesManagedTemplate()
    {
        DueloWebGLTemplateInstallPlan plan = DueloWebGLTemplateInstaller.GetInstallPlan(
            DueloWebGLTemplateInstaller.VersionMarker + "\n<html>current</html>",
            DueloWebGLTemplateInstaller.VersionMarker + "\n<html>old</html>",
            allowOverwriteUnmanaged: false
        );

        Assert.AreEqual(DueloWebGLTemplateInstallStatus.Updated, plan.Status);
        Assert.IsTrue(plan.Success);
        Assert.IsTrue(plan.ShouldWriteTemplate);
    }

    [Test]
    public void GetInstallPlanRefusesUnmanagedTemplate()
    {
        DueloWebGLTemplateInstallPlan plan = DueloWebGLTemplateInstaller.GetInstallPlan(
            DueloWebGLTemplateInstaller.VersionMarker + "\n<html>current</html>",
            "<html>developer custom template</html>",
            allowOverwriteUnmanaged: false
        );

        Assert.AreEqual(DueloWebGLTemplateInstallStatus.ConflictingTemplate, plan.Status);
        Assert.IsFalse(plan.Success);
        Assert.IsFalse(plan.ShouldWriteTemplate);
    }

    [Test]
    public void PackageAssetsResolveByStableGuid()
    {
        Assert.AreEqual(
            AssetDatabase.GUIDToAssetPath(DueloWebGLTemplateInstaller.CanonicalTemplateGuid),
            DueloWebGLTemplateInstaller.CanonicalTemplateSourcePath
        );
        Assert.That(
            DueloWebGLTemplateInstaller.CanonicalTemplateSourcePath,
            Does.EndWith("/Editor/Templates/WebGL/Duelo/index.html")
        );
        Assert.AreEqual(
            AssetDatabase.GUIDToAssetPath(DueloWebGLSettings.RequiredBridgePluginGuid),
            DueloWebGLSettings.RequiredBridgePluginPath
        );
        Assert.That(
            DueloWebGLSettings.RequiredBridgePluginPath,
            Does.EndWith("/Runtime/Plugins/WebGL/DueloBridge.jslib")
        );
    }
}
#endif
