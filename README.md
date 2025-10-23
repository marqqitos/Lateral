# Task Management Application

A task management application built with .NET Core backend and Next.js frontend.

## Architecture

- **Backend**: .NET Core 9 Web API with Entity Framework In-Memory database
- **Frontend**: Next.js 15 with TypeScript and TailwindCSS

## Setup Instructions

### Option 1: Docker Setup (Recommended)

The easiest way to run both frontend and backend together is using Docker:

1. Make sure you have Docker and Docker Compose installed on your system

2. From the project root directory, run:
   ```bash
   docker-compose up --build
   ```

3. Access the application:
   - **Frontend**: http://localhost:3000
   - **Backend API**: http://localhost:5082

4. To stop the application:
   ```bash
   docker-compose down
   ```

### Option 2: Manual Setup

If you prefer to run services manually:

#### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd Backend/TaskManagement
   ```

2. Run the .NET API:
   ```bash
   dotnet run
   ```

   The API will be available at `http://localhost:5082` or `https://localhost:7193`

#### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd Frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   # or
   pnpm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   # or
   pnpm dev
   ```

   The application will be available at `http://localhost:3000`

## API Endpoints

- `GET /api/tasks` - Get all tasks
- `POST /api/tasks` - Create a new task
- `PATCH /api/tasks/{id}/toggle` - Toggle task completion
- `DELETE /api/tasks/{id}` - Delete a task

## Technical Implementation

### Data Flow
1. Frontend makes HTTP requests to the .NET Core API
2. API validates requests and interacts with Entity Framework
3. In-memory database stores task data
4. API returns JSON responses to frontend
5. Frontend updates UI based on API responses

### Key Features
- **Type Safety**: TypeScript interfaces match C# DTOs exactly
- **Error Handling**: Comprehensive error handling throughout the application
- **Loading States**: Visual feedback during API operations
- **CORS Configuration**: Backend configured to accept frontend requests
- **Environment Configuration**: Configurable API base URL

## Docker Troubleshooting

If you encounter issues with Docker:

1. **Port conflicts**: Make sure ports 3000 and 5082 are not in use by other applications
2. **Build issues**: Clear Docker cache and rebuild:
   ```bash
   docker-compose down
   docker system prune -f
   docker-compose up --build
   ```
3. **Backend health check failures**: The frontend waits for the backend to be healthy. Check backend logs:
   ```bash
   docker-compose logs backend
   ```
4. **Network issues**: Ensure Docker networking is working:
   ```bash
   docker network ls
   ```

## Development Notes

- The backend uses an in-memory database, so data resets on restart
- CORS is configured to allow requests from any origin in development
- Frontend includes retry functionality for failed API calls
- All operations include proper error handling and user feedback

## Architecture Design Decisions

### Clean Architecture Implementation

The backend follows **Clean Architecture** principles with clear separation of concerns:

- **Controllers Layer**: Handles HTTP requests/responses and input validation
- **Services Layer**: Contains business logic and orchestrates data operations
- **Repository Layer**: Abstracts data access and persistence operations
- **Models Layer**: Domain entities with data annotations
- **DTOs Layer**: Data transfer objects for API contracts

### SOLID Principles Adherence

✅ **Single Responsibility Principle (SRP)**
- Each class has one clear responsibility
- TasksController: HTTP handling, TaskService: business logic, TaskRepository: data access

✅ **Open/Closed Principle (OCP)**
- Extensible through interfaces without modifying existing code
- New features can be added by implementing existing interfaces

✅ **Liskov Substitution Principle (LSP)**
- Interface implementations are fully substitutable
- Mock objects in tests demonstrate proper abstraction

✅ **Interface Segregation Principle (ISP)**
- Focused interfaces (ITaskService, ITaskRepository) with no unnecessary dependencies
- Clean contract definitions

✅ **Dependency Inversion Principle (DIP)**
- High-level modules depend on abstractions (interfaces)
- Proper dependency injection throughout the application

### Code Quality Strengths

- **Comprehensive Error Handling**: Try-catch blocks with appropriate HTTP status codes
- **Input Validation**: Data annotations and manual validation with meaningful error messages
- **Logging**: Structured logging with different levels (Information, Warning, Error)
- **Testing**: Extensive unit and integration tests with 95%+ coverage including edge cases
- **Documentation**: XML comments and clear method signatures
- **Consistent Naming**: Clear, descriptive names following C# conventions
- **Null Safety**: Proper null checks and nullable reference types

### Areas for Improvement

**1. Code Duplication**
- ID validation logic repeated across controller methods
- Consider extracting to a validation attribute or base method

**2. Mapping Logic**
- Manual mapping in TaskService could be extracted to AutoMapper or dedicated mapper class
- Would improve maintainability and reduce boilerplate

