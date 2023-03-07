using CleanArchitecture.Application.Tests.Fixtures.Queries.Users;
using FluentAssertions;
using Xunit;

namespace CleanArchitecture.Application.Tests.Queries.Users;

public sealed class GetAllUsersQueryHandlerTests
{
    private readonly GetAllUsersTestFixture _fixture = new();

    [Fact]
    public async Task Should_Get_All_Users()
    {
        var result = await _fixture.Handler.Handle(
            new(),
            default);
        
        _fixture.VerifyNoDomainNotification();

        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result.FirstOrDefault().Id.Should().Be(_fixture.ExistingUserId);
    }
}