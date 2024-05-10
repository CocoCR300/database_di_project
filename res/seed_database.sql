INSERT INTO userRole (type) VALUES ('Administrator'), ('Customer'), ('Lessor');

INSERT INTO user (userName, password, userRoleId)
	VALUES	('johndoe', '1234', 1),
			('carmackjohn', '5678', 1),
			
			('teddycodd', 'ABCD', 2),
			('jskeet', 'EFGH', 2),
			('possiblylizard', '0000', 2),
			('magnoalejandro', '2222', 2),
			
			('michaelcontoso', 'IJKL', 3),
			('ritchied', 'MNOP', 3),
			('billyg', '1111', 3);
	
INSERT INTO person (userName, firstName, lastName, emailAddress)
	VALUES	('johndoe', 'John', 'Doe', 'a'),
			('carmackjohn', 'John', 'Carmack', 'b'),
			('teddycodd', 'Frank', 'Codd', 'c'),
			('jskeet', 'John', 'Skeet', 'c'),
			('possiblylizard', 'Mark', 'Zuckerberg', 'd'),
			('magnoalejandro', 'Alejandro', 'Magno', 'e'),
			('michaelcontoso', 'Michael', 'Contoso', 'f'),
			('ritchied', 'Dennis', 'Ritchie', 'g'),
			('billyg', 'Bill', 'Gates', 'h');
	
INSERT INTO lodging (ownerPersonId, name, lodgingType, description, address)
	VALUES	(1, 'Malekus Mountain Lodge', 'Cabin', 'Mountain hotel', 'Aguas Claras, Upala'),
			(2, 'Dreams Las Mareas', 'Hotel', 'Luxurious hotel at the beach', 'Guanacaste'),
			(2, 'Hotel Boyeros', 'Hotel', 'Hotel in Liberia downtown', 'Liberia, Guanacaste'),
			(3, 'Hotel El Bramadero', 'Hotel', 'Hotel in Liberia downtown', 'Liberia, Guanacaste'),
			(3, 'La Baula Lodge', 'Cabin', 'Hotel in front of Tortuguero lagoon', 'Limón');

INSERT INTO room (lodgingId, roomNumber, occupied, perNightPrice, capacity)
	VALUES	(1, 1, false, 60000, 2),
			(1, 2, false, 60000, 2),
			(1, 3, false, 60000, 2),
			(2, 1, false, 500000, 6),
			(2, 2, false, 500000, 6),
			(2, 3, false, 500000, 6),
			(2, 4, false, 500000, 6),
			(2, 5, false, 500000, 6),
			(2, 6, false, 500000, 6);
	
-- INSERT INTO booking (lodging_id, customer_id, status_id, start_date, end_date)
-- 	VALUES	(1, 2, 1, '2024-08-10', '2024-08-20'),
-- 			(1, 1, 4, '2024-01-22', '2024-02-02'),
-- 			(2, 4, 4, '2024-07-09', '2024-07-30'),
-- 			(2, 2, 3, '2024-01-22', '2024-02-02'),
-- 			(2, 3, 3, '2024-01-22', '2024-02-02'),
-- 			(2, 1, 4, '2024-01-22', '2024-02-02'),
-- 			(3, 4, 1, '2024-08-22', '2024-08-02'),
-- 			(3, 3, 2, '2024-08-22', '2024-08-30'),
-- 			(3, 3, 2, '2024-10-22', '2024-10-28'),
-- 			(3, 2, 1, '2024-08-22', '2024-09-02'),
-- 			(4, 2, 3, '2024-01-22', '2024-02-02'),
-- 			(4, 1, 4, '2024-01-22', '2024-02-02'),
-- 			(4, 1, 1, '2024-12-22', '2025-01-02'),
-- 			(4, 3, 3, '2024-01-22', '2024-02-02'),
-- 			(4, 2, 2, '2024-07-10', '2024-07-22'),
-- 			(4, 4, 1, '2024-09-22', '2024-10-02');