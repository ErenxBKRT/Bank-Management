DROP TABLE agence CASCADE;
DROP TABLE client CASCADE;
DROP TABLE transaction CASCADE;
DROP TABLE compte CASCADE;
CREATE TABLE agence
(
    code_agence VARCHAR(4) NOT NULL,
    adresse_agence VARCHAR(30) UNIQUE NOT NULL,
    solde NUMERIC(18, 2) DEFAULT 0.00 NOT NULL,
    pin VARCHAR(4) NOT NULL,
    
    CONSTRAINT pk_agence_code_agence PRIMARY KEY (code_agence)
);

CREATE TABLE client
(
    id_client INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nom VARCHAR(20) NOT NULL,
    prenom VARCHAR(50),
    adresse VARCHAR(30) NOT NULL,
    contact VARCHAR(10) NOT NULL,
    bloque BOOLEAN DEFAULT false
);

CREATE TABLE compte 
(
    numero VARCHAR(10) NOT NULL,
    pin VARCHAR(4) NOT NULL,
    solde NUMERIC(18, 2) DEFAULT 0.00 NOT NULL,
    credit NUMERIC(18, 2) DEFAULT 0.00 NOT NULL,
    bloquer BOOLEAN DEFAULT false NOT NULL,
    refclient INT NOT NULL,

    CONSTRAINT pk_numero PRIMARY KEY (numero),
    CONSTRAINT fk_compte_client FOREIGN KEY (refclient) REFERENCES client(id_client)
);

CREATE TABLE transaction
(
    code VARCHAR(7) PRIMARY KEY NOT NULL,
    libelle VARCHAR(10),
    montant NUMERIC(18, 2) NOT NULL,
    date TIMESTAMP(0) DEFAULT CURRENT_TIMESTAMP(0) NOT NULL,
    nom VARCHAR(80),
    code_agence VARCHAR(4) NOT NULL,
    numero VARCHAR(10) NOT NULL,
    description VARCHAR(50),

    CONSTRAINT fk_transaction_numero FOREIGN KEY (numero) REFERENCES compte(numero),
    CONSTRAINT fk_trasactions_agence FOREIGN KEY (code_agence) REFERENCES agence(code_agence)
);
