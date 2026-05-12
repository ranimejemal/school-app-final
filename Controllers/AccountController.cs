using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Models;
using SchoolApp.Models.ViewModels;

namespace SchoolApp.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser>  _users;
    private readonly ILogger<AccountController>    _logger;

    public AccountController(SignInManager<ApplicationUser> signIn,
                              UserManager<ApplicationUser>  users,
                              ILogger<AccountController>    logger)
    {
        _signIn = signIn;
        _users  = users;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signIn.PasswordSignInAsync(
            model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {User} logged in at {Time}.", model.UserName, DateTime.UtcNow);
            HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString("o"));
            HttpContext.Session.SetString("UserAgent", Request.Headers.UserAgent.ToString());

            // Check if user must change their password (first login)
            var user = await _users.FindByNameAsync(model.UserName);
            if (user != null && user.MustChangePassword)
                return RedirectToAction(nameof(ForceChangePassword));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, rememberMe = model.RememberMe });

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {User} is locked out.", model.UserName);
            ModelState.AddModelError(string.Empty, "Compte temporairement bloqué. Réessayez dans quelques minutes.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Identifiant ou mot de passe incorrect.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
    {
        var user = await _signIn.GetTwoFactorAuthenticationUserAsync();
        if (user == null) return RedirectToAction(nameof(Login));
        ViewData["ReturnUrl"]  = returnUrl;
        ViewData["RememberMe"] = rememberMe;
        return View(new TwoFactorVerifyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(TwoFactorVerifyViewModel model, bool rememberMe, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _signIn.GetTwoFactorAuthenticationUserAsync();
        if (user == null) return RedirectToAction(nameof(Login));

        var code   = model.Code.Replace(" ", "").Replace("-", "");
        var result = await _signIn.TwoFactorAuthenticatorSignInAsync(code, rememberMe, model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {User} passed 2FA.", user.UserName);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut) { ModelState.AddModelError(string.Empty, "Compte bloqué."); return View(model); }
        ModelState.AddModelError(string.Empty, "Code invalide. Vérifiez votre application d'authentification.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User {User} logged out.", User.Identity?.Name);
        HttpContext.Session.Clear();
        await _signIn.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();

    // ── Force Change Password (first login) ───────────────────────────────────

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ForceChangePassword()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        if (!user.MustChangePassword) return RedirectToAction("Index", "Home");
        return View(new ForceChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceChangePassword(ForceChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        // Remove old password and set the new one (admin-created accounts have no "old" token challenge)
        var token  = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, model.NewPassword);

        if (result.Succeeded)
        {
            user.MustChangePassword = false;
            await _users.UpdateAsync(user);
            await _signIn.RefreshSignInAsync(user);
            TempData["Success"] = "Mot de passe modifié avec succès. Bienvenue !";
            return RedirectToAction("Index", "Home");
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError(string.Empty, err.Description);
        return View(model);
    }

    // ── Security / 2FA ────────────────────────────────────────────────────────

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> TwoFactorSetup()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var isEnabled = await _users.GetTwoFactorEnabledAsync(user);
        if (isEnabled) return View(new TwoFactorSetupViewModel { IsEnabled = true });

        await _users.ResetAuthenticatorKeyAsync(user);
        var key = await _users.GetAuthenticatorKeyAsync(user);
        return View(new TwoFactorSetupViewModel
        {
            SharedKey = FormatKey(key!),
            QrCodeUrl = GenerateQrCodeUri(user.UserName!, key!),
            IsEnabled = false
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactorSetup(TwoFactorVerifyViewModel model)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            var key2 = await _users.GetAuthenticatorKeyAsync(user);
            return View(new TwoFactorSetupViewModel { SharedKey = FormatKey(key2 ?? ""), QrCodeUrl = GenerateQrCodeUri(user.UserName!, key2 ?? "") });
        }

        var code    = model.Code.Replace(" ", "").Replace("-", "");
        var isValid = await _users.VerifyTwoFactorTokenAsync(user, _users.Options.Tokens.AuthenticatorTokenProvider, code);

        if (!isValid)
        {
            ModelState.AddModelError(string.Empty, "Code invalide. Vérifiez votre application d'authentification.");
            var key3 = await _users.GetAuthenticatorKeyAsync(user);
            return View(new TwoFactorSetupViewModel { SharedKey = FormatKey(key3 ?? ""), QrCodeUrl = GenerateQrCodeUri(user.UserName!, key3 ?? "") });
        }

        await _users.SetTwoFactorEnabledAsync(user, true);
        TempData["Success"] = "Authentification à deux facteurs activée avec succès !";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable2fa()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        await _users.SetTwoFactorEnabledAsync(user, false);
        TempData["Success"] = "Authentification à deux facteurs désactivée.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Security()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        ViewData["Is2faEnabled"] = await _users.GetTwoFactorEnabledAsync(user);
        ViewData["LoginTime"]    = HttpContext.Session.GetString("LoginTime");
        ViewData["UserAgent"]    = HttpContext.Session.GetString("UserAgent");
        ViewData["FailedCount"]  = await _users.GetAccessFailedCountAsync(user);
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword) { TempData["Error"] = "Les mots de passe ne correspondent pas."; return RedirectToAction(nameof(Security)); }
        if (newPassword.Length < 6)         { TempData["Error"] = "Au moins 6 caractères requis.";           return RedirectToAction(nameof(Security)); }

        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var result = await _users.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded) { await _signIn.RefreshSignInAsync(user); TempData["Success"] = "Mot de passe modifié avec succès."; }
        else                  { TempData["Error"] = "Mot de passe actuel incorrect."; }

        return RedirectToAction(nameof(Security));
    }

    private static string FormatKey(string key)
    {
        var sb = new System.Text.StringBuilder();
        int pos = 0;
        while (pos + 4 < key.Length) { sb.Append(key.AsSpan(pos, 4)); sb.Append(' '); pos += 4; }
        if (pos < key.Length) sb.Append(key.AsSpan(pos));
        return sb.ToString().ToLowerInvariant();
    }

    private static string GenerateQrCodeUri(string email, string key)
        => string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            Uri.EscapeDataString("SchoolApp"),
            Uri.EscapeDataString(email),
            key);
}
