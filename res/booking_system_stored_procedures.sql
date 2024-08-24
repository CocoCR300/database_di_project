USE restify

IF OBJECT_ID('RoomBookingList') IS NOT NULL
    DROP TYPE RoomBookingList;
GO

-- https://stackoverflow.com/a/42451702
-- https://stackoverflow.com/a/33773336
CREATE TYPE RoomBookingList AS TABLE
(
    roomNumber INT NOT NULL,
    cost DECIMAL NOT NULL,
    fees DECIMAL NOT NULL,
    discount DECIMAL NOT NULL,
    startDate DATE NOT NULL,
    endDate DATE NOT NULL
);
GO

IF OBJECT_ID('pa_crear_reservacion') IS NOT NULL
    DROP PROCEDURE pa_crear_reservacion;
GO

CREATE PROCEDURE pa_crear_reservacion
    @userName VARCHAR(50),
    @lodgingId INT,
    @roomBookings RoomBookingList READONLY
AS BEGIN
    DECLARE @customerId INT;
    SET @customerId = (SELECT personId FROM Person WHERE userName = @userName);

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
    SET @personId = (SELECT personId FROM Person WHERE userName = @userName);
    
    DELETE rb FROM RoomBooking AS rb
        JOIN Booking AS b ON b.bookingId = rb.bookingId
        WHERE b.customerPersonId = @personId AND status = 'Cancelled';
    
    DELETE b FROM Booking AS b
        LEFT JOIN RoomBooking AS rb ON rb.bookingId = b.bookingId
        WHERE customerPersonId = @personId AND rb.roomBookingId IS NULL;
END
GO

-- TESTS
-- DECLARE @roomBookings RoomBookingList;
-- INSERT INTO @roomBookings VALUES
--     (101, 10000, 0, 0, '2000-02-01', '2000-02-01'),
--     (102, 20000, 10, 10, '2023-02-01', '2022-02-02');
-- EXECUTE pa_crear_reservacion 'customer', 1, @roomBookings;

-- EXEC pa_reservaciones_usuario 'customer';
EXEC pa_eliminar_reservaciones_canceladas_usuario 'customer';