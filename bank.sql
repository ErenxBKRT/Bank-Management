DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS client CASCADE;
DROP TABLE IF EXISTS agence CASCADE;
DROP TABLE IF EXISTS carte_bancaire CASCADE;

CREATE TABLE agence
(
    code_agence VARCHAR(4) NOT NULL,
    adresse_agence VARCHAR(30) UNIQUE NOT NULL,
    solde DOUBLE PRECISION DEFAULT 0.00 NOT NULL,
    
    CONSTRAINT pk_agence_code_agence PRIMARY KEY (code_agence)
);

CREATE TABLE client
(
    id_client INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nom VARCHAR(20) NOT NULL,
    prenom VARCHAR(50),
    adresse VARCHAR(30) NOT NULL,
    contact VARCHAR(10) NOT NULL,
    solde DOUBLE PRECISION DEFAULT 0.00 NOT NULL,
    bloque BOOLEAN DEFAULT false,
    credit DOUBLE PRECISION DEFAULT 0.00 NOT NULL
);

CREATE TABLE transaction
(
    code VARCHAR(7) PRIMARY KEY NOT NULL,
    libelle VARCHAR(10),
    montant DOUBLE PRECISION NOT NULL,
    status VARCHAR(12) CHECK (status IN ("EN ATTENTE", "CONFIRMER"))
    date TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0) NOT NULL,
    nom VARCHAR(80),
    code_agence VARCHAR(4) NOT NULL,
    num_compte VARCHAR(10) NOT NULL,

    CONSTRAINT fk_transaction_numero FOREIGN KEY (num_compte) REFERENCES carte_bancaire(num_compte),
    CONSTRAINT fk_trasactions_agence FOREIGN KEY (code_agence) REFERENCES agence(code_agence)
);

CREATE TABLE carte_bancaire 
(
    num_compte VARCHAR(10) NOT NULL,
    date_creation TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0) NOT NULL,
    pin VARCHAR(4) NOT NULL,
    refclient INT NOT NULL,
    carte_bloquer BOOLEAN DEFAULT false NOT NULL,

    CONSTRAINT pk_carte_numero PRIMARY KEY (num_compte),
    CONSTRAINT fk_carte_client FOREIGN KEY (refclient) REFERENCES client(id_client)
);
