namespace jeu_de_point
{
    public class GrilleJeu
    {
        public int NombreLignes { get; private set; }
        public Dictionary<(int Col, int Row), Joueur> PointsPoses { get; } = new();
        public List<LigneAlignement> LignesAlignements { get; } = new();

        public GrilleJeu(int nombreLignes)
        {
            NombreLignes = Math.Max(2, nombreLignes);
        }

        public void Reinitialiser(int nombreLignes)
        {
            NombreLignes = Math.Max(2, nombreLignes);
            PointsPoses.Clear();
            LignesAlignements.Clear();
        }

        public bool PeutPoserPoint(int col, int row)
        {
            return !PointsPoses.ContainsKey((col, row));
        }

        public void PoserPoint(int col, int row, Joueur joueur)
        {
            PointsPoses[(col, row)] = joueur;
        }

        public bool RetirerPoint(int col, int row)
        {
            return PointsPoses.Remove((col, row));
        }

        public bool TryTrouverAlignementCinq((int Col, int Row) pointJoue, Joueur joueur, out LigneAlignement ligne)
        {
            ligne = default!;

            (int dCol, int dRow)[] directions =
            [
                (1, 0),   // Horizontal
                (0, 1),   // Vertical
                (1, 1),   // Diagonale descendante
                (1, -1)   // Diagonale montante
            ];

            foreach (var (dCol, dRow) in directions)
            {
                var sequence = new List<(int Col, int Row)>();

                int col = pointJoue.Col - dCol;
                int row = pointJoue.Row - dRow;
                while (PointsPoses.TryGetValue((col, row), out var jNeg) && ReferenceEquals(jNeg, joueur))
                {
                    sequence.Insert(0, (col, row));
                    col -= dCol;
                    row -= dRow;
                }

                int indexPointJoue = sequence.Count;
                sequence.Add(pointJoue);

                col = pointJoue.Col + dCol;
                row = pointJoue.Row + dRow;
                while (PointsPoses.TryGetValue((col, row), out var jPos) && ReferenceEquals(jPos, joueur))
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

                ligne = new LigneAlignement(debut, fin, joueur.Couleur);
                return true;
            }

            return false;
        }

        public bool AjouterLigneAlignementSiNouvelle(LigneAlignement ligne)
        {
            var normalisee = ligne.Normaliser();
            bool existe = LignesAlignements.Any(l =>
                l.Debut == normalisee.Debut &&
                l.Fin == normalisee.Fin &&
                l.Couleur.ToArgb() == normalisee.Couleur.ToArgb());

            if (!existe)
            {
                LignesAlignements.Add(normalisee);
                return true;
            }

            return false;
        }

        public bool PointEstProtege((int Col, int Row) point, Joueur joueur)
        {
            int argb = joueur.Couleur.ToArgb();

            foreach (var ligne in LignesAlignements)
            {
                if (ligne.Couleur.ToArgb() != argb)
                {
                    continue;
                }

                if (ligne.ContientPoint(point))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetGeometrie(Size clientSize, int padding, out GeometrieGrille geometrie)
        {
            geometrie = default;

            int usableWidth = clientSize.Width - (padding * 2);
            int usableHeight = clientSize.Height - (padding * 2);
            int gridSize = Math.Max(100, Math.Min(usableWidth, usableHeight));

            int startX = (clientSize.Width - gridSize) / 2;
            int startY = (clientSize.Height - gridSize) / 2;

            if (NombreLignes < 2)
            {
                return false;
            }

            float step = gridSize / (float)(NombreLignes - 1);
            geometrie = new GeometrieGrille(startX, startY, gridSize, step);
            return true;
        }

        public bool TryGetIntersectionProche(Point clickPoint, GeometrieGrille geo, out (int Col, int Row) intersection)
        {
            intersection = default;

            int nearestCol = (int)Math.Round((clickPoint.X - geo.StartX) / geo.Step);
            int nearestRow = (int)Math.Round((clickPoint.Y - geo.StartY) / geo.Step);

            nearestCol = Math.Clamp(nearestCol, 0, NombreLignes - 1);
            nearestRow = Math.Clamp(nearestRow, 0, NombreLignes - 1);

            float intersectionX = geo.StartX + (nearestCol * geo.Step);
            float intersectionY = geo.StartY + (nearestRow * geo.Step);

            float dx = clickPoint.X - intersectionX;
            float dy = clickPoint.Y - intersectionY;
            float distance = (float)Math.Sqrt((dx * dx) + (dy * dy));

            float maxSnapDistance = geo.Step * 0.45f;
            if (distance > maxSnapDistance)
            {
                return false;
            }

            intersection = (nearestCol, nearestRow);
            return true;
        }
    }

    public readonly struct GeometrieGrille
    {
        public int StartX { get; }
        public int StartY { get; }
        public int GridSize { get; }
        public float Step { get; }

        public GeometrieGrille(int startX, int startY, int gridSize, float step)
        {
            StartX = startX;
            StartY = startY;
            GridSize = gridSize;
            Step = step;
        }
    }

    public readonly struct LigneAlignement
    {
        public (int Col, int Row) Debut { get; }
        public (int Col, int Row) Fin { get; }
        public Color Couleur { get; }

        public LigneAlignement((int Col, int Row) debut, (int Col, int Row) fin, Color couleur)
        {
            Debut = debut;
            Fin = fin;
            Couleur = couleur;
        }

        public LigneAlignement Normaliser()
        {
            if (Debut.Col < Fin.Col) return this;
            if (Debut.Col > Fin.Col) return new LigneAlignement(Fin, Debut, Couleur);
            if (Debut.Row <= Fin.Row) return this;
            return new LigneAlignement(Fin, Debut, Couleur);
        }

        public bool ContientPoint((int Col, int Row) point)
        {
            int minCol = Math.Min(Debut.Col, Fin.Col);
            int maxCol = Math.Max(Debut.Col, Fin.Col);
            int minRow = Math.Min(Debut.Row, Fin.Row);
            int maxRow = Math.Max(Debut.Row, Fin.Row);

            if (point.Col < minCol || point.Col > maxCol || point.Row < minRow || point.Row > maxRow)
            {
                return false;
            }

            int dColSegment = Fin.Col - Debut.Col;
            int dRowSegment = Fin.Row - Debut.Row;
            int dColPoint = point.Col - Debut.Col;
            int dRowPoint = point.Row - Debut.Row;

            return (dColSegment * dRowPoint) == (dRowSegment * dColPoint);
        }
    }
}
