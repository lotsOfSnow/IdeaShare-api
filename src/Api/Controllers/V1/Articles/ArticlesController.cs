using AutoMapper;
using IdeaShare.Api.Models;
using IdeaShare.Application.Models.ArticleModels;
using IdeaShare.Application.Models.CommentModels;
using IdeaShare.Application.Interfaces;
using IdeaShare.Extensions.Microsoft.AspNetCore.Mvc.ModelBinding;
using IdeaShare.Extensions.System;
using IdeaShare.Extensions.System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdeaShare.Api.Areas.Articles.Controllers
{
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IArticleService _articleService;
        private readonly IMapper _mapper;

        public ArticlesController(IArticleService articleService, IMapper mapper, ICommentService commentService)
        {
            _articleService = articleService;
            _mapper = mapper;
            _commentService = commentService;
        }

        /// <summary>
        /// Returns articles from the database.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <param name="sort"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet(Contracts.V1.ApiRoutes.Articles.GetAll)]
        public async Task<IActionResult> GetMultiple(int? page, int? perPage, string? sort, string? tag, [ModelBinder(typeof(CommaDelimiterArrayModelBinder), Name = "Title")] IEnumerable<string>? title, ArticleType type = ArticleType.NORMAL)
        {
            //if(!string.IsNullOrEmpty(withTag))
            //{
            //    articlesFromDatabase = await _articleService.GetByTagAsync(withTag);
            //}
            //else

            if((!page.HasValue && perPage.HasValue) || (page.HasValue && !perPage.HasValue))
            {
                return BadRequest("You have to provide both page and perPage parameters for pagination.");
            }

            int startIndex = -1, length = -1;

            if(page.HasValue && perPage.HasValue)
            {
                startIndex = perPage.Value * (page.Value - 1);
                length = perPage.Value;
            }

            var articlesFromDatabase = await _articleService.GetRangeAsync(startIndex, length, sort, title, tag);

            Response.Headers.Add("X-Total-Count", (await _articleService.GetCountAsync(tag)).ToString());

            switch (type)
            {
                case ArticleType.NORMAL:
                    return Ok(_mapper.Map<IEnumerable<ArticleDetailsModel>>(articlesFromDatabase));
                case ArticleType.PREVIEW:
                    return Ok(_mapper.Map<IEnumerable<ArticlePreviewModel>>(articlesFromDatabase));
                default:
                    return BadRequest();
            }
        }

        [HttpGet(Contracts.V1.ApiRoutes.Articles.GetSingle)]
        public async Task<IActionResult> GetSingle(int articleId)
        {
            var article = await _articleService.GetAsync(articleId);

            if(article == null)
            {
                return NotFound($"Article with an ID of {articleId} cannot be found.");
            }

            var articleDisplayDto = _mapper.Map<ArticleDetailsModel>(article);

            return Ok(articleDisplayDto);
        }

        [HttpPost(Contracts.V1.ApiRoutes.Articles.Create)]
        public async Task<IActionResult> Create([FromForm] ArticleCreationModel model)
        {
            var result = await _articleService.CreateAsync(User.GetId(), model);

            if(result.Success)
            {
                return Created($"{Contracts.V1.ApiRoutes.Articles.GetSingle.FillRouteParameters(result.Payload.Id)}", _mapper.Map<ArticleDetailsModel>(result.Payload));
            }

            return BadRequest();
        }

        [Authorize]
        [HttpDelete(Contracts.V1.ApiRoutes.Articles.Delete)]
        public async Task<IActionResult> Delete(int articleId)
        {
            var result = await _articleService.RemoveAsync(User.GetId(), articleId);

            if (result.Success)
            {
                return NoContent();
            }

            ModelState.AddModelErrors(result.Errors);

            if(result.Errors.ContainsKey("Article"))
            {
                return NotFound(new ValidationResultModel(ModelState));
            }

            if (result.Errors.ContainsKey("User"))
            {
                return Unauthorized(new ValidationResultModel(ModelState));
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }


        [HttpGet(Contracts.V1.ApiRoutes.Articles.Comments.Get)]
        public async Task<IActionResult> GetComments(int articleId)
        {
            var result = await _commentService.GetForArticleAsync(articleId);
            if(!result.Success)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                return BadRequest(new ValidationResultModel(ModelState));
            }

            return Ok(_mapper.Map<IEnumerable<CommentDetailsModel>>(result.Payload));
        }

        [Authorize]
        [HttpPost(Contracts.V1.ApiRoutes.Articles.Likes.Create)]
        public async Task<IActionResult> AddLike(int articleId)
        {
            var result = await _articleService.AddLikeAsync(User.GetId(), articleId);

            if (result.Success)
            {
                return NoContent();
            }

            ModelState.AddModelErrors(result.Errors);

            return BadRequest(new ValidationResultModel(ModelState));
        }

        [Authorize]
        [HttpDelete(Contracts.V1.ApiRoutes.Articles.Likes.Delete)]
        public async Task<IActionResult> RemoveLike(int articleId)
        {
            var result = await _articleService.RemoveLikeByUserAsync(User.GetId(), articleId);

            if (result.Success)
            {
                return NoContent();
            }

            ModelState.AddModelErrors(result.Errors);

            return BadRequest(new ValidationResultModel(ModelState));
        }


        [Authorize]
        [HttpPost(Contracts.V1.ApiRoutes.Articles.Comments.Create)]
        public async Task<IActionResult> PostComment(int articleId, PostCommentModel model)
        {
            await _commentService.AddAsync(articleId, User.GetId(), model.Body);

            return Ok(model);
        }

        [Authorize]
        [HttpDelete(Contracts.V1.ApiRoutes.Articles.Comments.Delete)]
        public async Task<IActionResult> DeleteComment (int articleId, int commentId)
        {
            var result = await _commentService.RemoveCommentAsync(commentId, User.GetId());

            if(result.Success)
            {
                return NoContent();
            }

            ModelState.AddModelErrors(result.Errors);

            if(result.Errors.ContainsKey("Comment"))
            {
                return NotFound(new ValidationResultModel(ModelState));
            }

            if(result.Errors.ContainsKey("User"))
            {
                return Unauthorized(new ValidationResultModel(ModelState));
            }

            return StatusCode(StatusCodes.Status500InternalServerError);

        }

        [HttpPost(Contracts.V1.ApiRoutes.Articles.Update)]
        public async Task<IActionResult> Update([FromForm] ArticleEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _articleService.UpdateAsync(model, User.GetId());

            if(result.Success)
            {
                return NoContent();
            }

            ModelState.AddModelErrors(result.Errors);

            return BadRequest(new ValidationResultModel(ModelState));
        }
    }

    public enum ArticleType
    {
        NORMAL,
        PREVIEW
    }
}
