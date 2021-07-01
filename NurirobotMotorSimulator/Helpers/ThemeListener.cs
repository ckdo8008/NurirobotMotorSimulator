using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace NurirobotMotorSimulator.Helpers
{
    public delegate void ThemeChangedEvent(ThemeListener sender);

    public sealed class ThemeListener : IDisposable
    {
        /// <summary>
        /// Gets the Name of the Current Theme.
        /// </summary>
        public string CurrentThemeName
        {
            get { return this.CurrentTheme.ToString(); }
        }

        /// <summary>
        /// Gets or sets the Current Theme.
        /// </summary>
        public ApplicationTheme CurrentTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current theme is high contrast.
        /// </summary>
        public bool IsHighContrast { get; set; }

        /// <summary>
        /// Gets or sets which DispatcherQueue is used to dispatch UI updates.
        /// </summary>
        public DispatcherQueue DispatcherQueue { get; set; }

        /// <summary>
        /// An event that fires if the Theme changes.
        /// </summary>
        public event ThemeChangedEvent ThemeChanged;

        private AccessibilitySettings _accessible = new AccessibilitySettings();
        private UISettings _settings = new UISettings();

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeListener"/> class.
        /// </summary>
        /// <param name="dispatcherQueue">The DispatcherQueue that should be used to dispatch UI updates, or null if this is being called from the UI thread.</param>
        public ThemeListener(DispatcherQueue dispatcherQueue = null)
        {
            CurrentTheme = Application.Current.RequestedTheme;
            IsHighContrast = _accessible.HighContrast;

            DispatcherQueue = dispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

            _accessible.HighContrastChanged += Accessible_HighContrastChanged;
            _settings.ColorValuesChanged += Settings_ColorValuesChanged;

            // Fallback in case either of the above fail, we'll check when we get activated next.
            if (Window.Current != null)
            {
                Window.Current.CoreWindow.Activated += CoreWindow_Activated;
            }
        }

        private void Accessible_HighContrastChanged(AccessibilitySettings sender, object args)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("HighContrast Changed");
#endif

            UpdateProperties();
        }

        // Note: This can get called multiple times during HighContrast switch, do we care?
        private async void Settings_ColorValuesChanged(UISettings sender, object args)
        {
            await OnColorValuesChanged();
        }

        // Internal abstraction is used by the Unit Tests
        internal Task OnColorValuesChanged()
        {
            // Getting called off thread, so we need to dispatch to request value.
            return DispatcherQueue.EnqueueAsync(
                () =>
                {
                    // TODO: This doesn't stop the multiple calls if we're in our faked 'White' HighContrast Mode below.
                    if (CurrentTheme != Application.Current.RequestedTheme ||
                        IsHighContrast != _accessible.HighContrast)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Color Values Changed");
#endif

                        UpdateProperties();
                    }
                }, DispatcherQueuePriority.Normal);
        }

        private void CoreWindow_Activated(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.WindowActivatedEventArgs args)
        {
            if (CurrentTheme != Application.Current.RequestedTheme ||
                IsHighContrast != _accessible.HighContrast)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("CoreWindow Activated Changed");
#endif

                UpdateProperties();
            }
        }

        /// <summary>
        /// Set our current properties and fire a change notification.
        /// </summary>
        private void UpdateProperties()
        {
            // TODO: Not sure if HighContrastScheme names are localized?
            if (_accessible.HighContrast && _accessible.HighContrastScheme.IndexOf("white", StringComparison.OrdinalIgnoreCase) != -1)
            {
                // If our HighContrastScheme is ON & a lighter one, then we should remain in 'Light' theme mode for Monaco Themes Perspective
                IsHighContrast = false;
                CurrentTheme = ApplicationTheme.Light;
            }
            else
            {
                // Otherwise, we just set to what's in the system as we'd expect.
                IsHighContrast = _accessible.HighContrast;
                CurrentTheme = Application.Current.RequestedTheme;
            }

            ThemeChanged?.Invoke(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _accessible.HighContrastChanged -= Accessible_HighContrastChanged;
            _settings.ColorValuesChanged -= Settings_ColorValuesChanged;
            if (Window.Current != null)
            {
                Window.Current.CoreWindow.Activated -= CoreWindow_Activated;
            }
        }
    }
}
