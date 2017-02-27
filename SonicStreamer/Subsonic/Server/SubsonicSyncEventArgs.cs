using System;

namespace SonicStreamer.Subsonic.Server
{
    public class SubsonicSyncEventArgs : EventArgs
    {
        public string SyncedItemIndex { get; private set; }

        public SubsonicSyncEventArgs(string item)
        {
            SyncedItemIndex = item;
        }
    }
}