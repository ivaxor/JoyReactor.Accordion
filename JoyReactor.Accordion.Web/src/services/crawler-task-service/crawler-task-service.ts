import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CrawlerTaskResponse } from './crawler-task-response';
import { ConfigService } from '../config-service/config-service';

@Injectable({
  providedIn: 'root',
})
export class CrawlerTaskService {
  private configService = inject(ConfigService);
  private http = inject(HttpClient);

  get(): Observable<CrawlerTaskResponse[]> {
    return this.http.get<CrawlerTaskResponse[]>(`${this.configService.config!.apiRoot}/crawlerTasks`);
  }
}