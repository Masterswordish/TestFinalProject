using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Construction;
using Moq;
using Newtonsoft.Json.Converters;
using NuGet.Protocol;
using RecipeApi.Controllers;
using RecipeApi.Models;
using Xunit.Sdk;

namespace RecipeApiTests;

public class UserControllerTests
{
    private Mock<IUserRepository> _mockRepo;
    private UserController _controller;

    public UserControllerTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _controller = new UserController(_mockRepo.Object);
    }

    [Fact]
    public void GetFavouriteRecipe_returns_mockRecipe()
    {
        // arrange
        var id = 1;
        _mockRepo.Setup(x => x.GetFavouriteRecipe(id)).Returns(new RecipeResponse
        {
            Title = "Cool recipe",
            RecipeId = 1,
            Instructions = Array.Empty<string>(),
            Ingredients = Array.Empty<string>()
        });

        // act
        var result = _controller.GetFavouriteRecipe(id);
        var data = (result.Result as OkObjectResult)!.Value.As<RecipeResponse>();

        // assert
        result.Should().NotBeNull();
        data.Should().BeOfType(typeof(RecipeResponse));
        data.Title.Should().Be("Cool recipe");
    }

    [Fact]
    public void GetFavouriteRecipe_returns_errorMessage()
    {
        // arrange
        var id = 1;
        _mockRepo.Setup(x => x.GetFavouriteRecipe(id)).Returns(new RecipeResponse());

        // act
        var result = _controller.GetFavouriteRecipe(id);
        var data = (result.Result as NotFoundObjectResult)!.Value.As<string>();
        var statusCode = (result.Result as NotFoundObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Should().Be("Incorrect recipe Id.");
        statusCode.Equals(404);
    }

    [Fact]
    public void GetFavouriteRecipesByUserId_returns_List()
    {
        // arrange
        var id = 1;
        _mockRepo.Setup(x => x.UserExists(id)).Returns(true);
        _mockRepo.Setup(x => x.GetFavsByUserId(id)).Returns(new List<RecipeResponse>()
        {
            new RecipeResponse
            {
                RecipeId = 1,
                Title = "Lekker eten",
                Ingredients = new[] { "Ei", "spek", "pistolet" },
                Instructions = new[] { "Bak in pan", "Snijd broodje open", "Doe ei op broodje" }
            },
            new RecipeResponse
            {
                RecipeId = 2,
                Title = "Nice food",
                Ingredients = new[] { "Egg", "Bacon", "Little baguette" },
                Instructions = new[] { "Fry in pan", "Cut open bread", "Put egg on bread" }
            }
        });

        // act
        var result = _controller.GetFavouriteRecipesByUserId(id);
        var data = (result.Result as OkObjectResult)!.Value.As<List<RecipeResponse>>();
        var statusCode = (result.Result as OkObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Count.Should().Be(2);
        data[0].Title.Should().Be("Lekker eten");
        data[1].RecipeId.Should().Be(2);
        statusCode.Equals(200);
    }

    [Fact]
    public void GetFavouriteRecipesByUserId_returns_NotFound_whenNonExistingUser()
    {
        // arrange
        var id = 1;
        _mockRepo.Setup(x => x.UserExists(id)).Returns(false);

        // act
        var result = _controller.GetFavouriteRecipesByUserId(id);
        var data = (result.Result as NotFoundObjectResult)!.Value.As<string>();
        var statusCode = (result.Result as NotFoundObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Should().Be("User not found.");
        statusCode.Equals(404);
    }

    [Fact]
    public void AddFavouriteRecipeToUser_returns_CreatedAtAction_withNonExistingRecipe()
    {
        // arrange
        var id = 1;
        var recipeResponse = new RecipeResponse
        {
            UserId = id,
            RecipeId = 5,
            Title = "Niet bestaand recept",
            Ingredients = new[] { "Toverbal", "Feeënstof" },
            Instructions = new[] { "Geloof in magie", "gebruik je fantasie" }
        };
        var recipeDto = new RecipeDto
        {
            UserId = 1,
            Title = "Niet bestaand recept",
            Ingredients = new[] { "Toverbal", "Feeënstof" },
            Instructions = new[] { "Geloof in magie", "gebruik je fantasie" }
        };
        _mockRepo.Setup(x => x.UserExists(id)).Returns(true);
        _mockRepo.Setup(x => x.RecipeExistsInFav(It.IsAny<string>(), It.IsAny<int>())).Returns(false);
        _mockRepo.Setup(x => x.RecipeIdExists(It.IsAny<string>())).Returns(-1);
        _mockRepo.Setup(x => x.GetFavouriteRecipe(It.IsAny<int>())).Returns(recipeResponse);
        _mockRepo.Setup(x => x.AddRecipe(recipeDto))
            .ReturnsAsync(recipeResponse
            );

        // act
        var result = _controller.AddFavouriteRecipeToUser(recipeDto);
        var data = (result.Result as CreatedAtActionResult)!.Value.As<RecipeResponse>();
        var statusCode = (result.Result as CreatedAtActionResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Title.Should().Be("Niet bestaand recept");
        statusCode.Equals(201);
    }

    [Fact]
    public void AddFavouriteRecipeToUser_returns_Ok_result_withExistingRecipe()
    {
        // arrange
        var id = 1;
        var recipeResponse = new RecipeResponse
        {
            UserId = id,
            RecipeId = 5,
            Title = "Niet bestaand recept",
            Ingredients = new[] { "Toverbal", "Feeënstof" },
            Instructions = new[] { "Geloof in magie", "gebruik je fantasie" }
        };
        var recipeDto = new RecipeDto
        {
            UserId = 1,
            Title = "Niet bestaand recept",
            Ingredients = new[] { "Toverbal", "Feeënstof" },
            Instructions = new[] { "Geloof in magie", "gebruik je fantasie" }
        };
        _mockRepo.Setup(x => x.UserExists(id)).Returns(true);
        _mockRepo.Setup(x => x.RecipeExistsInFav(It.IsAny<string>(), It.IsAny<int>())).Returns(false);
        _mockRepo.Setup(x => x.RecipeIdExists(It.IsAny<string>())).Returns(5);
        var user = new User { Id = 1, Name = "Marta", Email = "test@a.com" };
        _mockRepo.Setup(x => x.GetUser(It.IsAny<int>()))
            .Returns(user);
        _mockRepo.Setup(x => x.AddRecipeToUser(user, "Niet bestaand recept"));

        // act
        var result = _controller.AddFavouriteRecipeToUser(recipeDto);
        var data = (result.Result as OkObjectResult)!.Value.As<RecipeResponse>();
        var statusCode = (result.Result as OkObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Title.Should().Be("Niet bestaand recept");
        data.Ingredients.Length.Should().Be(0);
        statusCode.Equals(200);
    }

    [Fact]
    public void AddFavouriteRecipeToUser_returns_NotFound_whenNonExistingUser()
    {
        // arrange
        var id = 1;
        _mockRepo.Setup(x => x.UserExists(id)).Returns(false);

        // act
        var result = _controller.AddFavouriteRecipeToUser(new()
        {
            UserId = id,
            Title = "Niet bestaand recept",
            Ingredients = new[] { "Toverbal", "Feeënstof" },
            Instructions = new[] { "Geloof in magie", "gebruik je fantasie" }
        });
        var data = (result.Result as NotFoundObjectResult)!.Value.As<string>();
        var statusCode = (result.Result as NotFoundObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Should().Be("User not found.");
        statusCode.Equals(404);
    }

    [Fact]
    public void AddFavouriteRecipeToUser_returns_Conflict_whenRecipeAlreadyInFavourites()
    {
        // arrange
        var id = 1;
        _mockRepo.Setup(x => x.UserExists(id)).Returns(true);
        _mockRepo.Setup(x => x.RecipeExistsInFav(It.IsAny<string>(), It.IsAny<int>())).Returns(true);

        // act
        var result = _controller.AddFavouriteRecipeToUser(new()
        {
            UserId = id,
            Title = "Niet bestaand recept",
            Ingredients = new[] { "Toverbal", "Feeënstof" },
            Instructions = new[] { "Geloof in magie", "gebruik je fantasie" }
        });
        var data = (result.Result as ConflictObjectResult)!.Value.As<string>();
        var statusCode = (result.Result as ConflictObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Should().Contain("Item already in favourites for user");
        statusCode.Equals(409);
    }

    [Fact]
    public void RemoveFavourite_returns_NoContent_withExistingUserIdAndRecipeId()
    {
        // arrange
        _mockRepo.Setup(x => x.DeleteFavourite(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

        // act
        var result = _controller.RemoveFavourite(1, 1);
        var statusCode = (result.Result as NoContentResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        statusCode.Equals(204);
    }

    [Fact]
    public void RemoveFavourite_returns_BadRequest_withNonExistingUserIdOrRecipeId()
    {
        // arrange
        _mockRepo.Setup(x => x.DeleteFavourite(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

        // act
        var result = _controller.RemoveFavourite(1, 1);
        var statusCode = (result.Result as BadRequestObjectResult)!.StatusCode.As<int>();
        var data = (result.Result as BadRequestObjectResult)!.Value.As<string>();
        // assert
        result.Should().NotBeNull();
        data.Should().Be("Id not found.");
        statusCode.Equals(400);
    }

    [Fact]
    public void AddUser_returns_OkayObjectResult_forNewUser()
    {
        // arrange
        var userDto = new UserDto
        {
            Name = "Jason",
            Email = "jason@gmail.com",
            ImageUrl = "www.google.com"
        };
        var user = new User
        {
            Id = 1,
            Name = "Jason",
            Email = "jason@gmail.com",
            ImageUrl = "www.google.com"
        };
        _mockRepo.Setup(x => x.UserByEmail(It.IsAny<string>())).Returns(user);
        _mockRepo.Setup(x => x.AddNewUser(userDto)).ReturnsAsync(user);

        // act
        var result = _controller.AddUser(userDto);
        var statusCode = (result.Result as OkObjectResult)!.StatusCode.As<int>();
        var data = (result.Result as OkObjectResult)!.Value.As<User>();
        // assert
        result.Should().NotBeNull();
        data.Name.Should().Be("Jason");
        statusCode.Equals(200);
    }

    [Fact]
    public void AddUser_returns_OkayObjectResult_withAlreadyExistingUser()
    {
        // arrange
        var userDto = new UserDto
        {
            Name = "Jason",
            Email = "jason@gmail.com",
            ImageUrl = "www.google.com"
        };
        _mockRepo.Setup(x => x.UserByEmail(It.IsAny<string>())).Returns(new User { Name = "Jason" });

        // act
        var result = _controller.AddUser(userDto);
        var statusCode = (result.Result as OkObjectResult)!.StatusCode.As<int>();
        var data = (result.Result as OkObjectResult)!.Value.As<User>();
        // assert
        result.Should().NotBeNull();
        data.Name.Should().Be("Jason");
        statusCode.Equals(200);
    }

    [Fact]
    public void GetNotes_returns_ListOfNotes_forCorrectUserIdAndRecipeId()
    {
        // arrange
        _mockRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns(new User { Name = "Shivani" });
        _mockRepo.Setup(x => x.GetRecipe(It.IsAny<int>())).Returns(new Recipe { Title = "Biryani" });
        _mockRepo.Setup(x => x.GetNotes(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<NoteResponse>
            {
                new NoteResponse(),
                new NoteResponse()
            });

        // act
        var result = _controller.GetNotes(1, 1);
        var data = (result as OkObjectResult)!.Value.As<List<NoteResponse>>();
        var statusCode = (result as OkObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Count.Should().Be(2);
        statusCode.Should().Be(200);
    }

    [Fact]
    public void GetNotes_returns_BadRequest_whenIncorrectUserIdOrRecipeId()
    {
        // arrange
        _mockRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User?)null);
        _mockRepo.Setup(x => x.GetRecipe(It.IsAny<int>())).Returns((Recipe?)null);

        // act
        var result = _controller.GetNotes(1, 1);
        var statusCode = (result as BadRequestObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        statusCode.Should().Be(400);
    }

    [Fact]
    public void GetNote_returns_NoteForCorrectNoteId()
    {
        // arrange
        _mockRepo.Setup(x => x.NoteExistsById(It.IsAny<int>())).Returns(true);
        _mockRepo.Setup(x => x.GetNote(It.IsAny<int>())).Returns(new NoteResponse
        {
            NoteId = 1,
            NoteText = "Test note."
        });

        // act
        var result = _controller.GetNote(1);
        var data = (result.Result as OkObjectResult).Value.As<NoteResponse>();
        var statusCode = (result.Result as OkObjectResult).StatusCode.As<int>();

        // assert
        data.NoteText.Should().Be("Test note.");
        statusCode.Equals(200);
    }

    [Fact]
    public void GetNote_returns_BadRequest_forIncorrectNoteId()
    {
        // arrange
        _mockRepo.Setup(x => x.NoteExistsById(It.IsAny<int>())).Returns(false);

        // act
        var result = _controller.GetNote(1);
        var data = (result.Result as BadRequestObjectResult).Value.As<string>();
        var statusCode = (result.Result as BadRequestObjectResult).StatusCode.As<int>();

        // assert
        data.Should().Be("NoteId is invalid.");
        statusCode.Equals(400);
    }

    [Fact]
    public void RemoveNote_returns_NoContent_withExistingNoteId()
    {
        // arrange
        _mockRepo.Setup(x => x.DeleteNote(It.IsAny<int>())).ReturnsAsync(true);

        // act
        var result = _controller.RemoveNote(1);
        var statusCode = (result.Result as NoContentResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        statusCode.Equals(204);
    }

    [Fact]
    public void RemoveNote_returns_BadRequest_withNotExistingNoteId()
    {
        // arrange
        _mockRepo.Setup(x => x.DeleteNote(It.IsAny<int>())).ReturnsAsync(false);

        // act
        var result = _controller.RemoveNote(1);
        var data = (result.Result as BadRequestObjectResult)!.Value.As<string>();
        var statusCode = (result.Result as BadRequestObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.Should().Be("Id not found.");
        statusCode.Equals(400);
    }

    [Fact]
    public void PostNote_returns_CreatedAtAction_withExistingUserIdAndRecipeId()
    {
        // arrange
        var user = new User { Id = 1 };
        var recipe = new Recipe { Id = 1 };
        var noteDto = new NoteDto
        {
            NoteText = "Hello",
            RecipeId = recipe.Id,
            UserId = user.Id
        };
        _mockRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns(user);
        _mockRepo.Setup(x => x.GetRecipe(It.IsAny<int>())).Returns(recipe);
        _mockRepo.Setup(x => x.AddNote(noteDto, user, recipe)).ReturnsAsync(new NoteResponse
        {
            NoteId = 1,
            NoteText = noteDto.NoteText,
            RecipeId = recipe.Id,
            UserId = user.Id
        });

        // act
        var result = _controller.PostNote(noteDto);
        var data = (result.Result as CreatedAtActionResult)!.Value.As<NoteResponse>();
        var statusCode = (result.Result as CreatedAtActionResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        data.NoteText.Should().Be("Hello");
        statusCode.Equals(201);
    }
    
    [Fact]
    public void PostNote_returns_BadRequest_whenIncorrectUserIdOrRecipeId()
    {
        // arrange
        _mockRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User?)null);
        _mockRepo.Setup(x => x.GetRecipe(It.IsAny<int>())).Returns((Recipe?)null);
        var noteDto = new NoteDto
        {
            NoteText = "Hello",
            RecipeId = 5,
            UserId = 5
        };

        // act
        var result = _controller.PostNote(noteDto);
        var statusCode = (result.Result as BadRequestObjectResult)!.StatusCode.As<int>();

        // assert
        result.Should().NotBeNull();
        statusCode.Should().Be(400);
    }
}