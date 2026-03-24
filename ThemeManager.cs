namespace jeu_de_point
{
    /// <summary>
    /// Gestionnaire de theme qui charge le theme local s'il existe, sinon le theme par defaut.
    /// </summary>
    public static class ThemeManager
    {
        private static ITheme? _themeCourant;

        public static ITheme Theme
        {
            get
            {
                _themeCourant ??= ChargerTheme();
                return _themeCourant;
            }
        }

        private static ITheme ChargerTheme()
        {
            // Essayer de charger le theme local
            // Le type ThemeLocal existe si le fichier ThemeLocal.cs est present
            try
            {
                var typeThemeLocal = Type.GetType("jeu_de_point.ThemeLocal");
                if (typeThemeLocal != null)
                {
                    var instance = Activator.CreateInstance(typeThemeLocal) as ITheme;
                    if (instance != null)
                    {
                        return instance;
                    }
                }
            }
            catch
            {
                // Si le chargement echoue, utiliser le theme par defaut
            }

            return new ThemeDefaut();
        }

        /// <summary>
        /// Force le rechargement du theme
        /// </summary>
        public static void Recharger()
        {
            _themeCourant = ChargerTheme();
        }

        /// <summary>
        /// Verifie si le theme local est utilise
        /// </summary>
        public static bool UtiliseThemeLocal => _themeCourant?.GetType().Name == "ThemeLocal";
    }
}
