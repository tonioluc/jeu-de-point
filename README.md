# Jeu de Point

Application WinForms en C# (.NET 8) avec sauvegarde des parties dans PostgreSQL.

## 1. Prerequis

Ce projet a ete developpe pour **Windows**.

Installez:

- **.NET SDK 8.0** (ou plus recent compatible `net8.0-windows`)
- **PostgreSQL** (serveur local recommande)
- Un client SQL PostgreSQL (au choix):
  - `psql` (ligne de commande), ou
  - pgAdmin
- (Optionnel) Visual Studio 2022 / VS Code

Verification rapide:

```powershell
dotnet --version
```

## 2. Cloner le projet

```powershell
git clone <url-du-repo>
cd jeu-de-point
```

## 3. Configurer la base de donnees

Le fichier `base.sql` cree la base `jeu_de_point` et les tables necessaires.

### Option A: avec `psql` (recommande)

1. Connectez-vous a PostgreSQL avec un compte admin:

```powershell
psql -U postgres -h localhost -p 5432
```

2. Depuis `psql`, executez:

```sql
\i 'D:/s6/prog-s4/jeu-de-point/base.sql'
```

Note: adaptez le chemin selon votre machine.

### Option B: avec pgAdmin

1. Ouvrez pgAdmin.
2. Creez une base nommee `jeu_de_point` (si elle n'existe pas).
3. Ouvrez `base.sql` et executez le script sur cette base.

## 4. Configurer la chaine de connexion

Par defaut, l'application utilise:

```text
Host=localhost;Port=5432;Database=jeu_de_point;Username=postgres;Password=admin
```

Cette valeur est definie dans `Connexion.cs` si la variable d'environnement `JEU_DE_POINT_DB` est absente.

## 5. Restaurer, compiler et lancer

Depuis la racine du projet (`jeu-de-point`):

```powershell
dotnet restore
dotnet build
dotnet run
```

L'application WinForms devrait s'ouvrir.

## 6. Depannage

### Erreur de connexion PostgreSQL

Symptomes possibles:

- Message "Connexion PostgreSQL indisponible"
- Message "Sauvegarde automatique indisponible"

Verification:

1. PostgreSQL est demarre.
2. La base `jeu_de_point` existe.
3. Le script `base.sql` a bien ete execute.
4. Les identifiants (`Username`, `Password`) sont corrects.
5. La variable `JEU_DE_POINT_DB` pointe vers la bonne instance.

### `dotnet run` echoue

Verification:

1. Vous etes sur Windows.
2. Le SDK .NET 8 est installe.
3. Vous lancez la commande dans le dossier contenant `jeu-de-point.csproj`.

## 8. Resume rapide (checklist)

1. Installer .NET 8 + PostgreSQL.
2. Cloner le repo.
3. Executer `base.sql`.
4. Configurer `JEU_DE_POINT_DB` (ou utiliser la valeur par defaut).
5. Lancer `dotnet run`.
