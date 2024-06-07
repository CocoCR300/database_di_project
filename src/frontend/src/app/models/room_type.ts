import { Photo } from "./photo";

export class RoomType
{
    public constructor(
        public id:              number,
        public fees:            number,
        public perNightPrice:   number,
        public capacity:        number,
        public lodgingId:       number,
        public name:            string,
        public photos:          Photo[]
    ) { }
}