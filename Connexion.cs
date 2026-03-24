using System.Text.Json;
using Npgsql;

namespace jeu_de_point;

public sealed class Connexion
{
    private readonly string connectionString;
    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

    public Connexion(string? connectionString = null)
    {
        this.connectionString = connectionString
            ?? Environment.GetEnvironmentVariable("JEU_DE_POINT_DB")
            ?? "Host=localhost;Port=5432;Database=jeu_de_point;Username=postgres;Password=admin";
    }

    public int CreerPartie(EtatPartieSauvegarde etat)
    {
        const string sql = @"
INSERT INTO parties
(date_creation, date_modification, grid_lines, index_joueur_courant, score_j1, score_j2, canon_y_j1, canon_y_j2, points_json, lignes_json)
VALUES
(@dateCreation, @dateMaj, @gridLines, @indexJoueurCourant, @scoreJ1, @scoreJ2, @canonYJ1, @canonYJ2, @pointsJson::jsonb, @lignesJson::jsonb)
RETURNING id;";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("dateCreation", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("dateMaj", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("gridLines", etat.GridLines);
        cmd.Parameters.AddWithValue("indexJoueurCourant", etat.IndexJoueurCourant);
        cmd.Parameters.AddWithValue("scoreJ1", etat.ScoreJ1);
        cmd.Parameters.AddWithValue("scoreJ2", etat.ScoreJ2);
        cmd.Parameters.AddWithValue("canonYJ1", etat.CanonYJ1);
        cmd.Parameters.AddWithValue("canonYJ2", etat.CanonYJ2);
        cmd.Parameters.AddWithValue("pointsJson", JsonSerializer.Serialize(etat.Points, jsonOptions));
        cmd.Parameters.AddWithValue("lignesJson", JsonSerializer.Serialize(etat.Lignes, jsonOptions));

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void MettreAJourEtatPartie(int partieId, EtatPartieSauvegarde etat)
    {
        const string sql = @"
UPDATE parties
SET date_modification = @dateMaj,
    grid_lines = @gridLines,
    index_joueur_courant = @indexJoueurCourant,
    score_j1 = @scoreJ1,
    score_j2 = @scoreJ2,
    canon_y_j1 = @canonYJ1,
    canon_y_j2 = @canonYJ2,
    points_json = @pointsJson::jsonb,
    lignes_json = @lignesJson::jsonb
WHERE id = @id;";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", partieId);
        cmd.Parameters.AddWithValue("dateMaj", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("gridLines", etat.GridLines);
        cmd.Parameters.AddWithValue("indexJoueurCourant", etat.IndexJoueurCourant);
        cmd.Parameters.AddWithValue("scoreJ1", etat.ScoreJ1);
        cmd.Parameters.AddWithValue("scoreJ2", etat.ScoreJ2);
        cmd.Parameters.AddWithValue("canonYJ1", etat.CanonYJ1);
        cmd.Parameters.AddWithValue("canonYJ2", etat.CanonYJ2);
        cmd.Parameters.AddWithValue("pointsJson", JsonSerializer.Serialize(etat.Points, jsonOptions));
        cmd.Parameters.AddWithValue("lignesJson", JsonSerializer.Serialize(etat.Lignes, jsonOptions));

        cmd.ExecuteNonQuery();
    }

    public void AjouterAction(ActionPartie action)
    {
        const string sql = @"
INSERT INTO actions_partie
(partie_id, ordre_action, horodatage, joueur_index, type_action, details_json)
VALUES
(@partieId, @ordreAction, @horodatage, @joueurIndex, @typeAction, @detailsJson::jsonb);";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("partieId", action.PartieId);
        cmd.Parameters.AddWithValue("ordreAction", action.OrdreAction);
        cmd.Parameters.AddWithValue("horodatage", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("joueurIndex", action.JoueurIndex);
        cmd.Parameters.AddWithValue("typeAction", action.TypeAction);
        cmd.Parameters.AddWithValue("detailsJson", JsonSerializer.Serialize(action.Details, jsonOptions));

        cmd.ExecuteNonQuery();
    }

    public List<PartieResume> ListerParties()
    {
        const string sql = @"
SELECT p.id, p.date_creation, p.date_modification, p.grid_lines, COUNT(a.id) AS total_actions
FROM parties p
LEFT JOIN actions_partie a ON a.partie_id = p.id
GROUP BY p.id
ORDER BY p.date_modification DESC;";

        var result = new List<PartieResume>();

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new PartieResume
            {
                Id = reader.GetInt32(0),
                DateCreation = reader.GetDateTime(1),
                DateModification = reader.GetDateTime(2),
                GridLines = reader.GetInt32(3),
                TotalActions = reader.GetInt32(4)
            });
        }

        return result;
    }

