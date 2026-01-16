import { inject, Injectable } from '@angular/core';
import { ConfigService } from '../../../services/config-service/config-service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CrawlerTaskResponse } from './crawler-task-response';

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