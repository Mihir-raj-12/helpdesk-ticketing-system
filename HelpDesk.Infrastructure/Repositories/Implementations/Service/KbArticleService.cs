using AutoMapper;
using HelpDesk.Core.Common;
using HelpDesk.Core.DTOs.KnowledgeBase;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Enums;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    public class KbArticleService : IKbArticleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserProvider _currentUserProvider;

        public KbArticleService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserProvider currentUserProvider    )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ApiResponse<KbArticleResponseDto>> GetArticleByIdAsync(int id)
        {
            var article = await _unitOfWork.KbArticles.GetByIdAsync(id);
            if (article == null) return ApiResponse<KbArticleResponseDto>.Failure("Article not found.");


            article.ViewCount += 1;
            await _unitOfWork.KbArticles.UpdateAsync(article);
            await _unitOfWork.SaveChangesAsync();
            // Optionally, you might want to load the Category name here using a custom repository method later,
            // but for now AutoMapper will handle the basic mapping.
            var responseDto = _mapper.Map<KbArticleResponseDto>(article);
            return ApiResponse<KbArticleResponseDto>.Success(responseDto);
        }

        public async Task<ApiResponse<IEnumerable<KbArticleResponseDto>>> GetAllPublishedAsync()
        {
            var articles = await _unitOfWork.KbArticles.GetAllAsync();
            var publishedArticles = articles.Where(a => a.Status == KbArticleStatus.Published).ToList();

            var responseDtos = _mapper.Map<IEnumerable<KbArticleResponseDto>>(publishedArticles);
            return ApiResponse<IEnumerable<KbArticleResponseDto>>.Success(responseDtos);
        }

        public async Task<ApiResponse<KbArticleResponseDto>> CreateArticleAsync(CreateKbArticleDto dto)
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return ApiResponse<KbArticleResponseDto>.Failure("User not found or unauthorized.");

            var article = _mapper.Map<KbArticle>(dto);

            // PRD 9.4: Support Agents create new articles in Draft status.
            article.Status = KbArticleStatus.Draft;
            article.VersionNumber = 1;

            await _unitOfWork.KbArticles.AddAsync(article);
            await _unitOfWork.SaveChangesAsync(); // Save to get the new Article ID

            // PRD 9.5: Every save creates a new version entry.
            var initialVersion = new KbArticleVersion
            {
                KbArticleId = article.Id,
                TitleSnapshot = article.Title,
                ContentSnapshot = article.Content,
                VersionNumber = article.VersionNumber,
                SavedByUserId = currentUserId
            };

            await _unitOfWork.KbArticleVersions.AddAsync(initialVersion);
            await _unitOfWork.SaveChangesAsync();

            var responseDto = _mapper.Map<KbArticleResponseDto>(article);
            return ApiResponse<KbArticleResponseDto>.Success(responseDto, "Draft article created successfully.");
        }

        public async Task<ApiResponse<KbArticleResponseDto>> UpdateArticleAsync(int id, UpdateKbArticleDto dto)
        {

            var currentUserId = _currentUserProvider.GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return ApiResponse<KbArticleResponseDto>.Failure("User not found or unauthorized.");

            var existingArticle = await _unitOfWork.KbArticles.GetByIdAsync(id);
            if (existingArticle == null) return ApiResponse<KbArticleResponseDto>.Failure("Article not found.");

            // --- THE VERSION ENGINE ---

            // 1. Take a snapshot of the CURRENT state before making any changes
            var snapshot = new KbArticleVersion
            {
                KbArticleId = existingArticle.Id,
                TitleSnapshot = existingArticle.Title,
                ContentSnapshot = existingArticle.Content,
                VersionNumber = existingArticle.VersionNumber,
                SavedByUserId = currentUserId
            };
            await _unitOfWork.KbArticleVersions.AddAsync(snapshot);

            // 2. Apply the new updates to the main article
            existingArticle.Title = dto.Title;
            existingArticle.Content = dto.Content;
            existingArticle.CategoryId = dto.CategoryId;
            existingArticle.Tags = dto.Tags;
            existingArticle.Status = dto.Status; // Allows Admin to publish

            // 3. Bump the version number!
            existingArticle.VersionNumber += 1;

            // 4. Save everything together in one transaction
            await _unitOfWork.KbArticles.UpdateAsync(existingArticle);
            await _unitOfWork.SaveChangesAsync();

            var responseDto = _mapper.Map<KbArticleResponseDto>(existingArticle);
            return ApiResponse<KbArticleResponseDto>.Success(responseDto, $"Article updated to Version {existingArticle.VersionNumber}.");
        }

        public async Task<ApiResponse<bool>> SubmitFeedbackAsync(int id, bool isHelpful)
        {
            var article = await _unitOfWork.KbArticles.GetByIdAsync(id);
            if (article == null) return ApiResponse<bool>.Failure("Article not found.");

            // --- NEW: Increment the correct feedback counter ---
            if (isHelpful)
            {
                article.HelpfulCount += 1;
            }
            else
            {
                article.NotHelpfulCount += 1;
            }

            await _unitOfWork.KbArticles.UpdateAsync(article);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Feedback recorded successfully.");
        }

        public async Task<ApiResponse<IEnumerable<KbArticleResponseDto>>> SearchArticlesAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return ApiResponse<IEnumerable<KbArticleResponseDto>>.Failure("Search keyword cannot be empty.");

            var lowerKeyword = keyword.ToLower();

            // 1. Fetch all published articles (Drafts should never appear in search results!)
            var allArticles = await _unitOfWork.KbArticles.GetAllAsync();
            var publishedArticles = allArticles.Where(a => a.Status == KbArticleStatus.Published);

            // 2. PRD 9.6: Filter and Score based on relevance
            var searchResults = publishedArticles
                .Where(a =>
                    (a.Title != null && a.Title.ToLower().Contains(lowerKeyword)) ||
                    (a.Tags != null && a.Tags.ToLower().Contains(lowerKeyword)) ||
                    (a.Content != null && a.Content.ToLower().Contains(lowerKeyword))
                )
                .Select(a => new
                {
                    Article = a,
                    // Assign weight: Title match is worth 3, Tags worth 2, Content worth 1
                    Score = (a.Title != null && a.Title.ToLower().Contains(lowerKeyword) ? 3 : 0) +
                            (a.Tags != null && a.Tags.ToLower().Contains(lowerKeyword) ? 2 : 0) +
                            (a.Content != null && a.Content.ToLower().Contains(lowerKeyword) ? 1 : 0)
                })
                .OrderByDescending(x => x.Score) // Highest score bubbles to the top
                .Select(x => x.Article)
                .ToList();

            if (!searchResults.Any())
                return ApiResponse<IEnumerable<KbArticleResponseDto>>.Failure("No articles found matching that keyword.");

            var responseDtos = _mapper.Map<IEnumerable<KbArticleResponseDto>>(searchResults);
            return ApiResponse<IEnumerable<KbArticleResponseDto>>.Success(responseDtos, $"Found {searchResults.Count} matching articles.");
        }
    }
}
