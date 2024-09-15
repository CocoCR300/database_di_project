USE restify;

-- Insertar datos en UserRole
INSERT INTO UserRole (type) VALUES ('Administrator'), ('Customer'), ('Lessor');

-- Insertar datos en User
INSERT INTO [User] (userName, userRoleId, password) VALUES 
('admin', 1, 'password123'),
('customer', 2, 'password123'),
('lessor', 3, 'password123');

-- Insertar datos en Person
INSERT INTO Person (userName, firstName, lastName, emailAddress) VALUES 
('admin', 'Admin', 'User', 'admin@example.com'),
('customer', 'John', 'Doe', 'john.doe@example.com'),
('lessor', 'Jane', 'Smith', 'jane.smith@example.com');

-- Insertar datos en PersonPhoneNumber
INSERT INTO PersonPhoneNumber (personId, phoneNumber) VALUES 
(1, '555-1234'),
(2, '555-5678'),
(3, '555-9101');

-- Insertar datos en Lodging
INSERT INTO Lodging (ownerPersonId, lodgingType, name, address, description, emailAddress) VALUES 
(3, 'Hotel', 'Generic Hotel', '123 Main St', 'A generic hotel description', 'hotel@example.com');

-- Insertar datos en LodgingPhoneNumber
INSERT INTO LodgingPhoneNumber (lodgingId, phoneNumber) VALUES 
(1, '555-1212');

-- Insertar datos en LodgingPhoto
INSERT INTO LodgingPhoto (lodgingId, fileName, ordering) VALUES 
(1, 'photo1.jpg', 1),
(1, 'photo2.jpg', 2);

-- Insertar datos en Perk
INSERT INTO Perk (name) VALUES 
('Free WiFi'),
('Breakfast Included'),
('Parking');

-- Insertar datos en LodgingPerk
INSERT INTO LodgingPerk (lodgingId, perkId) VALUES 
(1, 1),
(1, 2),
(1, 3);

-- Insertar datos en RoomType
INSERT INTO RoomType (lodgingId, name, perNightPrice, fees, capacity) VALUES 
(1, 'Standard Room', 100.00, 10.00, 2),
(1, 'Deluxe Room', 150.00, 15.00, 4);

-- Insertar datos en RoomTypePhoto
INSERT INTO RoomTypePhoto (roomTypeId, fileName, ordering) VALUES 
(1, 'room1.jpg', 1),
(2, 'room2.jpg', 1);

-- Insertar datos en Room
INSERT INTO Room (roomNumber, lodgingId, roomTypeId) VALUES 
(101, 1, 1),
(102, 1, 2);

-- Insertar datos en Booking
INSERT INTO Booking (customerPersonId, lodgingId) VALUES 
(2, 1);

-- Insertar datos en RoomBooking
INSERT INTO RoomBooking (bookingId, lodgingId, roomNumber, cost, fees, discount, status, startDate, endDate) VALUES 
(1, 1, 101, 100.00, 10.00, 0.00, 'Confirmed', '2023-01-01', '2023-01-02'),
(1, 1, 102, 150.00, 15.00, 0.00, 'Confirmed', '2023-01-01', '2023-01-02');