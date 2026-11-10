using Azure;
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
        string endpoint;

        if (vm.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            endpoint = "api/auth/register/organiser";

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
            endpoint = "api/auth/register/participant";

            request = new RegisterParticipantRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                CellNumber = vm.CellNumber!,
                Address = vm.Address,
                Email = vm.Email,
                Password = vm.Password
            };
        }

        var response = await _api.SendAsync(HttpMethod.Post, endpoint, request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine("Registration URL: " + endpoint);
        Console.WriteLine("Registration Status: " + response.StatusCode);
        Console.WriteLine("Registration Response: " + responseBody);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", responseBody);
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
        if (!ModelState.IsValid)
            return View(vm);

        var response = await _api.SendAsync(
            HttpMethod.Post,
            "api/auth/login",
            new LoginRequestDTO
            {
                AuthenticationRole = vm.AuthenticationRole,
                Email = vm.Email,
                Password = vm.Password
            });

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

        HttpContext.Session.SetInt32("UserId", auth.UserID);
        HttpContext.Session.SetString("Role", auth.AuthenticationRole);
        HttpContext.Session.SetString("DisplayName", auth.Email);
        HttpContext.Session.SetString("AccessToken", auth.AccessToken);

        if (auth.AuthenticationRole.Equals(
            "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Organiser");
        }

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
