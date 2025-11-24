# Template JWT Project

Dit project implementeert een complete ASP.NET Core Web API met Identity, JWT authenticatie, **Refresh Tokens**, rollen (Role-based Authorization), CORS configuratie en SQL Server database.

**🎯 Gebouwd met .NET 10.0**

## Features

- ✅ ASP.NET Core Identity met `ApplicationUser` (erft van `IdentityUser`)
- ✅ JWT Bearer Token authenticatie
- ✅ **Refresh Tokens voor automatische token renewal**
- ✅ **Role-based Authorization (User & Admin rollen)**
- ✅ **CORS configuratie voor frontend integratie**
- ✅ SQL Server database met Entity Framework Core
- ✅ Register & Login endpoints met automatische rol-toewijzing
- ✅ Admin endpoints voor gebruikersbeheer
- ✅ Beveiligde endpoints met `[Authorize]` attribuut
- ✅ Token revocation & logout from all devices
- ✅ **ASP.NET Core Identity metrics (nieuw in .NET 10)**

## Nieuwe Features in .NET 10

### ASP.NET Core Identity Metrics
.NET 10 introduceert nieuwe observability features voor Identity met metrics voor:
- User management: Creaties, updates, deletes
- Authentication: Login attempts, sign ins/outs, two-factor
- Password management: Password checks en token verificaties

Alle metrics zijn beschikbaar in de `Microsoft.AspNetCore.Identity` meter voor monitoring en observability.

### Authentication & Authorization Metrics
Nieuwe metrics voor authentication events:
- Authenticated request duration
- Challenge/Forbid counts
- Sign in/out counts
- Authorization requirement counts

## Rollen

Het systeem heeft twee standaard rollen:

- **User** - Standaard rol toegewezen bij registratie
- **Admin** - Beheerdersrol voor gebruikersbeheer

Rollen worden automatisch aangemaakt bij het opstarten van de applicatie via `RoleInitializer`.

## Database Tabellen

Na het runnen van de migraties zijn de volgende Identity tabellen aangemaakt:
- `AspNetUsers` - gebruikersgegevens
- `AspNetRoles` - rollen (Admin, User)
- `AspNetUserRoles` - koppeling tussen users en roles
- `AspNetUserClaims` - user claims
- `AspNetUserLogins` - externe login providers
- `AspNetUserTokens` - tokens voor password reset, etc.
- `AspNetRoleClaims` - role claims
- **`RefreshTokens`** - refresh tokens voor JWT renewal

## Configuratie

### URL
De API draait default op https://localhost:7003
dus wanneer je een post naar de register endpoint wilt maken om een gebruiker te registeren zou je dat in de 
development omgeving kunnen doen naar:  https://localhost:7003/api/auth/register


### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyProject;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!123456",
    "Issuer": "TemplateJwtProject",
    "Audience": "TemplateJwtProjectUsers",
    "ExpiryInMinutes": "60",
    "RefreshTokenExpiryInDays": "7"
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:1234",
      "https://localhost:1234"
    ]
  }
}
```

**⚠️ BELANGRIJK**: 
- Verander de `SecretKey` in productie naar een veilige, random gegenereerde key!
- Pas `AllowedOrigins` aan voor je productie frontend URL's
- **Access Token**: 60 minuten (kort voor beveiliging)
- **Refresh Token**: 7 dagen (lang voor gebruikerservaring)

### CORS Configuratie

De API ondersteunt CORS voor cross-origin requests vanuit frontend applicaties.

**Standaard toegestane origins:**
- `http://localhost:1234` (default)
- `https://localhost:1234`

**Development extra origins** (in `appsettings.Development.json`):
- `http://localhost:3000` (React default)
- `http://localhost:5173` (Vite default)
- `http://localhost:4200` (Angular default)

**CORS Policy eigenschappen:**
- ✅ AllowAnyMethod (GET, POST, PUT, DELETE, etc.)
- ✅ AllowAnyHeader (Content-Type, Authorization, etc.)
- ✅ AllowCredentials (voor cookies/credentials)

**Origins toevoegen:**
Voeg extra origins toe in `appsettings.json`:
```json
"CorsSettings": {
  "AllowedOrigins": [
    "http://localhost:1234",
    "https://yourdomain.com",
    "https://app.yourdomain.com"
  ]
}
```

## Refresh Tokens

### Wat zijn Refresh Tokens?

