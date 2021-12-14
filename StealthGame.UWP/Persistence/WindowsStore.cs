using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StealthGame.Persistence;
using StealthGame.UWP.Persistence;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(WindowsStore))]
namespace StealthGame.UWP.Persistence
{
    /// <summary>
    /// Storage for Windows.
    /// </summary>
    public class WindowsStore : IStore
    {
        /// <summary>
        /// Get the files.
        /// </summary>
        /// <returns>List of files.</returns>
        public async Task<IEnumerable<string>> GetFiles()
        {
            IReadOnlyList<StorageFile> files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
            return files.Select(file => file.Name);
        }

        /// <summary>
        /// Get the last modified time.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <returns>The last modified time.</returns>
        public async Task<DateTime> GetModifiedTime(string name)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(name);
            return (await file.GetBasicPropertiesAsync()).DateModified.DateTime;
        }
    }
}
