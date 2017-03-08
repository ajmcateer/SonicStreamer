using System;
using System.Threading.Tasks;

namespace SonicStreamer.Subsonic.Data
{
    public interface ISubsonicPlayableObject : ISubsonicMusicObject
    {
        string ArtistID { get; set; }
        string Artist { get; set; }
        string AlbumID { get; set; }
        string Album { get; set; }
        string Duration { get; set; }
        TimeSpan DurationTime { get; }
        string DurationOutput { get; }
        string Year { get; set; }
        string Path { get; set; }

        Task<Uri> GetSourceAsync();
    }
}
