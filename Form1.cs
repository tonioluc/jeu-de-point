using System.Drawing.Drawing2D;

namespace jeu_de_point
{
    public partial class Form1 : Form
    {
        private const int GridLines = 30;
        private const int PaddingAroundGrid = 60;

        // Méthode isolée pour dessiner la grille, appelable depuis d'autres endroits
        private void dessinerTerrain(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int usableWidth = ClientSize.Width - (PaddingAroundGrid * 2);
            int usableHeight = ClientSize.Height - (PaddingAroundGrid * 2);
            int gridSize = Math.Max(100, Math.Min(usableWidth, usableHeight));

            int startX = (ClientSize.Width - gridSize) / 2;
            int startY = (ClientSize.Height - gridSize) / 2;

            float step = gridSize / (float)(GridLines - 1);

            using var pen = new Pen(Color.Black, 2.4f) { DashStyle = DashStyle.Solid };

            for (int i = 0; i < GridLines; i++)
            {
                float y = startY + (i * step);
                g.DrawLine(pen, startX, y, startX + gridSize, y);

                float x = startX + (i * step);
                g.DrawLine(pen, x, startY, x, startY + gridSize);
            }
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // tracer le terrain
            dessinerTerrain(e.Graphics);
        }
    }
}
