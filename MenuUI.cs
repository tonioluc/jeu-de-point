namespace jeu_de_point
{
    public enum EtatEcran
    {
        MenuPrincipal,
        ConfigurationNouvellePartie,
        ChargementPartie,
        Partie
    }

    public class MenuUI
    {
        private readonly Form parent;
        private readonly Font baseFont;

        public Panel PanelMenu { get; private set; } = null!;
        public Panel PanelConfiguration { get; private set; } = null!;
        public Panel PanelChargement { get; private set; } = null!;
        public Panel CarteMenu { get; private set; } = null!;
        public Panel CarteConfiguration { get; private set; } = null!;
        public Panel CarteChargement { get; private set; } = null!;
        public FlowLayoutPanel PanelActionsPartie { get; private set; } = null!;

        public Button BoutonNouvellePartie { get; private set; } = null!;
        public Button BoutonChargerPartie { get; private set; } = null!;
        public Button BoutonRetourMenuConfiguration { get; private set; } = null!;
        public Button BoutonRetourMenuChargement { get; private set; } = null!;
        public NumericUpDown InputGridLines { get; private set; } = null!;
        public Button BoutonDemarrerPartie { get; private set; } = null!;
        public Button BoutonMenuPrincipalPartie { get; private set; } = null!;
        public Button BoutonNouvellePartiePartie { get; private set; } = null!;
        public ListBox ListeParties { get; private set; } = null!;
        public Button BoutonOuvrirPartie { get; private set; } = null!;
        public Button BoutonSupprimerPartie { get; private set; } = null!;

        public event Action? OnNouvellePartieClick;
        public event Action? OnChargerPartieClick;
        public event Action? OnMenuPrincipalClick;
        public event Action<int>? OnDemarrerPartieClick;
        public event Action? OnOuvrirPartieClick;
        public event Action? OnSupprimerPartieClick;

        public MenuUI(Form parent, Font baseFont)
        {
            this.parent = parent;
            this.baseFont = baseFont;
        }

        public void Initialiser()
        {
            CreerPanelMenu();
            CreerPanelConfiguration();
            CreerPanelChargement();
            CreerPanelActionsPartie();

            parent.Controls.Add(PanelMenu);
            parent.Controls.Add(PanelConfiguration);
            parent.Controls.Add(PanelChargement);
            parent.Controls.Add(PanelActionsPartie);

            parent.Resize += (_, _) =>
            {
                RecentrerLayout();
                PositionnerBoutonsHautDroite();
            };
        }

        private void CreerPanelMenu()
        {
            PanelMenu = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 246, 255)
            };

            var titreMenu = new Label
            {
                AutoSize = true,
                Text = "Jeu de point",
                Font = new Font(baseFont.FontFamily, 22f, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 63, 110),
                Margin = new Padding(0, 0, 0, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };

            BoutonNouvellePartie = CreerBouton("Nouvelle partie", 12f, Color.FromArgb(38, 112, 233));
            BoutonNouvellePartie.Margin = new Padding(0, 0, 0, 12);
            BoutonNouvellePartie.Click += (_, _) => OnNouvellePartieClick?.Invoke();

            BoutonChargerPartie = CreerBouton("Charger une partie", 12f, Color.FromArgb(92, 138, 214));
            BoutonChargerPartie.Click += (_, _) => OnChargerPartieClick?.Invoke();

            int largeurBoutons = Math.Max(
                TextRenderer.MeasureText(BoutonNouvellePartie.Text, BoutonNouvellePartie.Font).Width,
                TextRenderer.MeasureText(BoutonChargerPartie.Text, BoutonChargerPartie.Font).Width) + 90;

            var tailleBouton = new Size(largeurBoutons, 54);
            BoutonNouvellePartie.Size = tailleBouton;
            BoutonChargerPartie.Size = tailleBouton;

            CarteMenu = CreerCarte(34);
            var layout = CreerLayout(3);
            layout.Controls.Add(titreMenu, 0, 0);
            layout.Controls.Add(BoutonNouvellePartie, 0, 1);
            layout.Controls.Add(BoutonChargerPartie, 0, 2);
            CarteMenu.Controls.Add(layout);
            PanelMenu.Controls.Add(CarteMenu);
        }

        private void CreerPanelConfiguration()
        {
            PanelConfiguration = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 250, 255),
                Visible = false
            };

            BoutonRetourMenuConfiguration = CreerBouton("Menu principal", 10.5f, Color.FromArgb(88, 105, 128));
            BoutonRetourMenuConfiguration.Size = new Size(150, 38);
            BoutonRetourMenuConfiguration.Click += (_, _) => OnMenuPrincipalClick?.Invoke();

            var titre = new Label
            {
                AutoSize = true,
                Text = "Nouvelle partie",
                Font = new Font(baseFont.FontFamily, 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 74, 122),
                Margin = new Padding(0, 0, 0, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var labelGridLines = new Label
            {
                AutoSize = true,
                Text = "Nombre de lignes de la grille :",
                Font = new Font(baseFont.FontFamily, 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 71, 95),
                Margin = new Padding(0, 0, 0, 10)
            };

            InputGridLines = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 30,
                Value = 10,
                Width = 170,
                Font = new Font(baseFont.FontFamily, 12f, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 16)
            };

            BoutonDemarrerPartie = CreerBouton("Valider", 12f, Color.FromArgb(39, 166, 117));
            BoutonDemarrerPartie.Size = new Size(170, 48);
            BoutonDemarrerPartie.Click += (_, _) => OnDemarrerPartieClick?.Invoke((int)InputGridLines.Value);

            CarteConfiguration = CreerCarte(34);
            var layout = CreerLayout(4);
            layout.Controls.Add(titre, 0, 0);
            layout.Controls.Add(labelGridLines, 0, 1);
            layout.Controls.Add(InputGridLines, 0, 2);
            layout.Controls.Add(BoutonDemarrerPartie, 0, 3);
            CarteConfiguration.Controls.Add(layout);

            PanelConfiguration.Controls.Add(BoutonRetourMenuConfiguration);
            PanelConfiguration.Controls.Add(CarteConfiguration);
        }

        private void CreerPanelChargement()
        {
            PanelChargement = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 250, 255),
                Visible = false
            };

            BoutonRetourMenuChargement = CreerBouton("Menu principal", 10.5f, Color.FromArgb(88, 105, 128));
            BoutonRetourMenuChargement.Size = new Size(150, 38);
            BoutonRetourMenuChargement.Click += (_, _) => OnMenuPrincipalClick?.Invoke();

            var titre = new Label
            {
                AutoSize = true,
                Text = "Charger une partie",
                Font = new Font(baseFont.FontFamily, 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 74, 122),
                Margin = new Padding(0, 0, 0, 14)
            };

            ListeParties = new ListBox
            {
                Width = 560,
                Height = 260,
                Font = new Font(baseFont.FontFamily, 10f, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 12)
            };
            ListeParties.DoubleClick += (_, _) => OnOuvrirPartieClick?.Invoke();

            BoutonOuvrirPartie = CreerBouton("Ouvrir la partie selectionnee", 11f, Color.FromArgb(39, 166, 117));
            BoutonOuvrirPartie.Size = new Size(280, 44);
            BoutonOuvrirPartie.Margin = new Padding(0, 0, 0, 8);
            BoutonOuvrirPartie.Click += (_, _) => OnOuvrirPartieClick?.Invoke();

            BoutonSupprimerPartie = CreerBouton("Supprimer la partie", 10f, Color.FromArgb(200, 60, 60));
            BoutonSupprimerPartie.Size = new Size(200, 38);
            BoutonSupprimerPartie.Click += (_, _) => OnSupprimerPartieClick?.Invoke();

            CarteChargement = CreerCarte(30);
            var layout = CreerLayout(4);
            layout.Controls.Add(titre, 0, 0);
            layout.Controls.Add(ListeParties, 0, 1);
            layout.Controls.Add(BoutonOuvrirPartie, 0, 2);
            layout.Controls.Add(BoutonSupprimerPartie, 0, 3);
            CarteChargement.Controls.Add(layout);

            PanelChargement.Controls.Add(BoutonRetourMenuChargement);
            PanelChargement.Controls.Add(CarteChargement);
        }

        private void CreerPanelActionsPartie()
        {
            PanelActionsPartie = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Visible = false
            };

            BoutonMenuPrincipalPartie = CreerBouton("Menu principale", 10f, Color.FromArgb(88, 105, 128));
            BoutonMenuPrincipalPartie.Size = new Size(170, 36);
            BoutonMenuPrincipalPartie.Margin = new Padding(0, 0, 10, 0);
            BoutonMenuPrincipalPartie.Click += (_, _) => OnMenuPrincipalClick?.Invoke();

            BoutonNouvellePartiePartie = CreerBouton("Nouvelle partie", 10f, Color.FromArgb(38, 112, 233));
            BoutonNouvellePartiePartie.Size = new Size(170, 36);
            BoutonNouvellePartiePartie.Click += (_, _) => OnNouvellePartieClick?.Invoke();

            PanelActionsPartie.Controls.Add(BoutonMenuPrincipalPartie);
            PanelActionsPartie.Controls.Add(BoutonNouvellePartiePartie);
        }

        private Button CreerBouton(string texte, float fontSize, Color couleurFond)
        {
            var bouton = new Button
            {
                Text = texte,
                Font = new Font(baseFont.FontFamily, fontSize, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = couleurFond,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            bouton.FlatAppearance.BorderSize = 0;
            return bouton;
        }

        private Panel CreerCarte(int padding)
        {
            return new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(padding),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private TableLayoutPanel CreerLayout(int rowCount)
        {
            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 1,
                RowCount = rowCount,
                BackColor = Color.Transparent
            };
            for (int i = 0; i < rowCount; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            return layout;
        }

        public void AfficherEcran(EtatEcran ecran)
        {
            PanelMenu.Visible = ecran == EtatEcran.MenuPrincipal;
            PanelConfiguration.Visible = ecran == EtatEcran.ConfigurationNouvellePartie;
            PanelChargement.Visible = ecran == EtatEcran.ChargementPartie;
            PanelActionsPartie.Visible = ecran == EtatEcran.Partie;

            switch (ecran)
            {
                case EtatEcran.MenuPrincipal:
                    PanelMenu.BringToFront();
                    break;
                case EtatEcran.ConfigurationNouvellePartie:
                    PanelConfiguration.BringToFront();
                    break;
                case EtatEcran.ChargementPartie:
                    PanelChargement.BringToFront();
                    break;
                case EtatEcran.Partie:
                    PanelActionsPartie.BringToFront();
                    break;
            }

            RecentrerLayout();
            PositionnerBoutonsHautDroite();
        }

        public void RecentrerLayout()
        {
            CentrerDansPannel(CarteMenu, PanelMenu);
            CentrerDansPannel(CarteConfiguration, PanelConfiguration);
            CentrerDansPannel(CarteChargement, PanelChargement);
        }

        private void CentrerDansPannel(Panel carte, Panel panel)
        {
            if (carte != null && panel != null)
            {
                carte.Left = (panel.ClientSize.Width - carte.Width) / 2;
                carte.Top = (panel.ClientSize.Height - carte.Height) / 2;
            }
        }

        public void PositionnerBoutonsHautDroite()
        {
            PositionnerBoutonHautDroite(BoutonRetourMenuConfiguration, PanelConfiguration);
            PositionnerBoutonHautDroite(BoutonRetourMenuChargement, PanelChargement);

            if (PanelActionsPartie != null)
            {
                PanelActionsPartie.Left = parent.ClientSize.Width - PanelActionsPartie.Width - 18;
                PanelActionsPartie.Top = 18;
            }
        }

        private void PositionnerBoutonHautDroite(Button bouton, Panel panel)
        {
            if (bouton != null && panel != null)
            {
                bouton.Left = panel.ClientSize.Width - bouton.Width - 18;
                bouton.Top = 18;
            }
        }

        public void MettreAJourListeParties(List<PartieResume> parties)
        {
            ListeParties.DataSource = null;
            ListeParties.DataSource = parties;
        }

        public PartieResume? ObtenirPartieSelectionnee()
        {
            return ListeParties.SelectedItem as PartieResume;
        }
    }
}
