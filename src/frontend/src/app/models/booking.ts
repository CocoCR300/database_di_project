export class Booking
{
    constructor(
        public booking_id: number,
        public lodging_id: number,
        public customer_id: number,
        public status_id: number,
        public start_date: string,
        public end_date: string
    ) { }
}

export enum BookingStatus
{
    Created   = 1,
    Confirmed = 2,
    Cancelled = 3,
    Finished  = 4
}