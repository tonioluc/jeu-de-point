namespace jeu_de_point
{
    public partial class Form1 : Form
    {
        private const int PaddingAroundGrid = 60;

        private GrilleJeu grille = null!;
        private Joueur[] joueurs = [];
        private Canon[] canons = [];
        private int indexJoueurCourant;

        private readonly Connexion connexion = new();
        private GestionnaireSauvegarde sauvegarde = null!;
        private DessinateurJeu dessinateur = null!;
        private MenuUI menuUI = null!;
        private AnimationTir animationTir = null!;

        private EtatEcran etatEcran = EtatEcran.MenuPrincipal;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;

            InitialiserComposants();
            InitialiserEvenements();
            menuUI.AfficherEcran(EtatEcran.MenuPrincipal);
        }

        private void InitialiserComposants()
        {
            grille = new GrilleJeu(10);
            sauvegarde = new GestionnaireSauvegarde(connexion);
            dessinateur = new DessinateurJeu(Font);
            menuUI = new MenuUI(this, Font);
            animationTir = new AnimationTir();

            menuUI.Initialiser();
            InitialiserJoueurs();
        }

        private void InitialiserEvenements()
        {
            KeyPreview = true;
            MouseClick += OnMouseClick;
            KeyDown += OnKeyDown;

            animationTir.OnTick += () => Invalidate();

            menuUI.OnNouvellePartieClick += AfficherConfigurationNouvellePartie;
            menuUI.OnChargerPartieClick += AfficherChargementPartie;
            menuUI.OnMenuPrincipalClick += AfficherMenuPrincipal;
            menuUI.OnDemarrerPartieClick += DemarrerNouvellePartie;
            menuUI.OnOuvrirPartieClick += ChargerPartieSelectionnee;
            menuUI.OnSupprimerPartieClick += SupprimerPartieSelectionnee;
        }

        private void InitialiserJoueurs()
        {
            joueurs =
            [
                new Joueur("Joueur 1", Color.Red),
                new Joueur("Joueur 2", Color.Blue)
            ];

            canons =
            [
                new Canon(Color.Red, estGauche: true),
                new Canon(Color.Blue, estGauche: false)
            ];

            indexJoueurCourant = 0;
        }

        private Joueur JoueurCourant => joueurs[indexJoueurCourant];
        private Canon CanonCourant => canons[indexJoueurCourant];

        private void PasserAuJoueurSuivant()
        {
            indexJoueurCourant = (indexJoueurCourant + 1) % joueurs.Length;
        }

        #region Navigation Ecrans

        private void AfficherMenuPrincipal()
        {
            etatEcran = EtatEcran.MenuPrincipal;
            menuUI.AfficherEcran(etatEcran);
            Invalidate();
        }

        private void AfficherConfigurationNouvellePartie()
        {
            etatEcran = EtatEcran.ConfigurationNouvellePartie;
            menuUI.AfficherEcran(etatEcran);
            Invalidate();
        }

        private void AfficherChargementPartie()
        {
            etatEcran = EtatEcran.ChargementPartie;
            menuUI.AfficherEcran(etatEcran);
            ChargerListeParties();
            Invalidate();
        }

        private void ChargerListeParties()
        {
            try
            {
                var parties = sauvegarde.ListerParties();
                menuUI.MettreAJourListeParties(parties);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de lecture des parties: {ex.Message}",
                    "Base de donnees", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Gestion Partie

        private void DemarrerNouvellePartie(int nombreLignes)
        {
            grille.Reinitialiser(nombreLignes);
            InitialiserJoueurs();

            sauvegarde.CreerNouvellePartie(grille, joueurs, canons);

            etatEcran = EtatEcran.Partie;
            menuUI.AfficherEcran(etatEcran);
            Invalidate();
        }

        private void ChargerPartieSelectionnee()
        {
            var partie = menuUI.ObtenirPartieSelectionnee();
            if (partie is null) return;

            try
            {
                var etat = sauvegarde.ChargerPartie(partie.Id);
                if (etat is null)
                {
                    MessageBox.Show("Partie introuvable.", "Charger une partie",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                InitialiserJoueurs();
                sauvegarde.RestaurerEtat(etat, grille, joueurs, canons, out indexJoueurCourant);

                etatEcran = EtatEcran.Partie;
                menuUI.AfficherEcran(etatEcran);
                Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement: {ex.Message}",
                    "Base de donnees", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SupprimerPartieSelectionnee()
        {
            var partie = menuUI.ObtenirPartieSelectionnee();
            if (partie is null) return;

            var confirmation = MessageBox.Show(
                $"Voulez-vous vraiment supprimer la partie #{partie.Id} ?\nCette action est irreversible.",
                "Confirmer la suppression",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmation != DialogResult.Yes) return;

            try
            {
                sauvegarde.SupprimerPartie(partie.Id);
                ChargerListeParties();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de suppression: {ex.Message}",
                    "Base de donnees", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Evenements Souris et Clavier

        private void OnMouseClick(object? sender, MouseEventArgs e)
        {
            if (etatEcran != EtatEcran.Partie) return;

            if (!grille.TryGetGeometrie(ClientSize, PaddingAroundGrid, out var geo)) return;
            if (!grille.TryGetIntersectionProche(e.Location, geo, out var intersection)) return;
            if (!grille.PeutPoserPoint(intersection.Col, intersection.Row)) return;

            var joueur = JoueurCourant;
            grille.PoserPoint(intersection.Col, intersection.Row, joueur);

            if (grille.TryTrouverAlignementCinq(intersection, joueur, out var ligne))
            {
                if (grille.AjouterLigneAlignementSiNouvelle(ligne))
                {
                    joueur.AjouterPoint();
                }
            }

            SauvegarderAction("ClicSouris", new Dictionary<string, object?>
            {
                ["Col"] = intersection.Col,
                ["Row"] = intersection.Row
            });

            PasserAuJoueurSuivant();
            Invalidate();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (etatEcran != EtatEcran.Partie || joueurs.Length == 0) return;

            if (e.Control)
            {
                int puissance = Canon.ExtrairePuissanceDepuisTouche(e.KeyCode);
                if (puissance >= Canon.PuissanceMin)
                {
                    ExecuterTir(puissance);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                return;
            }

            var canon = CanonCourant;
            int anciennePosition = canon.PositionY;

            if (e.KeyCode == Keys.Up)
            {
                canon.Monter();
            }
            else if (e.KeyCode == Keys.Down)
            {
                canon.Descendre(grille.NombreLignes);
            }
            else
            {
                return;
            }

            if (canon.PositionY != anciennePosition)
            {
                Invalidate();
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void ExecuterTir(int puissance)
        {
            var canon = CanonCourant;
            int cibleCol = canon.CalculerColonneImpact(puissance, grille.NombreLignes);
            int cibleRow = canon.PositionY;
            var cible = (Col: cibleCol, Row: cibleRow);

            animationTir.Demarrer(indexJoueurCourant, cibleCol, cibleRow);

            if (grille.PointsPoses.TryGetValue(cible, out var joueurTouche))
            {
                var tireur = JoueurCourant;
                bool pointAdverse = !ReferenceEquals(joueurTouche, tireur);

                if (pointAdverse && !grille.PointEstProtege(cible, joueurTouche))
                {
                    grille.RetirerPoint(cible.Col, cible.Row);
                }
            }

            SauvegarderAction("Tir", new Dictionary<string, object?>
            {
                ["Puissance"] = puissance,
                ["ColonneImpact"] = cibleCol,
                ["LigneImpact"] = cibleRow
            });

            PasserAuJoueurSuivant();
            Invalidate();
        }

        private void SauvegarderAction(string typeAction, Dictionary<string, object?> details)
        {
            sauvegarde.SauvegarderAction(indexJoueurCourant, typeAction, details,
                grille, joueurs, canons, indexJoueurCourant);
        }

        #endregion

        #region Dessin

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (etatEcran != EtatEcran.Partie) return;
            if (!grille.TryGetGeometrie(ClientSize, PaddingAroundGrid, out var geo)) return;

            dessinateur.DessinerTerrain(e.Graphics, grille, geo);
            dessinateur.DessinerCanons(e.Graphics, canons, geo);
            dessinateur.DessinerAnimationTir(e.Graphics, animationTir, canons, geo);
            dessinateur.DessinerScores(e.Graphics, joueurs, geo, ForeColor);
            dessinateur.DessinerHUD(e.Graphics, JoueurCourant);
        }

        #endregion
    }
}
