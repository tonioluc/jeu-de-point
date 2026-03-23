using System.Drawing.Drawing2D;

namespace jeu_de_point
{
    public partial class Form1 : Form
    {
        private const int GridLines = 10;
        private const int PaddingAroundGrid = 60;

        private readonly Dictionary<(int Col, int Row), Joueur> pointsPoses = new();
        private Joueur[] joueurs = Array.Empty<Joueur>();
        private int indexJoueurCourant;

        // Conserver toutes les lignes tracées (ne plus effacer)
        private readonly List<((int Col, int Row) Debut, (int Col, int Row) Fin, Color Couleur)> lignesAlignements = new();

        // Méthode isolée pour brancher les événements souris
        private void InitialiserEcouteSouris()
        {
            MouseClick += Form1_MouseClick;
        }

        // Méthode isolée pour initiaiser les joueurs et le tour
        private void InitialiserJoueursEtTour()
        {
            joueurs =
            [
                new Joueur("Joueur 1", Color.Red),
                new Joueur("Joueur 2", Color.Blue)
            ];

            indexJoueurCourant = 0;
        }

        private Joueur JoueurCourant => joueurs[indexJoueurCourant];

        private void PasserAuJoueurSuivant()
        {
            indexJoueurCourant = (indexJoueurCourant + 1) % joueurs.Length;
        }

        // Normaliser une ligne pour que Debut <= Fin (lexicographique) afin d'éviter les doublons inversés
        private ((int Col, int Row) Debut, (int Col, int Row) Fin) NormaliserLigne((int Col, int Row) a, (int Col, int Row) b)
        {
            if (a.Col < b.Col) return (a, b);
            if (a.Col > b.Col) return (b, a);
            if (a.Row <= b.Row) return (a, b);
            return (b, a);
        }

        // Méthode isolée: détecter 5 points alignés (horizontale, verticale, oblique)
        private bool TryTrouverAlignementCinq((int Col, int Row) pointJoue, Joueur joueur, out ((int Col, int Row) Debut, (int Col, int Row) Fin) ligne)
        {
            ligne = default;

            (int dCol, int dRow)[] directions =
            [
                (1, 0),
                (0, 1),
                (1, 1),
                (1, -1)
            ];

            foreach (var (dCol, dRow) in directions)
            {
                var sequence = new List<(int Col, int Row)>();

                int col = pointJoue.Col - dCol;
                int row = pointJoue.Row - dRow;
                while (pointsPoses.TryGetValue((col, row), out var jNeg) && ReferenceEquals(jNeg, joueur))
                {
                    sequence.Insert(0, (col, row));
                    col -= dCol;
                    row -= dRow;
                }

                int indexPointJoue = sequence.Count;
                sequence.Add(pointJoue);

                col = pointJoue.Col + dCol;
                row = pointJoue.Row + dRow;
                while (pointsPoses.TryGetValue((col, row), out var jPos) && ReferenceEquals(jPos, joueur))
                {
                    sequence.Add((col, row));
                    col += dCol;
                    row += dRow;
                }

                if (sequence.Count < 5)
                {
                    continue;
                }

                int startIndex = Math.Clamp(indexPointJoue - 2, 0, sequence.Count - 5);
                var debut = sequence[startIndex];
                var fin = sequence[startIndex + 4];

                ligne = (debut, fin);
                return true;
            }

            return false;
        }

        private void Form1_MouseClick(object? sender, MouseEventArgs e)
        {
            if (!TryGetNearestIntersection(e.Location, out var intersection))
            {
                return;
            }

            if (pointsPoses.ContainsKey(intersection))
            {
                return;
            }

            var joueurQuiJoue = JoueurCourant;
            pointsPoses[intersection] = joueurQuiJoue;

            if (TryTrouverAlignementCinq(intersection, joueurQuiJoue, out var ligne))
            {
                var normalisee = NormaliserLigne(ligne.Debut, ligne.Fin);

                bool existe = lignesAlignements.Any(l => l.Debut == normalisee.Debut && l.Fin == normalisee.Fin && l.Couleur.ToArgb() == joueurQuiJoue.Couleur.ToArgb());
                if (!existe)
                {
                    lignesAlignements.Add((normalisee.Debut, normalisee.Fin, joueurQuiJoue.Couleur));
                    joueurQuiJoue.AjouterPoint();
                }
            }

            // Ne pas effacer les anciennes lignes : on ne remet pas ligneAlignementCourante à null, on garde la collection
            PasserAuJoueurSuivant();
            Invalidate();
        }

        // Méthode isolée: transforme un clic en intersection la plus proche
        private bool TryGetNearestIntersection(Point clickPoint, out (int Col, int Row) intersection)
        {
            intersection = default;

            if (!TryGetGridGeometry(out int startX, out int startY, out int gridSize, out float step))
            {
                return false;
            }

            int nearestCol = (int)Math.Round((clickPoint.X - startX) / step);
            int nearestRow = (int)Math.Round((clickPoint.Y - startY) / step);

            nearestCol = Math.Clamp(nearestCol, 0, GridLines - 1);
            nearestRow = Math.Clamp(nearestRow, 0, GridLines - 1);

            float intersectionX = startX + (nearestCol * step);
            float intersectionY = startY + (nearestRow * step);

            float dx = clickPoint.X - intersectionX;
            float dy = clickPoint.Y - intersectionY;
            float distance = (float)Math.Sqrt((dx * dx) + (dy * dy));

            float maxSnapDistance = step * 0.45f;
            if (distance > maxSnapDistance)
            {
                return false;
            }

            intersection = (nearestCol, nearestRow);
            return true;
        }

