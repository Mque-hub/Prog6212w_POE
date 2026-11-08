using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Contracts.DTOs;
using RaceDay.Mvc.Services;

using RaceDay.Mvc.ViewModels;
public class AuthController : Controller
{
    private readonly RaceDayApiClient _api;
    public AuthController(RaceDayApiClient api) => _api = api;
    [HttpGet]
    public IActionResult Register() => View();
    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        object request;

        if (vm.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            request = new RegisterOrganiserRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Address = vm.Address,
                Email = vm.Email,
                Password = vm.Password
            };
        }
        else
        {
            request = new RegisterParticipantRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                CellNumber = vm.CellNumber!,// in the same way that ? means nullable, ! tells the complier that you expect a value for CellNumber
                Address = vm.Address,
                Email = vm.Email,
                Password = vm.Password
            };
        }

        var response = await _api.SendAsync(HttpMethod.Post, "api/auth/register", request);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(vm);
        }
        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }
    [HttpGet]
    public IActionResult Login() => View();
    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var response = await _api.SendAsync(HttpMethod.Post, "api/auth/login",
            new LoginRequestDTO { Email = vm.Email, Password = vm.Password });
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }
        var auth = await _api.ReadAsync<AuthResponse>(response);
        if (auth == null)
        {
            ModelState.AddModelError("", "Login response could not be read.");
            return View(vm);
        }
        string? apiCookie = RaceDayApiClient.ExtractSessionCookie(response);

        if (!string.IsNullOrWhiteSpace(apiCookie))
            HttpContext.Session.SetString("ApiSessionCookie", apiCookie);

        HttpContext.Session.SetInt32("UserId", auth.UserID);
        HttpContext.Session.SetString("Role", auth.AuthenticationRole);
        HttpContext.Session.SetString("DisplayName", auth.Email);

        if (auth.AuthenticationRole.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Organiser");

        return RedirectToAction("Index", "Participant");
    }
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.SendAsync(HttpMethod.Post, "api/auth/logout");
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
