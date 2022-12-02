using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class NormalRenderFeature : ScriptableRendererFeature
{
    class NormalPass : ScriptableRenderPass
    {
        public void Setup()
        {
            ConfigureInput(ScriptableRenderPassInput.Normal);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
    
    NormalPass m_NormalPass;

    public override void Create()
    {
        m_NormalPass = new NormalPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_NormalPass.Setup();
        renderer.EnqueuePass(m_NormalPass);
    }
}
