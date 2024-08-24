-- In MS SQL Server NO ACTION works the same way as RESTRICT on MySQL
-- Look for "Delete Rule": https://learn.microsoft.com/en-us/sql/relational-databases/tables/modify-foreign-key-relationships?view=sql-server-ver16#SSMSProcedure

IF DB_ID('restify') IS NULL
	CREATE DATABASE restify

GO

USE restify;

GO

-- Tabla "Rol de usuario"
IF OBJECT_ID('UserRole') IS NULL
	CREATE TABLE UserRole (
	  userRoleId	INT 					NOT NULL IDENTITY(1,1),
	  type			VARCHAR(50)				NOT NULL,
  
	  CONSTRAINT UNIQUE_UserRole_type UNIQUE (type), -- New convension to name CONSTRAINTS UNIQUE_table_row
	  PRIMARY KEY (userRoleId)
  
	);

GO


-- Tabla "Usuario"
IF OBJECT_ID('User') IS NULL
	CREATE TABLE [User] (
	  userName		VARCHAR(50) 			NOT NULL,
	  userRoleId	INT 					NOT NULL,
	  password 		VARCHAR(100) 			NOT NULL,
  
	  PRIMARY KEY (userName),
  
	  CONSTRAINT UNIQUE_User_userName UNIQUE (userName),
  
	  CONSTRAINT FK_USER_USER_ROLE FOREIGN KEY (userRoleId) REFERENCES UserRole (userRoleId)
		ON DELETE NO ACTION
		ON UPDATE CASCADE
	);

GO


-- Tabla "Persona"
IF OBJECT_ID('Person') IS NULL
	CREATE TABLE Person (
	  personId		INT 					NOT NULL    IDENTITY,
	  userName 		VARCHAR(50) 			NOT NULL,
	  firstName 	VARCHAR(50) 			NOT NULL,
	  lastName 		VARCHAR(100) 			NOT NULL,
	  emailAddress 	VARCHAR(200) 			NOT NULL,
  
	  PRIMARY KEY (personId),
  
	  CONSTRAINT UNIQUE_Person_userName UNIQUE (userName),
  
	  CONSTRAINT FK_PERSON_USER FOREIGN KEY (userName) REFERENCES [User] (userName)
		ON DELETE CASCADE
		ON UPDATE CASCADE
	);

GO

