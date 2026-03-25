namespace jeu_de_point
{
    public static class ConfigEtudiant
    {
        private const string FichierConfig = "etudiant.txt";
        private const string ValeurDefaut = "XXXX";

        public static string NumeroEtudiant { get; private set; } = ValeurDefaut;

        static ConfigEtudiant()
        {
            Charger();
        }

        private static void Charger()
        {
            try
            {
                // Chercher dans plusieurs emplacements
                string[] chemins =
                [
                    // Répertoire de l'exécutable (bin/Debug ou bin/Release)
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FichierConfig),
                    // Répertoire du projet (en remontant de bin/Debug/net8.0-windows)
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", FichierConfig),
                    // Répertoire courant
                    FichierConfig
                ];

                foreach (var chemin in chemins)
                {
                    string cheminComplet = Path.GetFullPath(chemin);
                    if (File.Exists(cheminComplet))
                    {
                        string contenu = File.ReadAllText(cheminComplet).Trim();
                        if (!string.IsNullOrWhiteSpace(contenu))
                        {
                            NumeroEtudiant = contenu;
                            return;
                        }
                    }
                }
            }
            catch
            {
                NumeroEtudiant = ValeurDefaut;
            }
        }

        public static string ObtenirTexteComplet()
        {
            return $"ETU {NumeroEtudiant}";
        }
    }
}
