INSERT INTO user_role (type) VALUES ('Administrador'), ('Cliente'), ('Arrendante');

INSERT INTO user (name, password, role_id, email_address)
	VALUES	('johndoe', '1234', 1, 'johndoe@gmail.com'),
			('carmackjohn', '5678', 1, 'carmackjohn@id.com'),
			
			('teddycodd', 'ABCD', 2, 'teddycodd@yahoo.net'),
			('jskeet', 'EFGH', 2, 'jskeet@google.co'),
			('possiblylizard', '0000', 2, 'possiblylizard@meta.com'),
			('magnoalejandro', '2222', 2, 'magnoalejandro@macedonia.org'),
			
			('michaelcontoso', 'IJKL', 3, 'michael@contoso.eu'),
			('ritchied', 'MNOP', 3, 'dennis@belllabs.us'),
			('billyg', '1111', 3, 'billyg@dot.net');
	
INSERT INTO administrator (user_name, first_name, last_name, phone_number)
	VALUES	('johndoe', 'John', 'Doe', '(+01) 8888-8888'),
			('carmackjohn', 'John', 'Carmack', '(+02) 8888-8888');
	
INSERT INTO customer (user_name, first_name, last_name, phone_number)
	VALUES	('teddycodd', 'Frank', 'Codd', '(+10) 7777-7777'),
			('jskeet', 'John', 'Skeet', '(+11) 7777-7777'),
			('possiblylizard', 'Mark', 'Zuckerberg', '(+12) 7777-7777'),
			('magnoalejandro', 'Alejandro', 'Magno', '(+13) 7777-7777');
	
INSERT INTO lessor (user_name, first_name, last_name, phone_number)
	VALUES	('michaelcontoso', 'Michael', 'Contoso', '(+20) 5555-5555'),
			('ritchied', 'Dennis', 'Ritchie', '(+21) 5555-5555'),
			('billyg', 'Bill', 'Gates', '(+22) 5555-5555');
	
INSERT INTO booking_status (type)
	VALUES ('Creado'), ('Confirmado'), ('Finalizado'), ('Cancelado');
	
INSERT INTO lodging (lessor_id, name, description, address, per_night_price, available_rooms)
	VALUES	(1, 'Malekus Mountain Lodge', 'Mountain hotel', 'Aguas Claras, Upala', 60000, 8),
			(2, 'Dreams Las Mareas', 'Luxurious hotel at the beach', 'Guanacaste', 1000000, 1000),
			(2, 'Hotel Boyeros', 'Hotel in Liberia downtown', 'Liberia, Guanacaste', 500000, 200),
			(3, 'Hotel El Bramadero', 'Hotel in Liberia downtown', 'Liberia, Guanacaste', 400000, 150),
			(3, 'La Baula Lodge', 'Hotel in front of Tortuguero lagoon', 'Limón', 100000, 80);
	
INSERT INTO booking (lodging_id, customer_id, status_id, start_date, end_date)
	VALUES	(1, 2, 1, '2024-08-10', '2024-08-20'),
			(1, 1, 4, '2024-01-22', '2024-02-02'),
			(2, 4, 4, '2024-07-09', '2024-07-30'),
			(2, 2, 3, '2024-01-22', '2024-02-02'),
			(2, 3, 3, '2024-01-22', '2024-02-02'),
			(2, 1, 4, '2024-01-22', '2024-02-02'),
			(3, 4, 1, '2024-08-22', '2024-08-02'),
			(3, 3, 2, '2024-08-22', '2024-08-30'),
			(3, 3, 2, '2024-10-22', '2024-10-28'),
			(3, 2, 1, '2024-08-22', '2024-09-02'),
			(4, 2, 3, '2024-01-22', '2024-02-02'),
			(4, 1, 4, '2024-01-22', '2024-02-02'),
			(4, 1, 1, '2024-12-22', '2025-01-02'),
			(4, 3, 3, '2024-01-22', '2024-02-02'),
			(4, 2, 2, '2024-07-10', '2024-07-22'),
			(4, 4, 1, '2024-09-22', '2024-10-02');