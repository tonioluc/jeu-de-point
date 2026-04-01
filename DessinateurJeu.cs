using System.Drawing.Drawing2D;

namespace jeu_de_point
{
    public class DessinateurJeu
    {
        private readonly Font baseFont;
        private ITheme Theme => ThemeManager.Theme;

        public DessinateurJeu(Font baseFont)
        {
            this.baseFont = baseFont;
        }

        public void DessinerTerrain(Graphics g, GrilleJeu grille, GeometrieGrille geo)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DessinerLignesGrille(g, grille.NombreLignes, geo);
            DessinerPoints(g, grille.PointsPoses, geo);
            DessinerLignesAlignement(g, grille.LignesAlignements, geo);
        }

        private void DessinerLignesGrille(Graphics g, int gridLines, GeometrieGrille geo)
        {
            using var pen = new Pen(Color.LightGray, 2.4f) { DashStyle = DashStyle.Solid };

            for (int i = 0; i < gridLines; i++)
            {
                float y = geo.StartY + (i * geo.Step);
                g.DrawLine(pen, geo.StartX, y, geo.StartX + geo.GridSize, y);

                float x = geo.StartX + (i * geo.Step);
                g.DrawLine(pen, x, geo.StartY, x, geo.StartY + geo.GridSize);
            }
        }

        private void DessinerPoints(Graphics g, Dictionary<(int Col, int Row), Joueur> pointsPoses, GeometrieGrille geo)
        {
            float rayonPoint = Math.Max(4f, geo.Step * 0.15f);

            foreach (var pointPose in pointsPoses)
            {
                var (col, row) = pointPose.Key;
                var joueur = pointPose.Value;

                float x = geo.StartX + (col * geo.Step);
                float y = geo.StartY + (row * geo.Step);

                using var brush = new SolidBrush(joueur.Couleur);
                g.FillEllipse(brush, x - rayonPoint, y - rayonPoint, rayonPoint * 2, rayonPoint * 2);
            }
        }

