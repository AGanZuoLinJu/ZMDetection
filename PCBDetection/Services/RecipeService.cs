using PCBDetection.Models;
using System.IO;

namespace PCBDetection.Services;

public sealed class RecipeService : IRecipeService
{
    private readonly string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "CurrentRecipe.txt");

    public RecipeProfile CurrentRecipe { get; private set; } = new("PCB_TOP_AOI_V1");

    public Task<RecipeProfile> LoadCurrentRecipeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(settingsPath))
        {
            var recipeName = File.ReadAllText(settingsPath).Trim();
            if (!string.IsNullOrWhiteSpace(recipeName))
            {
                CurrentRecipe = new RecipeProfile(recipeName);
            }
        }

        return Task.FromResult(CurrentRecipe);
    }

    public Task SaveCurrentRecipeAsync(RecipeProfile recipe, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CurrentRecipe = recipe;

        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
        File.WriteAllText(settingsPath, recipe.RecipeName);
        return Task.CompletedTask;
    }
}
