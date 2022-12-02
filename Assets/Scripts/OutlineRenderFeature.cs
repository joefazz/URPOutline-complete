using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class OutlineSettings
{
    public Shader OutlineShader;
    public Color OutlineColor;
    public int Scale = 1;
    public float DepthNormalThresholdScale = 7.0f;
    public float DepthThreshold = 0.2f;
    public float NormalThreshold = 0.2f;
    public float DepthNormalThreshold;
}

public class OutlineRenderFeature : ScriptableRendererFeature
{
    OutlinePass m_OutlinePass;

    public OutlineSettings Settings;

    Material m_Material;

    public override void Create()
    {
        if (Settings.OutlineShader != null)
            m_Material = new Material(Settings.OutlineShader);

        m_OutlinePass = new OutlinePass(m_Material,
            Settings.OutlineColor,
            Settings.Scale,
            Settings.DepthThreshold,
            Settings.NormalThreshold,
            Settings.DepthNormalThreshold,
            Settings.DepthNormalThresholdScale);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;

        m_OutlinePass.ConfigureInput(ScriptableRenderPassInput.Color);
        m_OutlinePass.SetTarget(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_OutlinePass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Material);
    }
}
