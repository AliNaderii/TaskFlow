using FluentAssertions;
using Moq;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Organizations.Commands.CreateOrganization;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.UnitTests.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandHandlerTests
{
    private readonly Mock<IOrganizationRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateOrganizationCommandHandler _handler;

    public CreateOrganizationCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrganizationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateOrganizationCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsOrganizationId()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Organization");
        var organizationName = OrganizationName.Create("Valid Organization").Value;

        _repositoryMock
            .Setup(x => x.ExistsByNameAsync(organizationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidName_ReturnsError()
    {
        // Arrange
        var command = new CreateOrganizationCommand("ab"); // Too short

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_OrganizationAlreadyExists_ReturnsError()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Existing Organization");
        var organizationName = OrganizationName.Create("Existing Organization").Value;

        _repositoryMock
            .Setup(x => x.ExistsByNameAsync(organizationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationErrors.AlreadyExists);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryThrows_ThrowsException()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Organization");
        var organizationName = OrganizationName.Create("Valid Organization").Value;

        _repositoryMock
            .Setup(x => x.ExistsByNameAsync(organizationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}