using Microsoft.AspNetCore.Mvc.Rendering;

namespace Codebin.Models
{
    public class Language
    {
        public static IEnumerable<SelectListItem> GetLanguages() 
        {
            return new List<SelectListItem>
            {
                new SelectListItem (){ Text = "C++", Value = "CPP" },
                new SelectListItem (){ Text = "JavaScript", Value = "JavaScript" },
                new SelectListItem (){ Text = "C#", Value = "Csharp" },
                new SelectListItem (){ Text = "Go", Value = "Go" }
            };
        }
    }
}
