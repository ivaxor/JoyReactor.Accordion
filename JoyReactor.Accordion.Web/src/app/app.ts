import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from "./footer/footer";
import { Navbar } from "./navbar/navbar";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Footer, Navbar],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('joyreactor-accordion');
}