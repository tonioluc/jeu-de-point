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

        // Méthode isolée pour brancher les événements souris
        private void InitialiserEcouteSouris()
        {
            MouseClick += Form1_MouseClick;
        }

        // Méthode isolée pour initialiser les joueurs et le tour
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

            pointsPoses[intersection] = JoueurCourant;
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
            DessinerTourCourant(e.Graphics);
        }
    }
}
