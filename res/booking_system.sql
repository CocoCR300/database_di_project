CREATE DATABASE IF NOT EXISTS restify;

USE restify;

-- Tabla "Rol de usuario"
CREATE TABLE IF NOT EXISTS UserRole (
  userRoleId	INT 		UNSIGNED	NOT NULL AUTO_INCREMENT,
  type			VARCHAR(50)				NOT NULL,
  
  PRIMARY KEY (userRoleId),
  
  UNIQUE INDEX UNIQUE_type (type)
);

-- Tabla "Usuario"
CREATE TABLE IF NOT EXISTS User (
  userName		VARCHAR(50) 				NOT NULL,
  userRoleId	INT 			UNSIGNED 	NOT NULL,
  password 		VARCHAR(100) 				NOT NULL,
  
  PRIMARY KEY (userName),
  
  UNIQUE INDEX UNIQUE_userName (userName),
  
  CONSTRAINT FK_USER_USER_ROLE FOREIGN KEY (userRoleId) REFERENCES UserRole (userRoleId)
	ON DELETE RESTRICT
	ON UPDATE CASCADE
);


-- Tabla "Persona"
CREATE TABLE IF NOT EXISTS Person (
  personId		INT 			UNSIGNED	NOT NULL    AUTO_INCREMENT,
  userName 		VARCHAR(50) 				NOT NULL,
  firstName 	VARCHAR(50) 				NOT NULL,
  lastName 		VARCHAR(100) 				NOT NULL,
  emailAddress 	VARCHAR(200) 				NOT NULL,
  
  PRIMARY KEY (personId),
  
  UNIQUE INDEX UNIQUE_userName (userName),
  
  CONSTRAINT FK_PERSON_USER FOREIGN KEY (userName) REFERENCES User (userName)
	ON DELETE CASCADE
	ON UPDATE CASCADE
);

-- Tabla "Número de telefono de persona"
CREATE TABLE IF NOT EXISTS PersonPhoneNumber (
  personId 		INT 		UNSIGNED 	NOT NULL,
  phoneNumber 	CHAR(30) 				NOT NULL,
  
  INDEX FK_INDEX_PERSON_PHONE_NUMBER (personId),
  
  CONSTRAINT FK_PERSON_PHONE_NUMBER FOREIGN KEY (personId) REFERENCES Person (personId)
	ON DELETE CASCADE
	ON UPDATE CASCADE
);

-- Tabla "Alojamiento"
CREATE TABLE IF NOT EXISTS Lodging (
  lodgingId 	INT 			UNSIGNED 	NOT NULL      AUTO_INCREMENT,
  ownerPersonId INT 			UNSIGNED 	NOT NULL,
  lodgingType 	CHAR(50) 					NOT NULL,
  name 			VARCHAR(100) 				NOT NULL,
  address 		VARCHAR(300) 				NOT NULL,
  description 	VARCHAR(1000) 				NOT NULL,
  emailAddress	VARCHAR(200)				NOT NULL,	
  
  PRIMARY KEY (lodgingId),
  
  INDEX FK_INDEX_LODGING_PERSON (ownerPersonId),
  
  CONSTRAINT FK_LODGING_PERSON FOREIGN KEY (ownerPersonId) REFERENCES Person (personId)
	ON DELETE RESTRICT
	ON UPDATE CASCADE
);

-- Tabla "Número de telefono de alojamiento"
CREATE TABLE IF NOT EXISTS LodgingPhoneNumber (
  lodgingId 		INT 		UNSIGNED 	NOT NULL,
  phoneNumber 		CHAR(30) 				NOT NULL,
  
  INDEX FK_INDEX_LODGING_PHONE_NUMBER (lodgingId),
  
  CONSTRAINT FK_LODGING_PHONE_NUMBER FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
	ON DELETE CASCADE
	ON UPDATE CASCADE
);

