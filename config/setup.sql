DO $$ 
BEGIN

	CREATE UNLOGGED TABLE Balance
	(
		Id         SERIAL PRIMARY KEY,
		CustomerId INT NOT NULL,
		Amount     INT NOT NULL,
		UpdatedAt  TIMESTAMP NOT NULL
	);

	CREATE UNLOGGED TABLE Transaction
	(
		Id              SERIAL PRIMARY KEY,
		CustomerId      INT NOT NULL,
		Amount          INT NOT NULL,
		TransactionType CHAR(1) NOT NULL,
		Description     VARCHAR(10) NOT NULL,
		CreatedAt       TIMESTAMP NOT NULL
	);

	INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (1, 0, NOW());
    INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (2, 0, NOW());
    INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (3, 0, NOW());
    INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (4, 0, NOW());
    INSERT INTO Balance (CustomerId, Amount, UpdatedAt) VALUES (5, 0, NOW());
END $$;