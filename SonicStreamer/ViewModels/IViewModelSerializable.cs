using System.Threading.Tasks;

namespace SonicStreamer.ViewModels
{
    public interface IViewModelSerializable
    {
        /// <summary>
        /// Sichert die Daten des ViewModels in ein XML unter dem angegebenen Namen
        /// </summary>
        Task SaveViewModelAsync(string savename);

        /// <summary>
        /// Stellt die Daten des ViewModels aus einem XML unter mit dem angegebenen Namen wieder her
        /// </summary>
        Task RestoreViewModelAsync(string loadname);
    }
}