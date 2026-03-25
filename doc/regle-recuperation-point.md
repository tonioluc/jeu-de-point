# Regle de Recuperation de Point

## Description

Cette fonctionnalite permet a un joueur de recuperer une position ou il avait precedemment place un point. Quand un joueur tire sur une coordonnee ou il a deja place un point (par clic ou par recuperation precedente), son point se repose automatiquement.

## Scenario Type

1. **Joueur 1** place un point en (3,5)
2. **Joueur 2** tire sur (3,5) et detruit le point de Joueur 1
3. **Joueur 1** tire sur (3,5) → son point se repose automatiquement

## Regles Detaillees

### Quand un joueur tire sur une position :

| Situation | Resultat |
|-----------|----------|
| Position vide + joueur a deja place ici | Son point se pose |
| Point adverse (non protege) + joueur a deja place ici | Point adverse detruit ET son point se pose |
| Point adverse (non protege) + joueur n'a jamais place ici | Point adverse detruit seulement |
| Point dans une ligne tracee | Aucun effet (point protege) |
| Son propre point | Aucun effet |

### Ce qui donne le droit de recuperer :

- **Placement par clic** : enregistre la position pour le joueur
- **Recuperation par tir** : enregistre la position pour le joueur
- **Destruction seule** : n'enregistre PAS la position

## Implementation Technique

### Structure de donnees

```csharp
// Dans GrilleJeu.cs
public Dictionary<(int Col, int Row), HashSet<int>> PositionsTirees { get; }
```

- **Cle** : coordonnee (Col, Row)
- **Valeur** : ensemble des indices de joueurs qui ont place un point sur cette position

Le `HashSet<int>` permet de garder tous les joueurs qui ont marque une position, pas seulement le dernier.

### Methodes principales

#### EnregistrerTir (GrilleJeu.cs)

```csharp
public void EnregistrerTir(int col, int row, int joueurIndex)
{
    var key = (col, row);
    if (!PositionsTirees.TryGetValue(key, out var joueurs))
    {
        joueurs = new HashSet<int>();
        PositionsTirees[key] = joueurs;
    }
    joueurs.Add(joueurIndex);
}
```

Ajoute le joueur a l'ensemble des joueurs qui ont place un point sur cette position.

#### JoueurADejaTireSurPosition (GrilleJeu.cs)

```csharp
public bool JoueurADejaTireSurPosition(int col, int row, int joueurIndex)
{
    return PositionsTirees.TryGetValue((col, row), out var joueurs)
        && joueurs.Contains(joueurIndex);
}
```

Verifie si le joueur a deja place un point sur cette position.

#### PointEstDansUneLigne (GrilleJeu.cs)

```csharp
public bool PointEstDansUneLigne((int Col, int Row) point)
{
    foreach (var ligne in LignesAlignements)
    {
        if (ligne.ContientPoint(point))
        {
            return true;
        }
    }
    return false;
}
```

Verifie si un point fait partie d'une ligne tracee (donc protege).

### Logique dans ExecuterTir (Form1.cs)

```csharp
private void ExecuterTir(int puissance)
{
    // 1. Calculer la position cible
    int cibleCol = canon.CalculerColonneImpact(puissance, grille.NombreLignes);
    int cibleRow = canon.PositionY;

    // 2. Verifier si le joueur a deja place ici AVANT de modifier quoi que ce soit
    bool dejaTireSurCettePosition = grille.JoueurADejaTireSurPosition(
        cibleCol, cibleRow, indexJoueurCourant);

    // 3. Si point adverse non protege
    if (pointAdverse && !grille.PointEstProtege(cible, joueurTouche))
    {
        grille.RetirerPoint(cible.Col, cible.Row);

        // Si le tireur a deja place ici, il recupere
        if (dejaTireSurCettePosition)
        {
            grille.PoserPoint(cibleCol, cibleRow, tireur);
            grille.EnregistrerTir(cibleCol, cibleRow, indexJoueurCourant);
            // Verifier alignement 5...
        }
        // Sinon : destruction seule, pas d'enregistrement
    }

    // 4. Si position vide et joueur a deja place ici
    else if (!grille.PointsPoses.ContainsKey(cible) && dejaTireSurCettePosition)
    {
        grille.PoserPoint(cibleCol, cibleRow, tireur);
        grille.EnregistrerTir(cibleCol, cibleRow, indexJoueurCourant);
        // Verifier alignement 5...
    }
}
```

### Logique dans OnMouseClick (Form1.cs)

```csharp
private void OnMouseClick(object? sender, MouseEventArgs e)
{
    // Poser le point
    grille.PoserPoint(intersection.Col, intersection.Row, joueur);

    // Enregistrer cette position pour la regle de recuperation
    grille.EnregistrerTir(intersection.Col, intersection.Row, indexJoueurCourant);

    // Verifier alignement 5...
}
```

## Persistance (Sauvegarde/Chargement)

### Base de donnees (base.sql)

```sql
-- Colonne pour stocker les positions
positions_tirees_json JSONB NOT NULL DEFAULT '[]'::jsonb
```

### Format JSON

```json
[
    {"col": 3, "row": 5, "joueurIndex": 0},
    {"col": 3, "row": 5, "joueurIndex": 1},
    {"col": 7, "row": 2, "joueurIndex": 0}
]
```

Chaque entree represente un joueur qui a place un point sur une position. Une meme position peut avoir plusieurs entrees (une par joueur).

### Sauvegarde (GestionnaireSauvegarde.cs)

```csharp
var positionsTirees = new List<PositionTireeSauvegarde>();
foreach (var kvp in grille.PositionsTirees)
{
    foreach (var joueurIndex in kvp.Value)
    {
        positionsTirees.Add(new PositionTireeSauvegarde
        {
            Col = kvp.Key.Col,
            Row = kvp.Key.Row,
            JoueurIndex = joueurIndex
        });
    }
}
```

### Chargement (GestionnaireSauvegarde.cs)

```csharp
foreach (var pt in etat.PositionsTirees)
{
    grille.EnregistrerTir(pt.Col, pt.Row, pt.JoueurIndex);
}
```

## Fichiers Modifies

| Fichier | Modifications |
|---------|---------------|
| `GrilleJeu.cs` | Ajout de `PositionsTirees`, `EnregistrerTir()`, `JoueurADejaTireSurPosition()`, `PointEstDansUneLigne()` |
| `Form1.cs` | Modification de `ExecuterTir()` et `OnMouseClick()` |
| `GestionnaireSauvegarde.cs` | Modification de `ConstruireEtat()` et `RestaurerEtat()` |
| `Connexion.cs` | Ajout de `positions_tirees_json` dans les requetes SQL |
| `base.sql` | Migration pour ajouter la colonne `positions_tirees_json` |
