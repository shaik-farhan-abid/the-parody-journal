using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    // GET: Account/Login
    public IActionResult Login()
    {
        return View();  // It will look for Views/Account/Login.cshtml
    }
}