    public EtatPartieSauvegarde? ChargerEtatPartie(int partieId)
    {
        const string sql = @"
SELECT id, grid_lines, index_joueur_courant, score_j1, score_j2, canon_y_j1, canon_y_j2, points_json, lignes_json
FROM parties
WHERE id = @id;";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", partieId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var pointsJson = reader.IsDBNull(7) ? "[]" : reader.GetString(7);
        var lignesJson = reader.IsDBNull(8) ? "[]" : reader.GetString(8);

        return new EtatPartieSauvegarde
        {
            PartieId = reader.GetInt32(0),
            GridLines = reader.GetInt32(1),
            IndexJoueurCourant = reader.GetInt32(2),
            ScoreJ1 = reader.GetInt32(3),
            ScoreJ2 = reader.GetInt32(4),
            CanonYJ1 = reader.GetInt32(5),
            CanonYJ2 = reader.GetInt32(6),
            Points = JsonSerializer.Deserialize<List<PointSauvegarde>>(pointsJson, jsonOptions) ?? [],
            Lignes = JsonSerializer.Deserialize<List<LigneSauvegarde>>(lignesJson, jsonOptions) ?? []
        };
    }

    public void SupprimerPartie(int partieId)
    {
        const string sql = "DELETE FROM parties WHERE id = @id;";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", partieId);
        cmd.ExecuteNonQuery();
    }

    public List<ActionPartie> ListerActionsPartie(int partieId)
    {
        const string sql = @"
SELECT id, partie_id, ordre_action, horodatage, joueur_index, type_action, details_json
FROM actions_partie
WHERE partie_id = @partieId
ORDER BY ordre_action;";

        var result = new List<ActionPartie>();

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("partieId", partieId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var detailsJson = reader.IsDBNull(6) ? "{}" : reader.GetString(6);
            result.Add(new ActionPartie
            {
                PartieId = reader.GetInt32(1),
                OrdreAction = reader.GetInt32(2),
                JoueurIndex = reader.GetInt32(4),
                TypeAction = reader.GetString(5),
                Details = JsonSerializer.Deserialize<Dictionary<string, object?>>(detailsJson, jsonOptions) ?? []
            });
        }

        return result;
    }

    public int Executer(string sql, Dictionary<string, object?>? parameters = null)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        AjouterParametres(cmd, parameters);
        return cmd.ExecuteNonQuery();
    }

    public object? Scalar(string sql, Dictionary<string, object?>? parameters = null)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        AjouterParametres(cmd, parameters);
        return cmd.ExecuteScalar();
    }

    private static void AjouterParametres(NpgsqlCommand cmd, Dictionary<string, object?>? parameters)
    {
        if (parameters is null)
        {
            return;
        }

        foreach (var (key, value) in parameters)
        {
            cmd.Parameters.AddWithValue(key, value ?? DBNull.Value);
        }
    }
}

public sealed class PartieResume
{
    public int Id { get; init; }
    public DateTime DateCreation { get; init; }
    public DateTime DateModification { get; init; }
    public int GridLines { get; init; }
    public int TotalActions { get; init; }

    public override string ToString()
    {
        return $"Partie #{Id} | Grille {GridLines}x{GridLines} | Actions: {TotalActions} | Maj: {DateModification:dd/MM/yyyy HH:mm}";
    }
}

public sealed class EtatPartieSauvegarde
{
    public int PartieId { get; init; }
    public int GridLines { get; init; }
    public int IndexJoueurCourant { get; init; }
    public int ScoreJ1 { get; init; }
    public int ScoreJ2 { get; init; }
    public int CanonYJ1 { get; init; }
    public int CanonYJ2 { get; init; }
    public List<PointSauvegarde> Points { get; init; } = [];
    public List<LigneSauvegarde> Lignes { get; init; } = [];
}

public sealed class PointSauvegarde
{
    public int Col { get; init; }
    public int Row { get; init; }
    public int JoueurIndex { get; init; }
}

public sealed class LigneSauvegarde
{
    public int DebutCol { get; init; }
    public int DebutRow { get; init; }
    public int FinCol { get; init; }
    public int FinRow { get; init; }
    public int CouleurArgb { get; init; }
}

public sealed class ActionPartie
{
    public int PartieId { get; init; }
    public int OrdreAction { get; init; }
    public int JoueurIndex { get; init; }
    public string TypeAction { get; init; } = string.Empty;
    public Dictionary<string, object?> Details { get; init; } = [];
}