        private void DessinerLignesAlignement(Graphics g, List<LigneAlignement> lignes, GeometrieGrille geo)
        {
            foreach (var ligne in lignes)
            {
                float x1 = geo.StartX + (ligne.Debut.Col * geo.Step);
                float y1 = geo.StartY + (ligne.Debut.Row * geo.Step);
                float x2 = geo.StartX + (ligne.Fin.Col * geo.Step);
                float y2 = geo.StartY + (ligne.Fin.Row * geo.Step);

                using var pen = new Pen(ligne.Couleur, 4.5f);
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        public void DessinerCanons(Graphics g, Canon[] canons, GeometrieGrille geo)
        {
            using var fontCanon = new Font(baseFont.FontFamily, Theme.CanonTaillePoliceY, FontStyle.Bold);

            foreach (var canon in canons)
            {
                var rect = canon.ObtenirRectangle(geo.StartX, geo.StartY, geo.GridSize, Theme.CanonLargeur, Theme.CanonEspacement);
                DessinerCanon(g, rect, canon.Couleur, canon.PositionY, geo.Step, canon.EstGauche, fontCanon);
            }
        }

        private void DessinerCanon(Graphics g, Rectangle rectCanon, Color couleur, int yGrille, float step, bool estGauche, Font fontCanon)
        {
            using var brushCanon = new SolidBrush(Color.FromArgb(Theme.CanonOpaciteFond, couleur));
            using var penCanon = new Pen(couleur, Theme.CanonEpaisseurBordure);
            g.FillRectangle(brushCanon, rectCanon);
            g.DrawRectangle(penCanon, rectCanon);

            float y = rectCanon.Top + (yGrille * step);
            y = Math.Clamp(y, rectCanon.Top, rectCanon.Bottom);

            using var penIndicateur = new Pen(couleur, Theme.CanonEpaisseurIndicateur);
            g.DrawLine(penIndicateur, rectCanon.Left, y, rectCanon.Right, y);

            float xPoint = estGauche ? rectCanon.Left - 8 : rectCanon.Right + 8;
            using var brushPoint = new SolidBrush(couleur);
            g.FillEllipse(brushPoint, xPoint - 5, y - 5, 10, 10);

            string texte = $"Y: {yGrille}";
            SizeF tailleTexte = g.MeasureString(texte, fontCanon);
            float xTexte = estGauche ? rectCanon.Left - tailleTexte.Width - 14 : rectCanon.Right + 14;
            float yTexte = y - (tailleTexte.Height / 2f);
            g.DrawString(texte, fontCanon, brushPoint, xTexte, yTexte);
        }

        public void DessinerAnimationTir(Graphics g, AnimationTir animation, Canon[] canons, GeometrieGrille geo)
        {
            if (!animation.Active || canons.Length == 0)
            {
                return;
            }

            int tireurIndex = Math.Clamp(animation.TireurIndex, 0, canons.Length - 1);
            var canon = canons[tireurIndex];

            float y = geo.StartY + (animation.CibleRow * geo.Step);
            float xOrigine = canon.EstGauche
                ? geo.StartX - Theme.CanonEspacement
                : geo.StartX + geo.GridSize + Theme.CanonEspacement;
            float xCible = geo.StartX + (animation.CibleCol * geo.Step);
            float xProjectile = xOrigine + ((xCible - xOrigine) * animation.Progression);

            using var pen = new Pen(canon.Couleur, 1.8f);
            pen.DashStyle = DashStyle.Dot;
            g.DrawLine(pen, xOrigine, y, xProjectile, y);

            using var brush = new SolidBrush(canon.Couleur);
            g.FillEllipse(brush, xProjectile - 5, y - 5, 10, 10);
        }

        public void DessinerScores(Graphics g, Joueur[] joueurs, GeometrieGrille geo, Color foreColor)
        {
            if (joueurs.Length == 0)
            {
                return;
            }

            using var fontScore = new Font(baseFont.FontFamily, Theme.ScoreTaillePolice, FontStyle.Bold);
            float espace = Theme.ScoreEspacementJoueurs;
            float largeurTotale = 0f;

            foreach (var joueur in joueurs)
            {
                string nom = joueur.Nom;
                string valeur = $": {joueur.Score}";
                largeurTotale += g.MeasureString(nom, fontScore).Width;
                largeurTotale += g.MeasureString(valeur, fontScore).Width;
            }

            if (joueurs.Length > 1)
            {
                largeurTotale += espace * (joueurs.Length - 1);
            }

            float x = geo.StartX + ((geo.GridSize - largeurTotale) / 2f);
            float y = Math.Max(8f, geo.StartY - fontScore.Height - 12f);

            using var brushValeur = new SolidBrush(foreColor);
            foreach (var joueur in joueurs)
            {
                string nom = joueur.Nom;
                string valeur = $": {joueur.Score}";

                using var brushNom = new SolidBrush(joueur.Couleur);
                g.DrawString(nom, fontScore, brushNom, x, y);
                x += g.MeasureString(nom, fontScore).Width;

                g.DrawString(valeur, fontScore, brushValeur, x, y);
                x += g.MeasureString(valeur, fontScore).Width + espace;
            }
        }

        public void DessinerHUD(Graphics g, Joueur joueurCourant)
        {
            using var fontInfo = new Font(baseFont.FontFamily, Theme.HudTaillePolice, FontStyle.Bold);

            float x = Theme.HudMargeGauche;
            float y = Theme.HudMargeHaut;
            float hauteurBloc = g.MeasureString("Ag", fontInfo).Height + (Theme.HudPaddingVertical * 2);

            // Bloc ETU avec couleur vive
            string texteEtu = ConfigEtudiant.ObtenirTexteComplet();
            float largeurEtu = g.MeasureString(texteEtu, fontInfo).Width + (Theme.HudPaddingHorizontal * 2);
            var rectEtu = new RectangleF(x, y, largeurEtu, hauteurBloc);
            DessinerBlocInfo(g, rectEtu, texteEtu, Color.FromArgb(0, 120, 215), Color.FromArgb(0, 90, 180), fontInfo, Color.White);
            y += hauteurBloc + Theme.HudEspacementBlocs;

            // Bloc Tour avec couleur du joueur
            string texteTour = "Tour: ";
            string nomJoueur = joueurCourant.Nom;
            float largeurTour = g.MeasureString(texteTour + nomJoueur, fontInfo).Width + (Theme.HudPaddingHorizontal * 2);
            var rectTour = new RectangleF(x, y, largeurTour, hauteurBloc);
            DessinerBlocTour(g, rectTour, texteTour, nomJoueur, joueurCourant.Couleur, fontInfo);
            y += hauteurBloc + Theme.HudEspacementBlocs;

            // Blocs d'instructions
            string[] instructions =
            [
                "Fleches Haut/Bas: deplacer Y canon",
                "CTRL + 1..9: Tir (puissance)"
            ];

            var fonds = Theme.HudCouleursFond;
            var bordures = Theme.HudCouleursBordure;
            float largeurMax = instructions.Max(texte => g.MeasureString(texte, fontInfo).Width) + (Theme.HudPaddingHorizontal * 2);

            for (int i = 0; i < instructions.Length; i++)
            {
                var rect = new RectangleF(x, y, largeurMax, hauteurBloc);
                var fond = i < fonds.Length ? fonds[i] : fonds[0];
                var bordure = i < bordures.Length ? bordures[i] : bordures[0];
                DessinerBlocInfo(g, rect, instructions[i], fond, bordure, fontInfo, Color.FromArgb(240, 240, 240));
                y += hauteurBloc + Theme.HudEspacementBlocs;
            }
        }

        private void DessinerBlocTour(Graphics g, RectangleF rect, string prefixe, string nomJoueur, Color couleurJoueur, Font font)
        {
            // Fond avec la couleur du joueur (semi-transparent)
            using var brushFond = new SolidBrush(Color.FromArgb(180, couleurJoueur));
            g.FillRectangle(brushFond, rect);

            // Bordure avec la couleur du joueur
            using var penBordure = new Pen(couleurJoueur, 2);
            g.DrawRectangle(penBordure, rect.X, rect.Y, rect.Width, rect.Height);

            // Texte "Tour: " en blanc
            using var brushPrefixe = new SolidBrush(Color.White);
            g.DrawString(prefixe, font, brushPrefixe, rect.X + Theme.HudPaddingHorizontal, rect.Y + Theme.HudPaddingVertical);

            // Nom du joueur en jaune pour ressortir
            float largeurPrefixe = g.MeasureString(prefixe, font).Width;
            using var brushNom = new SolidBrush(Color.Yellow);
            g.DrawString(nomJoueur, font, brushNom, rect.X + Theme.HudPaddingHorizontal + largeurPrefixe, rect.Y + Theme.HudPaddingVertical);
        }

        private void DessinerBlocInfo(Graphics g, RectangleF rect, string texte, Color fond, Color bordure, Font font, Color couleurTexte)
        {
            using var brushFond = new SolidBrush(fond);
            using var brushTexte = new SolidBrush(couleurTexte);

            g.FillRectangle(brushFond, rect);

            if (Theme.HudEpaisseurBordure > 0)
            {
                using var penBordure = new Pen(bordure, Theme.HudEpaisseurBordure);
                g.DrawRectangle(penBordure, rect.X, rect.Y, rect.Width, rect.Height);
            }

            g.DrawString(texte, font, brushTexte, rect.X + Theme.HudPaddingHorizontal, rect.Y + Theme.HudPaddingVertical);
        }

        public RectangleF DessinerBlocSuggestion(Graphics g, int nombrePointsGagnants, bool estActif, float yPosition)
        {
            return DessinerBlocSuggestionGenerique(g, "Nombre de 4", nombrePointsGagnants, estActif, yPosition,
                Color.FromArgb(45, 125, 45), Color.FromArgb(60, 180, 60),
                Color.FromArgb(60, 160, 60), Color.FromArgb(100, 220, 100));
        }

        public RectangleF DessinerBlocSuggestion3(Graphics g, int nombrePoints, bool estActif, float yPosition)
        {
            return DessinerBlocSuggestionGenerique(g, "Nombre de 3", nombrePoints, estActif, yPosition,
                Color.FromArgb(180, 120, 30), Color.FromArgb(220, 160, 50),
                Color.FromArgb(210, 150, 40), Color.FromArgb(255, 200, 80));
        }

        private RectangleF DessinerBlocSuggestionGenerique(Graphics g, string label, int nombre, bool estActif, float yPosition,
            Color fondNormalActif, Color bordureNormalActif, Color fondActif, Color bordureActif)
        {
            using var fontInfo = new Font(baseFont.FontFamily, 12f, FontStyle.Bold);

            float x = Theme.HudMargeGauche;
            float y = yPosition;
            float hauteurBloc = g.MeasureString("Ag", fontInfo).Height + (Theme.HudPaddingVertical * 2) + 8;

            string texte = $"{label}: {nombre}";
            float largeurBloc = g.MeasureString(texte, fontInfo).Width + (Theme.HudPaddingHorizontal * 2) + 10;

            var rect = new RectangleF(x, y, largeurBloc, hauteurBloc);

            // Couleur selon l'etat et si cliquable
            Color fondNormal = nombre > 0 ? fondNormalActif : Color.FromArgb(80, 80, 80);
            Color bordureNormal = nombre > 0 ? bordureNormalActif : Color.FromArgb(120, 120, 120);

            // Si actif (suggestions affichees), couleur plus vive
            Color fond = estActif ? fondActif : fondNormal;
            Color bordure = estActif ? bordureActif : bordureNormal;

            using var brushFond = new SolidBrush(fond);
            using var penBordure = new Pen(bordure, 2.5f);

            g.FillRectangle(brushFond, rect);
            g.DrawRectangle(penBordure, rect.X, rect.Y, rect.Width, rect.Height);

            // Texte
            Color couleurTexte = nombre > 0 ? Color.White : Color.FromArgb(180, 180, 180);
            using var brushTexte = new SolidBrush(couleurTexte);
            float textY = rect.Y + ((rect.Height - fontInfo.Height) / 2);
            g.DrawString(texte, fontInfo, brushTexte, rect.X + Theme.HudPaddingHorizontal + 5, textY);

            // Indicateur "cliquable" si > 0
            if (nombre > 0)
            {
                using var fontSmall = new Font(baseFont.FontFamily, 7f, FontStyle.Italic);
                string hint = estActif ? "(cliquer pour masquer)" : "(cliquer pour afficher)";
                using var brushHint = new SolidBrush(Color.FromArgb(200, 200, 200));
                g.DrawString(hint, fontSmall, brushHint, rect.X + Theme.HudPaddingHorizontal + 5, rect.Bottom - 14);
            }

            return rect;
        }

        public void DessinerIndicateursSuggestion(Graphics g, List<(int Col, int Row)> pointsGagnants, GeometrieGrille geo, Color couleurJoueur)
        {
            DessinerIndicateursSuggestionAvecStyle(g, pointsGagnants, geo, couleurJoueur, System.Drawing.Drawing2D.DashStyle.Dash);
        }

        public void DessinerIndicateursSuggestion3(Graphics g, List<(int Col, int Row)> points, GeometrieGrille geo, Color couleurJoueur)
        {
            // Style different pour les suggestions de 3 (pointille plus large + etoile au lieu de +)
            DessinerIndicateursSuggestionAvecStyle(g, points, geo, Color.FromArgb(255, 180, 50), System.Drawing.Drawing2D.DashStyle.Dot, true);
        }

        private void DessinerIndicateursSuggestionAvecStyle(Graphics g, List<(int Col, int Row)> points, GeometrieGrille geo,
            Color couleur, System.Drawing.Drawing2D.DashStyle styleBordure, bool utiliserEtoile = false)
        {
            if (points.Count == 0)
            {
                return;
            }

            float rayonIndicateur = Math.Max(8f, geo.Step * 0.25f);

            foreach (var point in points)
            {
                float x = geo.StartX + (point.Col * geo.Step);
                float y = geo.StartY + (point.Row * geo.Step);

                // Cercle semi-transparent
                using var brushFond = new SolidBrush(Color.FromArgb(80, couleur));
                g.FillEllipse(brushFond, x - rayonIndicateur, y - rayonIndicateur, rayonIndicateur * 2, rayonIndicateur * 2);

                // Bordure
                using var penBordure = new Pen(Color.FromArgb(180, couleur), 2.5f);
                penBordure.DashStyle = styleBordure;
                g.DrawEllipse(penBordure, x - rayonIndicateur, y - rayonIndicateur, rayonIndicateur * 2, rayonIndicateur * 2);

                // Symbole au centre
                float tailleS = rayonIndicateur * 0.5f;
                using var penSymbole = new Pen(Color.FromArgb(200, Color.White), 2f);

                if (utiliserEtoile)
                {
                    // Etoile (X + |)
                    g.DrawLine(penSymbole, x - tailleS, y, x + tailleS, y);
                    g.DrawLine(penSymbole, x, y - tailleS, x, y + tailleS);
                    g.DrawLine(penSymbole, x - tailleS * 0.7f, y - tailleS * 0.7f, x + tailleS * 0.7f, y + tailleS * 0.7f);
                    g.DrawLine(penSymbole, x + tailleS * 0.7f, y - tailleS * 0.7f, x - tailleS * 0.7f, y + tailleS * 0.7f);
                }
                else
                {
                    // Simple +
                    g.DrawLine(penSymbole, x - tailleS, y, x + tailleS, y);
                    g.DrawLine(penSymbole, x, y - tailleS, x, y + tailleS);
                }
            }
        }
    }
}
