# 🚀 Avstickare – Backend (ASP.NET Core API)
## Av: Mikael Larsson  
### Mittuniversitetet, Självständigt arbete DT140G, vt 2025 

Detta är backend-API:t för Avstickare, en Progressive Web App för reseplanering. API:t är byggt med ASP.NET Core API och erbjuder inloggning, hantering av resor och stopp, integration mot Google Maps API, samt lagring i PostgreSQL.

---

## 🛠 Teknikstack

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL + PostGIS
- JWT-baserad autentisering
- Google Maps API (Directions, Places, Geocoding)

---

## 📦 Funktionalitet

- ✅ Registrering och inloggning
- 🧭 Skapa resor
- 📍 Hantera resestopp längs rutten
- ⭐ Markera platser som favoriter
- 📌 Reverse geocoding och POI-sökning med Google Maps

---

## 📡 API-endpoints
### 🔐 AuthController api/auth
| Metod | Endpoint          | Beskrivning                      | Skyddad |
| ----- | ----------------- | -------------------------------- | ------- |
| POST  | `/registrera`     | Registrera ny användare          | ❌       |
| POST  | `/logga-in`       | Logga in och få JWT-token        | ❌       |
| POST  | `/andra-losenord` | Ändra lösenord                   | ✅       |
| GET   | `/userinfo`       | Hämta info om inloggad användare | ✅       |

### 👤 AppUserController api/appuser
| Metod  | Endpoint | Beskrivning                 | Skyddad   |
| ------ | -------- | --------------------------- | --------- |
| GET    | `/`      | Hämta alla användare        | ✅ (Admin) |
| GET    | `/{id}`  | Hämta en specifik användare | ✅ (Admin) |
| PUT    | `/{id}`  | Uppdatera användarinfo      | ✅ (Admin) |
| DELETE | `/{id}`  | Radera en användare         | ✅ (Admin) |

### 🧭 TripController api/trip
| Metod  | Endpoint | Beskrivning                                  | Skyddad |
| ------ | -------- | -------------------------------------------- | ------- |
| POST   | `/plan`  | Planera rutt och få platsförslag längs vägen | ❌       |
| GET    | `/`      | Hämta alla resor för inloggad användare      | ✅       |
| GET    | `/{id}`  | Hämta en specifik sparad resa                | ✅       |
| POST   | `/`      | Spara ny resa                                | ✅       |
| DELETE | `/{id}`  | Ta bort sparad resa                          | ✅       |

### 🛑 TripStopController api/tripstop
| Metod  | Endpoint    | Beskrivning                       | Skyddad |
| ------ | ----------- | --------------------------------- | ------- |
| GET    | `/{tripId}` | Hämta alla stopp för en viss resa | ✅       |
| POST   | `/`         | Lägg till ett nytt stopp          | ✅       |
| DELETE | `/{id}`     | Ta bort ett stopp                 | ✅       |

### ⭐ FavoritePlaceController api/favoriteplace
| Metod  | Endpoint            | Beskrivning                                 | Skyddad |
| ------ | ------------------- | ------------------------------------------- | ------- |
| GET    | `/`                 | Hämta favoritplatser för inloggad användare | ✅       |
| GET    | `/exists/{placeId}` | Kontrollera om plats är favorit             | ✅       |
| POST   | `/`                 | Lägg till plats som favorit                 | ✅       |
| DELETE | `/remove/{placeId}` | Ta bort plats från favoriter                | ✅       |

📍 PlaceController api/place
| Metod | Endpoint     | Beskrivning                               | Skyddad |
| ----- | ------------ | ----------------------------------------- | ------- |
| GET   | `/{placeId}` | Hämta platsinformation från Google Places | ❌       |

## ⚙️ Konfiguration
Följande inställningar används för att uppkoppling till databas, JWT och Google Maps Api ska fungera.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DIN_DATABAS;Username=DITT_ANVÄNDARNAMN;Password=DITT_LÖSENORD"
  },
  "JwtSettings": {
    "SecretKey": "DIN_HEMLIGA_NYCKEL",
    "Issuer": "AvstickareAPI",
    "Audience": "DIN_AUDIENCE"
  },
  "GoogleApi": {
    "ApiKey": "DIN_GOOGLE_API_KEY"
  }
}
