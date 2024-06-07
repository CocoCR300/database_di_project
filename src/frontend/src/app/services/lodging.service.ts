import { Injectable } from "@angular/core";
import { Observable, of } from "rxjs";
import { BaseService } from "./base.service";
import { Lodging } from "../models/lodging";
import { AppResponse } from "../models/app_response";
import { Booking } from "../models/booking";

@Injectable({
    providedIn:'root'
})
export class LodgingService extends BaseService
{
    getLodgingBookings(lodgingId: number): Observable<Booking[]> {
        return this.get(`lodging/${lodgingId}/booking`, true);
    }

    deleteBookings(bookingIds: number[]): Observable<AppResponse> {
        return this.delete("booking", true, bookingIds);
    }

    getLessorLodgings(lessorId: number): Observable<Lodging[]> {
        return this.get<Lodging[]>(`lessor/${lessorId}/lodging`, true);
    }

    getLodging(lodgingId: number): Observable<Lodging> {
        return this.get(`lodging/${lodgingId}`);
    }

    getLodgings(pageSize: number, pageNumber: number): Observable<Lodging[]> {
        return this.get<Lodging[]>(`lodging/${pageSize}/${pageNumber}`);
    }

    deleteLodging(lodgingId: number): Observable<AppResponse> {
        return this.delete(`lodging/${lodgingId}`, true, null);
    }

    saveLodging(lodging: Lodging): Observable<AppResponse> {
        // TODO
        const lodgingTrimmed = {
            lodging_id: lodging.id,
            ownerId: lodging.ownerId,
            address: lodging.address,
            name: lodging.name,
            description: lodging.description,
        };

        return this.post("lodging", true, lodgingTrimmed);
    }

    updateLodging(lodging: Lodging): Observable<AppResponse> {
        // TODO
        const lodgingTrimmed = {
            lodging_id: lodging.id,
            address: lodging.address,
            name: lodging.name,
            description: lodging.description,
        };

        return this.put("lodging", true, lodgingTrimmed);
    }

    saveLodgingImage(lodgingId: number, imageFile: File) {
        return this.postFile(`lodging/${lodgingId}/image`, true, imageFile);
    }
}