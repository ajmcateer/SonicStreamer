using SonicStreamer.Subsonic.Server;
using System;
using System.Threading.Tasks;
using static SonicStreamer.ViewModels.LoginViewModel;

namespace SonicStreamer.Common.System
{
    public class SubsonicConnector
    {
        public BaseSubsonicConnection CurrentConnection { get; protected set; }

        public bool HasConnection => CurrentConnection != null;

        private static SubsonicConnector _current;

        public static SubsonicConnector Current
        {
            get { return _current ?? (_current = new SubsonicConnector()); }
            protected set { _current = value; }
        }

        /// <summary>
        /// Tries to connect to the server with the given data and returns the results. 
        /// Sets an instance of the connection in <see cref="CurrentConnection"/>
        /// </summary>
        /// <param name="url">Server address</param>
        /// <param name="user">Login sser</param>
        /// <param name="password">Login password</param>
        /// <param name="serverType">Typ of server</param>
        public async Task<Tuple<bool, string>> TryToConnect(string url, string user, string password,
            ServerType serverType)
        {
            BaseSubsonicConnection testConnector;
            switch (serverType)
            {
                case ServerType.Subsonic:
                    testConnector = new SubsonicConnection();
                    break;
                case ServerType.Madsonic:
                    testConnector = new MadsonicConnection();
                    break;
                default:
                    return new Tuple<bool, string>(false, "No Server type selected");
            }
            var connectorResult = await testConnector.Connect(url, user, password);
            CurrentConnection = connectorResult.Item1 ? testConnector : null;
            return new Tuple<bool, string>(connectorResult.Item1, connectorResult.Item2);
        }

        /// <summary>
        /// Establish a connection to Subsonic test server without Ping request
        /// </summary>
        public async Task SetSubsonicTestServerAsync()
        {
            CurrentConnection = new SubsonicConnection();
            await CurrentConnection.Connect("http://demo.subsonic.org", "guest1", "guest", true);
        }

        /// <summary>
        /// Removes the server connection instance
        /// </summary>
        public void ResetConnection()
        {
            CurrentConnection = null;
        }
    }
}