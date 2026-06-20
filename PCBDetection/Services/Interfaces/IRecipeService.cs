using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface IRecipeService
{
    RecipeProfile CurrentRecipe { get; }

    Task<RecipeProfile> LoadCurrentRecipeAsync(CancellationToken cancellationToken);

    Task SaveCurrentRecipeAsync(RecipeProfile recipe, CancellationToken cancellationToken);
}
