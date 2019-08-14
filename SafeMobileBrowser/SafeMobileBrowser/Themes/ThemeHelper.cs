﻿using SafeMobileBrowser.Helpers;
using Xamarin.Forms;

namespace SafeMobileBrowser.Themes
{
    public class ThemeHelper
    {
        public enum AppThemeMode
        {
            Light,
            Dark
        }

        /// <summary>
        /// Changes the theme of the app.
        /// Add additional switch cases for more themes you add to the app.
        /// This also updates the local key storage value for the selected theme.
        /// </summary>
        /// <param name="theme"></param>
        public static void ToggleTheme(AppThemeMode theme, bool isAppLaunched = false)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();

                // Update local key value with the new theme you select.
                Xamarin.Essentials.Preferences.Set(Constants.AppThemePreferenceKey, theme == AppThemeMode.Dark);

                switch (theme)
                {
                    case AppThemeMode.Light:
                        mergedDictionaries.Add(new LightTheme());
                        break;
                    case AppThemeMode.Dark:
                        mergedDictionaries.Add(new DarkTheme());
                        break;
                    default:
                        mergedDictionaries.Add(new LightTheme());
                        break;
                }

                DependencyService.Get<INativeThemeManager>().ChangeAppTheme(theme, isAppLaunched);
                MessagingCenter.Send((App)Application.Current, MessageCenterConstants.UpdateWelcomePageTheme);
            }
        }

        private static void ManuallyCopyThemes(ResourceDictionary fromResource, ResourceDictionary toResource)
        {
            foreach (var item in fromResource.Keys)
            {
                toResource[item] = fromResource[item];
            }
        }

        /// <summary>
        /// Reads current theme id from the local storage and loads it.
        /// </summary>
        public static void LoadTheme()
        {
            var currentTheme = CurrentTheme();
            ToggleTheme(currentTheme, true);
        }

        /// <summary>
        /// Gives current/last selected theme from the local storage.
        /// </summary>
        /// <returns></returns>
        public static AppThemeMode CurrentTheme()
        {
            bool isDarkTheme = Xamarin.Essentials.Preferences.Get(Constants.AppThemePreferenceKey, false);
            if (isDarkTheme)
                return AppThemeMode.Dark;
            else
                return AppThemeMode.Light;
        }
    }
}
