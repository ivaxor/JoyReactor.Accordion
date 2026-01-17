import { inject, Injectable } from '@angular/core';
import { ConfigService } from '../config-service/config-service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StatisticsResponse } from './statistics-response';

@Injectable({
  providedIn: 'root',
})
export class StatisticsService {
  private configService = inject(ConfigService);
  private http = inject(HttpClient);

  get(): Observable<StatisticsResponse> {
    return this.http.get<StatisticsResponse>(`${this.configService.config!.apiRoot}/statistics`);
  }
}