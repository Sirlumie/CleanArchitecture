﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Users;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.IntegrationTests.Extensions;
using CleanArchitecture.IntegrationTests.Fixtures;
using CleanArchitecture.IntegrationTests.Infrastructure.Auth;
using FluentAssertions;

namespace CleanArchitecture.IntegrationTests.Controller;

public sealed class UserControllerTests
{
    private readonly UserTestFixture _fixture = new();

    [OneTimeSetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test, Order(0)]
    public async Task Should_Get_All_User()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/user");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<UserViewModel>>();

        message?.Data.Should().NotBeNull();

        var content = message!.Data!.Items.ToList();

        content.Count.Should().Be(2);

        var currentUser = content.First(x => x.Id == TestAuthenticationOptions.TestUserId);

        currentUser.Role.Should().Be(UserRole.Admin);
        currentUser.Email.Should().Be(TestAuthenticationOptions.Email);
        currentUser.FirstName.Should().Be(TestAuthenticationOptions.FirstName);
        currentUser.LastName.Should().Be(TestAuthenticationOptions.LastName);
    }

    [Test, Order(1)]
    public async Task Should_Get_User_By_Id()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/user/" + TestAuthenticationOptions.TestUserId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<UserViewModel>();

        message?.Data.Should().NotBeNull();

        var content = message!.Data!;

        content.Id.Should().Be(TestAuthenticationOptions.TestUserId);
        content.Email.Should().Be(TestAuthenticationOptions.Email);
        content.FirstName.Should().Be(TestAuthenticationOptions.FirstName);
        content.LastName.Should().Be(TestAuthenticationOptions.LastName);
    }
    
    [Test, Order(2)]
    public async Task Should_Get_All_User_Including_Deleted()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/user?includeDeleted=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<UserViewModel>>();

        message?.Data.Should().NotBeNull();

        var content = message!.Data!.Items.ToList();

        content.Count.Should().Be(3);

        content.FirstOrDefault(x => x.Id == _fixture.DeletedUserId).Should().NotBeNull();
    }

    [Test, Order(3)]
    public async Task Should_Not_Get_Deleted_User_By_Id()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/user/" + _fixture.DeletedUserId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var message = await response.Content.ReadAsJsonAsync<UserViewModel>();
        message?.Data.Should().BeNull();
    }

    [Test, Order(4)]
    public async Task Should_Create_User()
    {
        var user = new CreateUserViewModel(
            "some@user.com",
            "Test",
            "Email",
            "1234#KSAD23s",
            Ids.Seed.TenantId);

        var response = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/user", user);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<Guid>();
        message?.Data.Should().NotBeEmpty();
    }

    [Test, Order(5)]
    public async Task Should_Login_User()
    {
        var user = new LoginUserViewModel(
            "admin@email.com",
            "!Password123#");

        var response = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/user/login", user);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<string>();
        message?.Data.Should().NotBeEmpty();
    }

    [Test, Order(6)]
    public async Task Should_Get_The_Current_Active_Users()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/user/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<UserViewModel>();

        message?.Data.Should().NotBeNull();

        var content = message!.Data!;

        content.Id.Should().Be(TestAuthenticationOptions.TestUserId);
        content.Email.Should().Be(TestAuthenticationOptions.Email);
        content.FirstName.Should().Be(TestAuthenticationOptions.FirstName);
        content.LastName.Should().Be(TestAuthenticationOptions.LastName);
    }

    [Test, Order(7)]
    public async Task Should_Update_User()
    {
        var user = new UpdateUserViewModel(
            Ids.Seed.UserId,
            "newtest@email.com",
            "NewTest",
            "NewEmail",
            UserRole.User,
            Ids.Seed.TenantId);

        var response = await _fixture.ServerClient.PutAsJsonAsync("/api/v1/user", user);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<UpdateUserViewModel>();

        message?.Data.Should().NotBeNull();

        var content = message!.Data;

        content.Should().BeEquivalentTo(user);

        // Check if user is really updated
        var userResponse = await _fixture.ServerClient.GetAsync("/api/v1/user/" + user.Id);

        userResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userMessage = await userResponse.Content.ReadAsJsonAsync<UserViewModel>();

        userMessage?.Data.Should().NotBeNull();

        var userContent = userMessage!.Data!;

        userContent.Id.Should().Be(user.Id);
        userContent.Email.Should().Be(user.Email);
        userContent.FirstName.Should().Be(user.FirstName);
        userContent.LastName.Should().Be(user.LastName);
        userContent.Role.Should().Be(user.Role);
    }

    [Test, Order(8)]
    public async Task Should_Change_User_Password()
    {
        var user = new ChangePasswordViewModel(
            "!Password123#",
            "!Password123#1");

        var response = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/user/changePassword", user);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<ChangePasswordViewModel>();

        message?.Data.Should().NotBeNull();

        var content = message!.Data;

        content.Should().BeEquivalentTo(user);

        // Verify the user can login with the new password
        var login = new LoginUserViewModel(
            TestAuthenticationOptions.Email,
            user.NewPassword);

        var loginResponse = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/user/login", login);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginMessage = await loginResponse.Content.ReadAsJsonAsync<string>();

        loginMessage?.Data.Should().NotBeEmpty();
    }

    [Test, Order(9)]
    public async Task Should_Delete_User()
    {
        var response = await _fixture.ServerClient.DeleteAsync("/api/v1/user/" + TestAuthenticationOptions.TestUserId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<Guid>();

        message?.Data.Should().NotBeEmpty();

        var content = message!.Data;
        content.Should().Be(TestAuthenticationOptions.TestUserId);

        var userResponse = await _fixture.ServerClient.GetAsync("/api/v1/user/" + TestAuthenticationOptions.TestUserId);

        userResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}