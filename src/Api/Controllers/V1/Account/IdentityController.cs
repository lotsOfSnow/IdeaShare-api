using System;
using System.Threading.Tasks;
using AutoMapper;
using IdeaShare.Api.Models;
using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Application.Options;
using IdeaShare.Application.Interfaces;
using IdeaShare.Contracts.V1.Requests;
using IdeaShare.Contracts.V1.Responses;
using IdeaShare.Extensions.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaShare.Api.Controllers.V1.Account
{
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IAccountService _identityService;
        private readonly IMapper _mapper;

        public IdentityController(IAccountService identityService, IMapper mapper)
        {
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _mapper = mapper;
        }

        /// <summary>
        /// Attempts to create a new user.
        /// </summary>
        /// <param name="registrationRequest">Model containing the information about the new user.</param>
        /// <returns>Created status on success, BadRequest with validation info on failure.</returns>
        [AllowAnonymous]
        [HttpPost(Contracts.V1.ApiRoutes.Users.Register)]
        public async Task<IActionResult> Register(AppUserRegistrationRequest registrationRequest)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Get", "Users");
            }

            var registrationResult = await _identityService.RegisterAsync(registrationRequest.Username, registrationRequest.Email, registrationRequest.Password);

            if (!registrationResult.Success)
            {
                foreach (var error in registrationResult.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                return BadRequest(new ValidationResultModel(ModelState));
            }

            return Created($"{Contracts.V1.ApiRoutes.Articles.GetSingle.FillRouteParameters(registrationResult.Payload.Id)}", _mapper.Map<AppUserDataModel>(registrationResult.Payload));
        }

        /// <summary>
        /// Attempts to sign in the user.
        /// </summary>
        /// <param name="loginRequest">Model containing the information required to sign in.</param>
        /// <returns>Ok status with auth token on success, BadRequest with validation info on fail.</returns>
        [AllowAnonymous]
        [HttpPost(Contracts.V1.ApiRoutes.Session.Create)]
        public async Task<IActionResult> Login(AppUserLoginRequest loginRequest)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Get", "Users");
            }

            var loginResult = await _identityService.LoginAsync(loginRequest.Email, loginRequest.Password);

            if(!loginResult.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = loginResult.Errors
                });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = loginResult.Token,
            });
        }
    }
}
