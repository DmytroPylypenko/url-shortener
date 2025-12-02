import { Injectable } from '@angular/core';
import { environment } from '../../../../src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { CreateShortUrlRequest, CreateShortUrlResponse } from '../../models/create-short-url';
import { Observable } from 'rxjs';
import { UrlRecordListItem } from '../../models/url-record';
import { catchError, of } from 'rxjs';

/**
 * Provides communication with the backend API for all URL-related operations.
 * This service is used by the Angular URL table, detail view,
 * and admin/user management features.
 */
@Injectable({
  providedIn: 'root',
})
export class UrlsService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly publicApi = `${environment.apiUrl}/public/urls`;
  private readonly manageApi = `${environment.apiUrl}/manage/urls`;
  private readonly http = inject(HttpClient);

  /**
   * Retrieves a list of all URL records.
   *
   * @returns Observable emitting an array of {@link UrlRecordListItem}.
   */
  getAll(): Observable<UrlRecordListItem[]> {
    return this.http.get<UrlRecordListItem[]>(this.publicApi, { withCredentials: true });
  }

  /**
   * Retrieves detailed information about a specific URL record.
   *
   * @param id Unique identifier of the URL record.
   * @returns Observable emitting the URL details.
   */
  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.publicApi}/${id}`, { withCredentials: true });
  }

  /**
   * Creates a new shortened URL for the authenticated user.
   *
   * @param model Request payload containing the original long URL.
   * @returns Observable emitting the created short URL record.
   */
  create(model: CreateShortUrlRequest): Observable<CreateShortUrlResponse> {
    return this.http.post<CreateShortUrlResponse>(this.manageApi, model, { withCredentials: true });
  }

  /**
   * Creates a new shortened URL for the authenticated user.
   *
   * @param model Request payload containing the original long URL.
   * @returns Observable emitting the created short URL record.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.manageApi}/${id}`, { withCredentials: true });
  }

  /**
   * Checks if the user is currently authenticated by verifying the HTTP-only cookie.
   * 
   * @returns Observable emitting an object with the isAuthenticated flag.
   */
  getAuthStatus(): Observable<{ isAuthenticated: boolean }> {
    return this.http.get<{ isAuthenticated: boolean }>(
      `${this.apiUrl}/status`, { withCredentials: true }
    );
  }
}
