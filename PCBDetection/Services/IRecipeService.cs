using PCBDetection.Models;

namespace PCBDetection.Services;

public interface IRecipeService
{
    RecipeProfile CurrentRecipe { get; }

    Task<RecipeProfile> LoadCurrentRecipeAsync(CancellationToken cancellationToken);

    Task SaveCurrentRecipeAsync(RecipeProfile recipe, CancellationToken cancellationToken);
}
