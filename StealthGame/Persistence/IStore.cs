using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StealthGame.Persistence
{
    /// <summary>
    /// Game storage interface
    /// </summary>
    public interface IStore
    {
        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <returns>The list of files.</returns>
        Task<IEnumerable<string>> GetFiles();

        /// <summary>
        /// Gets the last modified time.
        /// </summary>
        /// <param name="name">Name of the file.</param>
        /// <returns>The last modified time.</returns>
        Task<DateTime> GetModifiedTime(string name);
    }
}
