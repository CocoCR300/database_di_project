export class AppResponse
{
    constructor(
        public body: ResponseBody | any,
        public error: any,
        public ok: boolean,
        public status: number,
    )
    { }

    public static *getErrors(response: AppResponse) {
        const error = response.error;
        
        if (error.message) {
            yield error.message;
        }

        const errorMessagesByPropertyName = Object.entries<any>(error.errors);
        for (const [, messages] of errorMessagesByPropertyName) {
            for (const message of messages) {
                yield message;
            }
        }
    }
}

export class ResponseBody
{
    public constructor(
        message: string | null,
        values: any
    ) { }
}