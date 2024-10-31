import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseService } from "./base.service";
import { AppResponse } from "../models/app_response";
import { Booking, BookingStatus } from "../models/booking";
import { AppState } from "../models/app_state";
import { HttpClient } from "@angular/common/http";
import { BookingRequestData } from "../models/booking-request-data";
import { Payment } from "../models/payment";

@Injectable({
    providedIn:'root'
})
export class BookingService extends BaseService
{
    constructor(protected appState: AppState, protected http: HttpClient) {
        super(appState, http);
      }


    public postBooking(bookingRequestData: BookingRequestData): Observable<AppResponse> {
        return this.post("Booking", true, bookingRequestData);
    }

    public getBookingsByPersonId(userName: string): Observable<Booking[]> {
        return this.get<Booking[]>(`Booking/user/${userName}`, true);
    }

    public getLodgingBookings(lodgingId: number): Observable<Booking[]> {
        return this.get<Booking[]>(`Booking/lodging/${lodgingId}`, true);
    }

    public changeBookingStatus(userName: string, bookingId: number, bookingStatus: BookingStatus) {
      return this.patch(`booking/user/${userName}/${bookingId}/status`, true, bookingStatus);
    }

    public cancelBooking(userName: string, bookingId: number) {
      return this.changeBookingStatus(userName, bookingId, BookingStatus.Cancelled);
    }

    public confirmBooking(userName: string, bookingId: number) {
      return this.changeBookingStatus(userName, bookingId, BookingStatus.Confirmed);
    }

    public deleteBookings(customerUserName: string, bookingIds: number[]): Observable<AppResponse> {
      return this.delete(`booking/user/${customerUserName}`, true, bookingIds);
    }

    public payBooking(bookingId: number, paymentInformationId: number): Observable<AppResponse> {
      return this.post(`payment/${bookingId}`, true, { paymentInformationId });
    }
}
