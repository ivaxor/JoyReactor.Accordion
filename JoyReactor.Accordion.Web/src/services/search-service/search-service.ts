import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchResponse } from './search-response';
import { HttpClient } from '@angular/common/http';
import { SearchDownloadRequest } from './search-download-request';
import { ConfigService } from '../config-service/config-service';

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  private configService = inject(ConfigService);
  private http = inject(HttpClient);

  searchMediaByUrl(url: string): Observable<SearchResponse[]> {
    const request: SearchDownloadRequest = { pictureUrl: url };
    return this.http.post<SearchResponse[]>(`${this.configService.config!.apiRoot}/search/pictures/download`, request);
  }
}