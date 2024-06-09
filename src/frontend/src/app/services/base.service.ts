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
        let headers = new HttpHeaders();
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const options = { headers, body, observe: "response" as "body" };
        return this._http.delete<any>(this.urlAPI + route, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }
    
    post(route: string, requiresToken: boolean, body: any): Observable<AppResponse> {
        let headers = new HttpHeaders();
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const options = { headers, observe: "response" as "body" };
        return this._http.post<any>(this.urlAPI + route, body, options).pipe(
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

    postFiles(route: string, requiresToken: boolean, files: File[]): Observable<AppResponse> {
        let headers = new HttpHeaders();
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const formData: FormData = new FormData();
        
        for (const file of files) {
            formData.append("files", file);
        }

        const options = { headers, observe: "response" as "body" };
        return this._http.post<any>(this.urlAPI + route, formData, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }

    deleteImages(route: string, requiresToken: boolean, imageFileNames: string[]): Observable<AppResponse> {
        let headers = new HttpHeaders();
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const options = { headers, body: imageFileNames, observe: "response" as "body" };
        return this._http.delete<any>(this.urlAPI + route, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }

    patch(route: string, requiresToken: boolean, body: any): Observable<AppResponse> {
        let headers = new HttpHeaders();
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const options = { headers, observe: "response" as "body" };
        return this._http.patch<any>(this.urlAPI + route, body, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }

    put(route: string, requiresToken: boolean, body: any): Observable<AppResponse> {
        let headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded'});
        headers = this.appendTokenIfNeeded(requiresToken, headers);

        const realBody = new URLSearchParams();
        realBody.set("data", JSON.stringify(body));

        const options = { headers, observe: "response" as "body" };
        return this._http.put<any>(this.urlAPI + route, realBody, options).pipe(
            map(this.handleAppResponse),
            catchError(this.handleError)
        );
    }

    private handleAppResponse<T>(response: any) {
        response.ok = response.status >= 200 && response.status <= 299;

        if (response.ok) {
            return response;
        }
        
        return response.error as T;
    }

    private handleError<T>(response: any, caught: Observable<T>) {
        // TODO: What if it's a 500 error?
        return of(response as T);
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