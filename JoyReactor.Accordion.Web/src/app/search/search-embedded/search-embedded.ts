import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-embedded',
  imports: [CommonModule, FormsModule],
  templateUrl: './search-embedded.html',
  styleUrl: './search-embedded.scss',
})
export class SearchEmbedded {
  links: string = '';
}