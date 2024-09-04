USE restify

IF OBJECT_ID('pa_crear_reservacion') IS NOT NULL
    DROP PROCEDURE pa_crear_reservacion; 
GO

IF OBJECT_ID('pa_crear_alojamiento') IS NOT NULL
    DROP PROCEDURE pa_crear_alojamiento;
GO

IF OBJECT_ID('pa_actualizar_orden_fotos') IS NOT NULL
    DROP PROCEDURE pa_actualizar_orden_fotos;
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
    phoneNumber INT NOT NULL
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

IF OBJECT_ID('pa_id_persona_usuario') IS NOT NULL
    DROP PROCEDURE pa_id_persona_usuario;
GO

CREATE PROCEDURE pa_id_persona_usuario
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
CREATE PROCEDURE pa_crear_reservacion
    @userName VARCHAR(50),
    @lodgingId INT,
    @roomBookings RoomBookingList READONLY
AS BEGIN
    DECLARE @customerId INT;
    EXECUTE pa_id_persona_usuario @userName, @personId = @customerId OUTPUT;

    INSERT INTO Booking (customerPersonId, lodgingId) VALUES (@customerId, @lodgingId);
    DECLARE @bookingId INT;
    -- https://stackoverflow.com/a/7917724
    -- https://learn.microsoft.com/en-us/sql/t-sql/functions/scope-identity-transact-sql?view=sql-server-ver16
    SET @bookingId = SCOPE_IDENTITY(); -- Should return the last ID generated in this scope (in this case, the stored procedure)

    INSERT INTO RoomBooking
        (bookingId, lodgingId, roomNumber, cost, fees, discount, status, startDate, endDate)
        SELECT @bookingId, @lodgingId, roomNumber, cost * IIF(DATEDIFF(DAY, startDate, endDate) = 0, 1, DATEDIFF(DAY, startDate, endDate)), fees, discount, 'Created', startDate, endDate
            FROM @roomBookings;
END
GO

IF OBJECT_ID('pa_reservaciones_usuario') IS NOT NULL
    DROP PROCEDURE pa_reservaciones_usuario;
GO

CREATE PROCEDURE pa_reservaciones_usuario
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

IF OBJECT_ID('pa_eliminar_reservaciones_canceladas_usuario') IS NOT NULL
    DROP PROCEDURE pa_eliminar_reservaciones_canceladas_usuario;
GO

CREATE PROCEDURE pa_eliminar_reservaciones_canceladas_usuario
    @userName VARCHAR(50)
AS BEGIN
    DECLARE @personId INT;
    EXECUTE pa_id_persona_usuario @userName, @personId = @personId OUTPUT;
    
    DELETE rb FROM RoomBooking AS rb
        JOIN Booking AS b ON b.bookingId = rb.bookingId
        WHERE b.customerPersonId = @personId AND status = 'Cancelled';
    
    DELETE b FROM Booking AS b
        LEFT JOIN RoomBooking AS rb ON rb.bookingId = b.bookingId
        WHERE customerPersonId = @personId AND rb.roomBookingId IS NULL;
END
GO

IF OBJECT_ID('pa_cambiar_estado_reservacion') IS NOT NULL
    DROP PROCEDURE pa_cambiar_estado_reservacion;
GO

CREATE PROCEDURE pa_cambiar_estado_reservacion
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

-- Crea un alojamiento, incluyendo varios tipos de habitación, números de habitación,
-- números de teléfono, fotos y beneficios que deben pasarse en una tabla en los parámetros
-- correspondientes
CREATE PROCEDURE pa_crear_alojamiento
    @ownerUsername  VARCHAR(50),
    @lodgingType    CHAR(50),
    @name           VARCHAR(100),
    @address        VARCHAR(300),
    @description    VARCHAR(1000),
    @emailAddress   VARCHAR(200),
    @roomTypes      RoomTypeList    READONLY,
    @rooms          RoomList        READONLY,
    @phoneNumbers   PhoneNumberList READONLY,
    @photos         PhotoList       READONLY,
    @perks          IdList          READONLY
