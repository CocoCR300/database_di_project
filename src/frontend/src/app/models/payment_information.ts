export class PaymentInformation
{
    constructor(
        public id:                  number,
        public cardNumber:          number,
        public cardSecurityCode:    number,
        public cardExpiryDate:      Date,
        public cardHolderName:      string
    ) { }
}