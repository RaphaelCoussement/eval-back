 Dungeon Crawler - Quests Microservice
========================================

Ce microservice gère le catalogue des quêtes et le suivi de la progression des joueurs. Il est conçu pour être **robuste**, **idempotent** et parfaitement intégré dans une architecture pilotée par les événements.

Lancement Rapide
-------------------

### 1\. Prérequis

*   **Docker Desktop** (pour MongoDB et RabbitMQ).

*   **.NET 8 SDK**.

*   **Rider** ou **Visual Studio**.


### 2\. Infrastructure (Docker)

Lance les conteneurs nécessaires via la racine du projet :

`docker-compose up -d`

> Les ports par défaut sont : 27017 (MongoDB) et 5672/15672 (RabbitMQ).

### 3\. Démarrage du Service

*   Ouvre la solution dans ton IDE.

*   Vérifie le fichier appsettings.json (les chaînes de connexion y sont pré-configurées pour localhost).

*   Lance le projet **DungeonCrawler\_Quests\_Service.API**.

*   Accède au Swagger : https://localhost:1531/swagger 
    

Réponse aux exigences de l'évaluation
----------------------------------------


### Architecture & Patterns

*   **Clean Architecture** : Séparation stricte entre le Domain, l'Application (Logique), l'Infrastructure (Data) et l'API.

*   **CQRS avec MediatR** : Utilisation de Commands et Queries pour découpler les intentions métier de leur exécution.

*   **Communication Asynchrone** : Intégration de **Rebus** avec RabbitMQ pour consommer les événements DungeonCompleted provenant du service Game.


### Fiabilité & Idempotence

Pour garantir qu'un donjon terminé n'est compté qu'une seule fois (même en cas de rejeu du message), j'ai implémenté le **Pattern Inbox** :

*   Chaque événement possède un EventId unique.

*   Le DungeonCompletedHandler vérifie la présence de cet ID dans une collection MongoDB ProcessedEvents avant tout traitement.

*   **Résultat** : Le système assure un traitement "Exactly Once" au niveau applicatif.


### Logique Métier & État

*   **Modèle de données** : Gestion des QuestDefinition (catalogue) et PlayerQuest (progression individuelle).

*   **Transition d'état** : La logique de passage du statut InProgress à Completed est encapsulée dans le Domain, garantissant que les règles métier sont centralisées.


### Validation & Tests

*   **Tests Unitaires (NUnit)** : Validation de la logique de progression et du mécanisme d'idempotence.

*   **Traces de logs** : Utilisation de logs structurés pour suivre le flux d'un message, de sa réception à la mise à jour en base.


Outils de Démonstration
---------------------------

Pour faciliter l'évaluation sans nécessiter le microservice Game complet, un **SimulatorController** a été inclus :

*   **POST /api/simulator/dungeon-completed** : Permet de publier un message sur RabbitMQ.

*   **Test d'idempotence** : En envoyant deux fois le même EventId, tu peux démontrer via les logs que le second message est ignoré.

*   **Reset** : Un endpoint de reset est disponible pour remettre à zéro la progression d'un joueur et recommencer la démo.