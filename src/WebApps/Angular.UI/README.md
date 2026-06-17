# 🛒 Shopping.Angular

This is the **Angular 21+ Standalone** client application migrated from the ASP.NET Core Razor Pages `Shopping.Web` project. It integrates seamlessly with the shop-microservices YARP Gateway backend.

---

## 🏗️ Architecture & Features

- **Angular 21 Standalone APIs**: Fully module-less architecture utilizing modern features.
- **Signals State Management**: High-performance, reactive state management using Angular Signals for sepet updates and computed properties (cart item count, total price).
- **YARP API Proxying**: Out of the box configuration (`proxy.conf.json`) to forward microservices requests from `http://localhost:4200` to `https://localhost:6064` (YARP Gateway).
- **Premium Glassmorphic Design**: Curated dark theme styling, custom animations, card micro-interactions, responsive sidebars and checkout forms.
- **Google Fonts & FontAwesome**: Pre-configured global assets and icon kits.

---

## 📂 Folder Structure

```
src/
├── app/
│   ├── core/
│   │   └── models/           # TypeScript Interfaces mapped from C# DTOs
│   ├── features/
│   │   ├── home/             # Hero Slider + Showcase
│   │   ├── catalog/          # Product List + Detail
│   │   ├── cart/             # Shopping Cart layout
│   │   ├── checkout/         # Form validation + Checkout submission
│   │   ├── confirmation/     # Success page
│   │   └── orders/           # Customer orders history table
│   ├── services/
│   │   ├── catalog.service.ts
│   │   ├── basket.service.ts
│   │   └── ordering.service.ts
│   ├── shared/components/    # Shared visual components (Navbar, Footer, ProductCard)
│   ├── app.routes.ts         # Lazy-loaded routes
│   └── app.ts                # Main App entry with bootstrap lifecycle
├── environments/
│   └── environment.ts        # Environment configurations
└── styles.scss               # Global stylesheet with design tokens
```

---

## 🚀 How to Run Locally

### 1. Prerequisite
Make sure your backend docker-compose services (Catalog.API, Basket.API, Discount.Grpc, YARP Gateway) are running:
```bash
# In the solution root folder
docker-compose up -d
```

### 2. Install Dependencies
```bash
npm install
```

### 3. Run the Development Server
```bash
npm start
```
Navigate to `http://localhost:4200/`. The proxy will intercept `/catalog-service`, `/basket-service`, and `/ordering-service` requests and forward them automatically to the YARP Gateway.

### 4. Build the Project
```bash
npm run build
```
The optimized production bundle will be generated under the `dist/Shopping.Angular` folder.

