# TicketManagementApi — Contexte du projet

## ⚠️ Important
Ce projet est un **projet personnel d'entraînement au développement .NET**. Il n'est **pas lié à Betclic**, ne contient aucune donnée ni logique métier Betclic, et ne doit jamais en importer (code, exemples, ou dépendances). Il vit dans `training_project/` uniquement par commodité de rangement local.

## Objectif du projet
Construire une API de gestion de tickets de support avec deux points de vue métier :
- **Côté Client** : un demandeur crée un ticket (problème technique, demande, tâche).
- **Côté IT Workplace** : un technicien prend en charge, traite et résout les tickets.

Le but pédagogique est de pratiquer une API .NET "comme en entreprise" : couches propres, auth par rôles, persistance réelle, tests.

## Technologies

### En place actuellement
- **.NET 8** / ASP.NET Core Web API (`net8.0`, Nullable + ImplicitUsings activés)
- **Swashbuckle.AspNetCore** (Swagger/OpenAPI) pour la documentation et le test manuel des endpoints
- **Frontend statique** : HTML/CSS/JS "vanilla" (pas de framework, pas de build) servi directement par l'API via `wwwroot/` (`UseDefaultFiles()` + `UseStaticFiles()` dans `Program.cs`). Deux pages (`client.html`, `it.html`) reflétant les deux points de vue métier, plus une page d'accueil (`index.html`). Pas de rôle/permission réelle imposée entre les deux pages — juste deux UI différentes sur la même API. Helpers fetch partagés dans `api.js`.
- **`BackgroundService`** (`Microsoft.Extensions.Hosting`) pour la suppression automatique des tickets résolus depuis plus de 2 minutes (`TicketCleanupService`, balayage périodique toutes les 30s via `PeriodicTimer`).
- **`TimeProvider`** (natif .NET 8) injecté dans `TicketService` au lieu de `DateTime.UtcNow` en dur, pour permettre de tester le cleanup sans vrai délai d'attente.
- Aucune base de données, aucune auth pour l'instant (stockage en mémoire via `List<Ticket>`, protégé par un `lock` depuis l'introduction du balayage en arrière-plan qui mute la liste en concurrence des requêtes HTTP)

