import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseService } from "./base.service";
import { AppResponse } from "../models/app_response";
import { Booking } from "../models/booking";
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
        return this.get<Booking[]>(`Booking/user/${userName}/10/1`, true);
    }

    public confirmBooking() {

    }

    public deleteBookings(customerUserName: string, bookingIds: number[]): Observable<AppResponse> {
      return this.delete(`booking/user/${customerUserName}`, true, bookingIds);
    }

    public payBooking(bookingId: number, dateAndTime: string, amount: number, invoiceImageFile: File): Observable<AppResponse> {
      const form = new FormData();
      form.append("dateAndTime", dateAndTime);
      form.append("amount", amount.toFixed(2));
      form.append("invoiceImageFile", invoiceImageFile);

      return this.post(`payment/${bookingId}`, true, form);
    }
}