import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CrawlerTaskInfo } from "../crawler-task-info/crawler-task-info";
import { CrawlerTaskResponse } from '../../../services/crawler-task-service/crawler-task-response';
import { CrawlerTaskService } from '../../../services/crawler-task-service/crawler-task-service';

@Component({
  selector: 'app-crawler-tasks',
  imports: [CrawlerTaskInfo],
  templateUrl: './crawler-tasks.html',
  styleUrl: './crawler-tasks.scss',
})
export class CrawlerTasks implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  private crawlerTaskService = inject(CrawlerTaskService);
  crawlerTasks: CrawlerTaskResponse[] | null = null;

  ngOnInit(): void {
    this.crawlerTaskService.get().subscribe(crawlerTasks => {
      this.crawlerTasks = crawlerTasks?.sort(crawlerTask => crawlerTask.tag.numberId);
      this.changeDetector.markForCheck();
    })
  }
}