namespace MiniPipeline.Core
{
    public class DefaultPipelineProcessor : PipelineProcessorBase
    {
        public DefaultPipelineProcessor(IHttpContextPipeline pipeline,IEnumerable<IPipelineAddon> addons) : base(pipeline,addons)
        {
        }
    }
}
