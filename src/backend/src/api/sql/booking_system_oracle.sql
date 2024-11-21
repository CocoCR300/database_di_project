-- Integrantes
--  Jafet David Alvarado Barboza
--  Oscar Rojas Alvarado

CREATE DATABASE restify
    DATAFILE 'restify' SIZE 1G
    LOGFILE 'restify_log' SIZE 50M,
    COLLATE Modern_Spanish_CI_AS;
/

-- TABLAS

-- Tabla "Rol de usuario"
CREATE TABLE UserRole(
	userRoleId	NUMBER(4) 		GENERATED AS IDENTITY,
	type		VARCHAR2(50)	NOT NULL,
    
    PRIMARY KEY (userRoleId),
  
	CONSTRAINT UNIQUE_UserRole_type UNIQUE (type)
);
/

 --

-- Tabla "Usuario"
CREATE TABLE "User" (
	userName	VARCHAR2(50)		NOT NULL,
	userRoleId	NUMBER(4)   		NOT NULL,
	password	VARCHAR2(100)	    NOT NULL,
    
    PRIMARY KEY (userName),
  
	CONSTRAINT FK_USER_USER_ROLE FOREIGN KEY (userRoleId) REFERENCES UserRole (userRoleId)
);
/

 --


-- Tabla "Persona"
CREATE TABLE Person (
	personId		NUMBER(4) 		GENERATED AS IDENTITY PRIMARY KEY,
	userName 		VARCHAR2(50)	NOT NULL,
	firstName 		VARCHAR2(50) 	NOT NULL,
	lastName 		VARCHAR2(100) 	NOT NULL,
	emailAddress 	VARCHAR2(200) 	NOT NULL,
  
	CONSTRAINT UNIQUE_Person_userName UNIQUE (userName),
  
	CONSTRAINT FK_PERSON_USER FOREIGN KEY (userName) REFERENCES "User" (userName)
);
/

 --

-- Tabla "Número de telefono de persona"
CREATE TABLE PersonPhoneNumber (
	personId 	NUMBER(4) 		NOT NULL,
	phoneNumber VARCHAR2(30)	NOT NULL,
  
	CONSTRAINT FK_PERSON_PHONE_NUMBER	FOREIGN KEY (personId) REFERENCES Person (personId)
);
/

CREATE INDEX FK_INDEX_PERSON_PHONE_NUMBER
    ON PersonPhoneNumber(personId);
/

 --

-- Tabla "Alojamiento"
CREATE TABLE Lodging (
	lodgingId 		NUMBER(4) 			GENERATED AS IDENTITY,
	ownerPersonId	NUMBER(4) 			NOT NULL,
	lodgingType 	VARCHAR2(50) 	NOT NULL,
	name 			VARCHAR2(100)	NOT NULL,
	address 		VARCHAR2(300)	NOT NULL,
	description 	VARCHAR2(1000)	NOT NULL,
	emailAddress	VARCHAR2(200)	NOT NULL,
  
	PRIMARY KEY (lodgingId),
  
	CONSTRAINT FK_LODGING_PERSON FOREIGN KEY (ownerPersonId) REFERENCES Person (personId)
);
/

CREATE INDEX FK_INDEX_LODGING_PERSON
    ON Lodging(ownerPersonId);
/

 --

-- Tabla "Número de telefono de alojamiento"
CREATE TABLE LodgingPhoneNumber (
	lodgingId		NUMBER(4) 	NOT NULL,
	phoneNumber	VARCHAR2(30)	NOT NULL,
  
	CONSTRAINT FK_LODGING_PHONE_NUMBER FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
	);
/

CREATE INDEX FK_INDEX_LODGING_PHONE_NUMBER
    ON LodgingPhoneNumber(lodgingId);
/

 --

-- Tabla "Foto de alojamiento"
CREATE TABLE LodgingPhoto (
	lodgingId	NUMBER(4)		NOT NULL,
	fileName	VARCHAR2(75)	NOT NULL,
	ordering	NUMBER(3)		NOT NULL,

	CONSTRAINT UNIQUE_LodgingPhoto_fileName UNIQUE (fileName),
	
	CONSTRAINT FK_LODGING_LODGING_PHOTO	FOREIGN KEY (lodgingId)	REFERENCES Lodging (lodgingId)
);
/

