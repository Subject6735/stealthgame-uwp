using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using StealthGame.Model;
using StealthGame.Persistence;
using StealthGame.ViewModel;
using StealthGame.View;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace StealthGame
{
    public partial class App : Application
    {
        #region Fields

        private readonly IStealthGameDataAccess _dataAccess;
        private readonly StealthGameModel _model;
        private readonly StealthGameViewModel _viewModel;
        private readonly GamePage _gamePage;
        private readonly SettingsPage _settingsPage;

        private readonly IStore _store;
        private readonly StoredGameBrowserModel _storedGameBrowserModel;
        private readonly StoredGameBrowserViewModel _storedGameBrowserViewModel;
        private readonly LoadGamePage _loadGamePage;
        private readonly SaveGamePage _saveGamePage;

        private bool _timerEnabled;
        private readonly NavigationPage _mainPage;

        #endregion

        public App()
        {
            _dataAccess = DependencyService.Get<IStealthGameDataAccess>();

            _model = new StealthGameModel(_dataAccess);
            _model.PlayerDetected += new EventHandler<StealthGameEventArgs>(Model_PlayerDetected);
            _model.PlayerReachedExit += new EventHandler<StealthGameEventArgs>(Model_PlayerReachedExit);

            _viewModel = new StealthGameViewModel(_model);
            _viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            _viewModel.LoadGame += new EventHandler(ViewModel_LoadGame);
            _viewModel.SaveGame += new EventHandler(ViewModel_SaveGame);
            _viewModel.QuitGame += new EventHandler(ViewModel_QuitGame);
            _viewModel.PauseGame += new EventHandler(ViewModel_PauseGame);
            _viewModel.ResumeGame += new EventHandler(ViewModel_ResumeGame);
            _viewModel.Help += new EventHandler(ViewModel_Help);
            _viewModel.Settings += new EventHandler(ViewModel_Settings);

            _gamePage = new GamePage
            {
                BindingContext = _viewModel
            };

            _settingsPage = new SettingsPage
            {
                BindingContext = _viewModel
            };

            _store = DependencyService.Get<IStore>();
            _storedGameBrowserModel = new StoredGameBrowserModel(_store);
            _storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
            _storedGameBrowserViewModel.GameLoading += new EventHandler<StoredGameEventArgs>(StoredViewModel_GameLoading);
            _storedGameBrowserViewModel.GameSaving += new EventHandler<StoredGameEventArgs>(StoredViewModel_GameSaving);

            _loadGamePage = new LoadGamePage
            {
                BindingContext = _storedGameBrowserViewModel
            };

            _saveGamePage = new SaveGamePage
            {
                BindingContext = _storedGameBrowserViewModel
            };

            _mainPage = new NavigationPage(_gamePage);

            MainPage = _mainPage;
        }

        private void TimerOperations()
        {
            _model.MoveGuards();
            _viewModel.RefreshTable();
            _viewModel.RefreshVisionCone();
            _model.GuardDetect();
        }

        private void StartTimer()
        {
            _timerEnabled = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), () => { TimerOperations(); return _timerEnabled; });
        }

        private void StopTimer()
        {
            _timerEnabled = false;
        }

        protected override void OnStart()
        {
            _model.NewGame();
            _viewModel.RefreshTable();
            _viewModel.RefreshVisionCone();
            _viewModel.RefreshPlayerPos();
            StartTimer();
        }

        protected override void OnSleep()
        {
            StopTimer();

            try
            {
                _ = Task.Run(async () => await _model.SaveGameAsync("SuspendedGame"));
            }
            catch { }
        }

        protected override void OnResume()
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    await _model.LoadGameAsync("SuspendedGame");
                    _viewModel.RefreshTable();
                    _viewModel.RefreshVisionCone();
                    _viewModel.RefreshPlayerPos();

                    StartTimer();
                });

            }
            catch { }
        }

        private async void ViewModel_NewGame(object sender, EventArgs e)
        {
            _model.NewGame();
            _viewModel.RefreshTable();
            _viewModel.RefreshVisionCone();
            _viewModel.RefreshPlayerPos();

            _ = await _mainPage.PopAsync();

            if (!_timerEnabled)
            {
                StartTimer();
            }

            _viewModel.EnablePause = true;
            _viewModel.EnableResume = false;
        }

        private async void ViewModel_LoadGame(object sender, EventArgs e)
        {
            await _storedGameBrowserModel.UpdateAsync();
            await _mainPage.PushAsync(_loadGamePage);
        }

        private async void ViewModel_SaveGame(object sender, EventArgs e)
        {
            await _storedGameBrowserModel.UpdateAsync();
            await _mainPage.PushAsync(_saveGamePage);
        }

        private void ViewModel_QuitGame(object sender, EventArgs e)
        {
            StopTimer();
            Environment.Exit(0);
        }

        private async void ViewModel_Settings(object sender, EventArgs e)
        {
            StopTimer();
            _viewModel.EnablePause = false;
            _viewModel.EnableResume = true;
            await _mainPage.PushAsync(_settingsPage);
        }

        private void ViewModel_PauseGame(object sender, EventArgs e)
        {
            StopTimer();
            _viewModel.EnablePause = false;
            _viewModel.EnableResume = true;
        }

        private void ViewModel_ResumeGame(object sender, EventArgs e)
        {
            if (!_timerEnabled)
            {
                StartTimer();
            }

            _viewModel.EnablePause = true;
            _viewModel.EnableResume = false;
        }

        private async void ViewModel_Help(object sender, EventArgs e)
        {
            StopTimer();

            string help =
                "You are the blue square."
                + Environment.NewLine + "You have to reach the exit (indicated by a green area) to win the game."
                + Environment.NewLine + "Avoid being spotted by guards (red squares), they have a vision cone, indicated by light blue areas."
                + Environment.NewLine + "To move, use buttons: move up-down-left-right; you can only move vertically and horizontially."
                + Environment.NewLine + "You can change the difficulty by selecting it in the settings then starting a new game."
                + Environment.NewLine + "To pause the game, select pause, to resume, select resume."
                + Environment.NewLine + "To start a new game, load a game, save the game or quit, select settings.";

            await MainPage.DisplayAlert("StealthGame", help, "Ok");

            if (!_timerEnabled)
            {
                StartTimer();
                _viewModel.EnablePause = true;
                _viewModel.EnableResume = false;
            }
        }

        private async void StoredViewModel_GameLoading(object sender, StoredGameEventArgs e)
        {
            _ = await _mainPage.PopAsync();

            Debug.WriteLine(e.Name);

            try
            {
                await _model.LoadGameAsync(e.Name);
                _viewModel.RefreshTable();
                _viewModel.RefreshVisionCone();
                _viewModel.RefreshPlayerPos();

                if (!_timerEnabled)
                {
                    StartTimer();
                    _viewModel.EnablePause = true;
                    _viewModel.EnableResume = false;
                }
            }
            catch
            {
                await MainPage.DisplayAlert("StealthGame", "Loading game failed", "Ok");
            }
        }

        private async void StoredViewModel_GameSaving(object sender, StoredGameEventArgs e)
        {
            _ = await _mainPage.PopAsync();

            StopTimer();

            try
            {
                await _model.SaveGameAsync(e.Name);
            }
            catch { }

            await MainPage.DisplayAlert("StealthGame", "Saving successful", "Ok");
        }

        private async void Model_PlayerReachedExit(object sender, StealthGameEventArgs e)
        {
            if (e.IsOver)
            {
                StopTimer();

                await MainPage.DisplayAlert("StealthGame", "Congratulations! You reached the exit.", "Ok");

                _model.NewGame();
                _viewModel.RefreshTable();
                _viewModel.RefreshVisionCone();
                _viewModel.RefreshPlayerPos();

                if (!_timerEnabled)
                {
                    StartTimer();
                    _viewModel.EnablePause = true;
                    _viewModel.EnableResume = false;
                }
            }
        }

        private async void Model_PlayerDetected(object sender, StealthGameEventArgs e)
        {
            if (e.IsOver)
            {
                StopTimer();

                await MainPage.DisplayAlert("StealthGame", "Game over! You have been detected!", "Ok");

                _model.NewGame();
                _viewModel.RefreshTable();
                _viewModel.RefreshVisionCone();
                _viewModel.RefreshPlayerPos();

                if (!_timerEnabled)
                {
                    StartTimer();
                    _viewModel.EnablePause = true;
                    _viewModel.EnableResume = false;
                }
            }
        }
    }
}