        private bool TryGetGridGeometry(out int startX, out int startY, out int gridSize, out float step)
        {
            int usableWidth = ClientSize.Width - (PaddingAroundGrid * 2);
            int usableHeight = ClientSize.Height - (PaddingAroundGrid * 2);
            gridSize = Math.Max(100, Math.Min(usableWidth, usableHeight));

            startX = (ClientSize.Width - gridSize) / 2;
            startY = (ClientSize.Height - gridSize) / 2;

            if (GridLines < 2)
            {
                step = 0;
                return false;
            }

            step = gridSize / (float)(GridLines - 1);
            return true;
        }

        // Méthode isolée pour dessiner l'information du tour
        private void DessinerTourCourant(Graphics g)
        {
            if (joueurs.Length == 0)
            {
                return;
            }

            string texte = $"Tour: {JoueurCourant.Nom}";
            using var brush = new SolidBrush(JoueurCourant.Couleur);
            g.DrawString(texte, Font, brush, 20, 20);
        }

        // Méthode isolée pour dessiner le score des joueurs au-dessus de la grille
        private void DessinerScores(Graphics g)
        {
            if (joueurs.Length == 0)
            {
                return;
            }

            if (!TryGetGridGeometry(out int startX, out int startY, out int gridSize, out _))
            {
                return;
            }

            using var scoreFont = new Font(Font.FontFamily, 16f, FontStyle.Bold);
            float espace = 24f;

            float largeurTotale = 0f;
            foreach (var joueur in joueurs)
            {
                string nom = joueur.Nom;
                string valeur = $": {joueur.Score}";
                largeurTotale += g.MeasureString(nom, scoreFont).Width;
                largeurTotale += g.MeasureString(valeur, scoreFont).Width;
            }

            if (joueurs.Length > 1)
            {
                largeurTotale += espace * (joueurs.Length - 1);
            }

            float x = startX + ((gridSize - largeurTotale) / 2f);
            float y = Math.Max(8f, startY - scoreFont.Height - 12f);

            using var brushValeur = new SolidBrush(ForeColor);
            for (int i = 0; i < joueurs.Length; i++)
            {
                string nom = joueurs[i].Nom;
                string valeur = $": {joueurs[i].Score}";

                using var brushNom = new SolidBrush(joueurs[i].Couleur);
                g.DrawString(nom, scoreFont, brushNom, x, y);
                x += g.MeasureString(nom, scoreFont).Width;

                g.DrawString(valeur, scoreFont, brushValeur, x, y);
                x += g.MeasureString(valeur, scoreFont).Width + espace;
            }
        }

        // Méthode isolée pour dessiner toutes les lignes d'alignement déjà trouvées
        private void DessinerLignesAlignement(Graphics g, int startX, int startY, float step)
        {
            if (lignesAlignements.Count == 0)
            {
                return;
            }

            foreach (var (debut, fin, couleur) in lignesAlignements)
            {
                float x1 = startX + (debut.Col * step);
                float y1 = startY + (debut.Row * step);
                float x2 = startX + (fin.Col * step);
                float y2 = startY + (fin.Row * step);

                using var pen = new Pen(couleur, 4.5f);
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        // Méthode isolée pour dessiner la grille, appelable depuis d'autres endroits
        private void dessinerTerrain(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (!TryGetGridGeometry(out int startX, out int startY, out int gridSize, out float step))
            {
                return;
            }

            using var pen = new Pen(Color.LightGray, 2.4f) { DashStyle = DashStyle.Solid };

            for (int i = 0; i < GridLines; i++)
            {
                float y = startY + (i * step);
                g.DrawLine(pen, startX, y, startX + gridSize, y);

                float x = startX + (i * step);
                g.DrawLine(pen, x, startY, x, startY + gridSize);
            }

            float rayonPoint = Math.Max(4f, step * 0.15f);

            foreach (var pointPose in pointsPoses)
            {
                var (col, row) = pointPose.Key;
                var joueur = pointPose.Value;

                float x = startX + (col * step);
                float y = startY + (row * step);

                using var brush = new SolidBrush(joueur.Couleur);
                g.FillEllipse(brush, x - rayonPoint, y - rayonPoint, rayonPoint * 2, rayonPoint * 2);
            }

            // Dessiner toutes les lignes déjà trouvées (persistantes)
            DessinerLignesAlignement(g, startX, startY, step);
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;

            InitialiserJoueursEtTour();
            InitialiserEcouteSouris();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            dessinerTerrain(e.Graphics);
            DessinerScores(e.Graphics);
            DessinerTourCourant(e.Graphics);
        }
    }
}