CREATE INDEX FK_INDEX_LODGING_LODGING_PHOTO
    ON LodgingPhoto(lodgingId);
/

 --

CREATE TABLE Perk (
	perkId	NUMBER(4)		GENERATED AS IDENTITY,
	name	VARCHAR2(50)	NOT NULL,
	
	PRIMARY KEY (perkId),

	CONSTRAINT UNIQUE_Perk_name UNIQUE (name)
);
/

 --

-- Tabla "Alojamiento beneficio adicional"
CREATE TABLE LodgingPerk (
	lodgingId	NUMBER(4)	NOT NULL,
	perkId		NUMBER(4)	NOT NULL,
	
	CONSTRAINT FK_LODGING_LODGING_PERK	FOREIGN KEY (lodgingId)	REFERENCES Lodging (lodgingId),
	CONSTRAINT FK_PERK_LODGING_PERK		FOREIGN KEY (perkId)	REFERENCES Perk (perkId)
);
/

CREATE INDEX FK_INDEX_LODGING_LODGING_PERK
    ON LodgingPerk(lodgingId);

CREATE INDEX FK_INDEX_PERK_LODGING_PERK
    ON LodgingPerk(perkId);
/

 --

-- Tabla "Reservación"
CREATE TABLE Booking (
	bookingId 		    NUMBER(4) GENERATED AS IDENTITY,
	customerPersonId 	NUMBER(4) NOT NULL,
	lodgingId 		    NUMBER(4) NOT NULL,
  
	PRIMARY KEY (bookingId),
  
	CONSTRAINT FK_BOOKING_LODGING FOREIGN KEY (lodgingId) 		REFERENCES Lodging (lodgingId),
	CONSTRAINT FK_BOOKING_PERSON FOREIGN KEY (customerPersonId) 	REFERENCES Person (personId)
	);
/

CREATE INDEX FK_INDEX_BOOKING_LODGING
    ON Booking(lodgingId);


CREATE INDEX FK_INDEX_BOOKING_PERSON
    ON Booking(customerPersonId);
/
 --

-- Tabla "Información de pago"
CREATE TABLE PaymentInformation (
	paymentInformationId	NUMBER(4)				GENERATED AS IDENTITY,
	personId				NUMBER(4)				NOT NULL,
	cardNumber				VARCHAR2(16)		    NOT NULL,
	cardExpiryDate			DATE			        NOT NULL,
	cardSecurityCode		VARCHAR2(4)			    NOT NULL,
	cardHolderName			VARCHAR2(100)	        NOT NULL,

	CONSTRAINT PK_PaymentInformation_paymentInformationId
        PRIMARY KEY (paymentInformationId),

	CONSTRAINT FK_PAYMENT_INFORMATION_PERSON FOREIGN KEY (personId)
		REFERENCES Person (personId)
);
/

CREATE INDEX FK_INDEX_PAYMENT_INFORMATION_PERSON
    ON PaymentInformation(personId);
/

-- Tabla "Pago"
CREATE TABLE Payment (
	paymentId 				NUMBER(4) 		GENERATED AS IDENTITY PRIMARY KEY,
	bookingId 				NUMBER(4),
	paymentInformationId	NUMBER(4),
	amount					NUMBER(5,2)		NOT NULL,
	dateAndTime 			TIMESTAMP	    NOT NULL,
  
	CONSTRAINT FK_PAYMENT_BOOKING FOREIGN KEY (bookingId) REFERENCES Booking (bookingId),

	CONSTRAINT FK_PAYMENT_PAYMENT_INFORMATION FOREIGN KEY (paymentInformationId)
		REFERENCES PaymentInformation (paymentInformationId)
);
/

CREATE INDEX FK_INDEX_PAYMENT_BOOKING
    ON Payment(bookingId);

CREATE INDEX FK_INDEX_PAYMENT_PAYMENT_INFORMATION
    ON Payment(paymentInformationId);
/


