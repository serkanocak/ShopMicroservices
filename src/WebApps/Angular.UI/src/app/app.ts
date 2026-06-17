import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './shared/components/navbar/navbar';
import { Footer } from './shared/components/footer/footer';
import { BasketService } from './services/basket.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Footer],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly basketService = inject(BasketService);

  ngOnInit(): void {
    // Load default basket for "swn"
    this.basketService.loadBasket('swn').subscribe();
  }
}
