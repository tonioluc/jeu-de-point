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

            string[] informations =
            [
                $"Tour: {joueurCourant.Nom}",
                "Fleches Haut/Bas: deplacer Y canon",
                "CTRL + 1..9: Tir (puissance)"
            ];

            var fonds = Theme.HudCouleursFond;
            var bordures = Theme.HudCouleursBordure;

            float x = Theme.HudMargeGauche;
            float y = Theme.HudMargeHaut;
            float largeurMax = informations.Max(texte => g.MeasureString(texte, fontInfo).Width) + (Theme.HudPaddingHorizontal * 2);
            float hauteurBloc = g.MeasureString("Ag", fontInfo).Height + (Theme.HudPaddingVertical * 2);

            for (int i = 0; i < informations.Length; i++)
            {
                var rect = new RectangleF(x, y, largeurMax, hauteurBloc);
                var fond = i < fonds.Length ? fonds[i] : fonds[0];
                var bordure = i < bordures.Length ? bordures[i] : bordures[0];
                DessinerBlocInfo(g, rect, informations[i], fond, bordure, fontInfo);
                y += hauteurBloc + Theme.HudEspacementBlocs;
            }
        }

        private void DessinerBlocInfo(Graphics g, RectangleF rect, string texte, Color fond, Color bordure, Font font)
        {
            using var brushFond = new SolidBrush(fond);
            using var brushTexte = new SolidBrush(Color.FromArgb(240, 240, 240));

            g.FillRectangle(brushFond, rect);

            if (Theme.HudEpaisseurBordure > 0)
            {
                using var penBordure = new Pen(bordure, Theme.HudEpaisseurBordure);
                g.DrawRectangle(penBordure, rect.X, rect.Y, rect.Width, rect.Height);
                brushTexte.Color = Color.FromArgb(30, 30, 30);
            }

            g.DrawString(texte, font, brushTexte, rect.X + Theme.HudPaddingHorizontal, rect.Y + Theme.HudPaddingVertical);
        }
    }
}
