using System;
using System.Collections.ObjectModel;
using StealthGame.Model;

namespace StealthGame.ViewModel
{
    /// <summary>
    /// Stored game handler viewmodel.
    /// </summary>
    public class StoredGameBrowserViewModel : ViewModelBase
    {
        private StoredGameBrowserModel _model;

        private string _name;
        private DateTime _modified;

        /// <summary>
        /// Get the name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Get the last modified time.
        /// </summary>
        public DateTime Modified
        {
            get { return _modified; }
            set
            {
                if (_modified != value)
                {
                    _modified = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Load event.
        /// </summary>
        public event EventHandler<StoredGameEventArgs> GameLoading;

        /// <summary>
        /// Save event.
        /// </summary>
        public event EventHandler<StoredGameEventArgs> GameSaving;

        /// <summary>
        /// Stored game handler instance.
        /// </summary>
        /// <param name="model">The model.</param>
        public StoredGameBrowserViewModel(StoredGameBrowserModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            _model = model;
            _model.StoreChanged += new EventHandler(Model_StoreChanged);

            NewSaveCommand = new DelegateCommand(param => OnGameSaving((string)param));
            StoredGames = new ObservableCollection<StoredGameViewModel>();
            UpdateStoredGames();
        }

        /// <summary>
        /// New save command.
        /// </summary>
        public DelegateCommand NewSaveCommand { get; private set; }

        /// <summary>
        /// Stored games collection.
        /// </summary>
        public ObservableCollection<StoredGameViewModel> StoredGames { get; private set; }

        /// <summary>
        /// Update stored games.
        /// </summary>
        private void UpdateStoredGames()
        {
            StoredGames.Clear();

            foreach (StoredGameModel item in _model.StoredGames)
            {
                StoredGames.Add(new StoredGameViewModel
                {
                    Name = item.Name,
                    Modified = item.Modified,
                    LoadGameCommand = new DelegateCommand(param => OnGameLoading((string)param)),
                    SaveGameCommand = new DelegateCommand(param => OnGameSaving((string)param))
                });
            }
        }

        private void Model_StoreChanged(object sender, EventArgs e)
        {
            UpdateStoredGames();
        }

        private void OnGameLoading(string name)
        {
            GameLoading?.Invoke(this, new StoredGameEventArgs { Name = name });
        }

        private void OnGameSaving(string name)
        {
            GameSaving?.Invoke(this, new StoredGameEventArgs { Name = name });
        }
    }
}
