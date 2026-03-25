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

                // Chercher une fenêtre de 5 points avec au maximum 1 point par ligne existante
                for (int startIndex = 0; startIndex <= sequence.Count - 5; startIndex++)
                {
                    // Vérifier que le point joué est inclus dans cette fenêtre
                    if (indexPointJoue < startIndex || indexPointJoue > startIndex + 4)
                    {
                        continue;
                    }

                    // Extraire les 5 points de cette fenêtre
                    var pointsFenetre = new List<(int Col, int Row)>();
                    for (int i = startIndex; i < startIndex + 5; i++)
                    {
                        pointsFenetre.Add(sequence[i]);
                    }

                    // Vérifier qu'aucune ligne existante du même joueur ne contribue plus d'1 point
                    bool valide = true;
                    var ligneCandidate = new LigneAlignement(pointsFenetre[0], pointsFenetre[4], joueur.Couleur);

                    foreach (var ligneExistante in LignesAlignements)
                    {
                        bool memeCouleur = ligneExistante.Couleur.ToArgb() == joueur.Couleur.ToArgb();

                        if (memeCouleur)
                        {
                            // Règle 1: maximum 1 point par ligne existante du même joueur
                            int pointsDeMemeLigne = 0;
                            foreach (var pt in pointsFenetre)
                            {
                                if (ligneExistante.ContientPoint(pt))
                                {
                                    pointsDeMemeLigne++;
                                }
                            }

                            if (pointsDeMemeLigne > 1)
                            {
                                valide = false;
                                break;
                            }
                        }
                        else
                        {
                            // Règle 2: pas de chevauchement avec les lignes adverses
                            if (ligneCandidate.ChevaucheLigne(ligneExistante))
                            {
                                valide = false;
                                break;
                            }
                        }
                    }

                    if (valide)
                    {
                        var debut = sequence[startIndex];
                        var fin = sequence[startIndex + 4];
                        ligne = new LigneAlignement(debut, fin, joueur.Couleur);
                        return true;
                    }
                }
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

        public bool PointEstDejaTrace((int Col, int Row) point, Color couleur)
        {
            int argb = couleur.ToArgb();

            foreach (var ligne in LignesAlignements)
            {
                if (ligne.Couleur.ToArgb() == argb && ligne.ContientPoint(point))
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

        public bool ChevaucheLigne(LigneAlignement autre)
        {
            // Deux segments se chevauchent s'ils s'intersectent (pas seulement aux extrémités)
            // Utilise l'algorithme de détection d'intersection de segments

            int x1 = Debut.Col, y1 = Debut.Row;
            int x2 = Fin.Col, y2 = Fin.Row;
            int x3 = autre.Debut.Col, y3 = autre.Debut.Row;
            int x4 = autre.Fin.Col, y4 = autre.Fin.Row;

            // Calcul des produits vectoriels
            int d1 = Direction(x3, y3, x4, y4, x1, y1);
            int d2 = Direction(x3, y3, x4, y4, x2, y2);
            int d3 = Direction(x1, y1, x2, y2, x3, y3);
            int d4 = Direction(x1, y1, x2, y2, x4, y4);

            // Intersection générale (les segments se croisent)
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }

            // Cas colinéaires - vérifier le chevauchement
            if (d1 == 0 && d2 == 0 && d3 == 0 && d4 == 0)
            {
                // Les segments sont colinéaires, vérifier s'ils se chevauchent
                return SegmentsColineairesChevauchent(x1, y1, x2, y2, x3, y3, x4, y4);
            }

            return false;
        }

        private static int Direction(int ax, int ay, int bx, int by, int cx, int cy)
        {
            return (cx - ax) * (by - ay) - (cy - ay) * (bx - ax);
        }

        private static bool SegmentsColineairesChevauchent(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {
            // Projeter sur l'axe avec la plus grande variation
            int minA, maxA, minB, maxB;

            if (Math.Abs(x2 - x1) >= Math.Abs(y2 - y1))
            {
                // Projeter sur X
                minA = Math.Min(x1, x2);
                maxA = Math.Max(x1, x2);
                minB = Math.Min(x3, x4);
                maxB = Math.Max(x3, x4);
            }
            else
            {
                // Projeter sur Y
                minA = Math.Min(y1, y2);
                maxA = Math.Max(y1, y2);
                minB = Math.Min(y3, y4);
                maxB = Math.Max(y3, y4);
            }

            // Chevauchement si les intervalles se superposent (pas juste aux extrémités)
            int overlapStart = Math.Max(minA, minB);
            int overlapEnd = Math.Min(maxA, maxB);

            return overlapEnd > overlapStart; // Chevauchement strict (> au lieu de >=)
        }
    }
}
