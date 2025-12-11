# Kosher Clouds

A comprehensive web platform for a Jewish kosher restaurant with hookah services, providing online menu browsing, food ordering, table reservations, hookah pre-ordering, and administrative management tools.

## MVP Description

Kosher Clouds delivers a full-stack restaurant management and ordering system with:

- **Customer Features**: User authentication, multilingual menu browsing (Ukrainian/English), online food ordering with cart management, table reservations with hookah pre-ordering, Stripe payment integration, and product/order reviews
- **Admin Features**: Complete menu management (CRUD operations), order tracking and status updates, reservation management, and user administration
- **Technical Capabilities**: Microservices architecture with event-driven communication (RabbitMQ), real-time payment processing, email notifications, and responsive design for mobile and desktop

## Installation & Launch

### Prerequisites

- Docker & Docker Compose
- Node.js 18+ and npm
- .NET 8 SDK (for development)

### Local Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/KosherClouds.git
   cd KosherClouds
   ```

2. **Configure environment variables**
   - Set up `.env` files for each microservice
   - Configure Stripe API keys, database credentials, and RabbitMQ connection strings

3. **Start backend services (Docker)**
   ```bash
   docker-compose up --build
   ```
   This will launch all microservices, PostgreSQL, RabbitMQ, and Redis

4. **Start frontend development server**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

5. **Start Stripe webhook listener** (in a separate terminal)
   ```bash
   stripe listen --forward-to http://localhost:5006/api/payments/webhook
   ```

6. **Access the application**
   - Frontend: `http://localhost:3000`
   - API Gateway: `http://localhost:5000`
   - RabbitMQ Management: `http://localhost:15672`

### Test Accounts

- **Customer**: `user@kosherclouds.com` / `User@1234`
- **Admin**: `admin@kosherclouds.com` / `Admin@1234`

## Technology Stack

**Backend**:
- ASP.NET Core 8 Web API (Microservices)
- PostgreSQL (Database)
- RabbitMQ with MassTransit (Event-driven messaging)
- Redis (Caching)
- Stripe (Payment processing)
- TestContainers, xUnit (Testing)

**Frontend**:
- React 18 with TypeScript
- Vite (Build tool)
- Tailwind CSS v4
- Zustand (State management)
- react-i18next (Localization)
- Axios (API communication)

## Architecture

Microservices:
- **API Gateway**: Request routing and authentication
- **UserService**: User management and authentication
- **ProductService**: Menu and product management
- **CartService**: Shopping cart functionality
- **OrderService**: Order processing and status tracking
- **BookingService**: Table reservations and hookah pre-ordering
- **ReviewService**: Product and order reviews
- **PaymentService**: Stripe payment integration

## Team

| Name | Role |
|------|------|
| **Tomka Yuriy Yaroslavovych** | PO, Mentor |
| **Shevchenko Anastasiia** | PM, Tester |
| **Perevispa Vladyslav** | Full-stack developer, QA engineer |
| **Chigir Yelyzaveta** | Backend developer |
| **Maksymov Demian** | Business Analysis |
| **Varvariuk Kateryna** | UX/UI Designer, Frontend developer |
| **Melnyk Valeriia** | UX/UI Designer, Frontend developer |

## License

This project is developed as a university coursework.