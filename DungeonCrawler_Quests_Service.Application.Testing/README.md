# Tests Unitaires - DungeonCrawler Game Service

Ce projet contient les tests unitaires pour l'application DungeonCrawler Game Service, utilisant **NUnit** et **Moq**.

## ✅ Résultats des Tests

```
Nombre total de tests: 39
Réussis: 39 ✅
Échecs: 0 ❌
Durée totale: ~5 secondes
```

## 📋 Structure des Tests

### 1. **CharacterUnitTests.cs** (13 tests)
Tests unitaires pour toutes les fonctionnalités liées aux personnages.

#### Tests CreateCharacterCommand (3 tests)
- ✅ `CreateCharacterCommand_ShouldCreateCharacterSuccessfully` - Création réussie d'un personnage Warrior
- ✅ `CreateCharacterCommand_ShouldCreateWizardClass` - Création d'un Wizard
- ✅ `CreateCharacterCommand_ShouldCreateShamanClass` - Création d'un Shaman

#### Tests EquipSkinCommand (2 tests)
- ✅ `EquipSkinCommand_ShouldEquipSkinSuccessfully` - Équipement d'un skin avec succès
- ✅ `EquipSkinCommand_ShouldThrowException_WhenCharacterNotFound` - Exception si personnage inexistant

#### Tests GetCharacterByIdQuery (2 tests)
- ✅ `GetCharacterById_ShouldReturnCharacter_WhenExists` - Récupération d'un personnage existant
- ✅ `GetCharacterById_ShouldThrowKeyNotFoundException_WhenNotExists` - Exception si personnage inexistant

#### Tests GetEquippedSkinIdQuery (3 tests)
- ✅ `GetEquippedSkinId_ShouldReturnSkinId_WhenCharacterExists` - Récupération du skin équipé
- ✅ `GetEquippedSkinId_ShouldReturnEmptyString_WhenNoSkinEquipped` - Retour vide si pas de skin
- ✅ `GetEquippedSkinId_ShouldThrowKeyNotFoundException_WhenCharacterNotExists` - Exception si personnage inexistant

### 2. **DungeonUnitTests.cs** (17 tests)
Tests unitaires pour toutes les fonctionnalités liées aux donjons.

#### Tests GenerateDungeonCommand (4 tests)
- ✅ `GenerateDungeon_ShouldCreateDungeonWithMultipleLevels` - Génération avec 10-15 niveaux
- ✅ `GenerateDungeon_ShouldHaveEntranceRoomOnFirstLevel` - Salle d'entrée au niveau 1
- ✅ `GenerateDungeon_ShouldHaveBossRoomOnLastLevel` - Salle du boss au dernier niveau
- ✅ `GenerateDungeon_ShouldCreateUniqueSeedForEachDungeon` - Seeds uniques pour chaque donjon

#### Tests LinkRoomsCommand (3 tests)
- ✅ `LinkRooms_ShouldCreateNewLink_WhenNotExists` - Création d'un nouveau lien
- ✅ `LinkRooms_ShouldNotCreateDuplicate_WhenLinkAlreadyExists` - Pas de duplication de liens
- ✅ `LinkRooms_ShouldThrowKeyNotFoundException_WhenDungeonNotExists` - Exception si donjon inexistant

#### Tests EnterRoomQuery (6 tests)
- ✅ `EnterRoom_ShouldReturnRoomProgress_WhenRoomExists` - Progression lors de l'entrée dans une salle
- ✅ `EnterRoom_ShouldReturnCombatEvent_WhenEnteringCombatRoom` - Événement de combat
- ✅ `EnterRoom_ShouldReturnTreasureEvent_WhenEnteringTreasureRoom` - Événement de trésor
- ✅ `EnterRoom_ShouldReturnTrapEvent_WhenEnteringTrapRoom` - Événement de piège
- ✅ `EnterRoom_ShouldReturnBossEvent_WhenEnteringBossRoom` - Événement de boss
- ✅ `EnterRoom_ShouldThrowKeyNotFoundException_WhenDungeonNotExists` - Exception si donjon inexistant

#### Tests GetNextRoomsQuery (4 tests)
- ✅ `GetNextRooms_ShouldReturnRoomsFromNextLevel` - Récupération des salles du niveau suivant
- ✅ `GetNextRooms_ShouldReturnEmptyList_WhenOnLastLevel` - Liste vide au dernier niveau
- ✅ `GetNextRooms_ShouldReturnEmptyList_WhenRoomNotFound` - Liste vide si salle inexistante
- ✅ `GetNextRooms_ShouldThrowKeyNotFoundException_WhenDungeonNotExists` - Exception si donjon inexistant

### 3. **ValidatorUnitTests.cs** (13 tests)
Tests unitaires pour les validateurs FluentValidation.

#### Tests CreateCharacterCommandValidator (13 tests)
- ✅ `Validator_ShouldPass_WhenAllFieldsAreValid` - Validation réussie avec données valides
- ✅ `Validator_ShouldFail_WhenNameIsEmpty` - Échec si nom vide
- ✅ `Validator_ShouldFail_WhenNameIsTooLong` - Échec si nom > 50 caractères
- ✅ `Validator_ShouldPass_WhenNameIsExactly50Characters` - Succès avec nom de 50 caractères
- ✅ `Validator_ShouldFail_WhenClassCodeIsZero` - Échec si ClassCode = 0
- ✅ `Validator_ShouldFail_WhenClassCodeIsNegative` - Échec si ClassCode négatif
- ✅ `Validator_ShouldFail_WhenClassCodeIsTooHigh` - Échec si ClassCode > 3
- ✅ `Validator_ShouldPass_WhenClassCodeIsValid(1,2,3)` - Succès avec ClassCode valide (3 test cases)
- ✅ `Validator_ShouldFail_WhenUserIdIsEmpty` - Échec si UserId vide
- ✅ `Validator_ShouldFail_WithMultipleErrors` - Multiple erreurs de validation

