import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CrawlerTaskResponse } from '../../../services/crawler-task-service/crawler-task-response';

@Component({
  selector: 'app-crawler-task-info',
  imports: [],
  templateUrl: './crawler-task-info.html',
  styleUrl: './crawler-task-info.scss',
})
export class CrawlerTaskInfo implements OnChanges {
  @Input({ required: true }) crawlerTask!: CrawlerTaskResponse;
  percents: number | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    this.percents = this.crawlerTask?.pageLast && this.crawlerTask?.pageCurrent
      ? 100.0 / (this.crawlerTask.pageLast ?? 1.0) * (this.crawlerTask.pageCurrent ?? 0.0)
      : null;
  }
}