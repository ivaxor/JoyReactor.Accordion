import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { SearchMediaHistoryService } from '../../../services/search-media-history-service/search-media-history-service';
import { SearchMediaHistoryRecord } from '../../../services/search-media-history-service/search-media-history-record';
import { SearchMediaHistoryInfo } from "../search-media-history-info/search-media-history-info";

@Component({
  selector: 'app-search-media-history',
  imports: [SearchMediaHistoryInfo],
  templateUrl: './search-media-history.html',
  styleUrl: './search-media-history.scss',
})
export class SearchMediaHistory implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  private mediaHistoryService = inject(SearchMediaHistoryService);
  historyRecords: SearchMediaHistoryRecord[] = [];
  offset: number = 0;
  limit: number = 10;

  ngOnInit(): void {
    this.mediaHistoryService.records$.subscribe(historyRecords => {
      this.historyRecords = historyRecords;
      this.changeDetector.markForCheck();
    })
  }

  async onTrashClick(): Promise<void> {
    await this.mediaHistoryService.clear();
  }
}