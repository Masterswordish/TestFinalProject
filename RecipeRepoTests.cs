using FluentAssertions;
using RecipeApi.Controllers;
using RecipeApi.Data;
using Moq;
using RecipeApi.Models;

namespace RecipeApiTests;

public class RecipeRepoTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new Mock<IUserRepository>();
    private readonly RecipeContext _context;

    public RecipeRepoTests(RecipeContext context)
    {
        _context = context;
    }
    //
    // [Fact]
    // public void should_flatten_array()
    // {
    //     // arrange
    //     var inputArray = new[] { "Hello", "World" };
    //     _recipeRepoMock.Setup(x => x.FlattenArray(inputArray))
    //         .Returns("Poopy");
    //
    //     // act
    //     var result = _recipeRepo.FlattenArray(inputArray);
    //
    //     // assert
    //     result.Should().Be("Poopy");
    // }    
    //
    // [Fact]
    // public void should_flatten_array_empty()
    // {
    //     // arrange
    //     var inputArray = Array.Empty<string>();
    //
    //     // act
    //     var result = _recipeRepo.FlattenArray(inputArray);
    //
    //     // assert
    //     result.Should().Be("");
    // }
    //
    // [Fact]
    // public void should_create_array()
    // {
    //     // arrange
    //     var inputString = "Hello|World";
    //
    //     // act
    //     var result = _recipeRepo.CreateArray(inputString);
    //     var expected = new[] { "Hello", "World" };
    //     
    //     // assert
    //     Assert.Equal(expected, result);
    // }    
    //
    // [Fact]
    // public void should_create_array_from_empty_string()
    // {
    //     // arrange
    //     var inputString = Array.Empty<string>();
    //
    //     // act
    //     var result = _recipeRepo.FlattenArray(inputString);
    //     var expected = "";
    //
    //     // assert
    //     Assert.Equal(expected, result);
    // }

    // [Fact]
    // public void should_return_recipe_response()
    // {
    //     //arrange
    //     var recipeDto = new RecipeDto
    //     {
    //         Ingredients = new[] { "butter", "bacon" },
    //         Instructions = new[] { "fried" },
    //         Title = "Fried Bacon",
    //         UserId = 6
    //     };
    //     //act
    //     var repo = new UserRepository(_context);
    //     var result = repo.AddRecipe(recipeDto);
    //     
    //     //assert
    //     result.Result.Title.Equals("Fried Bacon");
    // }
    
    // [Fact]
    // public void should_return_true_for_existing_user()
    // {
    //     // arrange
    //     var repo = new UserRepository(_context);
    //     var input = 6;
    //     
    //     // act
    //     var result = repo.UserInDb(input);
    //
    //     // assert
    //     result.Should().Be(true);
    // }
}