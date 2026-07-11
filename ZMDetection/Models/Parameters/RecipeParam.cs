using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using ZMDetection.Tools;

namespace ZMDetection.Models
{
    [Serializable]
    [XmlRoot("RecipeParam")]
    public class RecipeParam
    {
        #region 单例
        public RecipeParam() { }
        private static RecipeParam? _recipeParam;
        private static readonly object? _lockObj = new object();
        public static RecipeParam? RecipeParamConfig
        {
            get
            {
                if (_recipeParam == null)
                {
                    lock (_lockObj!)
                    {
                        if (_recipeParam == null)
                        {
                            _recipeParam = new RecipeParam();
                        }
                    }
                }
                return _recipeParam;
            }
            set
            {
                _recipeParam = value;
            }
        }
        #endregion

        #region 属性
        /// <summary>
        /// 当前的机种名
        /// </summary>
        [XmlElement("CurrentRecipeName")]
        public string? CurrentRecipeName { get; set; }
        /// <summary>
        /// 所有导入的机种
        /// </summary>
        [XmlElement("Recipes")]
        public List<RecipeEntry>? AllRecipeName { get; set; }
        /// <summary>
        /// 当前跑的机种信息
        /// </summary>
        [XmlIgnore]
        public static RecipeEntry? CurrentRecipeInfo { get; set; }
        #endregion

        [XmlInclude(typeof(RecipeEntry))]
        public class RecipeEntry
        {
            public RecipeEntry() { }
            [XmlAttribute("Name")]
            public string? RecipeName { get; set; } = "AAAAAA";
        }
    }
}
