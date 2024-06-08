export class TokenIdentity
{
    public constructor(
        public userName: string,
        public role: string,
        public roleId: number,
        public personId: number,
        public nbf: string, // Not before
        public exp: string,
        public iss: string, // Issuer
        public aud: string  // Audience
    ) { }
}