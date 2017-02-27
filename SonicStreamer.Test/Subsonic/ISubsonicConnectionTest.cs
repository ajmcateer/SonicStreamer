using System.Threading.Tasks;

namespace SonicStreamer.Test.Subsonic
{
    internal interface ISubsonicConnectionTest
    {
        Task TestServerConnectionTestAsync();
        Task WrongEntriesTestAsync();
        Task EmptyAndNullEntriesTestAsync();
        Task GetArtistsTestAsync();
        Task GetArtistTestAsync();
        Task GetArtistInfoTestAsync();
        Task GetAlbumTestAsync();
        Task GetArtistTracksTestAsync();
        Task GetAlbumTracksTestAsync();
        Task GetTopSongsTestAsync();
        Task GetPlaylistsTestAsync();
        Task GetPlaylistTracksTestAsync();
        Task GetPodcastsTestAsync();
        Task GetPodcastEpisodesTestAsync();
        Task GetNewestPodcastEpisodesTestAsync();
        Task DownloadEpisodeToServerTestAsync();
        Task GetTopFolderTestAsync();
        Task GetFolderContentTestAsync();
        Task GetSearchResultsTestAsync();
        Task GetAlbumSectionTestAsync();
    }
}