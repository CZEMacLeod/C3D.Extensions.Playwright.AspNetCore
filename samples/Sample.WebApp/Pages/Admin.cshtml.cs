using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sample.WebApp.Pages;

[Authorize(Security.Policy.AdminPolicy)]
public class AdminModel : PageModel
{
    public void OnGet()
    {
    }
}
