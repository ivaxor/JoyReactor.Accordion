import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, filter, from, map, mergeMap, of, switchMap, take, tap } from 'rxjs';
import { HealthCheckResult } from './healthcheck-result';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '../config-service/config-service';
import { CrawlerTaskService } from '../crawler-task-service/crawler-task-service';

@Injectable({
  providedIn: 'root',
})
export class HealthCheckService {
  private http = inject(HttpClient);
  private configService = inject(ConfigService);
  private crawlerTaskService = inject(CrawlerTaskService);
  private resultsSubject = new BehaviorSubject<HealthCheckResult[] | null>(null);

  initialize(): void {
    this.configService.config$
      .pipe(
        filter(config => config !== null),
        mergeMap(config => this.http.get(`${config.apiRoot}/healthz`, { observe: 'response', responseType: 'text' })
          .pipe(
            map(response => {
              const result: HealthCheckResult = {
                name: 'JR Accordion',
                success: response.status === 200,
              };
              return result;
            }))))
      .subscribe(result => this.resultsSubject.next([...this.results ?? [], result]));

    /*
    this.crawlerTaskService.get()
      .pipe(
        map(tasks => Array.from(new Set(tasks.map(task => task.tag.api.hostName)))),
        switchMap(hostNames => from(hostNames)),
        mergeMap(hostName =>
          this.http.get(`https://${hostName}`, { observe: 'response', responseType: 'text' })
            .pipe(
              map(response => {
                const result: HealthCheckResult = {
                  name: hostName.replace('joyreactor', 'JoyReactor'),
                  success: response.status === 404,
                };
                return result;
              }),
              catchError(() => {
                const result: HealthCheckResult = {
                  name: hostName.replace('joyreactor', 'JoyReactor'),
                  success: false,
                };
                return of(result);
              }))))
      .subscribe(result => this.resultsSubject.next([...this.results ?? [], result]));
      */
  }

  results$ = this.resultsSubject.asObservable();
  get results(): HealthCheckResult[] | null {
    return this.resultsSubject.getValue();
  }
}