import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';

import { PortfolioApiService, PortfolioValuationResultDto } from '../api/portfolio-api.service';

type ChartMode = 'bar' | 'pie';

@Component({
  selector: 'app-portfolio-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CurrencyPipe,
    ReactiveFormsModule,

    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatButtonToggleModule,
    MatIconModule,
    MatSnackBarModule,

    BaseChartDirective,
  ],
  templateUrl: './portfolio-dashboard.component.html',
  styleUrls: ['./portfolio-dashboard.component.scss'],
})
export class PortfolioDashboardComponent {
  private fb = inject(FormBuilder);
  private api = inject(PortfolioApiService);
  private snack = inject(MatSnackBar);

  loading = signal(false);
  error = signal<string | null>(null);
  result = signal<PortfolioValuationResultDto | null>(null);

  chartMode = signal<ChartMode>('bar');

  form = this.fb.nonNullable.group({
    investorId: ['Investor0', [Validators.required, Validators.minLength(1), Validators.maxLength(64)]],
    date: ['2019-12-31', [Validators.required, Validators.pattern(/^\d{4}-\d{2}-\d{2}$/)]],
  });

  labels = computed(() => (this.result()?.compositionByType ?? []).map(x => x.type));
  values = computed(() => (this.result()?.compositionByType ?? []).map(x => x.value));

  barChartType: 'bar' = 'bar';
  barChartData = computed<ChartConfiguration<'bar'>['data']>(() => ({
    labels: this.labels(),
    datasets: [{ data: this.values(), label: 'Value' }],
  }));
  barChartOptions: ChartConfiguration<'bar'>['options'] = { responsive: true, scales: { y: { beginAtZero: true } } };

  pieChartType: 'pie' = 'pie';
  pieChartData = computed<ChartConfiguration<'pie'>['data']>(() => ({
    labels: this.labels(),
    datasets: [{ data: this.values() }],
  }));
  pieChartOptions: ChartConfiguration<'pie'>['options'] = { responsive: true, plugins: { legend: { position: 'bottom' } } };

  load(): void {
    this.error.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.snack.open('Fix validation errors.', 'OK', { duration: 2000 });
      return;
    }

    const investorId = this.form.controls.investorId.value.trim();
    const date = this.form.controls.date.value.trim();

    this.loading.set(true);
    this.result.set(null);

    this.api.getPortfolioValue(investorId, date).subscribe({
      next: (res) => {
        this.result.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.message ?? err?.message ?? 'Failed to load.';
        this.error.set(msg);
      },
    });
  }

  setChartMode(mode: ChartMode) {
    this.chartMode.set(mode);
  }

  trackByType(_: number, item: { type: string }) {
    return item.type;
  }
}
