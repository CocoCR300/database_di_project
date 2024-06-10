import { Injectable } from "@angular/core";
import { Observable, of } from "rxjs";
import { BaseService } from "./base.service";
import { Lodging } from "../models/lodging";
import { AppResponse } from "../models/app_response";
import { Booking } from "../models/booking";
import { Perk } from "../models/perk";
import { RoomType } from "../models/room_type";
import { Room } from "../models/room";

@Injectable({
    providedIn:'root'
})
export class LodgingService extends BaseService
{
    getPerks() {
        return this.get<Perk[]>("perk");
    }

    getLodgingBookings(lodgingId: number): Observable<Booking[]> {
        return this.get(`lodging/${lodgingId}/booking`, true);
    }

    getLodgingRooms(lodgingId: number): Observable<Room[]> {
        return this.get(`room/${lodgingId}`, false);
    }

    getLodgingRoomTypes(lodgingId: number): Observable<RoomType[]> {
        return this.get(`roomtype/${lodgingId}`, false);
    }

    deleteBookings(bookingIds: number[]): Observable<AppResponse> {
        return this.delete("booking", true, bookingIds);
    }

    getLessorLodgings(lessorUserName: string): Observable<Lodging[]> {
        return this.get<Lodging[]>(`lodging/lessor/${lessorUserName}`, false);
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
            id: lodging.id,
            address: lodging.address,
            name: lodging.name,
            description: lodging.description,
            type: lodging.type,
            emailAddress: lodging.emailAddress
        };

        return this.patch(`lodging/${lodging.id}`, true, lodgingTrimmed);
    }

    saveLodgingImages(lodgingId: number, images: File[]) {
        return this.postFiles(`lodging/${lodgingId}/photo`, true, images);
    }

    modifyLodgingImages(lodgingId: number, imagesData: any) {
        return this.patch(`lodging/${lodgingId}/photo`, true, imagesData);
    }

    deleteLodgingImages(lodgingId: number, imageFileNames: string[]) {
        return this.deleteImages(`lodging/${lodgingId}/photo`, true, imageFileNames);
    }

    addPerks(lodgingId: number, perkIds: number[]) {
        return this.post(`lodging/${lodgingId}/perk`, true, perkIds);
    }

    removePerks(lodgingId: number, perkIds: number[]) {
        return this.delete(`lodging/${lodgingId}/perk`, true, perkIds);
    }

    addPhoneNumbers(lodgingId: number, phoneNumbers: string[]) {
        return this.post(`lodging/${lodgingId}/phone_number`, true, phoneNumbers);
    }

    removePhoneNumbers(lodgingId: number, phoneNumbers: string[]) {
        return this.delete(`lodging/${lodgingId}/phone_number`, true, phoneNumbers);
    }

    addRoomType(lodgingId: number, roomType: RoomType) {
        return this.post(`roomtype/${lodgingId}`, true, roomType);
    }

    updateRoomType(lodgingId: number, roomType: RoomType) {
        return this.post(`roomtype/${lodgingId}/${roomType.id}`, true, roomType);
    }

    addRoomTypePhotos(lodgingId: number, roomTypeId: number, images: File[]) {
        return this.postFiles(`roomtype/${lodgingId}/${roomTypeId}/photo`, true, images);
    }

    deleteRoomTypePhotos(lodgingId: number, roomTypeId: number, imageFileNames: string[]) {
        return this.deleteImages(`roomtype/${lodgingId}/${roomTypeId}/photo`, true, imageFileNames);
    }
}