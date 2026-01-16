import { Component } from '@angular/core';
import { SearchEmbedded } from '../search-embedded/search-embedded';
import { SearchPicture } from '../search-picture/search-picture';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-root',
  imports: [SearchEmbedded, SearchPicture, CommonModule, FormsModule],
  templateUrl: './search-root.html',
  styleUrl: './search-root.scss',
})
export class SearchRoot {
  isPicture: boolean = true;
}