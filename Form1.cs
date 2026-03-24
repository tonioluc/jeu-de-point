using System.Drawing.Drawing2D;

namespace jeu_de_point
{
    public partial class Form1 : Form
    {
        private enum EtatEcran
        {
            MenuPrincipal,
            ConfigurationNouvellePartie,
            Partie
        }

        private int gridLines = 10;
        private const int PaddingAroundGrid = 60;
        private const int CanonWidth = 20;
        private const int CanonGap = 20;
        private const int PuissanceMin = 1;
        private const int PuissanceMax = 9;

        private readonly Dictionary<(int Col, int Row), Joueur> pointsPoses = new();
        private Joueur[] joueurs = Array.Empty<Joueur>();
        private int indexJoueurCourant;
        private int[] positionsCanonY = Array.Empty<int>();

        private readonly System.Windows.Forms.Timer timerAnimationTir = new() { Interval = 25 };
        private bool animationTirActive;
        private DateTime animationTirDebut;
        private int animationTireurIndex;
        private int animationCibleCol;
        private int animationCibleRow;

        private EtatEcran etatEcran = EtatEcran.MenuPrincipal;

        private Panel panelMenu = null!;
        private Panel panelConfiguration = null!;
        private Panel carteMenu = null!;
        private Panel carteConfiguration = null!;
        private FlowLayoutPanel panelActionsPartie = null!;

        private Button boutonNouvellePartie = null!;
        private Button boutonChargerPartie = null!;
        private Button boutonRetourMenuConfiguration = null!;
        private NumericUpDown inputGridLines = null!;
        private Button boutonDemarrerPartie = null!;
        private Button boutonMenuPrincipalPartie = null!;
        private Button boutonNouvellePartiePartie = null!;

        // Conserver toutes les lignes tracées (ne plus effacer)
        private readonly List<((int Col, int Row) Debut, (int Col, int Row) Fin, Color Couleur)> lignesAlignements = new();

        // Méthode isolée pour brancher les événements souris
        private void InitialiserEcouteSouris()
        {
            MouseClick += Form1_MouseClick;
        }

        // Méthode isolée pour brancher les événements clavier
        private void InitialiserEcouteClavier()
        {
            KeyPreview = true;
            KeyDown += Form1_KeyDown;
        }

        private void InitialiserAnimationTir()
        {
            timerAnimationTir.Tick += (_, _) =>
            {
                if (!animationTirActive)
                {
                    timerAnimationTir.Stop();
                    return;
                }

                if ((DateTime.UtcNow - animationTirDebut).TotalMilliseconds >= 220)
                {
                    animationTirActive = false;
                    timerAnimationTir.Stop();
                }

                Invalidate();
            };
        }

        private void ConfigurerStyleBouton(Button bouton, Color fond, Color texte)
        {
            bouton.FlatStyle = FlatStyle.Flat;
            bouton.FlatAppearance.BorderSize = 0;
            bouton.BackColor = fond;
            bouton.ForeColor = texte;
            bouton.Cursor = Cursors.Hand;
            bouton.UseVisualStyleBackColor = false;
        }

        private void InitialiserEcranAccueil()
        {
            panelMenu = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 246, 255)
            };

