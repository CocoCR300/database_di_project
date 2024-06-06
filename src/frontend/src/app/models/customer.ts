export class Customer{
    constructor(
        public customer_id: number,
        public first_name: string,
        public last_name: string,
        public email_address: string,
        public phone_number: string,
        public user_name: string
    ) { }
}