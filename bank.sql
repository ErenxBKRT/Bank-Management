DROP TABLE IF EXISTS transaction;
DROP TABLE IF EXISTS client;
DROP TABLE IF EXISTS employe;
DROP TABLE IF EXISTS agence;

CREATE TABLE agence
	(
		code_agence INT PRIMARY KEY NOT NULL,
		lieu VARCHAR(30) NOT NULL,
		solde_agence REAL DEFAULT 0.00,
		adresse_agence VARCHAR(30),
		active BOOLEAN DEFAULT true
	);

CREATE TABLE employe
	(
		id_employe VARCHAR(10) PRIMARY KEY NOT NULL,
		privilege BOOLEAN DEFAULT false,
		nom VARCHAR(30) NOT NULL,
		prenom VARCHAR(50),
		password VARCHAR(16),
		date_creation TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
		code_agence INT REFERENCES agence(code_agence)
	);

CREATE TABLE client
	(
		id_client VARCHAR(10) PRIMARY KEY NOT NULL,
		num_compte INT UNIQUE NOT NULL,
		nom VARCHAR(30) NOT NULL,
		prenom VARCHAR(50),
		adresse VARCHAR(30) NOT NULL,
		mail VARCHAR(100),
		contact NUMERIC(10, 0),
		solde REAL DEFAULT 0.00,
		anniversaire DATE NOT NULL,
		pin SMALLINT NOT NULL,
		date_creation TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
		bloque BOOLEAN DEFAULT false,

		id_employe VARCHAR(10) REFERENCES employe(id_employe),
		CONSTRAINT check_contact CHECK (contact IS NOT NULL OR mail IS NOT NULL)
	);

CREATE TABLE transaction
	(
		code_transaction VARCHAR(10) PRIMARY KEY NOT NULL,
		libelle VARCHAR(10),
		montant REAL NOT NULL,
		date_transaction TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,

		id_client VARCHAR(10) REFERENCES client(id_client),
		id_employe VARCHAR(10) REFERENCES employe(id_employe),
		code_agence INT REFERENCES agence(code_agence)
	);