AS BEGIN
    DECLARE @ownerId INT;
    EXECUTE pa_id_persona_usuario @ownerUsername, @personId = @ownerId OUTPUT;

    INSERT INTO Lodging (ownerPersonId, lodgingType, name, address, description, emailAddress)
        VALUES (@ownerId, @lodgingType, @name, @address, @description, @emailAddress);

    DECLARE @lodgingId INT;
    SET @lodgingId = SCOPE_IDENTITY();
    
    INSERT INTO RoomType (lodgingId, name, perNightPrice, fees, capacity)
        SELECT @lodgingId, name, perNightPrice, fees, capacity FROM @roomTypes;

    INSERT INTO Room (roomNumber, lodgingId, roomTypeId)
        SELECT roomNumber, @lodgingId, roomTypeId FROM @rooms;

    INSERT INTO LodgingPhoneNumber (lodgingId, phoneNumber)
        SELECT @lodgingId, phoneNumber FROM @phoneNumbers;

    INSERT INTO LodgingPhoto (lodgingId, fileName, ordering)
        SELECT @lodgingId, fileName, ordering FROM @photos;

    INSERT INTO LodgingPerk (lodgingId, perkId)
        SELECT @lodgingId, id FROM @perks;
END
GO

IF OBJECT_ID('pa_habitaciones_disponibles_alojamiento') IS NOT NULL
    DROP PROCEDURE pa_habitaciones_disponibles_alojamiento;
GO

-- Obtiene los números de habitación de un alojamiento y tipo especifico de
-- habitación, que no estén reservados en un periodo de tiempo determinado 
CREATE PROCEDURE pa_habitaciones_disponibles_alojamiento
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

IF OBJECT_ID('pa_eliminar_alojamiento') IS NOT NULL
    DROP PROCEDURE pa_eliminar_alojamiento;
GO

CREATE PROCEDURE pa_eliminar_alojamiento
    @lodgingId INT
AS BEGIN
    DELETE FROM RoomBooking WHERE lodgingId = @lodgingId;

    DELETE FROM Booking     WHERE lodgingId = @lodgingId;

    DELETE FROM Room        WHERE lodgingId = @lodgingId;

    DELETE FROM RoomType    WHERE lodgingId = @lodgingId;

    DELETE FROM LodgingPerk WHERE lodgingId = @lodgingId;

    DELETE FROM LodgingPhoneNumber WHERE lodgingId = @lodgingId;

    DELETE FROM LodgingPhoto WHERE lodgingId = @lodgingId;

    DELETE FROM Lodging      WHERE lodgingId = @lodgingId;
END
GO

-- Actualiza el campo orden de los registros de fotos en los que su nombre de archivo
-- coincidan con los de la tabla pasada a través del último parámetro
CREATE PROCEDURE pa_actualizar_orden_fotos
    @lodgingId  INT,
    @photos     PhotoList READONLY
AS BEGIN
    UPDATE LodgingPhoto SET LodgingPhoto.ordering = n.ordering
        FROM @photos AS lp
        JOIN @photos AS n ON n.fileName = lp.fileName
        WHERE LodgingPhoto.lodgingId = @lodgingId;
END
GO
---
--- Procedimientos almacenados de Payment
---

-- Realiza el pago, y modifica el estado de la reserva
CREATE PROCEDURE pa_realizar_pago
	@roomBookingId INT,
	@invoiceImageFileName VARCHAR (150)
AS BEGIN
	DECLARE @amount INT;
	SET @amount = (SELECT cost - discount + fees
	FROM RoomBooking WHERE roomBookingId = @roomBookingId);

	INSERT INTO Payment (bookingId, amount, dateAndTime, invoiceImageFileName) VALUES 
	(@roomBookingId, @amount, GETDATE(), @invoiceImageFileName);

	UPDATE RoomBooking SET status = 'Confirmed' WHERE @roomBookingId = roomBookingId;
END

-- Consulta sobre el estado
-- TESTS
-- DECLARE @roomBookings RoomBookingList;
-- INSERT INTO @roomBookings VALUES
--     (101, 10000, 0, 0, '2000-02-01', '2000-02-01'),
--     (102, 20000, 10, 10, '2023-02-01', '2022-02-02');
-- EXECUTE pa_crear_reservacion 'customer', 1, @roomBookings;

-- EXEC pa_reservaciones_usuario 'customer';
-- EXEC pa_eliminar_reservaciones_canceladas_usuario 'customer';
-- EXEC pa_cambiar_estado_reservacion 1, 'Confirmed';
EXEC pa_habitaciones_disponibles_alojamiento 1, 2, '2022-03-01', '2022-03-02';