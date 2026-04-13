namespace ConfigurationLogic.Graphics
{
    public static class GraphicsPluginFactory
    {
        public static IGraphicsPlugin CreateImageCompositorGraphicsPlugin(int width, int height)
        {
            return new ImageCompositorGraphicsPluginImpl(width, height);
        }

        public static IGraphicsPlugin CreateVulkanGraphicsPlugin(string appName, int width, int height)
        {
            return new VulkanGraphicsPluginImpl(appName, width, height);
        }

        public static IGraphicsPlugin CreateMoltenVulkanGraphicsPlugin()
        {
            throw new NotImplementedException();
        }

        //--If needed, add more graphics devices--//
    }
}
