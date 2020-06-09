using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdeaShare.Api.Models;
using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Application.Models.ArticleModels;
using IdeaShare.Application.Services;
using IdeaShare.Extensions.Microsoft.AspNetCore.Mvc.ModelBinding;
using IdeaShare.Extensions.System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaShare.Api.Controllers.V1.Users
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IArticleService _articleService;

        public UsersController(IUserService userService, IMapper mapper, IArticleService articleService, IAccountService accountService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper;
            _articleService = articleService;
            _accountService = accountService;
        }

        [HttpGet(Contracts.V1.ApiRoutes.Users.Get)]
        public async Task<IActionResult> Get(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest();
            }
            
            var user = await _userService.GetUserAsync(username);

            if(user == null)
            {
                return NotFound($"User {username} does not exist");
            }

            var mapped = _mapper.Map<AppUserDetailsModel>(user);

            return Ok(mapped);
        }

        [HttpGet(Contracts.V1.ApiRoutes.Users.Articles.Get)]
        public async Task<IActionResult> GetArticles(string username, string? sort)
        {
            var result = await _articleService.GetByUserAsync(username, sort);

            if(!result.Success)
            {
                ModelState.AddModelErrors(result.Errors);
                return NotFound(new ValidationResultModel(ModelState));
            }

            Response.Headers.Add("X-Total-Count", result.Payload.Count().ToString());
            return Ok(_mapper.Map<IEnumerable<ArticlePreviewModel>>(result.Payload));
        }

        [HttpGet(Contracts.V1.ApiRoutes.Users.Liked.Get)]
        public async Task<IActionResult> GetLikedArticles(string username, string? sort)
        {
            var result = await _articleService.GetLikedByUserAsync(username, sort);

            if(!result.Success)
            {
                ModelState.AddModelErrors(result.Errors);
                return NotFound(new ValidationResultModel(ModelState));
            }

            return Ok(_mapper.Map<IEnumerable<ArticlePreviewModel>>(result.Payload));
        }

        [Authorize]
        [HttpPost(Contracts.V1.ApiRoutes.Users.Update)]
        public async Task<IActionResult> Update(string username, [FromForm] AppUserUpdateModel updateModel)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest();
            }

            if (User.GetUsername() != username)
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserAsync(username);

            if (user == null)
            {
                return NotFound($"User {username} does not exist");
            }

            var result = await _accountService.UpdateAsync(user, updateModel);

            if(result.Success)
            {
                return NoContent();
            }

            ModelState.AddModelErrors(result.Errors);

            return BadRequest(new ValidationResultModel(ModelState));
        }
    }
}