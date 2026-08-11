CREATE TABLE agence (
    code_agence VARCHAR(4) PRIMARY KEY,
    adresse_agence VARCHAR(30) NOT NULL,
    solde NUMERIC(18,2) DEFAULT 0.00 NOT NULL,
    pin VARCHAR(4) NOT NULL
);

CREATE TABLE client (
    id_client INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nom VARCHAR(20) NOT NULL,
    prenom VARCHAR(30),
    adresse VARCHAR(30) NOT NULL,
    contact VARCHAR(10) NOT NULL,
    bloquer BOOLEAN DEFAULT false
);

CREATE TABLE compte (
    numero VARCHAR(10) NOT NULL,
    pin VARCHAR(4) NOT NULL,
    solde NUMERIC(18,2) DEFAULT 0.00 NOT NULL,
    credit NUMERIC(18,2) DEFAULT 0.00 NOT NULL,
    bloquer BOOLEAN DEFAULT false NOT NULL,
    refclient INT NOT NULL REFERENCES client(id_client) ON UPDATE CASCADE
);

CREATE TABLE transaction (
    code VARCHAR(7) NOT NULL,
    libelle VARCHAR(10) NOT NULL,
    montant NUMERIC(18,2) NOT NULL,
    date timestamp(0) DEFAULT CURRENT_TIMESTAMP(0) NOT NULL,
    nom VARCHAR(80),
    code_agence VARCHAR(4) REFERENCES agence(code_agence),
    numero VARCHAR(10) NOT NULL REFERENCES compte(numero) ON UPDATE CASCADE,
    description VARCHAR(50)
);