-- Tabla "Foto de alojamiento"
CREATE TABLE IF NOT EXISTS LodgingPhoto (
	lodgingId	INT			UNSIGNED	NOT NULL	AUTO_INCREMENT,
	fileName	VARCHAR(75)				NOT NULL,
	ordering	TINYINT		UNSIGNED	NOT NULL,
	
			INDEX FK_INDEX_LODGING_LODGING_PHOTO (lodgingId),
	UNIQUE	INDEX UNIQUE_fileName (fileName),
	
	CONSTRAINT FK_LODGING_LODGING_PHOTO	FOREIGN KEY (lodgingId)	REFERENCES Lodging (lodgingId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
);

-- Tabla "Beneficio adicional"
CREATE TABLE IF NOT EXISTS Perk (
	perkId	INT			UNSIGNED	NOT NULL	AUTO_INCREMENT,
	name	VARCHAR(50)				NOT NULL,
	
	PRIMARY KEY (perkId),
	
	UNIQUE INDEX UNIQUE_name (name)
);

-- Tabla "Alojamiento beneficio adicional"
CREATE TABLE IF NOT EXISTS LodgingPerk (
	lodgingId	INT			UNSIGNED	NOT NULL,
	perkId		INT			UNSIGNED	NOT NULL,
	
	INDEX FK_INDEX_LODGING_LODGING_PERK	(lodgingId),
	INDEX FK_INDEX_PERK_LODGING_PERK	(perkId),
	
	CONSTRAINT FK_LODGING_LODGING_PERK	FOREIGN KEY (lodgingId)	REFERENCES Lodging (lodgingId)
		ON DELETE CASCADE
		ON UPDATE CASCADE,
	CONSTRAINT FK_PERK_LODGING_PERK		FOREIGN KEY (perkId)	REFERENCES Perk (perkId)
		ON DELETE RESTRICT
		ON UPDATE CASCADE
);

-- Tabla "Reservación"
CREATE TABLE IF NOT EXISTS Booking (
  bookingId 		INT 		UNSIGNED 	NOT NULL	AUTO_INCREMENT,
  customerPersonId 	INT 		UNSIGNED 	NOT NULL,
  lodgingId 		INT 		UNSIGNED 	NOT NULL,
  
  PRIMARY KEY (bookingId),
  
  INDEX FK_INDEX_BOOKING_LODGING 	(lodgingId),
  INDEX FK_INDEX_BOOKING_PERSON 	(customerPersonId),
  
  CONSTRAINT FK_BOOKING_LODGING FOREIGN KEY (lodgingId) 		REFERENCES Lodging (lodgingId)
	ON DELETE RESTRICT
	ON UPDATE CASCADE,
  CONSTRAINT FK_BOOKING_PERSON	FOREIGN KEY (customerPersonId) 	REFERENCES Person (personId)
	ON DELETE CASCADE
	ON UPDATE CASCADE
);

-- Tabla "Pago"
CREATE TABLE IF NOT EXISTS Payment (
  paymentId 			INT 		UNSIGNED 	NOT NULL	AUTO_INCREMENT,
  bookingId 			INT 		UNSIGNED,
  amount				DECIMAL		UNSIGNED	NOT NULL,
  dateAndTime 			DATETIME 				NOT NULL,
  invoiceImageFileName	CHAR(75)					NULL	DEFAULT NULL,
  
  PRIMARY KEY (paymentId),
  
  INDEX FK_INDEX_PAYMENT_BOOKING (bookingId),
  
  CONSTRAINT FK_PAYMENT_BOOKING FOREIGN KEY (bookingId) REFERENCES Booking (bookingId)
	ON DELETE SET NULL
	ON UPDATE CASCADE
);

-- Tabla "Tipo de habitación"
CREATE TABLE IF NOT EXISTS RoomType (
	roomTypeId		INT 		UNSIGNED	NOT NULL	AUTO_INCREMENT,
	lodgingId		INT			UNSIGNED	NOT NULL,
	name			VARCHAR(75)				NOT NULL,
	perNightPrice 	DECIMAL 	UNSIGNED 	NOT NULL,
	fees			DECIMAL 	UNSIGNED 	NOT NULL,
	capacity		INT 		UNSIGNED	NOT NULL,
	
	PRIMARY KEY (roomTypeId),
	
	INDEX FK_INDEX_LODGING_ROOM_TYPE (lodgingId),
  
	CONSTRAINT FK_LODGING_ROOM_TYPE FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
);

-- Tabla "Foto de tipo de habitación"
CREATE TABLE IF NOT EXISTS RoomTypePhoto (
	roomTypeId	INT			UNSIGNED	NOT NULL,
	fileName	CHAR(75)				NOT NULL,
	ordering	TINYINT		UNSIGNED	NOT NULL,
	
			INDEX FK_INDEX_ROOM_TYPE_ROOM_TYPE_PHOTO (roomTypeId),
	UNIQUE	INDEX UNIQUE_fileName (fileName),
  
	CONSTRAINT FK_ROOM_TYPE_ROOM_TYPE_PHOTO FOREIGN KEY (roomTypeId) REFERENCES RoomType (roomTypeId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
);

-- Tabla "Habitación"
CREATE TABLE IF NOT EXISTS Room (
  roomNumber   	INT 	UNSIGNED 	NOT NULL,
  lodgingId   	INT 	UNSIGNED 	NOT NULL,
  roomTypeId	INT		UNSIGNED	NOT NULL,
  
  PRIMARY KEY (lodgingId, roomNumber),
  
  INDEX FK_INDEX_LODGING_ROOM (lodgingId),
  INDEX FK_INDEX_ROOM_ROOM_TYPE (roomTypeId),
  
  CONSTRAINT FK_LODGING_ROOM FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
	ON DELETE NO ACTION
	ON UPDATE CASCADE,
  CONSTRAINT FK_ROOM_ROOM_TYPE FOREIGN KEY (roomTypeId) REFERENCES RoomType (roomTypeId)
	ON DELETE CASCADE
	ON UPDATE CASCADE
);

-- Tabla "Reservación de habitación"
CREATE TABLE IF NOT EXISTS RoomBooking (
  roomBookingId INT 		UNSIGNED 	NOT NULL AUTO_INCREMENT,
  bookingId 	INT 		UNSIGNED 	NOT NULL,
  lodgingId 	INT 		UNSIGNED 	NOT NULL,
  roomNumber 	INT 		UNSIGNED 	NOT NULL,
  cost			DECIMAL 	UNSIGNED 	NOT NULL,
  fees			DECIMAL 	UNSIGNED 	NOT NULL,
  discount		DECIMAL		UNSIGNED	NOT NULL,
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
	ON DELETE CASCADE
	ON UPDATE CASCADE,
  CONSTRAINT FK_LODGING_ROOM_BOOKING 	FOREIGN KEY (lodgingId)				REFERENCES Lodging (lodgingId)
	ON DELETE NO ACTION
	ON UPDATE CASCADE
);

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
