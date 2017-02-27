namespace SonicStreamer.Test.Subsonic
{
    public class DemoServerConfig
    {
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public static DemoServerConfig GetSubsonicDemoServer()
        {
            return new DemoServerConfig
            {
                Server = "http://demo.subsonic.org",
                User = "guest1",
                Password = "guest"
            };
        }
    }
}