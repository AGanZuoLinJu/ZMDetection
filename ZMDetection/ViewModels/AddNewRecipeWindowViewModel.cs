using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZMDetection.EventAggregator;
using ZMDetection.Models;
using ZMDetection.Services;

namespace ZMDetection.ViewModels
{
    public sealed class AddNewRecipeWindowViewModel : BindableBase
    {
        private readonly IAddRecipeService addRecipeService;

        private string newRecipeName = string.Empty;
        public string NewRecipeName
        {
            get => newRecipeName;
            set => SetProperty(ref newRecipeName, value);
        }
        private string errorMsg = string.Empty;
        public string ErrorMessage
        {
            get => errorMsg;
            set => SetProperty(ref errorMsg, value);
        }
        public AddNewRecipeWindowViewModel(IAddRecipeService addRecipeService)
        {
            this.addRecipeService = addRecipeService;
            this.AddNewRecipeCommand = new DelegateCommand(AddNewRecipe);
        }
        public DelegateCommand AddNewRecipeCommand { get; }
        private void AddNewRecipe()
        {
            if(string.IsNullOrEmpty(NewRecipeName))
            {
                ErrorMessage = "机种名不能为空!";
                return;
            }
            foreach(var recipe in RecipeParam.RecipeParamConfig!.AllRecipeName!)
            {
                if (recipe.RecipeName!.Equals(NewRecipeName))
                {
                    ErrorMessage = "该机种已存在,不能重复添加!";
                    return;
                }
            }
            if (this.addRecipeService.AddNewRecipe(NewRecipeName))
            {
                MessageBox.Show(Application.Current.MainWindow, $"添加新机种[{NewRecipeName}]成功!");
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow, $"添加新机种[{NewRecipeName}]失败!");
            }
        }
    }
}
