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
        private ITheme Theme => ThemeManager.Theme;

        public Panel PanelMenu { get; private set; } = null!;
        public Panel PanelConfiguration { get; private set; } = null!;
        public Panel PanelChargement { get; private set; } = null!;
        public Panel CarteMenu { get; private set; } = null!;
        public Panel CarteConfiguration { get; private set; } = null!;
        public Panel CarteChargement { get; private set; } = null!;
        public FlowLayoutPanel PanelActionsPartie { get; private set; } = null!;
        public FlowLayoutPanel PanelBoutonsConfiguration { get; private set; } = null!;

        public Label LabelEtudiantMenu { get; private set; } = null!;
        public Label LabelEtudiantConfiguration { get; private set; } = null!;
        public Label LabelEtudiantChargement { get; private set; } = null!;
        public Label LabelEtudiantPartie { get; private set; } = null!;

        public Button BoutonNouvellePartie { get; private set; } = null!;
        public Button BoutonChargerPartie { get; private set; } = null!;
        public Button BoutonRetourMenuConfiguration { get; private set; } = null!;
        public Button BoutonChargerPartieConfiguration { get; private set; } = null!;
        public Button BoutonRetourMenuChargement { get; private set; } = null!;
        public NumericUpDown InputGridLines { get; private set; } = null!;
        public Button BoutonDemarrerPartie { get; private set; } = null!;
        public Button BoutonMenuPrincipalPartie { get; private set; } = null!;
        public Button BoutonNouvellePartiePartie { get; private set; } = null!;
        public Button BoutonChargerPartiePartie { get; private set; } = null!;
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
                BackColor = Theme.CouleurFondMenuPrincipal
            };

            LabelEtudiantMenu = CreerLabelEtudiant();
            PanelMenu.Controls.Add(LabelEtudiantMenu);

            var titreMenu = new Label
            {
                AutoSize = true,
                Text = "Jeu de point",
                Font = new Font(baseFont.FontFamily, Theme.TaillePoliceTitrePrincipal, FontStyle.Bold),
                ForeColor = Theme.CouleurTitrePrincipal,
                Margin = new Padding(0, 0, 0, Theme.MargeEntreBoutons + 6),
                TextAlign = ContentAlignment.MiddleCenter
            };

            BoutonNouvellePartie = CreerBouton("Nouvelle partie", Theme.TaillePoliceBoutonGrand, Theme.CouleurBoutonPrimaire);
            BoutonNouvellePartie.Margin = new Padding(0, 0, 0, Theme.MargeEntreBoutons);
            BoutonNouvellePartie.Click += (_, _) => OnNouvellePartieClick?.Invoke();

            BoutonChargerPartie = CreerBouton("Charger une partie", Theme.TaillePoliceBoutonGrand, Theme.CouleurBoutonSecondaire);
            BoutonChargerPartie.Click += (_, _) => OnChargerPartieClick?.Invoke();

            int largeurBoutons = Math.Max(
                TextRenderer.MeasureText(BoutonNouvellePartie.Text, BoutonNouvellePartie.Font).Width,
                TextRenderer.MeasureText(BoutonChargerPartie.Text, BoutonChargerPartie.Font).Width) + Theme.LargeurSupplementaireBoutonMenu;

            var hauteurBouton = Theme.TailleBoutonMenu.Height;
            var tailleBouton = new Size(largeurBoutons, hauteurBouton);
            BoutonNouvellePartie.Size = tailleBouton;
            BoutonChargerPartie.Size = tailleBouton;

            CarteMenu = CreerCarte(Theme.PaddingCarte);
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
                BackColor = Theme.CouleurFondConfiguration,
                Visible = false
            };

            LabelEtudiantConfiguration = CreerLabelEtudiant();
            PanelConfiguration.Controls.Add(LabelEtudiantConfiguration);

            // Panel pour les boutons en haut a droite
            PanelBoutonsConfiguration = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            BoutonChargerPartieConfiguration = CreerBouton("Charger", Theme.TaillePoliceBoutonPetit, Theme.CouleurBoutonSecondaire);
            BoutonChargerPartieConfiguration.Size = Theme.TailleBoutonChargerPartie;
            BoutonChargerPartieConfiguration.Margin = new Padding(0, 0, Theme.EspacementBoutonsPartie, 0);
            BoutonChargerPartieConfiguration.Click += (_, _) => OnChargerPartieClick?.Invoke();

            BoutonRetourMenuConfiguration = CreerBouton("Menu principal", Theme.TaillePoliceBoutonPetit, Theme.CouleurBoutonNeutre);
            BoutonRetourMenuConfiguration.Size = Theme.TailleBoutonRetour;
            BoutonRetourMenuConfiguration.Click += (_, _) => OnMenuPrincipalClick?.Invoke();

            PanelBoutonsConfiguration.Controls.Add(BoutonChargerPartieConfiguration);
            PanelBoutonsConfiguration.Controls.Add(BoutonRetourMenuConfiguration);

            var titre = new Label
            {
                AutoSize = true,
                Text = "Nouvelle partie",
                Font = new Font(baseFont.FontFamily, Theme.TaillePoliceTitreSecondaire, FontStyle.Bold),
                ForeColor = Theme.CouleurTitreSecondaire,
                Margin = new Padding(0, 0, 0, Theme.MargeEntreBoutons + 6),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var labelGridLines = new Label
            {
                AutoSize = true,
                Text = "Nombre de lignes de la grille :",
                Font = new Font(baseFont.FontFamily, Theme.TaillePoliceLabel, FontStyle.Bold),
                ForeColor = Theme.CouleurTexteLabel,
                Margin = new Padding(0, 0, 0, 10)
            };

            InputGridLines = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 30,
                Value = 9,
                Width = Theme.LargeurInputGridLines,
                Font = new Font(baseFont.FontFamily, Theme.TaillePoliceLabel, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, Theme.MargeEntreBoutons + 4)
            };

            BoutonDemarrerPartie = CreerBouton("Valider", Theme.TaillePoliceBoutonGrand, Theme.CouleurBoutonSucces);
            BoutonDemarrerPartie.Size = Theme.TailleBoutonValider;
            BoutonDemarrerPartie.Click += (_, _) => OnDemarrerPartieClick?.Invoke((int)InputGridLines.Value);

            CarteConfiguration = CreerCarte(Theme.PaddingCarte);
            var layout = CreerLayout(4);
            layout.Controls.Add(titre, 0, 0);
            layout.Controls.Add(labelGridLines, 0, 1);
            layout.Controls.Add(InputGridLines, 0, 2);
            layout.Controls.Add(BoutonDemarrerPartie, 0, 3);
            CarteConfiguration.Controls.Add(layout);

            PanelConfiguration.Controls.Add(PanelBoutonsConfiguration);
            PanelConfiguration.Controls.Add(CarteConfiguration);
        }

        private void CreerPanelChargement()
        {
            PanelChargement = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.CouleurFondChargement,
                Visible = false
            };

            LabelEtudiantChargement = CreerLabelEtudiant();
            PanelChargement.Controls.Add(LabelEtudiantChargement);

            BoutonRetourMenuChargement = CreerBouton("Menu principal", Theme.TaillePoliceBoutonPetit, Theme.CouleurBoutonNeutre);
            BoutonRetourMenuChargement.Size = Theme.TailleBoutonRetour;
            BoutonRetourMenuChargement.Click += (_, _) => OnMenuPrincipalClick?.Invoke();

            var titre = new Label
            {
                AutoSize = true,
                Text = "Charger une partie",
                Font = new Font(baseFont.FontFamily, Theme.TaillePoliceTitreSecondaire, FontStyle.Bold),
                ForeColor = Theme.CouleurTitreSecondaire,
                Margin = new Padding(0, 0, 0, Theme.MargeEntreBoutons + 2)
            };

            ListeParties = new ListBox
            {
                Width = Theme.TailleListeParties.Width,
                Height = Theme.TailleListeParties.Height,
                Font = new Font(baseFont.FontFamily, Theme.TaillePoliceListBox, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, Theme.MargeEntreBoutons)
            };
            ListeParties.DoubleClick += (_, _) => OnOuvrirPartieClick?.Invoke();

            BoutonOuvrirPartie = CreerBouton("Ouvrir la partie selectionnee", Theme.TaillePoliceBoutonMoyen, Theme.CouleurBoutonSucces);
            BoutonOuvrirPartie.Size = Theme.TailleBoutonOuvrir;
            BoutonOuvrirPartie.Margin = new Padding(0, 0, 0, 8);
            BoutonOuvrirPartie.Click += (_, _) => OnOuvrirPartieClick?.Invoke();

            BoutonSupprimerPartie = CreerBouton("Supprimer la partie", Theme.TaillePoliceBoutonPetit, Theme.CouleurBoutonDanger);
            BoutonSupprimerPartie.Size = Theme.TailleBoutonSupprimer;
            BoutonSupprimerPartie.Click += (_, _) => OnSupprimerPartieClick?.Invoke();

            CarteChargement = CreerCarte(Theme.PaddingCarteChargement);
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
            LabelEtudiantPartie = CreerLabelEtudiant();
            LabelEtudiantPartie.Visible = false;
            parent.Controls.Add(LabelEtudiantPartie);

            PanelActionsPartie = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = Theme.DirectionBoutonsPartie,
                WrapContents = false,
                BackColor = Color.Transparent,
                Visible = false
            };

            BoutonChargerPartiePartie = CreerBouton("Charger", Theme.TaillePoliceBoutonPetit, Theme.CouleurBoutonSecondaire);
            BoutonChargerPartiePartie.Size = Theme.TailleBoutonChargerPartie;
            BoutonChargerPartiePartie.Click += (_, _) => OnChargerPartieClick?.Invoke();

            BoutonMenuPrincipalPartie = CreerBouton("Menu principal", Theme.TaillePoliceBoutonPetit, Theme.CouleurBoutonNeutre);
            BoutonMenuPrincipalPartie.Size = Theme.TailleBoutonPartie;
            BoutonMenuPrincipalPartie.Click += (_, _) => OnMenuPrincipalClick?.Invoke();

            BoutonNouvellePartiePartie = CreerBouton("Nouvelle partie", Theme.TaillePoliceBoutonPetit, Theme.CouleurBoutonPrimaire);
            BoutonNouvellePartiePartie.Size = Theme.TailleBoutonPartie;
            BoutonNouvellePartiePartie.Click += (_, _) => OnNouvellePartieClick?.Invoke();

            // Ajuster les marges selon la direction (horizontale ou verticale)
            bool estVertical = Theme.DirectionBoutonsPartie == FlowDirection.TopDown ||
                               Theme.DirectionBoutonsPartie == FlowDirection.BottomUp;

            if (estVertical)
            {
                // Marges verticales (espacement en bas pour TopDown, en haut pour BottomUp)
                if (Theme.DirectionBoutonsPartie == FlowDirection.TopDown)
                {
                    BoutonChargerPartiePartie.Margin = new Padding(0, 0, 0, Theme.EspacementBoutonsPartie);
                    BoutonMenuPrincipalPartie.Margin = new Padding(0, 0, 0, Theme.EspacementBoutonsPartie);
                    BoutonNouvellePartiePartie.Margin = new Padding(0, 0, 0, 0);
                }
                else
                {
                    BoutonChargerPartiePartie.Margin = new Padding(0, Theme.EspacementBoutonsPartie, 0, 0);
                    BoutonMenuPrincipalPartie.Margin = new Padding(0, Theme.EspacementBoutonsPartie, 0, 0);
                    BoutonNouvellePartiePartie.Margin = new Padding(0, 0, 0, 0);
                }
            }
            else
            {
                // Marges horizontales (espacement a droite)
                BoutonChargerPartiePartie.Margin = new Padding(0, 0, Theme.EspacementBoutonsPartie, 0);
                BoutonMenuPrincipalPartie.Margin = new Padding(0, 0, Theme.EspacementBoutonsPartie, 0);
                BoutonNouvellePartiePartie.Margin = new Padding(0, 0, 0, 0);
            }

            PanelActionsPartie.Controls.Add(BoutonChargerPartiePartie);
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
                ForeColor = Theme.CouleurTexteBouton,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            bouton.FlatAppearance.BorderSize = Theme.EpaisseurBordureBouton;
            return bouton;
        }

        private Panel CreerCarte(int padding)
        {
            return new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.CouleurFondCarte,
                Padding = new Padding(padding),
                BorderStyle = Theme.StyleBordureCarte
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

        private Label CreerLabelEtudiant()
        {
            return new Label
            {
                AutoSize = true,
                Text = ConfigEtudiant.ObtenirTexteComplet(),
                Font = new Font(baseFont.FontFamily, 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(10, 6, 10, 6),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        public void AfficherEcran(EtatEcran ecran)
        {
            PanelMenu.Visible = ecran == EtatEcran.MenuPrincipal;
            PanelConfiguration.Visible = ecran == EtatEcran.ConfigurationNouvellePartie;
            PanelChargement.Visible = ecran == EtatEcran.ChargementPartie;
            PanelActionsPartie.Visible = ecran == EtatEcran.Partie;
            // Le label ETU de la partie est dessiné dans le HUD, pas besoin de l'afficher ici
            LabelEtudiantPartie.Visible = false;

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

            // Centrer les labels ETU en haut
            CentrerLabelEnHaut(LabelEtudiantMenu, PanelMenu);
            CentrerLabelEnHaut(LabelEtudiantConfiguration, PanelConfiguration);
            CentrerLabelEnHaut(LabelEtudiantChargement, PanelChargement);
            CentrerLabelEnHaut(LabelEtudiantPartie, parent);
        }

        private void CentrerLabelEnHaut(Label label, Control container)
        {
            if (label != null && container != null)
            {
                label.Left = (container.ClientSize.Width - label.Width) / 2;
                label.Top = 10;
            }
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
            var marge = Theme.MargeBoutonHautDroite;
            var margeHaut = Theme.MargeBoutonHaut;

            // Boutons de la page configuration
            if (PanelBoutonsConfiguration != null && PanelConfiguration != null)
            {
                PanelBoutonsConfiguration.Left = PanelConfiguration.ClientSize.Width - PanelBoutonsConfiguration.Width - marge;
                PanelBoutonsConfiguration.Top = margeHaut;
            }

            // Bouton de la page chargement
            PositionnerBoutonHautDroite(BoutonRetourMenuChargement, PanelChargement, marge, margeHaut);

            // Boutons de la page partie - en haut ou en bas selon le theme
            if (PanelActionsPartie != null)
            {
                PanelActionsPartie.Left = parent.ClientSize.Width - PanelActionsPartie.Width - marge;
                if (Theme.BoutonsPartieEnBas)
                {
                    PanelActionsPartie.Top = parent.ClientSize.Height - PanelActionsPartie.Height - marge;
                }
                else
                {
                    PanelActionsPartie.Top = margeHaut;
                }
            }
        }

        private void PositionnerBoutonHautDroite(Button bouton, Panel panel, int marge, int margeHaut)
        {
            if (bouton != null && panel != null)
            {
                bouton.Left = panel.ClientSize.Width - bouton.Width - marge;
                bouton.Top = margeHaut;
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
