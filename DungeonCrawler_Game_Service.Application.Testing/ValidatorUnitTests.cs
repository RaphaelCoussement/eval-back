using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Application.Features.Characters.Validators;

namespace DungeonCrawler_Game_Service.Application.Testing;

[TestFixture]
public class ValidatorUnitTests
{
    private CreateCharacterCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateCharacterCommandValidator();
    }

    #region CreateCharacterCommandValidator Tests

    [Test]
    public void Validator_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "ValidHero",
            ClassCode = 1,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validator_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "",
            ClassCode = 1,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Name"));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Name is required."));
        });
    }

    [Test]
    public void Validator_ShouldFail_WhenNameIsTooLong()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = new string('A', 51), // 51 caractères
            ClassCode = 1,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("Name"));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Name cannot exceed 50 characters."));
        });
    }

    [Test]
    public void Validator_ShouldPass_WhenNameIsExactly50Characters()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = new string('A', 50),
            ClassCode = 2,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_ShouldFail_WhenClassCodeIsZero()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "Hero",
            ClassCode = 0,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("ClassCode"));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("ClassCode must be between 1 and 3 included."));
        });
    }

    [Test]
    public void Validator_ShouldFail_WhenClassCodeIsNegative()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "Hero",
            ClassCode = -1,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validator_ShouldFail_WhenClassCodeIsTooHigh()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "Hero",
            ClassCode = 4,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("ClassCode"));
        });
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Validator_ShouldPass_WhenClassCodeIsValid(int classCode)
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "Hero",
            ClassCode = classCode,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "Hero",
            ClassCode = 1,
            UserId = ""
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo("UserId"));
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("UserId is required."));
        });
    }

    [Test]
    public void Validator_ShouldFail_WithMultipleErrors()
    {
        // Arrange
        var command = new CreateCharacterCommand
        {
            Name = "",
            ClassCode = 10,
            UserId = ""
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThanOrEqualTo(3));
            Assert.That(result.Errors.Select(e => e.PropertyName), Does.Contain("Name"));
            Assert.That(result.Errors.Select(e => e.PropertyName), Does.Contain("ClassCode"));
            Assert.That(result.Errors.Select(e => e.PropertyName), Does.Contain("UserId"));
        });
    }

    #endregion
}