-- Tabla "Tipo de habitación"
CREATE TABLE RoomType (
	roomTypeId		NUMBER(4) 				GENERATED AS IDENTITY,
	lodgingId		NUMBER(4)				NOT NULL,
	name			VARCHAR2(75)			NOT NULL,
	perNightPrice 	NUMBER(5,2) 			NOT NULL,
	fees			NUMBER(5,2) 			NOT NULL,
	capacity		NUMBER(5,2) 			NOT NULL,

	PRIMARY KEY (roomTypeId),
  
	CONSTRAINT FK_LODGING_ROOM_TYPE FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
);
/

CREATE INDEX FK_INDEX_LODGING_ROOM_TYPE
    ON RoomType(lodgingId);
/

 --

-- Tabla "Foto de tipo de habitación"
CREATE TABLE RoomTypePhoto (
	roomTypeId	NUMBER(4)		 NOT NULL,
	fileName	VARCHAR2(75)    NOT NULL,
	ordering	NUMBER(3)	    NOT NULL,

	CONSTRAINT UNIQUE_RoomTypePhoto_fileName UNIQUE (fileName),
  
	CONSTRAINT FK_ROOM_TYPE_ROOM_TYPE_PHOTO FOREIGN KEY (roomTypeId) REFERENCES RoomType (roomTypeId)
);
/

CREATE INDEX FK_INDEX_ROOM_TYPE_ROOM_TYPE_PHOTO
    ON RoomTypePhoto(roomTypeId);
/

 --

-- Tabla "Habitación"
CREATE TABLE Room (
	roomNumber	NUMBER(3) NOT NULL,
	lodgingId	NUMBER(4) NOT NULL,
	roomTypeId	NUMBER(4)	NOT NULL,
  
	PRIMARY KEY (lodgingId, roomNumber),
  
	CONSTRAINT FK_LODGING_ROOM FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId),
	CONSTRAINT FK_ROOM_ROOM_TYPE FOREIGN KEY (roomTypeId) REFERENCES RoomType (roomTypeId)
);
/

CREATE INDEX FK_INDEX_LODGING_ROOM
    ON Room(lodgingId);

CREATE INDEX FK_INDEX_ROOM_ROOM_TYPE
    ON Room(roomTypeId);
/

 --

-- Tabla "Reservación de habitación"
CREATE TABLE RoomBooking (
	roomBookingId	NUMBER(4) 		GENERATED AS IDENTITY,
	bookingId 		NUMBER(4) 		NOT NULL,
	lodgingId 		NUMBER(4) 		NOT NULL,
	roomNumber 		NUMBER(3) 		NOT NULL,
	cost			NUMBER(5,2) 	NOT NULL,
	fees			NUMBER(5,2) 	NOT NULL,
	discount		NUMBER(5,2)		NOT NULL,
	status			VARCHAR2(50)	NOT NULL,
	startDate 		DATE		NOT NULL,
	endDate 		DATE 		NOT NULL,
  
	PRIMARY KEY (roomBookingId),
  
	CONSTRAINT FK_ROOM_ROOM_BOOKING		FOREIGN KEY (lodgingId, roomNumber)	REFERENCES Room (lodgingId, roomNumber),
	CONSTRAINT FK_BOOKING_ROOM_BOOKING 	FOREIGN KEY (bookingId)				REFERENCES Booking (bookingId), -- Cycles or multiple cascade paths if CASCADE
	CONSTRAINT FK_LODGING_ROOM_BOOKING 	FOREIGN KEY (lodgingId)				REFERENCES Lodging (lodgingId)
);
/

CREATE INDEX FK_INDEX_ROOM_ROOM_BOOKING
    ON RoomBooking(roomNumber);

CREATE INDEX FK_INDEX_BOOKING_ROOM_BOOKING
    ON RoomBooking(bookingId);

CREATE INDEX FK_INDEX_LODGING_ROOM_BOOKING
    ON RoomBooking(lodgingId);
/

--
-- FUNCIONES
--
CREATE OR REPLACE FUNCTION fnIdPersonaUsuario
	(userName_IN IN VARCHAR2)
RETURN NUMBER IS personId INT;
BEGIN
    SELECT personId INTO personId FROM Person
        WHERE userName = userName_IN;
        
    RETURN (personId);
END;
/

CREATE OR REPLACE FUNCTION fnCalcularPagoTotalReserva
    (bookingId_IN IN INT)
