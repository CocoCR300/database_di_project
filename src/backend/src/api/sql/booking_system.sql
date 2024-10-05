-- In MS SQL Server NO ACTION works the same way as RESTRICT on MySQL
-- Look for "Delete Rule": https://learn.microsoft.com/en-us/sql/relational-databases/tables/modify-foreign-key-relationships?view=sql-server-ver16#SSMSProcedure

IF DB_ID('restify') IS NULL
	CREATE DATABASE restify
	    ON
        (
            NAME = N'restify',
            FILENAME = N'A:\Microsoft SQL Server Database Files\Data\restify.mdf',
            SIZE = 1GB,
            FILEGROWTH = 256MB,
            MAXSIZE = UNLIMITED
        )
        LOG ON
        (
            NAME = N'restify_log',
            FILENAME = N'A:\Microsoft SQL Server Database Files\Logs\restify_LOG.ldf',
            SIZE = 200MB,
            FILEGROWTH = 50MB,
            MAXSIZE = 1GB
		)
	COLLATE Modern_Spanish_CI_AS;

GO

USE restify;

GO

-- Tabla "Rol de usuario"
IF OBJECT_ID('UserRole') IS NULL
	CREATE TABLE UserRole (
		userRoleId	INT 		NOT NULL IDENTITY(1,1),
		type		VARCHAR(50)	NOT NULL,

		PRIMARY KEY (userRoleId),

		CONSTRAINT CK_UserRole_notEmpty CHECK (LEN(type) > 0),
  
		CONSTRAINT UNIQUE_UserRole_type UNIQUE (type)
	);

GO


-- Tabla "Usuario"
IF OBJECT_ID('User') IS NULL
	CREATE TABLE [User] (
		userName	VARCHAR(50)		NOT NULL,
		userRoleId	INT 			NOT NULL,
		password	VARCHAR(100)	NOT NULL,
  
		PRIMARY KEY (userName),

		CONSTRAINT CK_User_notEmpty CHECK
		(
			LEN(userName) > 0 AND LEN(password) > 0
		),
  
		CONSTRAINT UNIQUE_User_userName UNIQUE (userName),
  
		CONSTRAINT FK_USER_USER_ROLE FOREIGN KEY (userRoleId) REFERENCES UserRole (userRoleId)
			ON DELETE NO ACTION
			ON UPDATE CASCADE
	);

GO


