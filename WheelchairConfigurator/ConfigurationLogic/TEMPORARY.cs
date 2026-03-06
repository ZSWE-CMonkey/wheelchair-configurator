namespace ConfigurationLogic
{
    //THIS IS TEMPORARY EXAMPLE THAT INCLUDES ALL THINGS INSIDE THIS SINGLE FILE
    //TODO: DELETE THIS LATER, USED AS TESTING FOR TEAM

    public static class TemporaryFactory
    {
        public static ITemporaryLogic CreateTemporaryLogic()
        {
            return new TemporaryLogicImpl();
        }
    }

    public interface ITemporaryLogic
    {
        public string GetTemporary();
    }

    internal class TemporaryLogicImpl : ITemporaryLogic
    {
        public string GetTemporary()
        {
            return "TEMPORARY LOOOL";
        }
    }
}
