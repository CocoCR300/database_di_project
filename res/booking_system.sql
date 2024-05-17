CREATE DATABASE IF NOT EXISTS restify;

-- Tabla "Rol de usuario"
CREATE TABLE IF NOT EXISTS UserRole (
  userRoleId	INT 		UNSIGNED	NOT NULL AUTO_INCREMENT,
  type			VARCHAR(50)				  NOT NULL,
  
  PRIMARY KEY (userRoleId),
  
  UNIQUE INDEX UNIQUE_userRoleId (userRoleId)
);

-- Tabla "Usuario"
CREATE TABLE IF NOT EXISTS User (
  userName		VARCHAR(50) 			NOT NULL,
  userRoleId	INT 		UNSIGNED 	NOT NULL,
  password 		VARCHAR(100) 			NOT NULL,
  
  PRIMARY KEY (userName),
  
  UNIQUE INDEX UNIQUE_userName (userName),
  
  CONSTRAINT FK_USER_USER_ROLE FOREIGN KEY (userRoleId) REFERENCES UserRole (userRoleId)
);


-- Tabla "Persona"
CREATE TABLE IF NOT EXISTS Person (
  personId		INT 			UNSIGNED	NOT NULL    AUTO_INCREMENT,
  userName 		VARCHAR(50) 				NOT NULL,
  firstName 	VARCHAR(50) 				NOT NULL,
  lastName 		VARCHAR(100) 				NOT NULL,
  emailAddress 	VARCHAR(200) 				NOT NULL,
  
  PRIMARY KEY (personId),
  
  UNIQUE INDEX userName_UNIQUE (userName),
  
  CONSTRAINT FK_PERSON_USER FOREIGN KEY (userName) REFERENCES User (userName)
);

-- Tabla "Número de telefono"
CREATE TABLE IF NOT EXISTS PhoneNumber (
  personId 		INT 		UNSIGNED 	NOT NULL,
  phoneNumber 	CHAR(30) 				NOT NULL,
  
  INDEX FK_INDEX_PERSON_PHONE_NUMBER (personId),
  
  CONSTRAINT FK_PERSON_PHONE_NUMBER FOREIGN KEY (personId) REFERENCES Person (personId)
);

-- Tabla "Alojamiento"
CREATE TABLE IF NOT EXISTS Lodging (
  lodgingId 	INT 			UNSIGNED 	NOT NULL      AUTO_INCREMENT,
  ownerPersonId INT 			UNSIGNED 	NOT NULL,
  lodgingType 	VARCHAR(50) 				NOT NULL,
  name 			VARCHAR(100) 				NOT NULL,
  address 		VARCHAR(300) 				NOT NULL,
  description 	VARCHAR(1000) 				NOT NULL,
  
  PRIMARY KEY (lodgingId),
  
  INDEX FK_INDEX_LODGING_PERSON (ownerPersonId),
  
  CONSTRAINT FK_LODGING_PERSON FOREIGN KEY (ownerPersonId) REFERENCES Person (personId)
);

-- Tabla "Reservación"
CREATE TABLE IF NOT EXISTS Booking (
  bookingId 		INT 		UNSIGNED 	NOT NULL      AUTO_INCREMENT,
  customerPersonId 	INT 		UNSIGNED 	NOT NULL,
  lodgingId 		INT 		UNSIGNED 	NOT NULL,
  status 			VARCHAR(50) 			NOT NULL,
  startDate 		DATE 					NOT NULL,
  endDate 			DATE 					NOT NULL,
  
  PRIMARY KEY (bookingId),
  
  INDEX FK_INDEX_BOOKING_LODGING 	(lodgingId),
  INDEX FK_INDEX_BOOKING_PERSON 	(customerPersonId),
  
  CONSTRAINT FK_BOOKING_LODGING FOREIGN KEY (lodgingId) 		REFERENCES Lodging (lodgingId),
  CONSTRAINT FK_BOOKING_PERSON	FOREIGN KEY (customerPersonId) 	REFERENCES Person (personId)
);

-- Tabla "Pago"
CREATE TABLE IF NOT EXISTS Payment (
  paymentId 	INT 		UNSIGNED 	NOT NULL  AUTO_INCREMENT,
  bookingId 	INT 		UNSIGNED 	NOT NULL,
  dateAndTime 	DATETIME 				NOT NULL,
  
  PRIMARY KEY (paymentId),
  
  INDEX FK_INDEX_PAYMENT_BOOKING (bookingId),
  
  CONSTRAINT FK_PAYMENT_BOOKING FOREIGN KEY (bookingId) REFERENCES Booking (bookingId)
);

-- Tabla "Habitación"
CREATE TABLE IF NOT EXISTS Room (
  roomNumber   	INT 	UNSIGNED 	NOT NULL,
  lodgingId   	INT 	UNSIGNED 	NOT NULL,
  occupied 	  	TINYINT 			NOT NULL,
  perNightPrice DECIMAL UNSIGNED 	NOT NULL,
  capacity 		  INT 	UNSIGNED	NOT NULL,
  
  PRIMARY KEY (roomNumber, lodgingId),
  
  INDEX FK_INDEX_LODGING_ROOM (lodgingId),
  
  CONSTRAINT FK_LODGING_ROOM FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
);

-- Tabla "HabitaciónReservación"
CREATE TABLE IF NOT EXISTS RoomBooking (
  roomBookingId INT 	UNSIGNED NOT NULL AUTO_INCREMENT,
  bookingId 	INT 	UNSIGNED NOT NULL,
  roomNumber 	INT 	UNSIGNED NOT NULL,
  cost			DECIMAL UNSIGNED NOT NULL,
  fees			DECIMAL UNSIGNED NOT NULL,
  
  PRIMARY KEY (roomBookingId),
  
  INDEX FK_INDEX_ROOM_ROOM_BOOKING (roomNumber),
  INDEX FK_INDEX_BOOKING_ROOM_BOOKING (bookingId),
  
  CONSTRAINT FK_BOOKING_ROOM_BOOKING 	FOREIGN KEY (bookingId)		REFERENCES Booking (bookingId),
  CONSTRAINT FK_ROOM_ROOM_BOOKING		FOREIGN KEY (roomNumber)	REFERENCES Room (roomNumber)
);