Refresh tokens stellen gebruikers in staat om ingelogd te blijven zonder constant hun wachtwoord opnieuw in te voeren. Wanneer de JWT access token (60 min) verloopt, kan de frontend automatisch een nieuwe aanvragen met de refresh token (7 dagen).

**Zie [REFRESH_TOKENS.md](REFRESH_TOKENS.md) voor gedetailleerde documentatie en frontend implementatie voorbeelden.**

### Nieuwe Endpoints

#### Refresh Token - Verkrijg nieuwe access token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "kYj3L8Xm9QwErTy..."
}
```

**Response:**
```json
{
  "token": "eyJhbG...",
  "refreshToken": "p7K2m5N8vRxWqZt...",
  "email": "user@example.com",
  "roles": ["User"],
  "expiresAt": "2024-01-17T14:37:08Z"
}
```

#### Revoke Token - Trek refresh token in
```http
POST /api/auth/revoke-token
Authorization: Bearer {token}
Content-Type: application/json

{
  "refreshToken": "kYj3L8Xm9QwErTy..."
}
```

#### Logout All Devices - Trek alle refresh tokens in
```http
POST /api/auth/logout-all
Authorization: Bearer {token}
```

## Frontend Integratie

### JavaScript/TypeScript Voorbeeld met Refresh

```javascript
// Login request met CORS
const response = await fetch('https://localhost:7xxx/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  credentials: 'include',
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'Test123!'
  })
});

const data = await response.json();

// Sla beide tokens op
localStorage.setItem('accessToken', data.token);
localStorage.setItem('refreshToken', data.refreshToken);

// Automatische refresh bij 401
async function apiRequest(url, options = {}) {
  options.headers = {
    ...options.headers,
    'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
  };

  let response = await fetch(url, options);

  if (response.status === 401) {
    // Access token verlopen - refresh
    const refreshResponse = await fetch('https://localhost:7xxx/api/auth/refresh-token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ 
        refreshToken: localStorage.getItem('refreshToken') 
      })
    });

    if (refreshResponse.ok) {
      const newTokens = await refreshResponse.json();
      localStorage.setItem('accessToken', newTokens.token);
      localStorage.setItem('refreshToken', newTokens.refreshToken);

      // Retry met nieuwe token
      options.headers['Authorization'] = `Bearer ${newTokens.token}`;
      response = await fetch(url, options);
    } else {
      // Refresh failed - redirect to login
      window.location.href = '/login';
    }
  }

  return response;
}
```

### React Voorbeeld (met Axios)

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7xxx',
  withCredentials: true
});

// Request interceptor
api.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor voor auto-refresh
api.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        const response = await axios.post('/api/auth/refresh-token', { refreshToken });
        
        const { token, refreshToken: newRefreshToken } = response.data;
        localStorage.setItem('accessToken', token);
        localStorage.setItem('refreshToken', newRefreshToken);

        originalRequest.headers.Authorization = `Bearer ${token}`;
        return api(originalRequest);
      } catch (refreshError) {
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

## API Endpoints

### Auth Controller (Publiek)

#### 1. Register - Nieuwe gebruiker aanmaken
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Test123!",
  "confirmPassword": "Test123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "kYj3L8Xm9QwErTy...",
  "email": "user@example.com",
  "roles": ["User"],
  "expiresAt": "2024-01-17T13:37:08Z"
}
```

**Note:** Nieuwe gebruikers krijgen automatisch de "User" rol toegewezen.

