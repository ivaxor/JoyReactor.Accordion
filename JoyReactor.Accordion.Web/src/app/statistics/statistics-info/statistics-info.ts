import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { StatisticsService } from '../../../services/statistics-service/statistics-service';
import { StatisticsResponse } from '../../../services/statistics-service/statistics-response';

@Component({
  selector: 'app-statistics-info',
  imports: [],
  templateUrl: './statistics-info.html',
  styleUrl: './statistics-info.scss',
})
export class StatisticsInfo implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  private statisticsService = inject(StatisticsService);

  statistics: StatisticsResponse | null = null;

  ngOnInit(): void {
    this.statisticsService.get().subscribe(statistics => { this.statistics = statistics; this.changeDetector.markForCheck(); });
  }
}