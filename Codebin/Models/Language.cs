using Microsoft.AspNetCore.Mvc.Rendering;

namespace Codebin.Models
{
    public class Language
    {
        public static IEnumerable<SelectListItem> GetLanguages() 
        {
            return new List<SelectListItem>
            {
                new SelectListItem (){ Text = "C", Value = "C" },
                new SelectListItem (){ Text = "C++", Value = "CPP" },
                new SelectListItem (){ Text = "C#", Value = "Csharp" },
                new SelectListItem (){ Text = "CSS", Value = "CSS" },
                new SelectListItem (){ Text = "Java", Value = "Java" },
                new SelectListItem (){ Text = "JavaScript", Value = "JavaScript" },
                new SelectListItem (){ Text = "JSON", Value = "JSON" },
                new SelectListItem (){ Text = "Go", Value = "Go" },
                new SelectListItem (){ Text = "HTML", Value = "HTML" },
                new SelectListItem (){ Text = "PHP", Value = "PHP" },
                new SelectListItem (){ Text = "Python", Value = "Python" },
                new SelectListItem (){ Text = "Ruby", Value = "Ruby" },
                new SelectListItem (){ Text = "Rust", Value = "Rust" },
                new SelectListItem (){ Text = "Scala", Value = "Scala" },
                new SelectListItem (){ Text = "SQL", Value = "SQL" },
                new SelectListItem (){ Text = "Swift", Value = "Swift" },
                new SelectListItem (){ Text = "TypeScript", Value = "TypeScript" }
            };
        }
    }
}