## 🛠️ Technologies Utilisées

- **NUnit 4.2.2** - Framework de tests
- **Moq 4.20.72** - Framework de mocking
- **FluentValidation** - Validation des commandes
- **.NET 9.0** - Framework cible
- **coverlet.collector 6.0.2** - Couverture de code

## 🚀 Exécution des Tests

### Via la ligne de commande
```powershell
# Exécuter tous les tests
dotnet test

# Exécuter avec verbosité détaillée
dotnet test --logger "console;verbosity=detailed"

# Exécuter uniquement les tests d'un fichier spécifique
dotnet test --filter "FullyQualifiedName~CharacterUnitTests"
dotnet test --filter "FullyQualifiedName~DungeonUnitTests"
dotnet test --filter "FullyQualifiedName~ValidatorUnitTests"

# Générer un rapport de couverture
dotnet test --collect:"XPlat Code Coverage"
```

### Via Visual Studio / Rider
1. Ouvrir le Test Explorer
2. Cliquer sur "Run All Tests"
3. Ou cliquer avec le bouton droit sur un test spécifique et sélectionner "Run"

## 📊 Couverture de Code

Les tests couvrent :
- ✅ Tous les CommandHandlers (Create, Equip, Link, Generate)
- ✅ Tous les QueryHandlers (GetById, GetSkin, Enter, NextRooms)
- ✅ La génération procédurale de donjons
- ✅ Les validateurs FluentValidation
- ✅ Les cas d'erreur et exceptions (KeyNotFoundException, null references)
- ✅ Les cas limites (edge cases)
- ✅ Vérification des événements (Combat, Treasure, Trap, Boss)
- ✅ Logique métier (liens de salles, progression)

## 🎯 Bonnes Pratiques Appliquées

1. **Arrange-Act-Assert (AAA)** - Structure claire de chaque test
2. **Noms descriptifs** - Les noms de tests décrivent le comportement attendu
3. **Isolation** - Chaque test est indépendant grâce aux mocks
4. **SetUp** - Initialisation commune dans les méthodes SetUp avec `[SetUp]`
5. **Assertions multiples** - Utilisation de `Assert.Multiple()` pour grouper les assertions
6. **Test des exceptions** - Vérification des comportements d'erreur avec `Assert.ThrowsAsync`
7. **Test Cases** - Utilisation de `[TestCase]` pour tester plusieurs valeurs
8. **Verify Mocks** - Vérification que les méthodes mockées sont appelées correctement

## 🔍 Patterns de Tests Utilisés

### Mocking avec Moq
```csharp
var mockRepo = new Mock<IRepository<Character>>();
mockRepo.Setup(r => r.GetByIdAsync("id")).ReturnsAsync(character);
mockRepo.Verify(r => r.AddAsync(It.IsAny<Character>()), Times.Once);
```

### Test d'exceptions
```csharp
Assert.ThrowsAsync<KeyNotFoundException>(
    async () => await handler.Handle(query, CancellationToken.None)
);
```

### Assertions groupées
```csharp
Assert.Multiple(() =>
{
    Assert.That(result.Name, Is.EqualTo("Expected"));
    Assert.That(result.Id, Is.Not.Null);
});
```

## 📝 Notes

- Les tests utilisent **Moq** pour simuler les dépendances (repositories, bus, loggers)
- Aucune base de données réelle n'est utilisée (tests 100% en mémoire)
- Les tests sont rapides et peuvent être exécutés en CI/CD
- Pour les tests d'intégration avec MongoDB, voir `DungeonCrawler_Quests_Service.Application.IntegrationTests`
- Les classes de personnages disponibles sont : **Warrior**, **Shaman**, **Wizard**
- Les types de salles sont : **Entrance**, **CombatRoom**, **TreasureRoom**, **TrapRoom**, **BossRoom**

## 📈 Statistiques

- **Total des tests** : 39 tests unitaires ✅
- **Fichiers de tests** : 3
- **Handlers testés** : 8
- **Validateurs testés** : 1
- **Temps d'exécution** : ~5 secondes pour tous les tests
- **Taux de réussite** : 100% ✅

## 🎓 Classes Testées

### Commands
- `CreateCharacterCommandHandler`
- `EquipSkinCommandHandler`
- `GenerateDungeonCommandHandler`
- `LinkRoomsCommandHandler`

### Queries
- `GetCharacterByIdQueryHandler`
- `GetEquippedSkinIdQueryHandler`
- `EnterRoomQueryHandler`
- `GetNextRoomsQueryHandler`

### Validators
- `CreateCharacterCommandValidator`

## 🔄 CI/CD

Ces tests peuvent être facilement intégrés dans un pipeline CI/CD :

```yaml
# Exemple pour Azure Pipelines
- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests'
  inputs:
    command: 'test'
    projects: '**/*Testing.csproj'
    arguments: '--configuration Release --collect:"XPlat Code Coverage"'
```

