using System.Drawing.Drawing2D;

namespace jeu_de_point
{
    public partial class Form1 : Form
    {
        private const int GridLines = 9;
        private const int PaddingAroundGrid = 60;

        private readonly HashSet<(int Col, int Row)> pointsVerts = new();

        // Méthode isolée pour brancher les événements souris
        private void InitialiserEcouteSouris()
        {
            MouseClick += Form1_MouseClick;
        }

        private void Form1_MouseClick(object? sender, MouseEventArgs e)
        {
            if (TryGetNearestIntersection(e.Location, out var intersection))
            {
                pointsVerts.Add(intersection);
                Invalidate();
            }
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
            using var brush = new SolidBrush(Color.Green);

            foreach (var (col, row) in pointsVerts)
            {
                float x = startX + (col * step);
                float y = startY + (row * step);
                g.FillEllipse(brush, x - rayonPoint, y - rayonPoint, rayonPoint * 2, rayonPoint * 2);
            }
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;
            InitialiserEcouteSouris();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // tracer le terrain
            dessinerTerrain(e.Graphics);
        }
    }
}
