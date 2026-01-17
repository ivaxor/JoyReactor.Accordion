import { Component } from '@angular/core';
import { StatisticsInfo } from '../statistics-info/statistics-info';
import { CrawlerTasks } from '../crawler-tasks/crawler-tasks';

@Component({
  selector: 'app-statistics-root',
  imports: [StatisticsInfo, CrawlerTasks],
  templateUrl: './statistics-root.html',
  styleUrl: './statistics-root.scss',
})
export class StatisticsRoot {

}