-- Tabla "Número de telefono de persona"
IF OBJECT_ID('PersonPhoneNumber') IS NULL
	CREATE TABLE PersonPhoneNumber (
	  personId 		INT 					NOT NULL,
	  phoneNumber 	CHAR(30) 				NOT NULL,
  
	  INDEX FK_INDEX_PERSON_PHONE_NUMBER (personId),
  
	  CONSTRAINT FK_PERSON_PHONE_NUMBER FOREIGN KEY (personId) REFERENCES Person (personId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
	);

GO

-- Tabla "Alojamiento"
IF OBJECT_ID('Lodging') IS NULL
	CREATE TABLE Lodging (
	  lodgingId 	INT 					NOT NULL      IDENTITY,
	  ownerPersonId INT 					NOT NULL,
	  lodgingType 	CHAR(50) 				NOT NULL,
	  name 			VARCHAR(100) 			NOT NULL,
	  address 		VARCHAR(300) 			NOT NULL,
	  description 	VARCHAR(1000) 			NOT NULL,
	  emailAddress	VARCHAR(200)			NOT NULL,	
  
	  PRIMARY KEY (lodgingId),
  
	  INDEX FK_INDEX_LODGING_PERSON (ownerPersonId),
  
	  CONSTRAINT FK_LODGING_PERSON FOREIGN KEY (ownerPersonId) REFERENCES Person (personId)
		ON DELETE NO ACTION
		ON UPDATE CASCADE
	);

GO

-- Tabla "Número de telefono de alojamiento"
IF OBJECT_ID('LodgingPhoneNumber') IS NULL
	CREATE TABLE LodgingPhoneNumber (
	  lodgingId 		INT 				NOT NULL,
	  phoneNumber 		CHAR(30) 			NOT NULL,
  
	  INDEX FK_INDEX_LODGING_PHONE_NUMBER (lodgingId),
  
	  CONSTRAINT FK_LODGING_PHONE_NUMBER FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
	);

GO

-- Tabla "Foto de alojamiento"
IF OBJECT_ID('LodgingPhoto') IS NULL
	CREATE TABLE LodgingPhoto (
		lodgingId	INT						NOT NULL,
		fileName	VARCHAR(75)				NOT NULL,
		ordering	TINYINT					NOT NULL,
	
		INDEX FK_INDEX_LODGING_LODGING_PHOTO (lodgingId),
		CONSTRAINT UNIQUE_LodgingPhoto_fileName UNIQUE (fileName),
	
		CONSTRAINT FK_LODGING_LODGING_PHOTO	FOREIGN KEY (lodgingId)	REFERENCES Lodging (lodgingId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Beneficio adicional"
IF OBJECT_ID('Perk') IS NULL
	CREATE TABLE Perk (
		perkId	INT							NOT NULL	IDENTITY,
		name	VARCHAR(50)					NOT NULL,
	
		PRIMARY KEY (perkId),
	
		CONSTRAINT UNIQUE_Perk_name UNIQUE (name),
	);

GO

-- Tabla "Alojamiento beneficio adicional"
IF OBJECT_ID('LodgingPerk') IS NULL
	CREATE TABLE LodgingPerk (
		lodgingId	INT						NOT NULL,
		perkId		INT						NOT NULL,
	
		INDEX FK_INDEX_LODGING_LODGING_PERK	(lodgingId),
		INDEX FK_INDEX_PERK_LODGING_PERK	(perkId),
	
		CONSTRAINT FK_LODGING_LODGING_PERK	FOREIGN KEY (lodgingId)	REFERENCES Lodging (lodgingId)
			ON DELETE CASCADE
			ON UPDATE CASCADE,
		CONSTRAINT FK_PERK_LODGING_PERK		FOREIGN KEY (perkId)	REFERENCES Perk (perkId)
			ON DELETE NO ACTION
			ON UPDATE CASCADE
	);

GO

-- Tabla "Reservación"
IF OBJECT_ID('Booking') IS NULL
	CREATE TABLE Booking (
	  bookingId 		INT 				NOT NULL	IDENTITY,
	  customerPersonId 	INT 				NOT NULL,
	  lodgingId 		INT 				NOT NULL,
  
	  PRIMARY KEY (bookingId),
  
	  INDEX FK_INDEX_BOOKING_LODGING 	(lodgingId),
	  INDEX FK_INDEX_BOOKING_PERSON 	(customerPersonId),
  
	  CONSTRAINT FK_BOOKING_LODGING FOREIGN KEY (lodgingId) 		REFERENCES Lodging (lodgingId)
		ON DELETE NO ACTION
		ON UPDATE CASCADE,
	  CONSTRAINT FK_BOOKING_PERSON	FOREIGN KEY (customerPersonId) 	REFERENCES Person (personId)
		ON DELETE NO ACTION -- Cycles or multiple cascade paths if CASCADE 
		ON UPDATE NO ACTION -- Cycles or multiple cascade paths if CASCADE
	);

GO

-- Tabla "Pago"
IF OBJECT_ID('Payment') IS NULL
	CREATE TABLE Payment (
	  paymentId 			INT 			NOT NULL	IDENTITY,
	  bookingId 			INT,
	  amount				DECIMAL			NOT NULL,
	  dateAndTime 			DATETIME 		NOT NULL,
	  invoiceImageFileName	CHAR(75)		NULL		DEFAULT NULL,
  
	  PRIMARY KEY (paymentId),
  
	  INDEX FK_INDEX_PAYMENT_BOOKING (bookingId),
  
	  CONSTRAINT FK_PAYMENT_BOOKING FOREIGN KEY (bookingId) REFERENCES Booking (bookingId)
		ON DELETE SET NULL
		ON UPDATE CASCADE
	);

GO

-- Tabla "Tipo de habitación"
IF OBJECT_ID('RoomType') IS NULL
	CREATE TABLE RoomType (
		roomTypeId		INT 				NOT NULL	IDENTITY,
		lodgingId		INT					NOT NULL,
		name			VARCHAR(75)			NOT NULL,
		perNightPrice 	DECIMAL 			NOT NULL,
		fees			DECIMAL 			NOT NULL,
		capacity		INT 				NOT NULL,
	
		PRIMARY KEY (roomTypeId),
	
		INDEX FK_INDEX_LODGING_ROOM_TYPE (lodgingId),
  
		CONSTRAINT FK_LODGING_ROOM_TYPE FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Foto de tipo de habitación"
IF OBJECT_ID('RoomTypePhoto') IS NULL
	CREATE TABLE RoomTypePhoto (
		roomTypeId	INT						NOT NULL,
		fileName	CHAR(75)				NOT NULL,
		ordering	TINYINT					NOT NULL,
	
		INDEX FK_INDEX_ROOM_TYPE_ROOM_TYPE_PHOTO (roomTypeId),
		CONSTRAINT UNIQUE_RoomTypePhoto_fileName UNIQUE (fileName),
  
		CONSTRAINT FK_ROOM_TYPE_ROOM_TYPE_PHOTO FOREIGN KEY (roomTypeId) REFERENCES RoomType (roomTypeId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Habitación"
IF OBJECT_ID('Room') IS NULL
	CREATE TABLE Room (
	  roomNumber   	INT 					NOT NULL,
	  lodgingId   	INT 					NOT NULL,
	  roomTypeId	INT						NOT NULL,
  
	  PRIMARY KEY (lodgingId, roomNumber),
  
	  INDEX FK_INDEX_LODGING_ROOM (lodgingId),
	  INDEX FK_INDEX_ROOM_ROOM_TYPE (roomTypeId),
  
	  CONSTRAINT FK_LODGING_ROOM FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
		ON DELETE NO ACTION
		ON UPDATE CASCADE,
	  CONSTRAINT FK_ROOM_ROOM_TYPE FOREIGN KEY (roomTypeId) REFERENCES RoomType (roomTypeId)
		ON DELETE NO ACTION -- Cycles or multiple cascade paths if CASCADE
		ON UPDATE NO ACTION	-- Cycles or multiple cascade paths if CASCADE
	);

GO

-- Tabla "Reservación de habitación"
IF OBJECT_ID('RoomBooking') IS NULL
	CREATE TABLE RoomBooking (
	  roomBookingId INT 					NOT NULL IDENTITY,
	  bookingId 	INT 					NOT NULL,
	  lodgingId 	INT 					NOT NULL,
	  roomNumber 	INT 					NOT NULL,
	  cost			DECIMAL 				NOT NULL,
	  fees			DECIMAL 				NOT NULL,
	  discount		DECIMAL					NOT NULL,
	  status		CHAR(50)				NOT NULL,
	  startDate 	DATE					NOT NULL,
	  endDate 		DATE 					NOT NULL,
  
	  PRIMARY KEY (roomBookingId),
  
	  INDEX FK_INDEX_ROOM_ROOM_BOOKING (roomNumber),
	  INDEX FK_INDEX_BOOKING_ROOM_BOOKING (bookingId),
	  INDEX FK_INDEX_LODGING_ROOM_BOOKING (lodgingId),
  
	  CONSTRAINT FK_ROOM_ROOM_BOOKING		FOREIGN KEY (lodgingId, roomNumber)	REFERENCES Room (lodgingId, roomNumber)
		ON DELETE NO ACTION
		ON UPDATE CASCADE,
	  CONSTRAINT FK_BOOKING_ROOM_BOOKING 	FOREIGN KEY (bookingId)				REFERENCES Booking (bookingId)
		ON DELETE NO ACTION  -- Cycles or multiple cascade paths if CASCADE
		ON UPDATE NO ACTION, -- Cycles or multiple cascade paths if CASCADE
	  CONSTRAINT FK_LODGING_ROOM_BOOKING 	FOREIGN KEY (lodgingId)				REFERENCES Lodging (lodgingId)
		ON DELETE NO ACTION
		ON UPDATE NO ACTION -- Cycles or multiple cascade paths if CASCADE
	);

GO

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

-- Insertar datos en Payment
INSERT INTO Payment (bookingId, amount, dateAndTime, invoiceImageFileName) VALUES 
(1, 100.00, '2023-01-01 12:00:00', 'invoice1.jpg');
