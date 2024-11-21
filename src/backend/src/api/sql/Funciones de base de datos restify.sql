USE restify;

--
-- ESCALARES
--

--
-- fnIdPersonaUsuario
--
IF OBJECT_ID('dbo.fnIdPersonaUsuario') IS NOT NULL
    DROP FUNCTION dbo.fnIdPersonaUsuario;
GO

CREATE FUNCTION fnIdPersonaUsuario
    (@userName VARCHAR(50))
	RETURNS INT
AS BEGIN
	DECLARE @personId INT;
    SELECT @personId = personId FROM Person WHERE userName = @userName;

	RETURN @personId;
END
GO

--
-- fnCalcularPagoTotalReserva
--
IF OBJECT_ID('dbo.fnCalcularPagoTotalReserva') IS NOT NULL
    DROP FUNCTION dbo.fnCalcularPagoTotalReserva;
GO

CREATE FUNCTION fnCalcularPagoTotalPagoReserva
(
	@bookingId INT
)
RETURNS DECIMAL(18, 2)
AS BEGIN
	DECLARE @totalAmount DECIMAL(18, 0);
	SELECT @totalAmount = SUM(cost - discount + fees) FROM RoomBooking
		WHERE bookingId = @bookingId;

	RETURN @totalAmount;
END

GO

--
-- TABLA (VARIAS INSTRUCCIONES)
--

--
-- fnHabitacionesDisponiblesAlojamiento
--
IF OBJECT_ID('dbo.fnHabitacionesDisponiblesAlojamiento') IS NOT NULL
    DROP FUNCTION dbo.fnHabitacionesDisponiblesAlojamiento;
GO

CREATE FUNCTION fnHabitacionesDisponiblesAlojamiento
(
	@lodgingId  INT,
	@roomTypeId INT,
	@startDate  DATE,
	@endDate    DATE
)
RETURNS @HabitacionesPorAlojamiento TABLE
(
	RoomNumber INT
)
AS BEGIN
	INSERT @HabitacionesPorAlojamiento
		SELECT DISTINCT r.roomNumber FROM Room AS r
			LEFT JOIN RoomBooking AS rb
				ON  rb.roomNumber = r.roomNumber
				AND r.lodgingId  = @lodgingId
				AND (rb.status = 'Created' OR rb.status = 'Confirmed')
				AND (rb.startDate < @endDate AND rb.endDate > @startDate)
			WHERE   r.lodgingId = @lodgingId
					AND r.roomTypeId = @roomTypeId
					AND rb.roomBookingId IS NULL;
	RETURN
END

GO

--
-- fnReservacionesPorEstado
--
IF OBJECT_ID('dbo.fnReservacionesPorEstado') IS NOT NULL
    DROP FUNCTION dbo.fnReservacionesPorEstado;
GO

CREATE FUNCTION fnReservacionesPorEstado
(
	@status  char(50)
)
RETURNS @ReservacionesPorEstado TABLE
(
	roomBookingId	INT 		NOT NULL,
	bookingId 		INT 		NOT NULL,
	lodgingId 		INT 		NOT NULL,
	roomNumber 		INT 		NOT NULL,
	cost			DECIMAL 	NOT NULL,
	fees			DECIMAL 	NOT NULL,
	discount		DECIMAL		NOT NULL,
	status			CHAR(50)	NOT NULL,
	startDate 		DATE		NOT NULL,
	endDate 		DATE 		NOT NULL
)
AS BEGIN
	INSERT @ReservacionesPorEstado
	SELECT * FROM RoomBooking WHERE status = @status
	RETURN
END

GO

--
-- TABLA (EN LINEA)
--

--
-- fnReservacionesUsuario
--
IF OBJECT_ID('dbo.fnReservacionesUsuario') IS NOT NULL
	DROP FUNCTION dbo.fnReservacionesUsuario;
GO

CREATE FUNCTION fnReservacionesUsuario
(
    @userName VARCHAR(50),
    @status CHAR(50) = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        RoomBooking.roomBookingId,
        RoomBooking.bookingId AS roomBookingBookingId,
        RoomBooking.status AS roomBookingStatus,
        Booking.bookingId AS bookingBookingId,
        Person.personId,
        Person.userName,
        Person.firstName,
        Person.lastName
    FROM RoomBooking 
    JOIN Booking ON Booking.bookingId = RoomBooking.bookingId
    JOIN Person ON Person.personId = Booking.customerPersonId
    WHERE Person.userName = @userName
    AND (@status IS NULL OR RoomBooking.status = @status)
);

GO

--
-- fnObtenerInformacionPagoUsuario
--
IF OBJECT_ID('dbo.fnObtenerInformacionPagoUsuario') IS NOT NULL
	DROP FUNCTION dbo.fnObtenerInformacionPagoUsuario;
GO

CREATE FUNCTION fnObtenerInformacionPagoUsuario
	(@userName VARCHAR(50))
	RETURNS TABLE
AS RETURN
(
	SELECT * FROM PaymentInformation
		WHERE personId = dbo.fnIdPersonaUsuario(@userName)
);

GO