### Cibles à ajouter progressivement
- **Entity Framework Core** avec provider **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`) pour la persistance — choisi à la place de SQL Server car l'environnement de dev est Linux/WSL2 (pas de LocalDB) et on veut éviter la lourdeur d'un serveur de BDD en conteneur pour un projet de formation
- **ASP.NET Core Identity** pour la gestion des utilisateurs/rôles
- **JWT Bearer** (`Microsoft.AspNetCore.Authentication.JwtBearer`) pour l'authentification des appels API
- **xUnit** + **Moq** + **Shouldly** pour les tests unitaires et d'intégration (déjà en place, voir `TicketManagementApi.Tests/`)

## Décisions d'architecture (cibles à implémenter progressivement)

### Rôles & Authentification
- Deux rôles : `Client` (demandeur) et `ITAgent` (résolveur IT Workplace).
- **ASP.NET Core Identity** pour la gestion des utilisateurs/rôles en base, combiné à des **JWT (Bearer token)** pour l'authentification des appels API.
- Chaque ticket est lié à un utilisateur authentifié (voir modèle de données ci-dessous).

### Persistance
- **EF Core + SQLite**. L'implémentation actuelle en mémoire (`List<Ticket>` dans `TicketService`) est une étape intermédiaire, à remplacer par un vrai `ApplicationDbContext` + migrations EF Core.

### Modèle de données — évolutions prévues sur `Ticket`
En plus des champs existants (Title, Description, Status, Priority, CreatedAt, UpdatedAt), prévoir :
- `Requester` : identité (nom/email, ou lien vers l'utilisateur Identity) du demandeur côté Client.
- `AssignedTo` : technicien IT Workplace assigné au ticket (nullable tant que non pris en charge).
- Pas de notion de catégorie/type de ticket pour l'instant (délibérément exclu du scope).

### Fil de discussion (implémenté, en mémoire)
- `Models/TicketComment.cs` + `Enums/CommentAuthorRole.cs` (`Client`/`ITAgent`) : liste `Ticket.Comments`, alimentée via `POST /tickets/{id}/comments`, embarquée dans chaque `TicketResponse`.
- Pas de vraie auth : l'auteur est juste un tag de rôle envoyé dans la requête (`client.js` poste toujours `Client`, `it.js` toujours `ITAgent`), cohérent avec le reste du projet (deux UI non authentifiées sur la même API).

### Suppression automatique des tickets résolus (implémenté)
- Un ticket passé au statut `Resolved` (`UpdatedAt` mis à jour) est supprimé automatiquement 2 minutes plus tard par `TicketCleanupService`, indépendamment de toute visite de page.
- S'il repasse à un autre statut avant l'échéance, il n'est jamais supprimé (le balayage re-vérifie `Status == Resolved` à chaque passage).
- Compte à rebours visible dans l'espace IT (`it.js`), purement informatif côté client (peut être décalé de ~30s par rapport à la suppression réelle côté serveur).

### Architecture applicative
- Cible : `Controller → Service → Repository` (le pattern Repository doit être introduit en plus de la couche Service actuelle, pour découpler `TicketService` de l'accès aux données une fois EF Core en place).
- Niveau de complexité voulu : **intermédiaire** — pas de Clean Architecture stricte ni de CQRS/MediatR, l'accent est mis sur Repository + EF Core + tests plutôt que sur une architecture multi-projets.

### Tests
- Stack : **xUnit** (exécution) + **Moq** (mock des dépendances) + **Shouldly** (assertions lisibles).
- Objectif : tests unitaires sur les Services/Repositories, et si possible des tests d'intégration sur les Controllers (via `WebApplicationFactory`).

## État actuel du code (pour référence, à tenir à jour)
- `Models/`, `Enums/`, `DTOs/` : en place et corrects pour le CRUD de base + le fil de commentaires (`TicketComment`, `CommentAuthorRole`, `AddCommentRequest`, `TicketCommentResponse`).
- `Services/ITicketService.cs` + `Services/TicketService.cs` : implémentation CRUD + commentaires + cleanup, **en mémoire**, thread-safe (`lock`), horloge injectée via `TimeProvider` (à migrer vers EF Core + Repository plus tard).
- `Controllers/TicketsController.cs` : en place, 7 endpoints REST (les 6 CRUD + `POST /tickets/{id}/comments`), branché sur `ITicketService` via DI (`AddSingleton`, nécessaire tant que le stockage est en mémoire).
- `BackgroundServices/TicketCleanupService.cs` : `BackgroundService` enregistré via `AddHostedService`, supprime les tickets résolus depuis >2 min.
- `TicketManagementApi.Tests/` : projet xUnit + Moq + Shouldly en place, tests unitaires sur `TicketService` et `TicketsController` (29 tests, tous verts), incluant le cleanup testé via un `FakeTimeProvider` (pas de vrai délai d'attente).
- `wwwroot/` : frontend statique en place (`index.html`, `client.html`, `it.html`, `api.js`, `client.js`, `it.js`, `styles.css`), consomme les 7 endpoints REST (CRUD + commentaires), affiche le fil de discussion sur les deux pages et le compte à rebours de suppression côté IT.
- Pas encore d'authentification, pas d'EF Core — prochaines étapes logiques (EF Core en pause, voir "Ne pas faire").

## Ne pas faire
- Ne pas introduire de dépendance, nommage, ou logique métier issue de projets Betclic réels.
- Ne pas ajouter de champ "catégorie de ticket" sans le demander explicitly (exclu du scope actuel).
- Ne pas sur-architecturer (pas de CQRS/MediatR/Clean Architecture) sauf demande explicite future.
- **Ne pas implémenter la persistance (EF Core/SQLite, migrations, `ApplicationDbContext`)** : le manager de l'utilisateur veut voir cette partie spécifiquement avec lui. Rester sur le stockage en mémoire (`List<Ticket>`) jusqu'à nouvel ordre explicite de l'utilisateur, même si le reste du contexte décrit EF Core/SQLite comme cible future.
