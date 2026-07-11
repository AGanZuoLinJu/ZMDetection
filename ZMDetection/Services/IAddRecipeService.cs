using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZMDetection.Services
{
    public interface IAddRecipeService
    {
        bool ShowDialog(Window? owner);
        bool AddNewRecipe(string name);
    }
}
