import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { server } from "./global";
import { Observable, catchError, last, map, of } from "rxjs";
import { AppResponse } from "../models/app_response";
import { AppState } from "../models/app_state";


@Injectable({
    providedIn:'root'
})
export class BaseService {
    protected urlAPI: string

    constructor(
        protected _appState: AppState,
        protected _http : HttpClient
    ){
        this.urlAPI = server.url
    }

    get<T>(route: string, requiresToken = false): Observable<T> {
        const headers = this.appendTokenIfNeeded(requiresToken, new HttpHeaders());
        const options = { headers, observe: "response" as "body" }; // This is stupid...

        return this._http.get(this.urlAPI + route, options).pipe(
            map((response: any) =>
                {
                    let value;
                    if (response.ok) {
                        if (response.body.totalPages) {
                            value = response.body.values as T;
                        }
                        else {
                            value = response.body as T;
                        }

                        return value;
                    }

                    throw new Error(response.message);
            }),
            catchError(this.handleError)
        );
    }

    delete(route: string, requiresToken: boolean, body: any): Observable<AppResponse> {
        let headers = new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded');
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        if (body != null) {
            const jsonBody = JSON.stringify(body);
            body = new URLSearchParams();
            body.set("data", jsonBody);
        }

        const options = { headers, body, observe: "response" as "body" };
        return this._http.delete<any>(this.urlAPI + route, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }
    
    post(route: string, requiresToken: boolean, body: any): Observable<AppResponse> {
        let headers = new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded');
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const realBody = new URLSearchParams();
        realBody.set("data", JSON.stringify(body));

        const options = { headers, observe: "response" as "body" };
        return this._http.post<any>(this.urlAPI + route, realBody, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }

    postFile(route: string, requiresToken: boolean, file: File): Observable<AppResponse> {
        let headers = new HttpHeaders();
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const formData: FormData = new FormData();
        formData.append("file", file, file.name);

        const options = { headers, observe: "response" as "body" };
        return this._http.post<any>(this.urlAPI + route, formData, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }

    put(route: string, requiresToken: boolean, body: any): Observable<AppResponse> {
        let headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded'});
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const realBody = new URLSearchParams()
        realBody.set("data", JSON.stringify(body));

        const options = { headers, observe: "response" as "body" };
        return this._http.put<any>(this.urlAPI + route, realBody, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }

    patch(route: string, requiresToken: boolean, username:string,
        first_name = '', last_name = '', email_address = '',
        phone_number = 0
    ){
        let headers = new HttpHeaders({ 'Content-Type':'application/x-www-form-urlencoded' });
        headers = this.appendTokenIfNeeded(requiresToken,headers);
        
        let realBody: URLSearchParams[] = []
        const bodyAux = new URLSearchParams();
        bodyAux.set("name", JSON.stringify(username));
        realBody.push(bodyAux);
        if(first_name!=''){
            bodyAux.set("first_name", JSON.stringify(first_name));
            realBody.push(bodyAux);
        }
        if(last_name!=''){
            bodyAux.set("last_name", JSON.stringify(last_name));
            realBody.push(bodyAux);
        }
        if(email_address!=''){
            bodyAux.set("email_address", JSON.stringify(email_address));
            realBody.push(bodyAux);
        }
        if(phone_number!=0){
            bodyAux.set("phone_number", JSON.stringify(phone_number));
            realBody.push(bodyAux);
        }

        const options = { headers, observe: "response" as "body" };
        return this._http.patch<any>(this.urlAPI + route, realBody, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        )
    }

    private handleAppResponse<T>(response: any) {
        if (response.ok) {
            return response;
        }
        
        return response.error as T;
    }

    private handleError<T>(response: any, caught: Observable<T>) {
        // TODO: What if it's a 500 error?
        return of(response.error as T);
    }

    private appendTokenIfNeeded(requiresToken: boolean, headers: HttpHeaders): HttpHeaders
    {
        if (requiresToken) {
            const bearerToken = this._appState.token;

            if (bearerToken) {
                headers = headers.set("Authorization", `Bearer ${bearerToken}`);
            }
        }

        return headers;
    }
}