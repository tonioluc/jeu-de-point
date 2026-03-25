-- ============================================================
-- Script de creation de la base de donnees pour Jeu de Point
-- ============================================================

-- Etape 1: Creer la base de donnees (executer en tant que superuser)
CREATE DATABASE jeu_de_point;
\c jeu_de_point;
-- Etape 2: Se connecter a la base jeu_de_point puis executer ce qui suit

-- Table des parties
CREATE TABLE IF NOT EXISTS parties
(
    id                   INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    date_creation        TIMESTAMP NOT NULL DEFAULT NOW(),
    date_modification    TIMESTAMP NOT NULL DEFAULT NOW(),
    grid_lines           INTEGER   NOT NULL,
    index_joueur_courant INTEGER   NOT NULL,
    score_j1             INTEGER   NOT NULL DEFAULT 0,
    score_j2             INTEGER   NOT NULL DEFAULT 0,
    canon_y_j1           INTEGER   NOT NULL DEFAULT 0,
    canon_y_j2           INTEGER   NOT NULL DEFAULT 0,
    points_json          JSONB     NOT NULL DEFAULT '[]'::jsonb,
    lignes_json          JSONB     NOT NULL DEFAULT '[]'::jsonb,
    positions_tirees_json JSONB    NOT NULL DEFAULT '[]'::jsonb
);

-- Table des actions (historique de chaque action jouee)
CREATE TABLE IF NOT EXISTS actions_partie
(
    id           INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    partie_id    INTEGER     NOT NULL REFERENCES parties(id) ON DELETE CASCADE,
    ordre_action INTEGER     NOT NULL,
    horodatage   TIMESTAMP   NOT NULL DEFAULT NOW(),
    joueur_index INTEGER     NOT NULL,
    type_action  VARCHAR(64) NOT NULL,
    details_json JSONB       NOT NULL DEFAULT '{}'::jsonb
);

-- Index pour accelerer les requetes
CREATE INDEX IF NOT EXISTS ix_actions_partie_partie_id ON actions_partie(partie_id);
CREATE INDEX IF NOT EXISTS ix_actions_partie_ordre ON actions_partie(partie_id, ordre_action);
CREATE INDEX IF NOT EXISTS ix_parties_date_modification ON parties(date_modification DESC);

-- Migration: ajouter la colonne positions_tirees_json si elle n'existe pas
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'parties' AND column_name = 'positions_tirees_json'
    ) THEN
        ALTER TABLE parties ADD COLUMN positions_tirees_json JSONB NOT NULL DEFAULT '[]'::jsonb;
    END IF;
END $$;
