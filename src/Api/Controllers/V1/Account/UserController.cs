using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using IdeaShare.Api.Models;
using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Application.Models.ArticleModels;
using IdeaShare.Application.Models.CommentModels;
using IdeaShare.Application.Services;
using IdeaShare.Contracts.V1.Responses;
using IdeaShare.Extensions.Microsoft.AspNetCore.Mvc.ModelBinding;
using IdeaShare.Extensions.System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaShare.Api.Controllers.V1.Account
{
    public class UserController : ControllerBase
    {
        private readonly IAccountService _identityService;
        private readonly IArticleService _articleService;
        private readonly IMapper _mapper;
        private readonly ICommentService _commentService;

        public UserController(IAccountService identityService, IMapper mapper, IArticleService articleService, ICommentService commentService)
        {
            _identityService = identityService;
            _mapper = mapper;
            _articleService = articleService;
            _commentService = commentService;
        }

        [Authorize]
        [HttpGet(Contracts.V1.ApiRoutes.User.Get)]
        public async Task<IActionResult> Get()
        {
            var activeUser = await _identityService.GetCurrentUserAsync(User.GetId());

            if (activeUser == null)
            {
                return BadRequest();
            }

            var activeUserMapped = _mapper.Map<AppUserDataModel>(activeUser);

            var likedArticlesRequestResult = await _articleService.GetLikedByUserAsync(activeUser.UserName, null);

            var mappedLikedArticles = likedArticlesRequestResult.Success ? _mapper.Map<IEnumerable<ArticlePreviewModel>>(likedArticlesRequestResult.Payload) : null;

            var response = new ActiveUserResponse
            {
                User = activeUserMapped,
                LikedArticles = mappedLikedArticles
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet(Contracts.V1.ApiRoutes.User.Liked.GetAll)]
        public async Task<IActionResult> GetLikedArticles(string? sort)
        {
            var result = await _articleService.GetLikedByUserAsync(User.GetUsername(), sort);

            if (!result.Success)
            {
                ModelState.AddModelErrors(result.Errors);
                return NotFound(new ValidationResultModel(ModelState));
            }

            return Ok(_mapper.Map<IEnumerable<ArticlePreviewModel>>(result.Payload));
        }

        [Authorize]
        [HttpGet(Contracts.V1.ApiRoutes.User.Liked.GetSpecific)]
        public async Task<IActionResult> IsArticleLiked(int articleId)
        {
            var result = await _articleService.IsArticleLikedByUserAsync(User.GetUsername(), articleId);

            if (!result.Success)
            {
                ModelState.AddModelErrors(result.Errors);
                return NotFound(new ValidationResultModel(ModelState));
            }

            if (result.Payload)
            {
                return NoContent();
            }

            return NotFound();
        }

        [Authorize]
        [HttpGet(Contracts.V1.ApiRoutes.User.Articles.Comments)]
        public async Task<IActionResult> ArticlesComments()
        {
            var comments = await _commentService.GetAllForUserAsync(User.GetId());

            return Ok(_mapper.Map<IEnumerable<CommentDetailsModel>>(comments));
        }
    }
}