namespace jeu_de_point
{
    public class Joueur
    {
        public Joueur(string nom, Color couleur)
        {
            Nom = nom;
            Couleur = couleur;
        }

        public string Nom { get; }
        public Color Couleur { get; }
    }
}
