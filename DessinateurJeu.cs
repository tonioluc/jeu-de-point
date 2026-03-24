using System.Drawing.Drawing2D;

namespace jeu_de_point
{
    public class DessinateurJeu
    {
        private readonly Font fontInfo;
        private readonly Font fontScore;
        private readonly Font fontCanon;

        public DessinateurJeu(Font baseFont)
        {
            fontInfo = new Font(baseFont.FontFamily, 10.5f, FontStyle.Bold);
            fontScore = new Font(baseFont.FontFamily, 16f, FontStyle.Bold);
            fontCanon = new Font(baseFont.FontFamily, 9f, FontStyle.Bold);
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
            foreach (var canon in canons)
            {
                var rect = canon.ObtenirRectangle(geo.StartX, geo.StartY, geo.GridSize);
                DessinerCanon(g, rect, canon.Couleur, canon.PositionY, geo.Step, canon.EstGauche);
            }
        }

        private void DessinerCanon(Graphics g, Rectangle rectCanon, Color couleur, int yGrille, float step, bool estGauche)
        {
            using var brushCanon = new SolidBrush(Color.FromArgb(70, couleur));
            using var penCanon = new Pen(couleur, 2f);
            g.FillRectangle(brushCanon, rectCanon);
            g.DrawRectangle(penCanon, rectCanon);

            float y = rectCanon.Top + (yGrille * step);
            y = Math.Clamp(y, rectCanon.Top, rectCanon.Bottom);

            using var penIndicateur = new Pen(couleur, 3f);
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
                ? geo.StartX - Canon.Espacement
                : geo.StartX + geo.GridSize + Canon.Espacement;
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

            float espace = 24f;
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
            string[] informations =
            [
                $"Tour: {joueurCourant.Nom}",
                "Fleches Haut/Bas: deplacer Y canon",
                "CTRL + 1..9: Tir (puissance)"
            ];

            Color[] fonds =
            [
                Color.FromArgb(226, 240, 255),
                Color.FromArgb(234, 246, 236),
                Color.FromArgb(255, 241, 222)
            ];

            Color[] bordures =
            [
                Color.FromArgb(49, 112, 194),
                Color.FromArgb(62, 145, 95),
                Color.FromArgb(198, 128, 34)
            ];

            float x = 12f;
            float y = 12f;
            float largeurMax = informations.Max(texte => g.MeasureString(texte, fontInfo).Width) + 24f;
            float hauteurBloc = g.MeasureString("Ag", fontInfo).Height + 12f;

            for (int i = 0; i < informations.Length; i++)
            {
                var rect = new RectangleF(x, y, largeurMax, hauteurBloc);
                DessinerBlocInfo(g, rect, informations[i], fonds[i], bordures[i]);
                y += hauteurBloc + 8f;
            }
        }

        private void DessinerBlocInfo(Graphics g, RectangleF rect, string texte, Color fond, Color bordure)
        {
            using var brushFond = new SolidBrush(fond);
            using var penBordure = new Pen(bordure, 1.6f);
            using var brushTexte = new SolidBrush(Color.FromArgb(30, 30, 30));

            g.FillRectangle(brushFond, rect);
            g.DrawRectangle(penBordure, rect.X, rect.Y, rect.Width, rect.Height);
            g.DrawString(texte, fontInfo, brushTexte, rect.X + 10f, rect.Y + 6f);
        }
    }
}
