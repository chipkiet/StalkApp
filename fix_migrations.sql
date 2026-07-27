DROP TABLE IF EXISTS "__efmigrationshistory";
DROP TABLE IF EXISTS "__EFMigrationsHistory";

CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
    ('20260712083126_InitialCreate', '8.0.14'),
    ('20260723181423_AddDeleteConversationFeatures', '8.0.14');

SELECT * FROM "__EFMigrationsHistory";
