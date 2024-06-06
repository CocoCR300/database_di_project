USE restify_v2;

INSERT INTO userRole (type) VALUES ('Administrator'), ('Customer'), ('Lessor');

INSERT INTO User (userName, userRoleId, password)
	VALUES	('root', 1, 'root'),
			('generic_customer', 2, ''),
			('generic_lessor', 3, '');
	
INSERT INTO Person (personId, userName, firstName, lastName, emailAddress)
	VALUES	(1, 'root', 'Root', '', ''),
			(2, 'generic_customer', 'Generic Customer', '', ''),
			(3, 'generic_lessor', 'Generic Lessor', '', '');
	
INSERT INTO Lodging (lodgingId, ownerPersonId, lodgingType, name, address, description, emailAddress)
	VALUES (1, 1, 'Hotel', 'Generic Lodging', '', '', '');

INSERT INTO User (userName, userRoleId, password)
	VALUES	('admin1', 1, 'adminpassword1'),
			('admin2', 1, 'adminpassword2'),
			('customer1', 2, 'customerpassword1'),
			('customer2', 2, 'customerpassword2'),
			('lessor1', 3, 'lessorpassword1'),
			('lessor2', 3, 'lessorpassword2'),
			('lessor3', 3, 'lessorpassword3'),
			('customer3', 2, 'customerpassword3'),
			('admin3', 1, 'adminpassword3'),
			('customer4', 2, 'customerpassword4');

INSERT INTO Person (userName, firstName, lastName, emailAddress)
	VALUES	('admin1', 'Admin', 'One', 'admin1@example.com'),
			('admin2', 'Admin', 'Two', 'admin2@example.com'),
			('customer1', 'Customer', 'One', 'customer1@example.com'),
			('customer2', 'Customer', 'Two', 'customer2@example.com'),
			('lessor1', 'Lessor', 'One', 'lessor1@example.com'),
			('lessor2', 'Lessor', 'Two', 'lessor2@example.com'),
			('lessor3', 'Lessor', 'Three', 'lessor3@example.com'),
			('customer3', 'Customer', 'Three', 'customer3@example.com'),
			('admin3', 'Admin', 'Three', 'admin3@example.com'),
			('customer4', 'Customer', 'Four', 'customer4@example.com');

INSERT INTO PersonPhoneNumber (personId, phoneNumber)
	VALUES	(1, '123-456-7890'),
			(2, '123-456-7891'),
			(3, '123-456-7892'),
			(4, '123-456-7893'),
			(5, '123-456-7894'),
			(6, '123-456-7895'),
			(7, '123-456-7896'),
			(8, '123-456-7897'),
			(9, '123-456-7898'),
			(10, '123-456-7899');

INSERT INTO Lodging (ownerPersonId, lodgingType, name, address, description, emailAddress)
	VALUES	(5, 'Apartment', 'Sunny Apartment', '123 Main St', 'A bright and sunny apartment', 'sunnyapartment@example.com'),
			(5, 'GuestHouse', 'Cozy Guest House', '456 Elm St', 'A cozy guest house', 'cozyguesthouse@example.com'),
			(6, 'Hotel', 'Luxury Hotel', '789 Oak St', 'A luxury hotel', 'luxuryhotel@example.com'),
			(6, 'Lodge', 'Mountain Lodge', '101 Pine St', 'A lodge in the mountains', 'mountainlodge@example.com'),
			(7, 'Motel', 'Budget Motel', '202 Cedar St', 'A budget-friendly motel', 'budgetmotel@example.com'),
			(7, 'VacationRental', 'Beachfront Rental', '303 Beach St', 'A rental by the beach', 'beachfrontrental@example.com'),
			(5, 'Apartment', 'Urban Apartment', '404 City St', 'An urban apartment', 'urbanapartment@example.com'),
			(6, 'GuestHouse', 'Garden Guest House', '505 Garden St', 'A guest house with a garden', 'gardenguesthouse@example.com'),
			(7, 'Hotel', 'Downtown Hotel', '606 Central St', 'A hotel in downtown', 'downtownhotel@example.com'),
			(6, 'Lodge', 'Forest Lodge', '707 Forest St', 'A lodge in the forest', 'forestlodge@example.com');

INSERT INTO LodgingPhoneNumber (lodgingId, phoneNumber)
	VALUES	(1, '234-567-8901'),
			(2, '234-567-8902'),
			(3, '234-567-8903'),
			(4, '234-567-8904'),
			(5, '234-567-8905'),
			(6, '234-567-8906'),
			(7, '234-567-8907'),
			(8, '234-567-8908'),
			(9, '234-567-8909'),
			(10, '234-567-8910');

