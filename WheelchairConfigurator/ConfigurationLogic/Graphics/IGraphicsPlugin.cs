namespace ConfigurationLogic.Graphics
{
    public interface IGraphicsPlugin
    {
        public bool Initialize();
        public void Render();
        public void Deinitialize();
    }
}