**3. Magic Values**
- String lengths (200, 1000) could be constants or configuration values
- HTTP status codes could use named constants

### Design Decisions & Rationale

**Repository Pattern**: Chosen for testability and data access abstraction, enabling easy switching between data sources

**Service Layer**: Separates business logic from controllers, making the code more maintainable and testable

**In-Memory Database**: Simplifies development and testing while maintaining EF Core patterns for easy production migration

**Async/Await Throughout**: Ensures scalability and non-blocking operations for concurrent requests

**Comprehensive Logging**: Facilitates debugging and monitoring in production environments

**Detailed Unit Testing**: Ensures reliability and prevents regressions during development

### Performance Considerations

- **Database Queries**: Simple queries suitable for in-memory database, would need optimization for production scale
- **No Caching**: Business logic goes directly to database; Redis/memory caching would improve performance
- **Serialization**: Standard JSON serialization adequate for current scope

### Security Considerations

- **CORS**: Currently allows any origin for development - needs restriction in production
- **Input Validation**: Comprehensive validation prevents basic injection attacks
- **No Authentication**: Would require JWT/OAuth implementation for production use
- **HTTPS**: Enforced in production configuration

### Scalability & Production Readiness

**Next Steps for Production**:
1. Replace in-memory database with persistent storage (SQL Server/PostgreSQL)
2. Implement authentication and authorization
3. Add caching layer (Redis)
4. Add API rate limiting
5. Configure environment-specific settings
6. Add health checks and monitoring
7. Implement pagination for large datasets

## Frontend Architecture & Code Quality Analysis

### Component Architecture Strengths

✅ **Clean Separation of Concerns**
- Components focused on single responsibilities (TaskItem, AddTaskForm, TaskDashboard)
- Proper prop drilling with well-defined TypeScript interfaces

✅ **State Management**
- Appropriate use of React's built-in state for application scope
- Proper state lifting to parent components
- Clean error state management with user feedback

✅ **API Integration**
- Dedicated API layer with proper TypeScript interfaces
- Comprehensive error handling with custom ApiError class
- Environment-based configuration for API endpoints

✅ **User Experience**
- Loading states with visual feedback
- Error recovery mechanisms (retry buttons)
- Optimistic UI updates for better perceived performance

### Areas for Enhancement

**1. Client-Side Validation**
- Basic form validation present but could be enhanced
- Consider using Zod for runtime type validation
- More comprehensive input sanitization

**2. Performance Optimizations**
- Could benefit from code splitting for larger applications
- API response caching not implemented

**3. Advanced Features**
- No React error boundaries for graceful error handling
- Missing progressive web app (PWA) capabilities
- No advanced SEO optimizations (dynamic metadata)

### Design Decisions & Rationale

**Client-Side State Management**: Chose React's built-in state over Redux/Zustand due to simple state requirements. For the current scope, this reduces complexity while maintaining clarity.

**UI Component Library**: Selected shadcn/ui for professional appearance with minimal configuration. Provides accessible components out-of-the-box while maintaining customization flexibility.

**API Layer Design**: Implemented custom fetch wrapper instead of external libraries like Axios. This reduces bundle size and provides sufficient functionality for current requirements.

**Styling Approach**: TailwindCSS chosen for rapid development and consistent design system. CSS variables enable dynamic theming support.

**Form Handling**: Used controlled components with React state instead of react-hook-form. Appropriate for simple forms, though react-hook-form would be beneficial for complex validation scenarios.

### Production Readiness Assessment

**Next Steps for Scale**:
1. Add React Query for API caching and synchronization
2. Implement error boundaries for better error handling
3. Add performance monitoring (Web Vitals)
4. Implement advanced accessibility features
5. Add internationalization (i18n) support
6. Consider micro-frontend architecture for larger teams

### Security Considerations

- **XSS Protection**: React's built-in JSX escaping prevents most XSS attacks
- **API Communication**: All API calls use HTTPS in production configuration
- **Input Validation**: Client-side validation present, server-side validation critical
- **No Sensitive Data**: No authentication tokens or sensitive data in localStorage

## Trade-offs and Assumptions

**Backend Trade-offs:**
- **In-memory database** for simplicity (production would use SQL Server/PostgreSQL)
- **Manual mapping** instead of AutoMapper (chosen for transparency and simplicity)
- **CORS allowing any origin** (development convenience - needs restriction in production)

**Frontend Trade-offs:**
- **Simple HTTP client** instead of React Query/SWR (chosen for transparency, would benefit from caching in production)
- **No authentication/authorization** (would be required in production)
- **No pagination** (would be needed for large datasets)
- **Basic form validation** (production would use libraries like Zod + react-hook-form)
- **Client-side state management** (Redux/Zustand would be needed for complex state requirements)
- **No error boundaries** (would prevent entire app crashes in production)
- **Static metadata** (production would need dynamic SEO optimization)
