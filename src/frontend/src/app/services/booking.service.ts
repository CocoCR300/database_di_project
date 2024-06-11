import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BaseService } from "./base.service";
import { AppResponse } from "../models/app_response";
import { Booking } from "../models/booking";
import { AppState } from "../models/app_state";
import { HttpClient } from "@angular/common/http";
import { BookingRequestData } from "../models/booking-request-data";

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
}