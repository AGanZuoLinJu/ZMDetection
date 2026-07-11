using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZMDetection.Models;
using ZMDetection.Views;
using static ZMDetection.Models.RecipeParam;

namespace ZMDetection.Services
{
    public sealed class AddRecipeService : IAddRecipeService
    {
        private readonly IContainerProvider containerProvider;
        public AddRecipeService(IContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
        }
        public bool ShowDialog(Window? owner)
        {
            AddNewRecipeWindow window = this.containerProvider.Resolve<AddNewRecipeWindow>();
            window.Owner = owner;
            return window.ShowDialog() == true;
        }
        public bool AddNewRecipe(string name)
        {
            RecipeParam.RecipeParamConfig!.AllRecipeName!.Add(new RecipeEntry
            {
                RecipeName = name,
            });
            return true;
        }
    }
}
