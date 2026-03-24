namespace jeu_de_point
{
    /// <summary>
    /// Theme par defaut de l'application.
    /// Ce fichier est versionne dans git et contient les valeurs par defaut.
    /// </summary>
    public class ThemeDefaut : ITheme
    {
        // Couleurs de fond
        public virtual Color CouleurFondMenuPrincipal => Color.FromArgb(240, 246, 255);
        public virtual Color CouleurFondConfiguration => Color.FromArgb(245, 250, 255);
        public virtual Color CouleurFondChargement => Color.FromArgb(245, 250, 255);
        public virtual Color CouleurFondCarte => Color.White;

        // Couleurs des boutons
        public virtual Color CouleurBoutonPrimaire => Color.FromArgb(38, 112, 233);
        public virtual Color CouleurBoutonSecondaire => Color.FromArgb(92, 138, 214);
        public virtual Color CouleurBoutonSucces => Color.FromArgb(39, 166, 117);
        public virtual Color CouleurBoutonDanger => Color.FromArgb(200, 60, 60);
        public virtual Color CouleurBoutonNeutre => Color.FromArgb(88, 105, 128);
        public virtual Color CouleurTexteBouton => Color.White;

        // Couleurs des textes
        public virtual Color CouleurTitrePrincipal => Color.FromArgb(32, 63, 110);
        public virtual Color CouleurTitreSecondaire => Color.FromArgb(40, 74, 122);
        public virtual Color CouleurTexteLabel => Color.FromArgb(55, 71, 95);

        // Tailles de police
        public virtual float TaillePoliceTitrePrincipal => 22f;
        public virtual float TaillePoliceTitreSecondaire => 18f;
        public virtual float TaillePoliceBoutonGrand => 12f;
        public virtual float TaillePoliceBoutonMoyen => 11f;
        public virtual float TaillePoliceBoutonPetit => 10f;
        public virtual float TaillePoliceLabel => 12f;
        public virtual float TaillePoliceListBox => 10f;

        // Padding et marges
        public virtual int PaddingCarte => 34;
        public virtual int PaddingCarteChargement => 30;
        public virtual int MargeEntreBoutons => 12;
        public virtual int MargeBoutonHautDroite => 18;
        public virtual int MargeBoutonHaut => 18;
        public virtual int EspacementBoutonsPartie => 10;

        // Tailles des boutons
        public virtual Size TailleBoutonMenu => new(0, 54);
        public virtual Size TailleBoutonRetour => new(150, 38);
        public virtual Size TailleBoutonValider => new(170, 48);
        public virtual Size TailleBoutonOuvrir => new(280, 44);
        public virtual Size TailleBoutonSupprimer => new(200, 38);
        public virtual Size TailleBoutonPartie => new(170, 36);
        public virtual Size TailleBoutonChargerPartie => new(150, 36);

        // Tailles des composants
        public virtual int LargeurInputGridLines => 170;
        public virtual Size TailleListeParties => new(560, 260);

        // Bordures
        public virtual int EpaisseurBordureBouton => 0;
        public virtual BorderStyle StyleBordureCarte => BorderStyle.FixedSingle;
        public virtual int RayonBordureBouton => 0;

        // Disposition
        public virtual bool CentrerBoutonsMenu => true;
        public virtual int LargeurSupplementaireBoutonMenu => 90;

        // Position des boutons en jeu (en bas a droite par defaut)
        public virtual bool BoutonsPartieEnBas => true;
        public virtual FlowDirection DirectionBoutonsPartie => FlowDirection.LeftToRight;

        // HUD (informations en jeu)
        public virtual int HudMargeGauche => 12;
        public virtual int HudMargeHaut => 12;
        public virtual int HudEspacementBlocs => 8;
        public virtual float HudTaillePolice => 10.5f;
        public virtual int HudPaddingHorizontal => 12;
        public virtual int HudPaddingVertical => 6;
        public virtual float HudEpaisseurBordure => 1.6f;
        public virtual Color[] HudCouleursFond => [
            Color.FromArgb(226, 240, 255),
            Color.FromArgb(234, 246, 236),
            Color.FromArgb(255, 241, 222)
        ];
        public virtual Color[] HudCouleursBordure => [
            Color.FromArgb(49, 112, 194),
            Color.FromArgb(62, 145, 95),
            Color.FromArgb(198, 128, 34)
        ];

        // Scores
        public virtual float ScoreTaillePolice => 16f;
        public virtual int ScoreEspacementJoueurs => 24;

        // Canon
        public virtual float CanonTaillePoliceY => 9f;
        public virtual int CanonLargeur => 20;
        public virtual int CanonEspacement => 20;
        public virtual float CanonEpaisseurIndicateur => 3f;
        public virtual float CanonEpaisseurBordure => 2f;
        public virtual int CanonOpaciteFond => 70;

        // Grille de jeu
        public virtual int PaddingAutourGrille => 60;
    }

    public interface ITheme
    {
        // Couleurs de fond
        Color CouleurFondMenuPrincipal { get; }
        Color CouleurFondConfiguration { get; }
        Color CouleurFondChargement { get; }
        Color CouleurFondCarte { get; }

        // Couleurs des boutons
        Color CouleurBoutonPrimaire { get; }
        Color CouleurBoutonSecondaire { get; }
        Color CouleurBoutonSucces { get; }
        Color CouleurBoutonDanger { get; }
        Color CouleurBoutonNeutre { get; }
        Color CouleurTexteBouton { get; }

        // Couleurs des textes
        Color CouleurTitrePrincipal { get; }
        Color CouleurTitreSecondaire { get; }
        Color CouleurTexteLabel { get; }

        // Tailles de police
        float TaillePoliceTitrePrincipal { get; }
        float TaillePoliceTitreSecondaire { get; }
        float TaillePoliceBoutonGrand { get; }
        float TaillePoliceBoutonMoyen { get; }
        float TaillePoliceBoutonPetit { get; }
        float TaillePoliceLabel { get; }
        float TaillePoliceListBox { get; }

        // Padding et marges
        int PaddingCarte { get; }
        int PaddingCarteChargement { get; }
        int MargeEntreBoutons { get; }
        int MargeBoutonHautDroite { get; }
        int MargeBoutonHaut { get; }
        int EspacementBoutonsPartie { get; }

        // Tailles des boutons
        Size TailleBoutonMenu { get; }
        Size TailleBoutonRetour { get; }
        Size TailleBoutonValider { get; }
        Size TailleBoutonOuvrir { get; }
        Size TailleBoutonSupprimer { get; }
        Size TailleBoutonPartie { get; }
        Size TailleBoutonChargerPartie { get; }

        // Tailles des composants
        int LargeurInputGridLines { get; }
        Size TailleListeParties { get; }

        // Bordures
        int EpaisseurBordureBouton { get; }
        BorderStyle StyleBordureCarte { get; }
        int RayonBordureBouton { get; }

        // Disposition
        bool CentrerBoutonsMenu { get; }
        int LargeurSupplementaireBoutonMenu { get; }

        // Position des boutons en jeu
        bool BoutonsPartieEnBas { get; }
        FlowDirection DirectionBoutonsPartie { get; }

        // HUD
        int HudMargeGauche { get; }
        int HudMargeHaut { get; }
        int HudEspacementBlocs { get; }
        float HudTaillePolice { get; }
        int HudPaddingHorizontal { get; }
        int HudPaddingVertical { get; }
        float HudEpaisseurBordure { get; }
        Color[] HudCouleursFond { get; }
        Color[] HudCouleursBordure { get; }

        // Scores
        float ScoreTaillePolice { get; }
        int ScoreEspacementJoueurs { get; }

        // Canon
        float CanonTaillePoliceY { get; }
        int CanonLargeur { get; }
        int CanonEspacement { get; }
        float CanonEpaisseurIndicateur { get; }
        float CanonEpaisseurBordure { get; }
        int CanonOpaciteFond { get; }

        // Grille de jeu
        int PaddingAutourGrille { get; }
    }
}