-- Tabla "Persona"
IF OBJECT_ID('Person') IS NULL
	CREATE TABLE Person (
		personId		INT 			NOT NULL    IDENTITY,
		userName 		VARCHAR(50)		NOT NULL,
		firstName 		VARCHAR(50) 	NOT NULL,
		lastName 		VARCHAR(100) 	NOT NULL,
		emailAddress 	VARCHAR(200) 	NOT NULL,
  
		PRIMARY KEY (personId),

		CONSTRAINT CK_Person_notEmpty CHECK
		(
			LEN(userName)		> 0 AND
			LEN(firstName)		> 0 AND
			LEN(lastName)		> 0 AND
			LEN(emailAddress)	> 0
		),
  
		CONSTRAINT UNIQUE_Person_userName UNIQUE (userName),
  
		CONSTRAINT FK_PERSON_USER FOREIGN KEY (userName) REFERENCES [User] (userName)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Número de telefono de persona"
IF OBJECT_ID('PersonPhoneNumber') IS NULL
	CREATE TABLE PersonPhoneNumber (
		personId 	INT 		NOT NULL,
		phoneNumber CHAR(30)	NOT NULL,
  
		INDEX FK_INDEX_PERSON_PHONE_NUMBER (personId),

		CONSTRAINT CK_PersonPhoneNumber_notEmpty CHECK (LEN(phoneNumber) > 0),
  
		CONSTRAINT FK_PERSON_PHONE_NUMBER	FOREIGN KEY (personId) REFERENCES Person (personId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Alojamiento"
IF OBJECT_ID('Lodging') IS NULL
	CREATE TABLE Lodging (
		lodgingId 		INT 			NOT NULL IDENTITY,
		ownerPersonId	INT 			NOT NULL,
		lodgingType 	CHAR(50) 		NOT NULL,
		name 			VARCHAR(100)	NOT NULL,
		address 		VARCHAR(300)	NOT NULL,
		description 	VARCHAR(1000)	NOT NULL,
		emailAddress	VARCHAR(200)	NOT NULL,
  
		PRIMARY KEY (lodgingId),
  
		INDEX FK_INDEX_LODGING_PERSON (ownerPersonId),

		CONSTRAINT CK_Lodging_notEmpty CHECK
		(
			LEN(lodgingType)	> 0 AND
			LEN(name)			> 0 AND
			LEN(address)		> 0 AND
			LEN(description)	> 0 AND
			LEN(emailAddress)	> 0
		),
  
		CONSTRAINT FK_LODGING_PERSON FOREIGN KEY (ownerPersonId) REFERENCES Person (personId)
			ON DELETE NO ACTION
			ON UPDATE CASCADE
	);

GO

-- Tabla "Número de telefono de alojamiento"
IF OBJECT_ID('LodgingPhoneNumber') IS NULL
	CREATE TABLE LodgingPhoneNumber (
	  lodgingId		INT 		NOT NULL,
	  phoneNumber	CHAR(30)	NOT NULL,
  
	  INDEX FK_INDEX_LODGING_PHONE_NUMBER (lodgingId),

	  CONSTRAINT CK_LodgingPhoneNumber_notEmpty CHECK (LEN(phoneNumber) > 0),
  
	  CONSTRAINT FK_LODGING_PHONE_NUMBER FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
		ON DELETE CASCADE
		ON UPDATE CASCADE
	);

GO

-- Tabla "Foto de alojamiento"
IF OBJECT_ID('LodgingPhoto') IS NULL
	CREATE TABLE LodgingPhoto (
		lodgingId	INT			NOT NULL,
		fileName	VARCHAR(75)	NOT NULL,
		ordering	TINYINT		NOT NULL,
	
		INDEX FK_INDEX_LODGING_LODGING_PHOTO (lodgingId),

		CONSTRAINT CK_LodgingPhoto_notEmpty CHECK (LEN(fileName) > 0),

		CONSTRAINT UNIQUE_LodgingPhoto_fileName UNIQUE (fileName),
	
		CONSTRAINT FK_LODGING_LODGING_PHOTO	FOREIGN KEY (lodgingId)	REFERENCES Lodging (lodgingId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Beneficio adicional"
IF OBJECT_ID('Perk') IS NULL
	CREATE TABLE Perk (
		perkId	INT			NOT NULL	IDENTITY,
		name	VARCHAR(50)	NOT NULL,
	
		PRIMARY KEY (perkId),

		CONSTRAINT CK_Perk_notEmpty CHECK (LEN(name) > 0),
	
		CONSTRAINT UNIQUE_Perk_name UNIQUE (name),
	);

GO

-- Tabla "Alojamiento beneficio adicional"
IF OBJECT_ID('LodgingPerk') IS NULL
	CREATE TABLE LodgingPerk (
		lodgingId	INT	NOT NULL,
		perkId		INT	NOT NULL,
	
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
	  bookingId 		INT NOT NULL IDENTITY,
	  customerPersonId 	INT NOT NULL,
	  lodgingId 		INT NOT NULL,
  
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

-- Tabla "Información de pago"
IF OBJECT_ID('PaymentInformation') IS NULL
	CREATE TABLE PaymentInformation (
		paymentInformationId	INT				NOT NULL IDENTITY(1, 1),
		personId				INT				NOT NULL,
		cardNumber				CHAR(16)		NOT NULL,
		cardExpiryDate			DATE			NOT NULL,
		cardSecurityCode		CHAR(4)			NOT NULL,
		cardHolderName			VARCHAR(100)	NOT NULL

		PRIMARY KEY (paymentInformationId),

		INDEX FK_INDEX_PAYMENT_INFORMATION_PERSON (personId),

		CONSTRAINT CK_PaymentInformation_validCardDetails CHECK
		(
			LEN(cardNumber)			> 0 AND cardNumber			NOT LIKE '%[^0-9]%' AND
			LEN(cardSecurityCode)	> 0 AND cardSecurityCode	NOT LIKE '%[^0-9]%' AND
			LEN(cardHolderName)		> 0 AND
			ABS(DATEDIFF(day, GETDATE(), cardExpiryDate)) >= 0
		),

		CONSTRAINT FK_PAYMENT_INFORMATION_PERSON FOREIGN KEY (personId)
			REFERENCES Person (personId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Pago"
IF OBJECT_ID('Payment') IS NULL
	CREATE TABLE Payment (
		paymentId 				INT 		NOT NULL IDENTITY(1, 1),
		bookingId 				INT,
		paymentInformationId	INT,
		amount					DECIMAL		NOT NULL,
		dateAndTime 			DATETIME	NOT NULL,
  
		PRIMARY KEY (paymentId),

		INDEX FK_INDEX_PAYMENT_BOOKING (bookingId),
		INDEX FK_INDEX_PAYMENT_PAYMENT_INFORMATION (paymentInformationId),

		CONSTRAINT CK_Payment_amountMoreEqualZero CHECK (amount >= 0),
  
		CONSTRAINT FK_PAYMENT_BOOKING FOREIGN KEY (bookingId) REFERENCES Booking (bookingId)
			ON DELETE SET NULL
			ON UPDATE CASCADE,

		CONSTRAINT FK_PAYMENT_PAYMENT_INFORMATION FOREIGN KEY (paymentInformationId)
			REFERENCES PaymentInformation (paymentInformationId)
			ON DELETE SET NULL
			ON UPDATE NO ACTION -- Cycles or multiple cascade paths if CASCADE
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

		CONSTRAINT CK_RoomType_notEmpty CHECK (LEN(name) > 0),
		CONSTRAINT CK_RoomType_amountsGreaterEqualZero CHECK
		(
			perNightPrice	>	0	AND
			fees			>=	0	AND
			capacity		>	0
		),
  
		CONSTRAINT FK_LODGING_ROOM_TYPE FOREIGN KEY (lodgingId) REFERENCES Lodging (lodgingId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Foto de tipo de habitación"
IF OBJECT_ID('RoomTypePhoto') IS NULL
	CREATE TABLE RoomTypePhoto (
		roomTypeId	INT		 NOT NULL,
		fileName	CHAR(75) NOT NULL,
		ordering	TINYINT	 NOT NULL,
	
		INDEX FK_INDEX_ROOM_TYPE_ROOM_TYPE_PHOTO (roomTypeId),

		CONSTRAINT CK_RoomTypePhoto_notEmpty CHECK (LEN(fileName) > 0),

		CONSTRAINT UNIQUE_RoomTypePhoto_fileName UNIQUE (fileName),
  
		CONSTRAINT FK_ROOM_TYPE_ROOM_TYPE_PHOTO FOREIGN KEY (roomTypeId) REFERENCES RoomType (roomTypeId)
			ON DELETE CASCADE
			ON UPDATE CASCADE
	);

GO

-- Tabla "Habitación"
IF OBJECT_ID('Room') IS NULL
	CREATE TABLE Room (
	  roomNumber	INT NOT NULL,
	  lodgingId		INT NOT NULL,
	  roomTypeId	INT	NOT NULL,
  
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
		roomBookingId	INT 		NOT NULL IDENTITY,
		bookingId 		INT 		NOT NULL,
		lodgingId 		INT 		NOT NULL,
		roomNumber 		INT 		NOT NULL,
		cost			DECIMAL 	NOT NULL,
		fees			DECIMAL 	NOT NULL,
		discount		DECIMAL		NOT NULL,
		status			CHAR(50)	NOT NULL,
		startDate 		DATE		NOT NULL,
		endDate 		DATE 		NOT NULL,
  
		PRIMARY KEY (roomBookingId),
  
		INDEX FK_INDEX_ROOM_ROOM_BOOKING (roomNumber),
		INDEX FK_INDEX_BOOKING_ROOM_BOOKING (bookingId),
		INDEX FK_INDEX_LODGING_ROOM_BOOKING (lodgingId),

		CONSTRAINT CK_RoomBooking_amountsGreaterEqualZero CHECK
		(
			cost		>= 0 AND
			fees		>= 0 AND
			discount	>= 0
		),
		CONSTRAINT CK_RoomBooking_notEmpty	CHECK (LEN(status) > 0),
		CONSTRAINT CK_RoomBookng_validDates CHECK
		(
			DATEDIFF(day, GETDATE(), startDate) <= 0 AND
			DATEDIFF(day, startDate, endDate)	<= 0
		),
  
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


--
-- Procedimientos almacenados
--

IF OBJECT_ID('paCrearReservacion') IS NOT NULL
    DROP PROCEDURE paCrearReservacion; 
GO

IF OBJECT_ID('paActualizarOrdenFotos') IS NOT NULL
    DROP PROCEDURE paActualizarOrdenFotos;
GO

IF TYPE_ID('IdList') IS NOT NULL
    DROP TYPE IdList;

IF TYPE_ID('PhoneNumberList') IS NOT NULL
    DROP TYPE PhoneNumberList;

IF TYPE_ID('PhotoList') IS NOT NULL
    DROP TYPE PhotoList;

IF TYPE_ID('RoomBookingList') IS NOT NULL
    DROP TYPE RoomBookingList;

IF TYPE_ID('RoomTypeList') IS NOT NULL
    DROP TYPE RoomTypeList;

IF TYPE_ID('RoomList') IS NOT NULL
    DROP TYPE RoomList;
GO

-- https://stackoverflow.com/a/42451702
-- https://stackoverflow.com/a/33773336
CREATE TYPE IdList AS TABLE
(
    id INT NOT NULL
);

CREATE TYPE PhoneNumberList AS TABLE
(
    phoneNumber CHAR(30) NOT NULL
);

CREATE TYPE PhotoList AS TABLE
(
    fileName VARCHAR(75)    NOT NULL,
    ordering TINYINT        NOT NULL
);

CREATE TYPE RoomBookingList AS TABLE
(
    roomNumber  INT     NOT NULL,
    cost        DECIMAL NOT NULL,
    fees        DECIMAL NOT NULL,
    discount    DECIMAL NOT NULL,
    startDate   DATE    NOT NULL,
    endDate     DATE    NOT NULL
);

CREATE TYPE RoomTypeList AS TABLE
(
    name            VARCHAR(75) NOT NULL,
    perNightPrice   DECIMAL     NOT NULL,
    fees            DECIMAL     NOT NULL,
    capacity        INT         NOT NULL
);

CREATE TYPE RoomList AS TABLE
(
    roomNumber INT NOT NULL,
    roomTypeId INT NOT NULL
);
GO

--
-- Procedimientos almacenados de Usuario
--

IF OBJECT_ID('paIdPersonaUsuario') IS NOT NULL
    DROP PROCEDURE paIdPersonaUsuario;
GO

CREATE PROCEDURE paIdPersonaUsuario
    @userName VARCHAR(50),
    @personId INT OUTPUT
AS BEGIN
    SELECT @personId = personId FROM Person WHERE userName = @userName;
END
GO

--
-- Procedimientos almacenados de Booking
--

-- Crea una reservación, que puede incluir múltiples habitaciones,
-- especificadas en una tabla pasada por el último parámetro 
CREATE PROCEDURE paCrearReservacion
    @userName		VARCHAR(50),
    @lodgingId		INT,
    @roomBookings	RoomBookingList READONLY,
	@bookingId		INT OUTPUT
AS BEGIN
    DECLARE @customerId INT;
	DECLARE @result TABLE (id INT);
    EXECUTE paIdPersonaUsuario @userName, @personId = @customerId OUTPUT;
	
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Booking (customerPersonId, lodgingId)
			OUTPUT INSERTED.bookingId INTO @result
			VALUES (@customerId, @lodgingId);
        SELECT @bookingId = id FROM @result;

        INSERT INTO RoomBooking
            (bookingId, lodgingId, roomNumber, cost, fees, discount, status, startDate, endDate)
            SELECT @bookingId, @lodgingId, roomNumber, cost * IIF(DATEDIFF(DAY, startDate, endDate) = 0, 1, DATEDIFF(DAY, startDate, endDate)), fees, discount, 'Created', startDate, endDate
                FROM @roomBookings;
        
        COMMIT TRANSACTION;
		RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('paReservacionesUsuario') IS NOT NULL
    DROP PROCEDURE paReservacionesUsuario;
GO

CREATE PROCEDURE paReservacionesUsuario
    @userName VARCHAR(50),
    @status CHAR(50) = NULL
AS BEGIN
    IF @status IS NULL
        SELECT * FROM RoomBooking AS rb
            JOIN Booking AS b ON b.bookingId = rb.bookingId 
            JOIN Person AS p ON p.personId = b.customerPersonId
            WHERE p.userName = @userName;
    ELSE
        SELECT * FROM RoomBooking AS rb
            JOIN Booking AS b ON b.bookingId = rb.bookingId 
            JOIN Person AS p ON p.personId = b.customerPersonId
            WHERE p.userName = @userName AND rb.status = @status;
END
GO

IF OBJECT_ID('paEliminarReservacionesCanceladasUsuario') IS NOT NULL
    DROP PROCEDURE paEliminarReservacionesCanceladasUsuario;
GO

CREATE PROCEDURE paEliminarReservacionesCanceladasUsuario
    @userName VARCHAR(50)
AS BEGIN
    DECLARE @personId INT;
    EXECUTE paIdPersonaUsuario @userName, @personId = @personId OUTPUT;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE rb FROM RoomBooking AS rb
            JOIN Booking AS b ON b.bookingId = rb.bookingId
            WHERE b.customerPersonId = @personId AND status = 'Cancelled';
        
        DELETE b FROM Booking AS b
            LEFT JOIN RoomBooking AS rb ON rb.bookingId = b.bookingId
            WHERE customerPersonId = @personId AND rb.roomBookingId IS NULL;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('paCambiarEstadoReservacion') IS NOT NULL
    DROP PROCEDURE paCambiarEstadoReservacion;
GO

CREATE PROCEDURE paCambiarEstadoReservacion
    @bookingId INT,
    @status CHAR(50)
AS BEGIN
    UPDATE RoomBooking SET status = @status
        WHERE bookingId = @bookingId;
END
GO

--
-- Procedimientos almacenados de Lodging y Room
--

IF OBJECT_ID('paCrearAlojamiento') IS NOT NULL
    DROP PROCEDURE paCrearAlojamiento;
GO

CREATE PROCEDURE paCrearAlojamiento
    @ownerId		INT,
    @lodgingType    CHAR(50),
    @name           VARCHAR(100),
    @address        VARCHAR(300),
    @description    VARCHAR(1000),
    @emailAddress   VARCHAR(200),
	@lodgingId		INT OUTPUT
AS BEGIN
	IF @lodgingType = 'Apartment' OR @lodgingType = 'VacationRental'
	BEGIN
		RETURN 1;
	END

    BEGIN TRY
        BEGIN TRANSACTION;

		DECLARE @result TABLE (id INT);
        INSERT INTO Lodging (ownerPersonId, lodgingType, name, address, description, emailAddress)
		    OUTPUT INSERTED.lodgingId INTO @result
            VALUES (@ownerId, @lodgingType, @name, @address, @description, @emailAddress);
		SELECT @lodgingId = id FROM @result;

        COMMIT TRANSACTION;
		RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('paCrearAlojamientoSinHabitaciones') IS NOT NULL
	DROP PROCEDURE paCrearAlojamientoSinHabitaciones;

GO

-- Crea un alojamiento, estableciendo un unico tipo de habitacion y habitacion
-- en el mismo, que seran usados para las reservaciones hechas en el alojamiento
CREATE PROCEDURE paCrearAlojamientoSinHabitaciones
	@ownerId		INT,
    @lodgingType    CHAR(50),
    @name           VARCHAR(100),
    @address        VARCHAR(300),
    @description    VARCHAR(1000),
    @emailAddress   VARCHAR(200),
	@perNightPrice	DECIMAL,
	@fees			DECIMAL,
	@capacity		INT,
	@lodgingId		INT OUTPUT
AS BEGIN
	IF @lodgingType <> 'Apartment' AND @lodgingType <> 'VacationRental'
	BEGIN
		RETURN 1;
	END

    BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @result IdList;
        INSERT INTO Lodging (ownerPersonId, lodgingType, name, address, description, emailAddress)
		    OUTPUT INSERTED.lodgingId INTO @result
            VALUES (@ownerId, @lodgingType, @name, @address, @description, @emailAddress);
		SELECT @lodgingId = id FROM @result;
        
		INSERT INTO RoomType (lodgingId, name, perNightPrice, fees, capacity)
			OUTPUT INSERTED.roomTypeId INTO @result
            VALUES (@lodgingId, 'Alojamiento', @perNightPrice, @fees, @capacity);

		DECLARE @roomTypeId INT;
		SELECT @roomTypeId = id FROM @result;
		INSERT INTO Room (roomNumber, roomTypeId, lodgingId)
			VALUES (0, @roomTypeId, @lodgingId);

        COMMIT TRANSACTION;
		RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID('paHabitacionesDisponiblesAlojamiento') IS NOT NULL
    DROP PROCEDURE paHabitacionesDisponiblesAlojamiento;
GO

-- Obtiene los números de habitación de un alojamiento y tipo especifico de
-- habitación, que no estén reservados en un periodo de tiempo determinado 
CREATE PROCEDURE paHabitacionesDisponiblesAlojamiento
    @lodgingId  INT,
    @roomTypeId INT,
    @startDate  DATE,
    @endDate    DATE
AS BEGIN
    SELECT DISTINCT r.roomNumber FROM Room AS r
        LEFT JOIN RoomBooking AS rb
            ON  rb.roomNumber = r.roomNumber
            AND r.lodgingId  = @lodgingId
            AND (rb.status = 'Created' OR rb.status = 'Confirmed')
            AND (rb.startDate < @endDate AND rb.endDate > @startDate)
        WHERE   r.lodgingId = @lodgingId
                AND r.roomTypeId = @roomTypeId
                AND rb.roomBookingId IS NULL;
END
GO

IF OBJECT_ID('paEliminarAlojamiento') IS NOT NULL
    DROP PROCEDURE paEliminarAlojamiento;
GO

CREATE PROCEDURE paEliminarAlojamiento
    @lodgingId INT
AS BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;

		DELETE FROM RoomBooking WHERE lodgingId = @lodgingId;

		DELETE FROM Booking     WHERE lodgingId = @lodgingId;

		DELETE FROM Room        WHERE lodgingId = @lodgingId;

		DELETE FROM RoomType    WHERE lodgingId = @lodgingId;

		DELETE FROM LodgingPerk WHERE lodgingId = @lodgingId;

		DELETE FROM LodgingPhoneNumber WHERE lodgingId = @lodgingId;

		DELETE FROM LodgingPhoto WHERE lodgingId = @lodgingId;

		DELETE FROM Lodging      WHERE lodgingId = @lodgingId;

		COMMIT TRANSACTION;
		RETURN 0;

	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW
	END CATCH
END
GO

-- Actualiza el campo orden de los registros de fotos en los que su nombre de archivo
-- coincidan con los de la tabla pasada a través del último parámetro
CREATE PROCEDURE paActualizarOrdenFotos
    @lodgingId  INT,
    @photos     PhotoList READONLY
AS BEGIN
    UPDATE LodgingPhoto SET LodgingPhoto.ordering = n.ordering
        FROM @photos AS lp
        JOIN @photos AS n ON n.fileName = lp.fileName
        WHERE LodgingPhoto.lodgingId = @lodgingId;
END
GO

--
-- Procedimientos almacenados de PaymentInformation
--

IF OBJECT_ID('paObtenerInformacionPagoUsuario') IS NOT NULL
	DROP PROCEDURE paObtenerInformacionPagoUsuario;
GO

CREATE PROCEDURE paObtenerInformacionPagoUsuario
	@userName VARCHAR(50)
AS BEGIN
	DECLARE @personId INT;
	EXEC paIdPersonaUsuario @userName, @personId OUTPUT;
	
	SELECT * FROM PaymentInformation
		WHERE personId = @personId;
END

GO

IF OBJECT_ID('paInsertarInformacionPago') IS NOT NULL
	DROP PROCEDURE paInsertarInformacionPago;
GO

CREATE PROCEDURE paInsertarInformacionPago
	@userName				VARCHAR(50),
	@cardNumber				CHAR(16),
	@cardExpiryDate			DATE,
	@cardHolderName			VARCHAR(100),
	@cardSecurityCode		CHAR(4),
	@paymentInformationId	INT OUTPUT
AS BEGIN
	DECLARE @existingPaymentInformationId INT;
	DECLARE @personId INT;

	EXEC paIdPersonaUsuario @userName, @personId OUTPUT;

	SELECT @existingPaymentInformationId = paymentInformationId
		FROM PaymentInformation
		WHERE cardNumber = @cardNumber AND personId = @personId;

	IF @existingPaymentInformationId IS NOT NULL
	BEGIN
		RETURN 1;
	END

	DECLARE @result TABLE (id INT);
	INSERT INTO PaymentInformation (personId, cardNumber, cardExpiryDate, cardHolderName, cardSecurityCode)
		OUTPUT INSERTED.paymentInformationId INTO @result
		VALUES (@personId, @cardNumber, @cardExpiryDate, @cardHolderName, @cardSecurityCode);

	SELECT @paymentInformationId = id FROM @result;
	RETURN 0;
END

GO

---
--- Procedimientos almacenados de Payment
---

IF OBJECT_ID('paRealizarPago') IS NOT NULL
    DROP PROCEDURE paRealizarPago;
GO

-- Realiza el pago, y modifica el estado de la reserva
CREATE PROCEDURE paRealizarPago
	@bookingId				INT,
	@paymentInformationId	INT,
	@paymentId				INT			OUTPUT,
	@paymentAmount			DECIMAL		OUTPUT,
	@dateAndTime			DATETIME	OUTPUT
AS BEGIN
	DECLARE @amount INT;
	DECLARE @paymentInformationPersonId INT = NULL;
	DECLARE @existingPaymentId INT = NULL;
	DECLARE @personId INT;

	SELECT @personId = customerPersonId FROM Booking WHERE bookingId = @bookingId;
	SELECT @amount = SUM(cost - discount + fees) FROM RoomBooking
		WHERE bookingId = @bookingId;
	SELECT @paymentInformationPersonId = personId FROM PaymentInformation
		WHERE paymentInformationId = @paymentInformationId;

	IF @personId <> @paymentInformationPersonId
	BEGIN
		RETURN 1;
	END

	SELECT @existingPaymentId = paymentId FROM Payment WHERE bookingId = @bookingId;

	IF @existingPaymentId IS NOT NULL
	BEGIN
		RETURN 2;
	END

	BEGIN TRY
		BEGIN TRANSACTION;
		DECLARE @result TABLE (id INT, amount DECIMAL, dateAndTime DATETIME);

		INSERT INTO Payment (bookingId, paymentInformationId, amount, dateAndTime)
			OUTPUT INSERTED.paymentId, INSERTED.amount, INSERTED.dateAndTime INTO @result
			VALUES (@bookingId, @paymentInformationId, @amount, GETDATE());

		UPDATE RoomBooking SET status = 'Confirmed' WHERE bookingId = @bookingId;
		SELECT @paymentId = id, @paymentAmount = amount, @dateAndTime = dateAndTime FROM @result;

		COMMIT TRANSACTION;
		RETURN 0;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END

GO

--
-- Procedimientos almacenados de Usuario y Persona
--
	
-- Ingresar un usuario en el sistema
IF OBJECT_ID('paInsertarUsuarioYPersona') IS NOT NULL
    DROP PROCEDURE paInsertarUsuarioYPersona;
GO

CREATE PROCEDURE paInsertarUsuarioYPersona
    @userName		VARCHAR(50),
    @userRoleId		INT,
    @password		VARCHAR(100),
    @firstName		VARCHAR(50),
    @lastName		VARCHAR(100),
    @emailAddress	VARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM [User] WHERE userName = @userName)
    BEGIN
        RETURN 1;
    END

    IF EXISTS (SELECT 1 FROM Person WHERE userName = @userName)
    BEGIN
        RETURN 2;
    END

	BEGIN TRY
        BEGIN TRANSACTION;
		INSERT INTO [User] (userName, userRoleId, password)
			VALUES (@userName, @userRoleId, @password);

        INSERT INTO Person (userName, firstName, lastName, emailAddress)
			VALUES (@userName, @firstName, @lastName, @emailAddress);

        COMMIT TRANSACTION;
		RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


--Actualizar el usuario
IF OBJECT_ID('paActualizarUsuarioYPersona') IS NOT NULL
    DROP PROCEDURE paActualizarUsuarioYPersona;
GO

CREATE PROCEDURE paActualizarUsuarioYPersona
    @userName			VARCHAR(50),
    @newPassword		VARCHAR(100),
    @newFirstName		VARCHAR(50),
    @newLastName		VARCHAR(100),
    @newEmailAddress	VARCHAR(200)
AS BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [User] WHERE userName = @userName)
    BEGIN
        RETURN 1;
    END

    IF NOT EXISTS (SELECT 1 FROM Person WHERE userName = @userName)
    BEGIN
        return 2;
    END

	BEGIN TRY
		BEGIN TRANSACTION;
		UPDATE [User] SET password = @newPassword WHERE userName = @userName;

        UPDATE Person SET	firstName		= @newFirstName,
							lastName		= @newLastName,
							emailAddress	= @newEmailAddress
            WHERE userName = @userName;

        COMMIT TRANSACTION;
		RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


--Eliminar un usuario
IF OBJECT_ID('paEliminarUsuarioYPersona') IS NOT NULL
    DROP PROCEDURE paEliminarUsuarioYPersona;
GO

CREATE PROCEDURE paEliminarUsuarioYPersona
    @userName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [User] WHERE userName = @userName)
    BEGIN
		RETURN 1;
    END

	DECLARE @personId INT;
	EXECUTE paIdPersonaUsuario @userName, @personId OUTPUT;

    IF @personId IS NULL
    BEGIN
		RETURN 2;
    END

	BEGIN TRY
		BEGIN TRANSACTION;

		DELETE rb FROM RoomBooking AS rb JOIN Booking AS b ON b.bookingId = rb.bookingId
			WHERE b.customerPersonId = @personId;

		DELETE FROM Booking WHERE customerPersonId = @personId;

		DELETE FROM PersonPhoneNumber	WHERE personId = @personId;
        DELETE FROM Person				WHERE userName = @userName;
        DELETE FROM [User]				WHERE userName = @userName;

        COMMIT TRANSACTION;
		RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO