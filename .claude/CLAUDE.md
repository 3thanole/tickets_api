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
- Aucune base de données, aucune auth pour l'instant (stockage en mémoire via `List<Ticket>`)

### Cibles à ajouter progressivement
- **Entity Framework Core** avec provider **SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer`) pour la persistance
- **ASP.NET Core Identity** pour la gestion des utilisateurs/rôles
- **JWT Bearer** (`Microsoft.AspNetCore.Authentication.JwtBearer`) pour l'authentification des appels API
- **xUnit** + **Moq** + **Shouldly** pour les tests unitaires et d'intégration

## Décisions d'architecture (cibles à implémenter progressivement)

### Rôles & Authentification
- Deux rôles : `Client` (demandeur) et `ITAgent` (résolveur IT Workplace).
- **ASP.NET Core Identity** pour la gestion des utilisateurs/rôles en base, combiné à des **JWT (Bearer token)** pour l'authentification des appels API.
- Chaque ticket est lié à un utilisateur authentifié (voir modèle de données ci-dessous).

### Persistance
- **EF Core + SQL Server**. L'implémentation actuelle en mémoire (`List<Ticket>` dans `TicketService`) est une étape intermédiaire, à remplacer par un vrai `ApplicationDbContext` + migrations EF Core.

### Modèle de données — évolutions prévues sur `Ticket`
En plus des champs existants (Title, Description, Status, Priority, CreatedAt, UpdatedAt), prévoir :
- `Requester` : identité (nom/email, ou lien vers l'utilisateur Identity) du demandeur côté Client.
- `AssignedTo` : technicien IT Workplace assigné au ticket (nullable tant que non pris en charge).
- Historique de commentaires : fil d'échange horodaté entre Client et IT sur un ticket donné (nouvelle entité liée, ex. `TicketComment`).
- Pas de notion de catégorie/type de ticket pour l'instant (délibérément exclu du scope).

### Architecture applicative
- Cible : `Controller → Service → Repository` (le pattern Repository doit être introduit en plus de la couche Service actuelle, pour découpler `TicketService` de l'accès aux données une fois EF Core en place).
- Niveau de complexité voulu : **intermédiaire** — pas de Clean Architecture stricte ni de CQRS/MediatR, l'accent est mis sur Repository + EF Core + tests plutôt que sur une architecture multi-projets.

### Tests
- Stack : **xUnit** (exécution) + **Moq** (mock des dépendances) + **Shouldly** (assertions lisibles).
- Objectif : tests unitaires sur les Services/Repositories, et si possible des tests d'intégration sur les Controllers (via `WebApplicationFactory`).

## État actuel du code (pour référence, à tenir à jour)
- `Models/`, `Enums/`, `DTOs/` : en place et corrects pour le CRUD de base.
- `Services/ITicketService.cs` + `Services/TicketService.cs` : implémentation CRUD complète mais **en mémoire** (à migrer vers EF Core + Repository).
- `Controllers/` : pas encore de `TicketsController` (en cours d'implémentation, voir plan associé).
- Pas encore d'authentification, pas d'EF Core, pas de tests — ce sont les prochaines étapes logiques une fois le CRUD de base branché.

## Ne pas faire
- Ne pas introduire de dépendance, nommage, ou logique métier issue de projets Betclic réels.
- Ne pas ajouter de champ "catégorie de ticket" sans le demander explicitly (exclu du scope actuel).
- Ne pas sur-architecturer (pas de CQRS/MediatR/Clean Architecture) sauf demande explicite future.
