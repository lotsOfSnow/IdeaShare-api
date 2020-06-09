using AutoMapper;
using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Application.Models.ArticleModels;
using IdeaShare.Application.Models.CommentModels;
using IdeaShare.Application.Options;
using IdeaShare.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace IdeaShare.Application.Common.Mapping
{
    public class SetFeaturedImageUrlAction : IMappingAction<Article, ArticleBaseModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageSettings _imageSettings;

        public SetFeaturedImageUrlAction(IHttpContextAccessor httpContextAccessor, ImageSettings imageSettings)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _imageSettings = imageSettings ?? throw new ArgumentNullException(nameof(imageSettings));
        }

        public void Process(Article source, ArticleBaseModel destination, ResolutionContext context)
        {
            if(source.FeaturedImage == null)
            {
                return;
            }

            destination.FeaturedImage = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}/{_imageSettings.FeaturedImage.Location}/{source.FeaturedImage}";
        }
    }

    public class SetProfilePictureUrlAction : IMappingAction<AppUser, AppUserBaseModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ImageSettings _imageSettings;

        public SetProfilePictureUrlAction(IHttpContextAccessor httpContextAccessor, ImageSettings imageSettings)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _imageSettings = imageSettings ?? throw new ArgumentNullException(nameof(imageSettings));
        }

        public void Process(AppUser source, AppUserBaseModel destination, ResolutionContext context)
        {
            if (source.ProfilePicture == null)
            {
                return;
            }

            destination.ProfilePicture = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}/{_imageSettings.ProfilePicture.Location}/{source.ProfilePicture}";
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //AppUser
            CreateMap<AppUser, AppUserBaseModel>()
                .AfterMap<SetProfilePictureUrlAction>()
                .Include<AppUser, AppUserDetailsModel>()
                .Include<AppUser, AppUserDataModel>();

            CreateMap<AppUser, AppUserDetailsModel>()
                .ForMember(dest => dest.FirstName, opt => opt.Condition(src => (!src.IsRealNameHidden)))
                .ForMember(dest => dest.LastName, opt => opt.Condition(src => (!src.IsRealNameHidden)))
                .ForMember(dest => dest.ArticlesWritten, opt => opt.MapFrom(src => src.Articles.Count))
                .ForMember(dest => dest.LikesReceived, opt => opt.MapFrom(src => src.Articles.Select(x => x.Likes.Count).Sum()));

            CreateMap<AppUser, AppUserDataModel>()
                .ForMember(dest => dest.ArticlesWritten, opt => opt.MapFrom(src => src.Articles.Count))
                .ForMember(dest => dest.LikesReceived, opt => opt.MapFrom(src => src.Articles.Select(x => x.Likes.Count).Sum()));

            //Article
            CreateMap<Article, ArticleBasicModel>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(x => x.Author.UserName));

            CreateMap<Article, ArticleBaseModel>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(x => x.TagList.Select(x => x.TagId).ToList()))
                .ForMember(dest => dest.Likes, opt => opt.MapFrom(x => x.Likes.Count))
                .AfterMap<SetFeaturedImageUrlAction>()
                .Include<Article, ArticleDetailsModel>() // so that rules apply to derived classes
                .Include<Article, ArticlePreviewModel>();

            CreateMap<Article, ArticleDetailsModel>()
                .AfterMap((src, dest) =>
                {
                    dest.Description = GetArticleDescription(src.Body, src.Description);
                });

            CreateMap<Article, ArticlePreviewModel>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.UserName))
                .AfterMap((src, dest) =>
                {
                    dest.Description = GetArticleDescription(src.Body, src.Description);
                });

            //Comment
            CreateMap<Comment, CommentDetailsModel>();

            //Tag
            CreateMap<ArticleTag, ArticleTagModel>().ForMember(x => x.Tag, opt => opt.MapFrom(src => src.TagId));
        }

        public string GetArticleDescription(string body, string description)
        {
            if(string.IsNullOrEmpty(description))
            {
                return (body.Length >= 50) ? body.Substring(0, 50) : body;
            }

            return description;
        }
    }
}
