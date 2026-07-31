DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS clients CASCADE;
DROP TABLE IF EXISTS employe CASCADE;
DROP TABLE IF EXISTS agence CASCADE;
DROP TABLE IF EXISTS bank CASCADE;

CREATE TABLE bank 
(
    id_bank INT PRIMARY KEY DEFAULT 1 CHECK (id_bank = 1),
	name VARCHAR(10) UNIQUE NOT NULL,
	solde DOUBLE PRECISION
);
INSERT INTO bank (name, solde) VALUES ('BANKKUN', 1040000500.75);

CREATE TABLE agence
(
    code_agence VARCHAR(4) PRIMARY KEY NOT NULL,
    lieu VARCHAR(30) NOT NULL,
    adresse_agence VARCHAR(30),
    actives BOOLEAN DEFAULT true,

    id_bank INT DEFAULT 1 NOT NULL CHECK (id_bank = 1) REFERENCES bank(id_bank) 
);
INSERT INTO agence (code_agence, lieu, adresse_agence) VALUES ('404', 'VOID', '404-34-VOID');

CREATE TABLE employe
(
    id_employe VARCHAR(10) NOT NULL,
    nom VARCHAR(30) NOT NULL,
    prenom VARCHAR(50),
    passwords VARCHAR(16),
    date_creation TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0),
    code_agence VARCHAR(4) REFERENCES agence(code_agence),

    PRIMARY KEY (id_employe, code_agence)
);
INSERT INTO employe (id_employe, nom, prenom, passwords, code_agence) VALUES ('-404', 'Doe', 'Jane', 'error404', '404');

CREATE TABLE clients
(
    id_client VARCHAR(10) PRIMARY KEY NOT NULL,
    nom VARCHAR(30) NOT NULL,
    prenom VARCHAR(50),
    adresse VARCHAR(30) NOT NULL,
    mail VARCHAR(100),
    contact VARCHAR(10),
    solde DOUBLE PRECISION DEFAULT 0.00,
    pin VARCHAR(4) NOT NULL,
    date_creation TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0),
    bloque BOOLEAN DEFAULT false,
    dette REAL DEFAULT 0.00,

    id_employe VARCHAR(10),
    code_agence VARCHAR(4),
    FOREIGN KEY (id_employe, code_agence) REFERENCES employe(id_employe, code_agence),
    CONSTRAINT check_contact CHECK (contact IS NOT NULL OR mail IS NOT NULL)
);
INSERT INTO clients (id_client, nom, prenom, adresse, mail, solde, pin) VALUES ('0404', 'Doe', 'John', 'VOIDSTREET', 'johndoe@gmail.com', 40404.04, '0404');

CREATE TABLE transactions
(
    code_transaction VARCHAR(10) PRIMARY KEY NOT NULL,
    libelle VARCHAR(10),
    montant DOUBLE PRECISION NOT NULL,
    date_transaction TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0),

    id_client VARCHAR(10) REFERENCES clients(id_client),
    code_agence VARCHAR(4),
    id_employe VARCHAR(10),
    FOREIGN KEY (id_employe, code_agence) REFERENCES employe(id_employe, code_agence)
);