#### 2. Login - Inloggen met bestaande gebruiker
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Test123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "kYj3L8Xm9QwErTy...",
  "email": "user@example.com",
  "roles": ["User"],
  "expiresAt": "2024-01-17T13:37:08Z"
}
```

### Test Controller (Beveiligd met Rollen)

#### 3. User Endpoint - Toegankelijk voor User rol
```http
GET /api/test/user
Authorization: Bearer {token}
```

#### 4. Admin Endpoint - Alleen toegankelijk voor Admin rol
```http
GET /api/test/admin
Authorization: Bearer {token}
```

#### 5. User or Admin Endpoint - Toegankelijk voor beide rollen
```http
GET /api/test/user-or-admin
Authorization: Bearer {token}
```

**Response:**
```json
{
  "message": "This endpoint is accessible by Users or Admins",
  "user": "user@example.com",
  "roles": ["User"]
}
```

### Admin Controller (Alleen Admin toegang)

#### 6. Assign Role - Wijs een rol toe aan een gebruiker
```http
POST /api/admin/assign-role
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "email": "user@example.com",
  "role": "Admin"
}
```

**Response:**
```json
{
  "message": "Role Admin assigned successfully",
  "email": "user@example.com",
  "roles": ["User", "Admin"]
}
```

#### 7. Remove Role - Verwijder een rol van een gebruiker
```http
POST /api/admin/remove-role
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "email": "user@example.com",
  "role": "Admin"
}
```

#### 8. Get All Users - Lijst van alle gebruikers met rollen
```http
GET /api/admin/users
Authorization: Bearer {admin-token}
```

**Response:**
```json
[
  {
    "id": "123-456-789",
    "email": "user@example.com",
    "userName": "user@example.com",
    "roles": ["User"]
  },
  {
    "id": "987-654-321",
    "email": "admin@example.com",
    "userName": "admin@example.com",
    "roles": ["User", "Admin"]
  }
]
```

### Weather Forecast (Beveiligd)

#### 9. Get Weather - Beveiligd endpoint (JWT vereist)
```http
GET /weatherforecast
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "date": "2024-01-18",
    "temperatureC": 25,
    "temperatureF": 76,
    "summary": "Warm"
  }
]
```

## Gebruik met Postman/curl

### 1. Register en Login
```bash
# Register (krijgt automatisch User rol + refresh token)
curl -X POST https://localhost:7xxx/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!","confirmPassword":"Test123!"}'

# Login
curl -X POST https://localhost:7xxx/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}'
```

### 2. Maak een Admin gebruiker

**Optie 1: Via database** (eerste keer)
Voer rechtstreeks in de database uit:
```sql
-- Vind de User ID
SELECT Id FROM AspNetUsers WHERE Email = 'admin@example.com'

-- Vind de Admin Role ID
SELECT Id FROM AspNetRoles WHERE Name = 'Admin'

-- Voeg de koppeling toe
INSERT INTO AspNetUserRoles (UserId, RoleId) 
VALUES ('user-id-hier', 'admin-role-id-hier')
```

**Optie 2: Via code** - Voeg dit toe aan `Program.cs` na RoleInitializer:
```csharp
// Maak een admin gebruiker aan (eenmalig)
var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
var adminEmail = "admin@example.com";
if (await userManager.FindByEmailAsync(adminEmail) == null)
{
    var adminUser = new ApplicationUser
    {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true
    };
    await userManager.CreateAsync(adminUser, "Admin123!");
    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
}
```

### 3. Gebruik beveiligde endpoints
```bash
# Test User endpoint
curl -X GET https://localhost:7xxx/api/test/user \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"

# Test Admin endpoint (alleen met Admin rol)
curl -X GET https://localhost:7xxx/api/test/admin \
  -H "Authorization: Bearer YOUR_ADMIN_JWT_TOKEN_HERE"
```

## JWT Token Structuur

De JWT token bevat de volgende claims:
- `sub` - Email van de gebruiker
- `jti` - Unieke token ID
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` - User ID
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress` - Email
- **`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`** - Rollen van de gebruiker

Je kunt de token decoderen op [jwt.io](https://jwt.io) om de claims te bekijken.

## Rol-gebaseerde Autorisatie

### In Controllers gebruiken:

```csharp
// Alleen Admin
[Authorize(Roles = Roles.Admin)]
public IActionResult AdminOnly() { }

// Alleen User
[Authorize(Roles = Roles.User)]
public IActionResult UserOnly() { }

// User OF Admin
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
public IActionResult UserOrAdmin() { }

// Beide rollen vereist (EN)
[Authorize(Roles = Roles.User)]
[Authorize(Roles = Roles.Admin)]
public IActionResult UserAndAdmin() { }
```

### Rollen in code controleren:

```csharp
// Check of gebruiker een specifieke rol heeft
if (User.IsInRole(Roles.Admin))
{
    // Admin-specifieke logica
}

// Haal alle rollen op
var roles = User.Claims
    .Where(c => c.Type == ClaimTypes.Role)
    .Select(c => c.Value)
    .ToList();
