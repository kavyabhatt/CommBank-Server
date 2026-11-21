using CommBank.Controllers;
using CommBank.Services;
using CommBank.Models;
using CommBank.Tests.Fake;
using Microsoft.AspNetCore.Mvc;

namespace CommBank.Tests;

public class GoalControllerTests
{
    private readonly FakeCollections collections;

    public GoalControllerTests()
    {
        collections = new();
    }

    [Fact]
    public async void GetAll()
    {
        // Arrange
        var goals = collections.GetGoals();
        var users = collections.GetUsers();
        IGoalsService goalsService = new FakeGoalsService(goals, goals[0]);
        IUsersService usersService = new FakeUsersService(users, users[0]);
        GoalController controller = new(goalsService, usersService);

        // Act
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext = httpContext;
        var result = await controller.Get();

        // Assert
        var index = 0;
        foreach (Goal goal in result)
        {
            Assert.IsAssignableFrom<Goal>(goal);
            Assert.Equal(goals[index].Id, goal.Id);
            Assert.Equal(goals[index].Name, goal.Name);
            index++;
        }
    }

    [Fact]
    public async void Get()
    {
        // Arrange
        var goals = collections.GetGoals();
        var users = collections.GetUsers();
        IGoalsService goalsService = new FakeGoalsService(goals, goals[0]);
        IUsersService usersService = new FakeUsersService(users, users[0]);
        GoalController controller = new(goalsService, usersService);

        // Act
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext = httpContext;
        var result = await controller.Get(goals[0].Id!);

        // Assert
        Assert.IsAssignableFrom<Goal>(result.Value);
        Assert.Equal(goals[0], result.Value);
        Assert.NotEqual(goals[1], result.Value);
    }

    [Fact]
    public async Task UpdatePersistsGoalIconForSubsequentRead()
    {
        var goals = collections.GetGoals();
        var users = collections.GetUsers();
        IGoalsService goalsService = new FakeGoalsService(goals, goals[0]);
        IUsersService usersService = new FakeUsersService(users, users[0]);
        GoalController controller = new(goalsService, usersService);
        var originalGoal = goals[0];
        var updatedGoal = new Goal
        {
            Id = originalGoal.Id,
            Name = originalGoal.Name,
            Icon = "🏠",
            TargetAmount = originalGoal.TargetAmount,
            TargetDate = originalGoal.TargetDate,
            Balance = originalGoal.Balance,
            Created = originalGoal.Created,
            TransactionIds = originalGoal.TransactionIds,
            TagIds = originalGoal.TagIds,
            UserId = originalGoal.UserId
        };

        var updateResult = await controller.Update(originalGoal.Id!, updatedGoal);
        var readResult = await controller.Get(originalGoal.Id!);

        Assert.IsType<NoContentResult>(updateResult);
        Assert.Equal("🏠", readResult.Value?.Icon);
    }

    [Fact]
    public async Task GetForUser()
    {
        // Arrange
        var goals = collections.GetGoals();
        var users = collections.GetUsers();
        var userId = users[0].Id!;
        goals[0].UserId = userId;
        goals[1].UserId = userId;
        goals[2].UserId = users[1].Id;
        IGoalsService goalsService = new FakeGoalsService(goals, goals[0]);
        IUsersService usersService = new FakeUsersService(users, users[0]);
        GoalController controller = new(goalsService, usersService);

        // Act
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        controller.ControllerContext.HttpContext = httpContext;
        var result = await controller.GetForUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);

        foreach (Goal goal in result)
        {
            Assert.IsAssignableFrom<Goal>(goal);
            Assert.Equal(userId, goal.UserId);
        }
    }
}