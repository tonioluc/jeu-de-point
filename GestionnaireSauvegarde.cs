namespace jeu_de_point
{
    public class GestionnaireSauvegarde
    {
        private readonly Connexion connexion;
        private int? partieCouranteId;
        private int ordreAction;
        private bool erreurAffichee;

        public int? PartieCouranteId => partieCouranteId;
        public bool EstConnecte => partieCouranteId.HasValue;

        public GestionnaireSauvegarde(Connexion connexion)
        {
            this.connexion = connexion;
        }

        public bool CreerNouvellePartie(GrilleJeu grille, Joueur[] joueurs, Canon[] canons)
        {
            try
            {
                var etat = ConstruireEtat(grille, joueurs, canons, 0);
                partieCouranteId = connexion.CreerPartie(etat);
                ordreAction = 0;
                EnregistrerAction(0, "DEBUT_PARTIE", new Dictionary<string, object?>
                {
                    ["gridLines"] = grille.NombreLignes
                });
                return true;
            }
            catch (Exception ex)
            {
                partieCouranteId = null;
                MessageBox.Show($"Connexion PostgreSQL indisponible: {ex.Message}",
                    "Base de donnees", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        public void SauvegarderAction(int joueurIndex, string typeAction, Dictionary<string, object?> details,
            GrilleJeu grille, Joueur[] joueurs, Canon[] canons, int indexJoueurCourant)
        {
            if (partieCouranteId is null)
            {
                return;
            }

            try
            {
                var etat = ConstruireEtat(grille, joueurs, canons, indexJoueurCourant);
                connexion.MettreAJourEtatPartie(partieCouranteId.Value, etat);

                ordreAction++;
                EnregistrerAction(joueurIndex, typeAction, details);
                erreurAffichee = false;
            }
            catch (Exception ex)
            {
                if (!erreurAffichee)
                {
                    erreurAffichee = true;
                    MessageBox.Show($"Sauvegarde automatique indisponible: {ex.Message}",
                        "Base de donnees", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void EnregistrerAction(int joueurIndex, string typeAction, Dictionary<string, object?> details)
        {
            if (partieCouranteId is null) return;

            connexion.AjouterAction(new ActionPartie
            {
                PartieId = partieCouranteId.Value,
                OrdreAction = ordreAction,
                JoueurIndex = joueurIndex,
                TypeAction = typeAction,
                Details = details
            });
        }

        public List<PartieResume> ListerParties()
        {
            return connexion.ListerParties();
        }

        public EtatPartieSauvegarde? ChargerPartie(int partieId)
        {
            var etat = connexion.ChargerEtatPartie(partieId);
            if (etat is not null)
            {
                partieCouranteId = partieId;
                ordreAction = connexion.Scalar(
                    "SELECT COALESCE(MAX(ordre_action), 0) FROM actions_partie WHERE partie_id = @partieId",
                    new Dictionary<string, object?> { ["partieId"] = partieId }) is int i ? i : 0;
            }
            return etat;
        }

        public void SupprimerPartie(int partieId)
        {
            connexion.SupprimerPartie(partieId);
        }

        private EtatPartieSauvegarde ConstruireEtat(GrilleJeu grille, Joueur[] joueurs, Canon[] canons, int indexJoueurCourant)
        {
            var points = new List<PointSauvegarde>();
            foreach (var point in grille.PointsPoses)
            {
                int joueurIndex = Array.FindIndex(joueurs, j => ReferenceEquals(j, point.Value));
                if (joueurIndex >= 0)
                {
                    points.Add(new PointSauvegarde
                    {
                        Col = point.Key.Col,
                        Row = point.Key.Row,
                        JoueurIndex = joueurIndex
                    });
                }
            }

            var lignes = grille.LignesAlignements.Select(l => new LigneSauvegarde
            {
                DebutCol = l.Debut.Col,
                DebutRow = l.Debut.Row,
                FinCol = l.Fin.Col,
                FinRow = l.Fin.Row,
                CouleurArgb = l.Couleur.ToArgb()
            }).ToList();

            var positionsTirees = new List<PositionTireeSauvegarde>();
            foreach (var kvp in grille.PositionsTirees)
            {
                foreach (var joueurIndex in kvp.Value)
                {
                    positionsTirees.Add(new PositionTireeSauvegarde
                    {
                        Col = kvp.Key.Col,
                        Row = kvp.Key.Row,
                        JoueurIndex = joueurIndex
                    });
                }
            }

            return new EtatPartieSauvegarde
            {
                PartieId = partieCouranteId ?? 0,
                GridLines = grille.NombreLignes,
                IndexJoueurCourant = indexJoueurCourant,
                ScoreJ1 = joueurs.Length > 0 ? joueurs[0].Score : 0,
                ScoreJ2 = joueurs.Length > 1 ? joueurs[1].Score : 0,
                CanonYJ1 = canons.Length > 0 ? canons[0].PositionY : 0,
                CanonYJ2 = canons.Length > 1 ? canons[1].PositionY : 0,
                Points = points,
                Lignes = lignes,
                PositionsTirees = positionsTirees
            };
        }

        public void RestaurerEtat(EtatPartieSauvegarde etat, GrilleJeu grille, Joueur[] joueurs, Canon[] canons, out int indexJoueurCourant)
        {
            grille.Reinitialiser(etat.GridLines);

            joueurs[0].DefinirScore(etat.ScoreJ1);
            joueurs[1].DefinirScore(etat.ScoreJ2);
            indexJoueurCourant = Math.Clamp(etat.IndexJoueurCourant, 0, joueurs.Length - 1);

            if (canons.Length > 0)
            {
                canons[0].DefinirPositionY(etat.CanonYJ1, grille.NombreLignes);
            }
            if (canons.Length > 1)
            {
                canons[1].DefinirPositionY(etat.CanonYJ2, grille.NombreLignes);
            }

            foreach (var p in etat.Points)
            {
                if (p.JoueurIndex >= 0 && p.JoueurIndex < joueurs.Length)
                {
                    grille.PoserPoint(p.Col, p.Row, joueurs[p.JoueurIndex]);
                }
            }

            foreach (var l in etat.Lignes)
            {
                grille.LignesAlignements.Add(new LigneAlignement(
                    (l.DebutCol, l.DebutRow),
                    (l.FinCol, l.FinRow),
                    Color.FromArgb(l.CouleurArgb)));
            }

            foreach (var pt in etat.PositionsTirees)
            {
                grille.EnregistrerTir(pt.Col, pt.Row, pt.JoueurIndex);
            }
        }
    }
}