INSERT INTO LodgingPhoto (lodgingId, fileName, ordering)
	VALUES	(1, 'photo1.jpg', 1),
			(1, 'photo2.jpg', 2),
			(2, 'photo3.jpg', 1),
			(2, 'photo4.jpg', 2),
			(3, 'photo5.jpg', 1),
			(4, 'photo6.jpg', 1),
			(5, 'photo7.jpg', 1),
			(6, 'photo8.jpg', 1),
			(7, 'photo9.jpg', 1),
			(8, 'photo10.jpg', 1);

INSERT INTO Perk (name)
	VALUES	('Free WiFi'),
			('Pool'),
			('Parking'),
			('Breakfast Included'),
			('Pet Friendly'),
			('Gym'),
			('Spa'),
			('Airport Shuttle'),
			('24/7 Reception'),
			('Room Service');

INSERT INTO LodgingPerk (lodgingId, perkId)
	VALUES	(1, 1),
			(1, 2),
			(2, 3),
			(3, 4),
			(4, 5),
			(5, 6),
			(6, 7),
			(7, 8),
			(8, 9),
			(9, 10);

INSERT INTO Booking (customerPersonId, lodgingId, status)
	VALUES	(3, 1, 'Created'),
			(4, 2, 'Confirmed'),
			(3, 3, 'Cancelled'),
			(4, 4, 'Finished'),
			(3, 5, 'Created'),
			(4, 6, 'Confirmed'),
			(3, 7, 'Cancelled'),
			(4, 8, 'Finished'),
			(3, 9, 'Created'),
			(4, 10, 'Confirmed');

INSERT INTO Payment (bookingId, amount, dateAndTime, invoiceImageFileName)
	VALUES	(1, 100.00, '2024-01-01 12:00:00', 'invoice1.jpg'),
			(2, 200.00, '2024-01-02 13:00:00', 'invoice2.jpg'),
			(3, 300.00, '2024-01-03 14:00:00', 'invoice3.jpg'),
			(4, 400.00, '2024-01-04 15:00:00', 'invoice4.jpg'),
			(5, 500.00, '2024-01-05 16:00:00', 'invoice5.jpg'),
			(6, 600.00, '2024-01-06 17:00:00', 'invoice6.jpg'),
			(7, 700.00, '2024-01-07 18:00:00', 'invoice7.jpg'),
			(8, 800.00, '2024-01-08 19:00:00', 'invoice8.jpg'),
			(9, 900.00, '2024-01-09 20:00:00', 'invoice9.jpg'),
			(10, 1000.00, '2024-01-10 21:00:00', 'invoice10.jpg');

INSERT INTO RoomType (lodgingId, name, perNightPrice, fees, capacity)
	VALUES	(1, 'Standard Room', 50.00, 5.00, 2),
			(2, 'Deluxe Room', 100.00, 10.00, 2),
			(3, 'Suite', 150.00, 15.00, 4),
			(4, 'Family Room', 200.00, 20.00, 4),
			(5, 'Single Room', 30.00, 3.00, 1),
			(6, 'Double Room', 60.00, 6.00, 2),
			(7, 'Triple Room', 90.00, 9.00, 3),
			(8, 'Quad Room', 120.00, 12.00, 4),
			(9, 'Queen Room', 140.00, 14.00, 2),
			(10, 'King Room', 160.00, 16.00, 2);

INSERT INTO RoomTypePhoto (roomTypeId, fileName, ordering)
	VALUES	(1, 'roomphoto1.jpg', 1),
			(2, 'roomphoto2.jpg', 1),
			(3, 'roomphoto3.jpg', 1),
			(4, 'roomphoto4.jpg', 1),
			(5, 'roomphoto5.jpg', 1),
			(6, 'roomphoto6.jpg', 1),
			(7, 'roomphoto7.jpg', 1),
			(8, 'roomphoto8.jpg', 1),
			(9, 'roomphoto9.jpg', 1),
			(10, 'roomphoto10.jpg', 1);

INSERT INTO Room (roomNumber, lodgingId, roomTypeId)
	VALUES	(101, 1, 1),
			(102, 1, 1),
			(201, 2, 2),
			(202, 2, 2),
			(301, 3, 3),
			(302, 3, 3),
			(401, 4, 4),
			(402, 4, 4),
			(501, 5, 5),
			(502, 5, 5);

INSERT INTO RoomBooking (bookingId, lodgingId, roomNumber, cost, fees, discount, status, startDate, endDate)
	VALUES 	(1, 1, 101, 55.00, 5.00, 0.00, 'Created', '2024-02-01', '2024-02-10'),
			(2, 2, 201, 110.00, 10.00, 0.00, 'Confirmed', '2024-02-02', '2024-02-11'),
			(3, 3, 301, 165.00, 15.00, 0.00, 'Cancelled', '2024-02-03', '2024-02-12'),
			(4, 4, 401, 220.00, 20.00, 0.00, 'Finished', '2024-02-04', '2024-02-13'),
			(5, 5, 501, 33.00, 3.00, 0.00, 'Created', '2024-02-05', '2024-02-14');
