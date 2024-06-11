import { Lodging } from "./lodging";
import { Payment } from "./payment";
import { RoomBooking } from "./room_booking";

export class Booking
{
    constructor(
        public id:                  number,
        public lodgingId:           number,
        public customerId:          number,
        public roomBookings:        RoomBooking[],
        public payment:             Payment | null,
        public customerUserName:    string,
        public lodging:             Lodging | null
    ) { }
}

export enum BookingStatus
{
    Created   = 0,
    Confirmed = 1,
    Cancelled = 2,
    Finished  = 3 
}