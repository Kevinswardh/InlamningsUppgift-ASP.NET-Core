# Projektplattform – ASP.NET Core MVC

Detta är ett fullständigt ASP.NET Core MVC-projekt med stöd för rollbaserad åtkomst, projekt- och användarhantering, extern inloggning samt mörkt läge.

## 🚀 Funktioner

- Inloggning & registrering
- Extern inloggning (Google, GitHub m.fl.)
- Skapa, redigera, radera projekt
- Lägg till och ta bort teammedlemmar
- Roller: Admin, Manager, Team Member, Customer, User
- Profilhantering med profilbild (uppladdad eller extern)
- Dark mode-inställning per användare
- Projektstatus (Startat, Slutfört)
- Filtrering, sökning och sortering
- Responsiv designv

## 🧱 Teknisk översikt

**Arkitektur:**
- PresentationLayer (Controllers, Views, ViewModels)
- ApplicationLayer (Services)
- Cross-cutting Concerns (DTOs, interfaces)
- InfrastructureLayer (Repositories)
- DataAccessLayer (DbContext och entiteter)
- SecurityLayer (Microsoft Identity)

**Databas:**
- Microsoft SQL Server
- IdentityDbContext för användare
- ApplicationProjectDbContext för projektdata

## 📁 Struktur
/PresentationLayer
/Controllers
/ViewModels

/ApplicationLayer
/Services

/CrossCuttingConcerns
/FormDTOs
/Interfaces

/InfrastructureLayer
/Repositories

/DataAccessLayer
/DbContexts

/SecurityLayer
/Identity
/AuthServices


## 🧑‍💼 Roller och behörighet

| Roll       | Skapa/Redigera | Visa projekt | Radera projekt |
|------------|----------------|--------------|----------------|
| Admin      | ✅              | Alla         | ✅              |
| Manager    | ✅              | Alla         | ✅              |
| TeamMember | Endast egna    | Egna         | ❌              |
| Customer   | ❌              | Egna         | ❌              |
| User       | ❌              | ❌           | ❌              |

## 🔐 Säkerhet

- All åtkomst skyddas med `[Authorize]`
- Roller sätts via Identity
- Claims används för dark mode och användarinfo
- Inloggning via Identity eller extern provider

## 🌙 Dark Mode

Användaren kan aktivera mörkt läge, vilket sparas som en inställning i databasen och används som claim vid inloggning.

## 🖼️ Profilbilder

- Användaren kan ladda upp en egen bild.
- Alternativt använda bild från extern inloggningsleverantör.





