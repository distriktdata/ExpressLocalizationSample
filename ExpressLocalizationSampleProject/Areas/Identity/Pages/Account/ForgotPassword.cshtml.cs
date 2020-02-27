﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ExpressLocalizationSampleProject.LocalizationResources;
using LazZiya.ExpressLocalization;
using LazZiya.ExpressLocalization.DataAnnotations;
using LazZiya.ExpressLocalization.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpressLocalizationSampleProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ISharedCultureLocalizer _loc;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender, ISharedCultureLocalizer loc)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _loc = loc;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [ExRequired]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var culture = CultureInfo.CurrentCulture.Name;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation", new { culture });
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code, culture },
                    protocol: Request.Scheme);

                var str = _loc.GetLocalizedString(key:LocalizedBackendMessages.ResetPasswordEmailBody, args: HtmlEncoder.Default.Encode(callbackUrl));

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    _loc.GetLocalizedString(LocalizedBackendMessages.ResetPasswordEmailTitle),
                    str);

                return RedirectToPage("./ForgotPasswordConfirmation", new { culture });
            }

            return Page();
        }
    }
}