```

## CORS Troubleshooting

### CORS errors in browser?

1. **Controleer de origin in appsettings.json**
   ```json
   "CorsSettings": {
     "AllowedOrigins": ["http://localhost:3000"]
   }
   ```

2. **Controleer de volgorde van middleware in Program.cs**
   ```csharp
   app.UseCors("DefaultCorsPolicy"); // VOOR UseAuthentication!
   app.UseAuthentication();
   app.UseAuthorization();
   ```

3. **Browser console errors**
   - `Access-Control-Allow-Origin` missing → Origin niet toegestaan
   - Preflight `OPTIONS` request fails → CORS policy niet correct
   - Credentials errors → Check `AllowCredentials()` in CORS policy

4. **Test CORS met curl**
   ```bash
   curl -H "Origin: http://localhost:1234" \
        -H "Access-Control-Request-Method: POST" \
        -H "Access-Control-Request-Headers: Content-Type" \
        -X OPTIONS --verbose \
        https://localhost:7xxx/api/auth/login
   ```

## Password Requirements

De volgende password eisen zijn geconfigureerd:
- Minimaal 6 karakters
- Minimaal 1 cijfer
- Minimaal 1 hoofdletter
- Minimaal 1 kleine letter
- Geen speciale tekens vereist (kan worden aangepast in `Program.cs`)

## Database Migraties

Als je wijzigingen maakt aan de models:

```bash
# Nieuwe migratie aanmaken
dotnet ef migrations add MigrationName

# Database updaten
dotnet ef database update

# Migratie verwijderen (laatste)
dotnet ef migrations remove
```

## Uitbreidingen

Je kunt de `ApplicationUser` class uitbreiden met extra properties:

```csharp
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}
```

Na wijzigingen: run een nieuwe migratie!

## NuGet Packages

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (10.0.0)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.0)

## Project Structuur

```
TemplateJwtProject/
├── Constants/
│   └── Roles.cs                   # Rol constanten
├── Controllers/
│   ├── AuthController.cs          # Login/Register/Refresh endpoints
│   ├── AdminController.cs         # Admin-only endpoints
│   ├── TestController.cs          # Voorbeeld rol-gebaseerde endpoints
│   └── WeatherForecastController.cs
├── Data/
│   └── AppDbContext.cs            # EF Core DbContext + RefreshTokens
├── Models/
│   ├── ApplicationUser.cs         # Custom Identity User
│   ├── RefreshToken.cs            # Refresh token model
│   └── DTOs/
│       ├── RegisterDto.cs
│       ├── LoginDto.cs
│       ├── AuthResponseDto.cs     # Inclusief refresh token
│       ├── RefreshTokenDto.cs     # Voor refresh requests
│       └── AssignRoleDto.cs       # Voor rol-beheer
├── Services/
│   ├── JwtService.cs              # JWT token generatie met rollen
│   ├── RefreshTokenService.cs     # Refresh token beheer
│   └── RoleInitializer.cs         # Initialiseer rollen bij startup
└── Program.cs                     # App configuratie + CORS
```

## Documentatie

- **[README.md](README.md)** - Hoofddocumentatie (dit bestand)
- **[REFRESH_TOKENS.md](REFRESH_TOKENS.md)** - Gedetailleerde refresh token documentatie
- **[ADMIN_SETUP.md](ADMIN_SETUP.md)** - Guide voor eerste admin gebruiker

## Tips

1. **Swagger/OpenAPI**: Het project gebruikt OpenAPI - ga naar `/openapi` in development mode
2. **JWT Debuggen**: Gebruik [jwt.io](https://jwt.io) om tokens te decoderen en claims te bekijken
3. **Database bekijken**: Gebruik SQL Server Management Studio of Visual Studio SQL Server Object Explorer
4. **CORS**: Configuratie is al aanwezig! Pas alleen de origins aan in `appsettings.json`
5. **Eerste Admin**: Maak handmatig een admin aan via database of voeg code toe in `Program.cs`
6. **Browser DevTools**: Check Network tab voor CORS headers in response
7. **Refresh Tokens**: Sla deze veilig op (localStorage of httpOnly cookies)

## Volgende Stappen

- [x] Roles implementeren (Admin, User)
- [x] Roles in JWT tokens
- [x] Role-based authorization endpoints
- [x] Admin management endpoints
- [x] CORS configuratie voor frontend
- [x] **Refresh tokens implementatie**
- [ ] Email confirmatie implementeren
- [ ] Password reset functionaliteit
- [ ] Rate limiting
- [ ] Swagger authenticatie configureren
- [ ] Custom roles (naast User en Admin)
- [ ] Claims-based authorization
