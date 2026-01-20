import { Component } from '@angular/core';
import { NavbarHealthCheck } from "../navbar-healthcheck/navbar-healthcheck";
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar-root',
  imports: [RouterModule, NavbarHealthCheck],
  templateUrl: './navbar-root.html',
  styleUrl: './navbar-root.scss',
})
export class NavbarRoot { }