namespace SonicStreamer.Subsonic.Exception
{
    public class SubsonicConnectionException : System.Exception
    {
        public SubsonicConnectionException()
        {
        }

        public SubsonicConnectionException(string message) : base(message)
        {
        }

        public SubsonicConnectionException(string message, System.Exception inner) : base(message, inner)
        {
        }
    }
}