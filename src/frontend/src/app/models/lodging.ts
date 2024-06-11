import { Perk } from "./perk";
import { Room } from "./room";
import { RoomType } from "./room_type";
import { User } from "./user";

export class Lodging {
    public roomTypeMaxPrice: number | null = null;
    public roomTypeMinPrice: number | null = null;

    constructor(
        public id:              number,
        public ownerId:         number,
        public type:            any ,
        public name:            string,
        public description:     string,
        public address:         string,
        public emailAddress:    string,
        public perNightPrice:   number | null,
        public fees:            number | null,
        public capacity:        number | null,
        public phoneNumbers:    string[] | null,
        public photos:          string[] | null,
        public perks:           Perk[] | null,
        public rooms:           Room[] | null,
        public roomTypes:       RoomType[] | null,
        public owner:           User | null)
        { }

    public static offersRooms(lodging: Lodging) {
        return this.typeOffersRooms(lodging.type);
    }

    public static typeOffersRooms(lodgingType: number) {
        return lodgingType != LodgingType.Apartment && lodgingType != LodgingType.VacationRental;
    }
}

export enum LodgingType
{
	Apartment,
	GuestHouse,
	Hotel,
	Lodge,
	Motel,
	VacationRental
}
