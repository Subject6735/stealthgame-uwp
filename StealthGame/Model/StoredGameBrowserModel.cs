using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StealthGame.Persistence;

namespace StealthGame.Model
{
    /// <summary>
    /// Stored model handler.
    /// </summary>
    public class StoredGameBrowserModel
    {
        private readonly IStore _store;

        /// <summary>
        /// Store changed event.
        /// </summary>
        public event EventHandler StoreChanged;

        public StoredGameBrowserModel(IStore store)
        {
            _store = store;
            StoredGames = new List<StoredGameModel>();
        }

        /// <summary>
        /// Get the stored games.
        /// </summary>
        public List<StoredGameModel> StoredGames { get; private set; }

        /// <summary>
        /// Update stored games.
        /// </summary>
        public async Task UpdateAsync()
        {
            if (_store == null)
                return;

            StoredGames.Clear();

            // load saved games
            foreach (string name in await _store.GetFiles())
            {
                if (name == "SuspendedGame")
                    continue;

                StoredGames.Add(new StoredGameModel
                {
                    Name = name,
                    Modified = await _store.GetModifiedTime(name)
                });
            }

            StoredGames = StoredGames.OrderByDescending(item => item.Modified).ToList();

            OnSavesChanged();
        }

        private void OnSavesChanged()
        {
            StoreChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
