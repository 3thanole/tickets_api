# TicketManagementApi — Contexte du projet

## ⚠️ Important
Ce projet est un **projet personnel d'entraînement au développement .NET**. Il n'est **pas lié à Betclic**, ne contient aucune donnée ni logique métier Betclic, et ne doit jamais en importer (code, exemples, ou dépendances). Il vit dans `training_project/` uniquement par commodité de rangement local.

## Objectif du projet
Construire une API de gestion de tickets de support avec deux points de vue métier :
- **Côté Client** : un demandeur crée un ticket (problème technique, demande, tâche).
- **Côté IT Workplace** : un technicien prend en charge, traite et résout les tickets.

Le but pédagogique est de pratiquer une API .NET "comme en entreprise" : couches propres, auth par rôles, persistance réelle, tests.

## Style de collaboration souhaité
L'utilisateur est **débutant en C#/.NET**. Quand tu écris ou modifies du code : explique brièvement les parties techniques nouvelles ou non triviales (syntaxe, concept, pourquoi ce choix) pour qu'il comprenne ce qui est écrit — mais reste **concis** dans ces explications, pas de longs pavés systématiques. Priorité : qu'il comprenne le code produit, sans que chaque message devienne un cours magistral.

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

### Architecture applicative (implémenté)
- **Architecture hexagonale** (Ports & Adapters), consigne manager. Le cœur métier (`TicketService`) ne dépend que du port `Repositories/ITicketRepository.cs` (interface définie pour le cœur), jamais d'un détail de stockage concret.
- `Repositories/InMemoryTicketRepository.cs` : adapter sortant, contient tout ce qui était avant dans `TicketService` (`_tickets`, `lock`, compteurs d'id `_nextId`/`_nextCommentId`) — ce sont des concerns de stockage, pas des règles métier. Retourne des **copies** (clone) des tickets à chaque lecture : sans ça, le cœur lirait des champs après que le verrou du repository a déjà été relâché, une race condition qui n'existait pas avant (tout — recherche + lecture — se faisait auparavant dans un seul et même `lock`).
- `TicketsController.cs` = adapter entrant, **inchangé** : il ne dépendait déjà que de l'interface `ITicketService`, jamais de la classe concrète.
- DI : `ITicketRepository` et `ITicketService` tous les deux `AddSingleton` (Singleton→Singleton, pas de souci de durée de vie — le repository doit survivre entre requêtes comme avant, le service n'a plus d'état propre).
- Pas de CQRS/MediatR ni de couches supplémentaires — l'archi hexagonale ne justifie pas d'empiler d'autres patterns non demandés.

### Validation des entrées API (implémenté)
- **FluentValidation** (+ `FluentValidation.DependencyInjectionExtensions`), en plus des `[Required]`/`[MaxLength]` déjà en place sur les DTOs.
- `Validators/CreateTicketRequestValidator.cs` : règle technique (`Priority` doit être une valeur d'enum définie — le `JsonStringEnumConverter` actuel autorise par défaut un entier hors-limites) + règle métier (`Description` obligatoire — **changement de comportement volontaire et assumé**, ce champ reste `string?` optionnel au niveau du DTO/modèle, seule la validation le rend obligatoire).
- `Validators/AddCommentRequestValidator.cs` : règle technique (`AuthorRole` enum défini) + règle métier (`Message` non vide — en pratique déjà couvert par le `[Required]` existant, gardé à titre d'exemple illustratif).
- `Filters/ValidationFilter.cs` : un seul filtre global (`IAsyncActionFilter`, enregistré via `AddControllers(options => options.Filters.Add<ValidationFilter>())`) qui déclenche automatiquement le validator correspondant pour n'importe quel DTO d'action — `[ApiController]` ne branche pas FluentValidation tout seul, contrairement aux attributs `[Required]`/`[MaxLength]`. Réponse 400 au même format (`ValidationProblemDetails`) que les validations automatiques existantes.
- Volontairement limité à 2 validators — pas une couverture exhaustive, juste pour illustrer technique + métier.

### Tests
- Stack : **xUnit** (exécution) + **Moq** (mock des dépendances) + **Shouldly** (assertions lisibles).
- Convention : AAA (Arrange/Act/Assert) + nommage `MethodeTestee_Contexte_ResultatAttendu`.
- Objectif : tests unitaires sur les Services/Repositories/Validators, et si possible des tests d'intégration sur les Controllers (via `WebApplicationFactory`).

## État actuel du code (pour référence, à tenir à jour)
- `Models/`, `Enums/`, `DTOs/` : en place et corrects pour le CRUD de base + le fil de commentaires (`TicketComment`, `CommentAuthorRole`, `AddCommentRequest`, `TicketCommentResponse`).
- `Repositories/ITicketRepository.cs` + `Repositories/InMemoryTicketRepository.cs` : port + adapter sortant (archi hexagonale), stockage en mémoire, thread-safe (`lock`), retourne des copies des tickets.
- `Services/ITicketService.cs` + `Services/TicketService.cs` : cœur métier, dépend uniquement de `ITicketRepository` (+ `TimeProvider`), ne garde plus aucun état mutable propre.
- `Controllers/TicketsController.cs` : en place, 7 endpoints REST (les 6 CRUD + `POST /tickets/{id}/comments`), branché sur `ITicketService` via DI (`AddSingleton`).
- `BackgroundServices/TicketCleanupService.cs` : `BackgroundService` enregistré via `AddHostedService`, supprime les tickets résolus depuis >2 min.
- `ExceptionHandling/GlobalExceptionHandler.cs` : `IExceptionHandler` (natif .NET 8, `AddExceptionHandler` + `AddProblemDetails`, branché en tout premier dans le pipeline via `app.UseExceptionHandler()`). Capture uniquement les exceptions **non prévues** (bugs) échappant à toute la pipeline ; logue le détail technique (message + stack trace) en `Error` via `ILogger`, renvoie un `ProblemDetails` générique (500, jamais le message brut de l'exception) au client. Ne remplace pas le pattern existant `null`/`NotFound()` du controller pour les cas métier attendus (ticket/commentaire introuvable) — volontairement inchangé.
- `Validators/CreateTicketRequestValidator.cs` + `Validators/AddCommentRequestValidator.cs` + `Filters/ValidationFilter.cs` : FluentValidation branché globalement sur les actions du controller (voir section Validation ci-dessus).
- `TicketManagementApi.Tests/` : projet xUnit + Moq + Shouldly + FluentValidation en place, 39 tests tous verts (`TicketService`, `TicketsController`, `InMemoryTicketRepository`, les 2 validators), cleanup testé via un `FakeTimeProvider` (pas de vrai délai d'attente).
- `wwwroot/` : frontend statique en place (`index.html`, `client.html`, `it.html`, `api.js`, `client.js`, `it.js`, `styles.css`), consomme les 7 endpoints REST (CRUD + commentaires), affiche le fil de discussion sur les deux pages et le compte à rebours de suppression côté IT. Un `POST /tickets` sans `description` renvoie désormais 400 (changement de contrat assumé).
- Pas encore d'authentification, pas d'EF Core — prochaines étapes logiques (EF Core en pause, voir "Ne pas faire").

## Ne pas faire
- Ne pas introduire de dépendance, nommage, ou logique métier issue de projets Betclic réels.
- Ne pas ajouter de champ "catégorie de ticket" sans le demander explicitly (exclu du scope actuel).
- ~~Ne pas sur-architecturer (pas de CQRS/MediatR/Clean Architecture)~~ — **levé** : consigne manager explicite d'introduire une architecture hexagonale (voir section Architecture applicative). Rester néanmoins raisonnable : pas de CQRS/MediatR à moins d'une demande explicite ultérieure, l'archi hexagonale ne justifie pas d'empiler d'autres patterns non demandés.
- **Ne pas implémenter la persistance (EF Core/SQLite, migrations, `ApplicationDbContext`)** : le manager de l'utilisateur veut voir cette partie spécifiquement avec lui. Rester sur le stockage en mémoire (`List<Ticket>`) jusqu'à nouvel ordre explicite de l'utilisateur, même si le reste du contexte décrit EF Core/SQLite comme cible future.
