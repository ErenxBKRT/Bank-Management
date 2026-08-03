DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS clients CASCADE;
DROP TABLE IF EXISTS employe CASCADE;
DROP TABLE IF EXISTS agence CASCADE;
DROP TABLE IF EXISTS admins CASCADE; 
DROP TABLE IF EXISTS carte_bancaire CASCADE;

CREATE TABLE admins (username VARCHAR(10) NOT NULL CHECK (username = 'superuser'));

INSERT INTO admins VALUES ('superuser');

CREATE TABLE agence
(
    code_agence VARCHAR(4) NOT NULL,
    lieu VARCHAR(30) NOT NULL,
    adresse_agence VARCHAR(30) UNIQUE NOT NULL,
    actives BOOLEAN DEFAULT true,
    solde DOUBLE PRECISION DEFAULT 0.00 NOT NULL,
    
    CONSTRAINT pk_agence_code_agence PRIMARY KEY (code_agence)
);
-- INSERT INTO agence (code_agence, lieu, adresse_agence) VALUES ('404', 'VOID', '404-34-VOID');

CREATE TABLE employe
(
    id_employe VARCHAR(10) NOT NULL,
    nom VARCHAR(30) NOT NULL,
    prenom VARCHAR(50),
    passwords VARCHAR(16),
    date_entre TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0)NOT NULL,
    code_agence VARCHAR(4) NOT NULL,

    CONSTRAINT fk_employe_code_agence FOREIGN KEY (code_agence) REFERENCES agence(code_agence),
    CONSTRAINT pk_employe_id_employe_code_agence PRIMARY KEY (id_employe, code_agence)
);
-- INSERT INTO employe (id_employe, nom, prenom, passwords, code_agence) VALUES ('-404', 'Doe', 'Jane', 'error404', '404');

CREATE TABLE clients
(
    id_client VARCHAR(10) PRIMARY KEY NOT NULL,
    nom VARCHAR(30) NOT NULL,
    prenom VARCHAR(50),
    adresse VARCHAR(30) NOT NULL,
    mail VARCHAR(50),
    contact VARCHAR(10),
    solde DOUBLE PRECISION DEFAULT 0.00 NOT NULL,
    pin VARCHAR(4) NOT NULL,
    date_creation TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0) NOT NULL,
    bloque BOOLEAN DEFAULT false,
    dette DOUBLE PRECISION DEFAULT 0.00 NOT NULL,
    id_employe VARCHAR(10) NULL,
    code_agence VARCHAR(4) NULL,

    CONSTRAINT fk_clients_employe_agence FOREIGN KEY (id_employe, code_agence) REFERENCES employe(id_employe, code_agence),
    CONSTRAINT clients_check_contact_mail CHECK (contact IS NOT NULL OR mail IS NOT NULL)
);
-- INSERT INTO clients (id_client, nom, prenom, adresse, mail, solde, pin) VALUES ('0404', 'Doe', 'John', 'VOIDSTREET', 'johndoe@gmail.com', 40404.04, '0404');

CREATE TABLE transactions
(
    code_transaction VARCHAR(7) PRIMARY KEY NOT NULL,
    libelle VARCHAR(10),
    montant DOUBLE PRECISION NOT NULL,
    date_transaction TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0) NOT NULL,
    nom VARCHAR(80),
    id_employe VARCHAR(10) NULL,
    code_agence VARCHAR(4) NULL,
    refclient VARCHAR(10),

    CONSTRAINT fk_transaction_refclient FOREIGN KEY (refclient) REFERENCES clients(id_client),
    CONSTRAINT fk_trasactions_agence FOREIGN KEY (code_agence) REFERENCES agence(code_agence),
    CONSTRAINT fk_transaction_agence_employe FOREIGN KEY (id_employe, code_agence) REFERENCES employe(id_employe, code_agence) MATCH SIMPLE
);

CREATE TABLE carte_bancaire 
(
    id_client VARCHAR(10), 
    date_creation TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
    CONSTRAINT fk_carte_bancaire FOREIGN KEY (id_client) REFERENCES clients(id_client)
);