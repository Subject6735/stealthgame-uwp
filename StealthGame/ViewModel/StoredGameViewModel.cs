using System;

namespace StealthGame.ViewModel
{
    /// <summary>
    /// Stored game viewmodel.
    /// </summary>
    public class StoredGameViewModel : ViewModelBase
    {
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
        /// Load command.
        /// </summary>
        public DelegateCommand LoadGameCommand { get; set; }

        /// <summary>
        /// Save command.
        /// </summary>
        public DelegateCommand SaveGameCommand { get; set; }
    }
}
