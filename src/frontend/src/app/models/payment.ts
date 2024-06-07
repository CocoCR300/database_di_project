export class Payment
{
    public constructor(
        public id:                      number,
        public dateAndTime:             string,
        public amount:                  number,
        public invoiceImageFileName:    string,
        public bookingId:               number | null,
    ) { } 
}