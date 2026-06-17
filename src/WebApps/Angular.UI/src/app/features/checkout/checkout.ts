import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BasketService } from '../../services/basket.service';
import { BasketCheckout } from '../../core/models/basket.model';

@Component({
  selector: 'app-checkout',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss'
})
export class Checkout implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  readonly basketService = inject(BasketService);

  protected checkoutForm!: FormGroup;
  protected readonly isSubmitting = signal(false);

  ngOnInit(): void {
    if (this.basketService.cartItems().length === 0) {
      this.router.navigate(['/cart']);
      return;
    }

    this.checkoutForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      emailAddress: ['', [Validators.required, Validators.email]],
      addressLine: ['', Validators.required],
      country: ['Turkey', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', [Validators.required]],
      
      cardName: ['', Validators.required],
      cardNumber: ['', [Validators.required, Validators.pattern('^[0-9]{16}$')]],
      expiration: ['', [Validators.required, Validators.pattern('^(0[1-9]|1[0-2])\\/?([0-9]{2})$')]],
      cvv: ['', [Validators.required, Validators.pattern('^[0-9]{3}$')]],
      paymentMethod: [1, Validators.required]
    });
  }

  onSubmit(): void {
    if (this.checkoutForm.invalid) {
      this.checkoutForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const formVal = this.checkoutForm.value;
    const checkoutDto: BasketCheckout = {
      ...formVal,
      userName: this.basketService.basketSignal()?.userName ?? 'swn',
      customerId: '58c49479-ec65-4de2-86e7-033c546291aa',
      totalPrice: this.basketService.totalPrice()
    };

    this.basketService.checkoutBasket(checkoutDto).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        if (res.isSuccess) {
          this.router.navigate(['/confirmation']);
        } else {
          alert('Failed to place order. Please try again.');
        }
      },
      error: () => {
        this.isSubmitting.set(false);
        alert('An error occurred. Please try again.');
      }
    });
  }
}
