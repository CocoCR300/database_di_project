export class AppResponse
{
    constructor(
        public body: any,
        public errors: any[] | null,
        public message: string,
        public ok: boolean,
        public status: number,
    )
    { }

    public static *getErrors(response: AppResponse) {
        const errorMessagesByPropertyName = Object.entries(response.errors!);
        for (const [, messages] of errorMessagesByPropertyName) {
            for (const message of messages) {
                yield message;
            }
        }
    }
}