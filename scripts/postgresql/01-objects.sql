SELECT 'CREATE DATABASE contoso'
WHERE NOT EXISTS (
    SELECT
    FROM pg_database
    WHERE datname = 'contoso'
)\gexec

\c contoso;

CREATE USER appuser WITH PASSWORD 'pass@word1';
