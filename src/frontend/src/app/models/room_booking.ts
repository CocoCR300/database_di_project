// src/app/models/room_booking.ts
export class RoomBooking {
    constructor(
        public id: number,
        public startDate: string,
        public endDate: string,
        public cost: number,
        public discount: number,
        public fees: number,
        public status: string,
        public bookingId: string,
        public lodgingId: number,
        public roomNumber: number,
        public booking: any, // Cambia esto si tienes un tipo específico para Booking
        public room: any // Cambia esto si tienes un tipo específico para Room
    ) {}
}
