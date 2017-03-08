using Microsoft.HockeyApp;
using SonicStreamer.Pages;
using SonicStreamer.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SonicStreamer.Common.System;

namespace SonicStreamer
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    public sealed partial class App : Application
    {
        private bool _isInBackgroundMode;

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt.  Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            HockeyClient.Current.Configure("a9852be06be949f48dd3e16d271f9bc5");
            InitializeComponent();
            MemoryManager.AppMemoryUsageLimitChanging += MemoryManager_AppMemoryUsageLimitChanging;
            MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;
            EnteredBackground += App_EnteredBackground;
            LeavingBackground += App_LeavingBackground;
            Suspending += OnSuspending;
            Resuming += OnResuming;
            UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            ApplicationView.PreferredLaunchViewSize = new Size(1200.0, 760.0);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = false;
            }
#endif

            await CreateRootFrameAsync(e.PreviousExecutionState, e.Arguments);

            // Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate();
        }


        private async Task CreateRootFrameAsync(ApplicationExecutionState previousExecutionState, string arguments)
        {
            var rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (previousExecutionState == ApplicationExecutionState.Terminated)
                {
                    var loginVm = Current.Resources[Constants.ViewModelLogin] as LoginViewModel;
                    if (loginVm != null)
                    {
                        loginVm.RestoreData();
                        if (await loginVm.LoginAsync())
                        {
                            var restoreTasks = new List<Task>();
                            foreach (var ressource in Current.Resources)
                            {
                                var viewModel = ressource.Value as IViewModelSerializable;
                                if (viewModel != null)
                                {
                                    restoreTasks.Add(viewModel.RestoreViewModelAsync(ressource.Key as string));
                                }
                            }
                            await Task.WhenAll(restoreTasks);
                        }
                    }
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                // übergeben werden
                rootFrame.Navigate(typeof(LoginPage), arguments);
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            var saveTasks = new List<Task>();
            foreach (var ressource in Current.Resources)
            {
                var viewModel = ressource.Value as IViewModelSerializable;
                if (viewModel != null)
                {
                    saveTasks.Add(viewModel.SaveViewModelAsync(ressource.Key as string));
                }
            }
            await Task.WhenAll(saveTasks);
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
            //throw new NotImplementedException();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HockeyClient.Current.TrackException(e.Exception);
            e.Handled = true;
        }

        private void MemoryManager_AppMemoryUsageLimitChanging(object sender, AppMemoryUsageLimitChangingEventArgs e)
        {
            if (MemoryManager.AppMemoryUsage >= e.NewLimit)
            {
                ReduceMemoryUsage(e.NewLimit);
            }
        }

        private void MemoryManager_AppMemoryUsageIncreased(object sender, object e)
        {
            var level = MemoryManager.AppMemoryUsageLevel;
            if (level == AppMemoryUsageLevel.OverLimit || level == AppMemoryUsageLevel.High)
            {
                ReduceMemoryUsage(MemoryManager.AppMemoryUsageLimit);
            }
        }

        public void ReduceMemoryUsage(ulong limit)
        {
            if (_isInBackgroundMode && Window.Current.Content != null)
            {
                Window.Current.Content = null;
            }
            GC.Collect();
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            _isInBackgroundMode = true;
        }

        private async void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            _isInBackgroundMode = false;

            if (Window.Current.Content == null)
            {
                await CreateRootFrameAsync(ApplicationExecutionState.Running, string.Empty);
            }
        }
    }
}