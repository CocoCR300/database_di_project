export class AppResponse
{
    constructor(
        public body: ResponseBody | any,
        public errors: any[] | null,
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

export class ResponseBody
{
    public constructor(
        message: string | null,
        values: any
    ) { }
}