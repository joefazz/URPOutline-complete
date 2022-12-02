using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class OutlinePass : ScriptableRenderPass
{
    readonly float m_Scale, m_DepthThreshold, m_NormalThreshold, m_DepthNormalThreshold, m_DepthNormalThresholdScale;
    readonly Color m_Color;
    RenderTargetIdentifier m_CameraColorTarget;
    Material m_Material;
    static int colorID = Shader.PropertyToID("_OutlineColor");
    static int scaleID = Shader.PropertyToID("_Scale");
    static int depthThresholdID = Shader.PropertyToID("_DepthThreshold");
    static int normalThresholdID = Shader.PropertyToID("_NormalThreshold");
    static int depthNormalThresholdID = Shader.PropertyToID("_DepthNormalThreshold");
    static int depthNormalThresholdScaleID = Shader.PropertyToID("_DepthNormalThresholdScale");

    public OutlinePass(Material material, Color color, float scale, float depthThreshold, float normalThreshold, float depthNormalThreshold, float depthNormalThresholdScale)
    {
        m_Material = material;
        m_Color = color;
        m_Scale = scale;
        m_DepthThreshold = depthThreshold;
        m_NormalThreshold = normalThreshold;
        m_DepthNormalThreshold = depthNormalThreshold;
        m_DepthNormalThresholdScale = depthNormalThresholdScale;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void SetTarget(RenderTargetIdentifier colorHandle)
    {
        m_CameraColorTarget = colorHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        base.OnCameraSetup(cmd, ref renderingData);
        ConfigureTarget(new RenderTargetIdentifier(m_CameraColorTarget, 0, CubemapFace.Unknown, -1));
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var camera = renderingData.cameraData.camera;
        if (camera.cameraType != CameraType.Game)
            return;

        if (m_Material == null)
            return;

        CommandBuffer cb = CommandBufferPool.Get(name: "OutlinePass");
        cb.BeginSample("Outline Pass");

        m_Material.SetColor(colorID, m_Color);
        m_Material.SetFloat(scaleID, m_Scale);
        m_Material.SetFloat(depthThresholdID, m_DepthThreshold);
        m_Material.SetFloat(normalThresholdID, m_NormalThreshold);
        m_Material.SetFloat(depthNormalThresholdID, m_DepthNormalThreshold);
        m_Material.SetFloat(depthNormalThresholdScaleID, m_DepthNormalThresholdScale);

        cb.SetRenderTarget(new RenderTargetIdentifier(m_CameraColorTarget, 0, CubemapFace.Unknown, -1));
        cb.ClearRenderTarget(true, false, clearColor);
        cb.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material);

        cb.EndSample("Outline Pass");
        context.ExecuteCommandBuffer(cb);
        cb.Clear();
        CommandBufferPool.Release(cb);
    }
}

