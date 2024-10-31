export class PaymentInformation
{
    constructor(
        public id:                  number,
        public cardNumber:          string,
        public cardSecurityCode:    string,
        public cardExpiryDate:      Date,
        public cardHolderName:      string
    ) { }
}