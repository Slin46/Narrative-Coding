#if UNITY_6000_5_OR_NEWER
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Works around Pipeline 1.0.0 publishing its descriptor during OnCodeLoaded,
/// when Unity 6.5 forbids AssetDatabase.IsAssetImportWorkerProcess().
/// Retry at the first idle editor update, without changing server permissions or settings.
/// </summary>
[InitializeOnLoad]
internal static class UnityMcpDiscoveryFix
{
    static UnityMcpDiscoveryFix()
    {
        EditorApplication.delayCall += PublishWhenReady;
    }

    private static void PublishWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += PublishWhenReady;
            return;
        }

        if (AssetDatabase.IsAssetImportWorkerProcess())
        {
            return;
        }

        try
        {
            var publisher = Type.GetType("Unity.Pipeline.PipelineDescriptorPublisher, Unity.Pipeline");
            if (publisher == null || UnityEditor.PackageManager.PackageInfo.FindForAssembly(publisher.Assembly)?.version != "1.0.0")
            {
                return;
            }

            // Leave a publisher that started successfully alone. The package owns its heartbeat
            // and normal shutdown, including removing the descriptor when code unloads.
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            var descriptor = publisher.GetField("s_File", flags);
            var start = publisher.GetMethod("Start", flags);
            if (descriptor != null && descriptor.GetValue(null) == null && start != null)
            {
                start.Invoke(null, null);
                if (descriptor.GetValue(null) == null)
                {
                    Debug.LogWarning("Unity MCP discovery could not start; check the Pipeline console messages.");
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Unity MCP discovery retry failed: " + exception.GetBaseException().Message);
        }
    }
}
#endif
