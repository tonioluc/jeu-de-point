using System.Drawing.Drawing2D;

namespace jeu_de_point
{
    public class Canon
    {
        public const int Largeur = 20;
        public const int Espacement = 20;
        public const int PuissanceMin = 1;
        public const int PuissanceMax = 9;

        public int PositionY { get; private set; }
        public Color Couleur { get; }
        public bool EstGauche { get; }

        public Canon(Color couleur, bool estGauche)
        {
            Couleur = couleur;
            EstGauche = estGauche;
            PositionY = 0;
        }

        public void Monter()
        {
            if (PositionY > 0)
            {
                PositionY--;
            }
        }

        public void Descendre(int maxY)
        {
            if (PositionY < maxY - 1)
            {
                PositionY++;
            }
        }

        public void DefinirPositionY(int positionY, int maxY)
        {
            PositionY = Math.Clamp(positionY, 0, maxY - 1);
        }

        public int CalculerColonneImpact(int puissance, int gridLines)
        {
            int maxCol = gridLines - 1;
            int puissanceBornee = Math.Clamp(puissance, PuissanceMin, PuissanceMax);

            double projectionColonne1Based = (puissanceBornee * gridLines) / (double)PuissanceMax;
            int colonne1Based = Math.Max(1, (int)Math.Floor(projectionColonne1Based));
            int colDepuisGauche = Math.Clamp(colonne1Based - 1, 0, maxCol);

            return EstGauche ? colDepuisGauche : maxCol - colDepuisGauche;
        }

        public Rectangle ObtenirRectangle(int startX, int startY, int gridSize)
        {
            return ObtenirRectangle(startX, startY, gridSize, Largeur, Espacement);
        }

        public Rectangle ObtenirRectangle(int startX, int startY, int gridSize, int largeur, int espacement)
        {
            int x = EstGauche
                ? startX - espacement - largeur
                : startX + gridSize + espacement;

            return new Rectangle(x, startY, largeur, gridSize);
        }

        public static int ExtrairePuissanceDepuisTouche(Keys keyCode)
        {
            return keyCode switch
            {
                Keys.D1 or Keys.NumPad1 => 1,
                Keys.D2 or Keys.NumPad2 => 2,
                Keys.D3 or Keys.NumPad3 => 3,
                Keys.D4 or Keys.NumPad4 => 4,
                Keys.D5 or Keys.NumPad5 => 5,
                Keys.D6 or Keys.NumPad6 => 6,
                Keys.D7 or Keys.NumPad7 => 7,
                Keys.D8 or Keys.NumPad8 => 8,
                Keys.D9 or Keys.NumPad9 => 9,
                _ => 0
            };
        }
    }

    public class AnimationTir
    {
        private readonly System.Windows.Forms.Timer timer;
        private DateTime debut;
        private const int DureeMs = 220;

        public bool Active { get; private set; }
        public int TireurIndex { get; private set; }
        public int CibleCol { get; private set; }
        public int CibleRow { get; private set; }

        public event Action? OnTermine;
        public event Action? OnTick;

        public AnimationTir()
        {
            timer = new System.Windows.Forms.Timer { Interval = 25 };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!Active)
            {
                timer.Stop();
                return;
            }

            if (Progression >= 1f)
            {
                Active = false;
                timer.Stop();
                OnTermine?.Invoke();
            }

            OnTick?.Invoke();
        }

        public void Demarrer(int tireurIndex, int cibleCol, int cibleRow)
        {
            Active = true;
            debut = DateTime.UtcNow;
            TireurIndex = tireurIndex;
            CibleCol = cibleCol;
            CibleRow = cibleRow;
            timer.Start();
        }

        public float Progression
        {
            get
            {
                if (!Active) return 0f;
                float p = (float)((DateTime.UtcNow - debut).TotalMilliseconds / DureeMs);
                return Math.Clamp(p, 0f, 1f);
            }
        }
    }
}