RETURN NUMBER IS totalAmount NUMBER(18, 2);
BEGIN
    SELECT SUM(cost - discount + fees) INTO totalAmount FROM RoomBooking
        WHERE bookingId = bookingId_IN;
    
    RETURN (totalAmount);
END;
/

--
-- PROCEDIMIENTOS ALMACENADOS
--
CREATE OR REPLACE PROCEDURE paEliminarAlojamiento
    (lodgingId_IN IN INT)
AS BEGIN
	BEGIN
		DELETE FROM RoomBooking WHERE lodgingId = lodgingId_IN;

		DELETE FROM Booking     WHERE lodgingId = lodgingId_IN;

		DELETE FROM Room        WHERE lodgingId = lodgingId_IN;

		DELETE FROM RoomType    WHERE lodgingId = lodgingId_IN;

		DELETE FROM LodgingPerk WHERE lodgingId = lodgingId_IN;

		DELETE FROM LodgingPhoneNumber WHERE lodgingId = lodgingId_IN;

		DELETE FROM LodgingPhoto WHERE lodgingId = lodgingId_IN;

		DELETE FROM Lodging      WHERE lodgingId = lodgingId_IN;

		COMMIT;
	EXCEPTION WHEN OTHERS THEN
		ROLLBACK;
		RAISE;
	END;
END;
/

CREATE OR REPLACE PROCEDURE paCambiarEstadoReservacion
    (bookingId_IN IN INT,
     status_IN IN CHAR
    )
AS BEGIN
    UPDATE RoomBooking SET status = status_IN
        WHERE bookingId = bookingId_IN;
END;
/

--
-- Vistas
--
CREATE VIEW vUsuariosPersonas
AS
    SELECT u.*, p.personId, p.firstName, p.lastName, p.emailAddress FROM "User" u
        INNER JOIN Person p ON u.userName = p.userName;
/

CREATE VIEW vHoteles
AS
    SELECT * FROM Lodging l
        WHERE l.lodgingType = 'Hotel';
/

--
-- Desencadenadores
--
CREATE OR REPLACE TRIGGER disProhibirBorradoPago
	BEFORE DELETE ON Payment
    FOR EACH ROW
BEGIN
    RAISE_APPLICATION_ERROR(-20000, 'No se permite borrar registros de pagos.');
END;
/

CREATE OR REPLACE TRIGGER disConfirmarReservaAlRegistrarPago
	AFTER INSERT ON Payment
    REFERENCING NEW AS new
    FOR EACH ROW
BEGIN
	UPDATE RoomBooking rb SET rb.status = 'Confirmed'
        WHERE rb.bookingId = :new.bookingId;
END;
/

DROP FUNCTION fnIdPersonaUsuario;
DROP FUNCTION fnCalcularPagoTotalReserva;

DROP PROCEDURE paEliminarAlojamiento;
DROP PROCEDURE paCambiarEstadoReservacion;

DROP VIEW vUsuariosPersonas;
DROP VIEW vHoteles;

DROP TRIGGER disConfirmarReservaAlRegistrarPago;
DROP TRIGGER disProhibirBorradoPago;

DROP TABLE Payment CASCADE CONSTRAINTS;
DROP TABLE PaymentInformation CASCADE CONSTRAINTS;
DROP TABLE Room CASCADE CONSTRAINTS;
DROP TABLE RoomTypePhoto CASCADE CONSTRAINTS;
DROP TABLE RoomType CASCADE CONSTRAINTS;
DROP TABLE RoomBooking CASCADE CONSTRAINTS;
DROP TABLE Booking CASCADE CONSTRAINTS;
DROP TABLE LodgingPhoneNumber CASCADE CONSTRAINTS;
DROP TABLE LodgingPerk CASCADE CONSTRAINTS;
DROP TABLE Perk CASCADE CONSTRAINTS;
DROP TABLE LodgingPhoto CASCADE CONSTRAINTS;
DROP TABLE Lodging CASCADE CONSTRAINTS;
DROP TABLE PersonPhoneNumber CASCADE CONSTRAINTS;
DROP TABLE Person CASCADE CONSTRAINTS;
DROP TABLE "User" CASCADE CONSTRAINTS;
DROP TABLE UserRole CASCADE CONSTRAINTS;