            panelConfiguration = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 250, 255),
                Visible = false
            };

            var titreMenu = new Label
            {
                AutoSize = true,
                Text = "Jeu de point",
                Font = new Font(Font.FontFamily, 22f, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 63, 110),
                Margin = new Padding(0, 0, 0, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };

            boutonNouvellePartie = new Button
            {
                Text = "Nouvelle partie",
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 12)
            };
            ConfigurerStyleBouton(boutonNouvellePartie, Color.FromArgb(38, 112, 233), Color.White);
            boutonNouvellePartie.Click += (_, _) => AfficherConfigurationNouvellePartie();

            boutonChargerPartie = new Button
            {
                Text = "Charger une partie",
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                Margin = new Padding(0)
            };
            ConfigurerStyleBouton(boutonChargerPartie, Color.FromArgb(92, 138, 214), Color.White);
            boutonChargerPartie.Click += (_, _) => MessageBox.Show("Le scénario 'Charger une partie' sera ajouté ensuite.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            int largeurBoutonsMenu = Math.Max(
                TextRenderer.MeasureText(boutonNouvellePartie.Text, boutonNouvellePartie.Font).Width,
                TextRenderer.MeasureText(boutonChargerPartie.Text, boutonChargerPartie.Font).Width) + 90;

            var tailleBoutonMenu = new Size(largeurBoutonsMenu, 54);
            boutonNouvellePartie.Size = tailleBoutonMenu;
            boutonChargerPartie.Size = tailleBoutonMenu;

            carteMenu = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(34),
                BorderStyle = BorderStyle.FixedSingle
            };

            var layoutMenu = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            layoutMenu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutMenu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutMenu.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layoutMenu.Controls.Add(titreMenu, 0, 0);
            layoutMenu.Controls.Add(boutonNouvellePartie, 0, 1);
            layoutMenu.Controls.Add(boutonChargerPartie, 0, 2);
            carteMenu.Controls.Add(layoutMenu);
            panelMenu.Controls.Add(carteMenu);

            boutonRetourMenuConfiguration = new Button
            {
                Text = "Menu principal",
                Font = new Font(Font.FontFamily, 10.5f, FontStyle.Bold),
                Size = new Size(150, 38)
            };
            ConfigurerStyleBouton(boutonRetourMenuConfiguration, Color.FromArgb(88, 105, 128), Color.White);
            boutonRetourMenuConfiguration.Click += (_, _) => AfficherMenuPrincipal();

            var titreConfiguration = new Label
            {
                AutoSize = true,
                Text = "Nouvelle partie",
                Font = new Font(Font.FontFamily, 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 74, 122),
                Margin = new Padding(0, 0, 0, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var labelGridLines = new Label
            {
                AutoSize = true,
                Text = "Nombre de lignes de la grille :",
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 71, 95),
                Margin = new Padding(0, 0, 0, 10)
            };

            inputGridLines = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 30,
                Value = 10,
                Width = 170,
                Font = new Font(Font.FontFamily, 12f, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 16)
            };

            boutonDemarrerPartie = new Button
            {
                Text = "Valider",
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                Size = new Size(170, 48),
                Margin = new Padding(0)
            };
            ConfigurerStyleBouton(boutonDemarrerPartie, Color.FromArgb(39, 166, 117), Color.White);
            boutonDemarrerPartie.Click += (_, _) => DemarrerNouvellePartie((int)inputGridLines.Value);

            carteConfiguration = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(34),
                BorderStyle = BorderStyle.FixedSingle
            };

            var layoutConfiguration = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            layoutConfiguration.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutConfiguration.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutConfiguration.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutConfiguration.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layoutConfiguration.Controls.Add(titreConfiguration, 0, 0);
            layoutConfiguration.Controls.Add(labelGridLines, 0, 1);
            layoutConfiguration.Controls.Add(inputGridLines, 0, 2);
            layoutConfiguration.Controls.Add(boutonDemarrerPartie, 0, 3);
            carteConfiguration.Controls.Add(layoutConfiguration);

            panelConfiguration.Controls.Add(boutonRetourMenuConfiguration);
            panelConfiguration.Controls.Add(carteConfiguration);

            panelActionsPartie = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Visible = false
            };

            boutonMenuPrincipalPartie = new Button
            {
                Text = "Menu principale",
                Font = new Font(Font.FontFamily, 10f, FontStyle.Bold),
                Size = new Size(170, 36),
                Margin = new Padding(0, 0, 10, 0)
            };
            ConfigurerStyleBouton(boutonMenuPrincipalPartie, Color.FromArgb(88, 105, 128), Color.White);
            boutonMenuPrincipalPartie.Click += (_, _) => AfficherMenuPrincipal();

            boutonNouvellePartiePartie = new Button
            {
                Text = "Nouvelle partie",
                Font = new Font(Font.FontFamily, 10f, FontStyle.Bold),
                Size = new Size(170, 36),
                Margin = new Padding(0)
            };
            ConfigurerStyleBouton(boutonNouvellePartiePartie, Color.FromArgb(38, 112, 233), Color.White);
            boutonNouvellePartiePartie.Click += (_, _) => AfficherConfigurationNouvellePartie();

            panelActionsPartie.Controls.Add(boutonMenuPrincipalPartie);
            panelActionsPartie.Controls.Add(boutonNouvellePartiePartie);

            Controls.Add(panelMenu);
            Controls.Add(panelConfiguration);
            Controls.Add(panelActionsPartie);

            Resize += (_, _) =>
            {
                RecentrerLayout();
                PositionnerBoutonsHautDroite();
            };

            RecentrerLayout();
            PositionnerBoutonsHautDroite();
        }

        private void PositionnerBoutonsHautDroite()
        {
            if (panelConfiguration != null && boutonRetourMenuConfiguration != null)
            {
                boutonRetourMenuConfiguration.Left = panelConfiguration.ClientSize.Width - boutonRetourMenuConfiguration.Width - 18;
                boutonRetourMenuConfiguration.Top = 18;
            }

            if (panelActionsPartie != null)
            {
                panelActionsPartie.Left = ClientSize.Width - panelActionsPartie.Width - 18;
                panelActionsPartie.Top = 18;
            }
        }

        private void RecentrerLayout()
        {
            if (carteMenu != null)
            {
                carteMenu.Left = (panelMenu.ClientSize.Width - carteMenu.Width) / 2;
                carteMenu.Top = (panelMenu.ClientSize.Height - carteMenu.Height) / 2;
            }

            if (carteConfiguration != null)
            {
                carteConfiguration.Left = (panelConfiguration.ClientSize.Width - carteConfiguration.Width) / 2;
                carteConfiguration.Top = (panelConfiguration.ClientSize.Height - carteConfiguration.Height) / 2;
            }
        }

        private void AfficherMenuPrincipal()
        {
            etatEcran = EtatEcran.MenuPrincipal;
            panelMenu.Visible = true;
            panelConfiguration.Visible = false;
            panelActionsPartie.Visible = false;
            panelMenu.BringToFront();
            RecentrerLayout();
            Invalidate();
        }

        private void AfficherConfigurationNouvellePartie()
        {
            etatEcran = EtatEcran.ConfigurationNouvellePartie;
            panelMenu.Visible = false;
            panelConfiguration.Visible = true;
            panelActionsPartie.Visible = false;
            panelConfiguration.BringToFront();
            RecentrerLayout();
            PositionnerBoutonsHautDroite();
            Invalidate();
        }

        private void DemarrerNouvellePartie(int valeurGridLines)
        {
            gridLines = Math.Max(2, valeurGridLines);
            pointsPoses.Clear();
            lignesAlignements.Clear();

            InitialiserJoueursEtTour();
            positionsCanonY = Enumerable.Repeat(0, joueurs.Length).ToArray();

            etatEcran = EtatEcran.Partie;
            panelMenu.Visible = false;
            panelConfiguration.Visible = false;
            panelActionsPartie.Visible = true;
            panelActionsPartie.BringToFront();
            PositionnerBoutonsHautDroite();
            Invalidate();
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
            if (etatEcran != EtatEcran.Partie)
            {
                return;
            }

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

            PasserAuJoueurSuivant();
            Invalidate();
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (etatEcran != EtatEcran.Partie || joueurs.Length == 0 || positionsCanonY.Length != joueurs.Length)
            {
                return;
            }

            if (e.Control)
            {
                int puissance = ExtrairePuissanceDepuisTouche(e.KeyCode);
                if (puissance >= PuissanceMin)
                {
                    ExecuterTir(puissance);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }

                return;
            }

            int index = indexJoueurCourant;
            int anciennePosition = positionsCanonY[index];

            if (e.KeyCode == Keys.Up)
            {
                positionsCanonY[index] = Math.Max(0, positionsCanonY[index] - 1);
            }
            else if (e.KeyCode == Keys.Down)
            {
                positionsCanonY[index] = Math.Min(gridLines - 1, positionsCanonY[index] + 1);
            }
            else
            {
                return;
            }

            if (positionsCanonY[index] != anciennePosition)
            {
                Invalidate();
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private int ExtrairePuissanceDepuisTouche(Keys keyCode)
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

        private void ExecuterTir(int puissance)
        {
            int tireurIndex = indexJoueurCourant;
            int cibleRow = positionsCanonY[tireurIndex];
            int cibleCol = CalculerColonneImpactDepuisPuissance(tireurIndex, puissance);
            var cible = (Col: cibleCol, Row: cibleRow);

            DemarrerAnimationTir(tireurIndex, cibleCol, cibleRow);

            if (pointsPoses.TryGetValue(cible, out var joueurTouche))
            {
                var tireur = joueurs[tireurIndex];
                bool pointAdverse = !ReferenceEquals(joueurTouche, tireur);

                if (pointAdverse && !PointEstProtegeParAlignement(cible, joueurTouche))
                {
                    pointsPoses.Remove(cible);
                }
            }

            PasserAuJoueurSuivant();
            Invalidate();
        }

        private int CalculerColonneImpactDepuisPuissance(int tireurIndex, int puissance)
        {
            int maxCol = gridLines - 1;
            int distance = (puissance * maxCol) / PuissanceMax;

            if (tireurIndex == 0)
            {
                return Math.Clamp(distance, 0, maxCol);
            }

            return Math.Clamp(maxCol - distance, 0, maxCol);
        }

        private void DemarrerAnimationTir(int tireurIndex, int cibleCol, int cibleRow)
        {
            animationTirActive = true;
            animationTirDebut = DateTime.UtcNow;
            animationTireurIndex = tireurIndex;
            animationCibleCol = cibleCol;
            animationCibleRow = cibleRow;
            timerAnimationTir.Start();
        }

        private bool PointEstProtegeParAlignement((int Col, int Row) point, Joueur joueur)
        {
            int argb = joueur.Couleur.ToArgb();

            foreach (var ligne in lignesAlignements)
            {
                if (ligne.Couleur.ToArgb() != argb)
                {
                    continue;
                }

                if (PointAppartientSegment(point, ligne.Debut, ligne.Fin))
                {
                    return true;
                }
            }

            return false;
        }

        private bool PointAppartientSegment((int Col, int Row) point, (int Col, int Row) debut, (int Col, int Row) fin)
        {
            int minCol = Math.Min(debut.Col, fin.Col);
            int maxCol = Math.Max(debut.Col, fin.Col);
            int minRow = Math.Min(debut.Row, fin.Row);
            int maxRow = Math.Max(debut.Row, fin.Row);

            if (point.Col < minCol || point.Col > maxCol || point.Row < minRow || point.Row > maxRow)
            {
                return false;
            }

            int dColSegment = fin.Col - debut.Col;
            int dRowSegment = fin.Row - debut.Row;
            int dColPoint = point.Col - debut.Col;
            int dRowPoint = point.Row - debut.Row;

            return (dColSegment * dRowPoint) == (dRowSegment * dColPoint);
        }

        // Méthode isolée pour dessiner l'information du tour
        private void DessinerTourCourant(Graphics g)
        {
            if (joueurs.Length == 0)
            {
                return;
            }

            string texte = $"Tour: {JoueurCourant.Nom}  |  Flèches Haut/Bas: Y canon  |  CTRL+1..9: Tir";
            using var brush = new SolidBrush(JoueurCourant.Couleur);
            g.DrawString(texte, Font, brush, 20, 20);
        }

        private void DessinerAnimationTir(Graphics g, int startX, int startY, int gridSize, float step)
        {
            if (!animationTirActive || joueurs.Length == 0)
            {
                return;
            }

            float progression = (float)((DateTime.UtcNow - animationTirDebut).TotalMilliseconds / 220d);
            progression = Math.Clamp(progression, 0f, 1f);

            int tireurIndex = Math.Clamp(animationTireurIndex, 0, joueurs.Length - 1);
            int row = Math.Clamp(animationCibleRow, 0, gridLines - 1);
            int col = Math.Clamp(animationCibleCol, 0, gridLines - 1);

            float y = startY + (row * step);
            float xOrigine = tireurIndex == 0 ? startX - CanonGap : startX + gridSize + CanonGap;
            float xCible = startX + (col * step);
            float xProjectile = xOrigine + ((xCible - xOrigine) * progression);

            using var pen = new Pen(joueurs[tireurIndex].Couleur, 1.8f);
            pen.DashStyle = DashStyle.Dot;
            g.DrawLine(pen, xOrigine, y, xProjectile, y);

            using var brush = new SolidBrush(joueurs[tireurIndex].Couleur);
            g.FillEllipse(brush, xProjectile - 5, y - 5, 10, 10);
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

        private void DessinerCanonEtIndicateur(Graphics g, Rectangle rectCanon, Color couleurJoueur, int yGrille, float step, bool estGauche)
        {
            using var brushCanon = new SolidBrush(Color.FromArgb(70, couleurJoueur));
            using var penCanon = new Pen(couleurJoueur, 2f);
            g.FillRectangle(brushCanon, rectCanon);
            g.DrawRectangle(penCanon, rectCanon);

            float y = rectCanon.Top + (yGrille * step);
            y = Math.Clamp(y, rectCanon.Top, rectCanon.Bottom);

            using var penIndicateur = new Pen(couleurJoueur, 3f);
            g.DrawLine(penIndicateur, rectCanon.Left, y, rectCanon.Right, y);

            float xPoint = estGauche ? rectCanon.Left - 8 : rectCanon.Right + 8;
            using var brushPoint = new SolidBrush(couleurJoueur);
            g.FillEllipse(brushPoint, xPoint - 5, y - 5, 10, 10);

            string texte = $"Y: {yGrille}";
            using var fontTexte = new Font(Font.FontFamily, 9f, FontStyle.Bold);
            SizeF tailleTexte = g.MeasureString(texte, fontTexte);
            float xTexte = estGauche ? rectCanon.Left - tailleTexte.Width - 14 : rectCanon.Right + 14;
            float yTexte = y - (tailleTexte.Height / 2f);
            g.DrawString(texte, fontTexte, brushPoint, xTexte, yTexte);
        }

        // Méthode isolée pour dessiner les canons et leurs indicateurs Y
        private void DessinerCanons(Graphics g, int startX, int startY, int gridSize, float step)
        {
            if (joueurs.Length < 2 || positionsCanonY.Length != joueurs.Length)
            {
                return;
            }

            var rectCanonGauche = new Rectangle(startX - CanonGap - CanonWidth, startY, CanonWidth, gridSize);
            var rectCanonDroit = new Rectangle(startX + gridSize + CanonGap, startY, CanonWidth, gridSize);

            DessinerCanonEtIndicateur(g, rectCanonGauche, joueurs[0].Couleur, positionsCanonY[0], step, estGauche: true);
            DessinerCanonEtIndicateur(g, rectCanonDroit, joueurs[1].Couleur, positionsCanonY[1], step, estGauche: false);
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

            for (int i = 0; i < gridLines; i++)
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

            // Dessiner les canons des joueurs et l'indicateur de coordonnée Y
            DessinerCanons(g, startX, startY, gridSize, step);
            DessinerAnimationTir(g, startX, startY, gridSize, step);
        }

        // Méthode isolée: transforme un clic en intersection la plus proche
        private bool TryGetNearestIntersection(Point clickPoint, out (int Col, int Row) intersection)
        {
            intersection = default;

            if (!TryGetGridGeometry(out int startX, out int startY, out _, out float step))
            {
                return false;
            }

            int nearestCol = (int)Math.Round((clickPoint.X - startX) / step);
            int nearestRow = (int)Math.Round((clickPoint.Y - startY) / step);

            nearestCol = Math.Clamp(nearestCol, 0, gridLines - 1);
            nearestRow = Math.Clamp(nearestRow, 0, gridLines - 1);

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

            if (gridLines < 2)
            {
                step = 0;
                return false;
            }

            step = gridSize / (float)(gridLines - 1);
            return true;
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeRedraw = true;

            InitialiserJoueursEtTour();
            positionsCanonY = Enumerable.Repeat(0, joueurs.Length).ToArray();
            InitialiserEcouteSouris();
            InitialiserEcouteClavier();
            InitialiserAnimationTir();
            InitialiserEcranAccueil();
            AfficherMenuPrincipal();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (etatEcran != EtatEcran.Partie)
            {
                return;
            }

            dessinerTerrain(e.Graphics);
            DessinerScores(e.Graphics);
            DessinerTourCourant(e.Graphics);
        }
    }
}
