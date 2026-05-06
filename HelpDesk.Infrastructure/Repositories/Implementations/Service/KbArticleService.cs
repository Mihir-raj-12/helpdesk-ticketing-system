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
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

            var article = await _unitOfWork.KbArticles.GetByIdAsync(id);
            if (article == null) return ApiResponse<KbArticleResponseDto>.Failure("Article not found.");

            // --- FIX 4 (S04): Hide Drafts from Regular Users ---
            if (article.Status == KbArticleStatus.Draft)
            {
                if (currentUserRole == "RegularUser")
                {
                    return ApiResponse<KbArticleResponseDto>.Failure("Article not found or you do not have permission to view it.");
                }

                // Hide from Support Agents if it's not their draft
                if (currentUserRole == "SupportAgent" && article.AuthorUserId != currentUserId)
                {
                    return ApiResponse<KbArticleResponseDto>.Failure("You can only view your own draft articles.");
                }
            }

            // Only increment view count if a Regular User is viewing it
            if (currentUserRole == "RegularUser")
            {
                article.ViewCount += 1;
                await _unitOfWork.KbArticles.UpdateAsync(article, a => a.ViewCount);
                await _unitOfWork.SaveChangesAsync();
            }

            return ApiResponse<KbArticleResponseDto>.Success(_mapper.Map<KbArticleResponseDto>(article));
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
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

            if (string.IsNullOrEmpty(currentUserId))
                return ApiResponse<KbArticleResponseDto>.Failure("User not found or unauthorized.");

            var article = _mapper.Map<KbArticle>(dto);

            // PRD 9.4: Support Agents create new articles in Draft status.
            // --- FIX 1 (L23): Force Support Agents to Draft Status ---
            if (currentUserRole == "SupportAgent")
            {
                article.Status = KbArticleStatus.Draft; // Agents cannot publish directly!
            }
            else
            {
                article.Status = dto.Status; // Admins can choose Published or Draft
            }

            // --- FIX 2 (L22): Track Authorship ---
            article.AuthorUserId = currentUserId;
            article.LastUpdatedByUserId = currentUserId;
            article.CreatedDate = DateTime.UtcNow;
            article.LastUpdatedDate = DateTime.UtcNow;
            var createdArticle = await _unitOfWork.KbArticles.AddAsync(article);

            // Create the initial Version history record
            var version = new KbArticleVersion
            {
                KbArticleId = createdArticle.Id,
                Title = createdArticle.Title,
                Content = createdArticle.Content,
                VersionNumber = 1,
                UpdatedByUserId = currentUserId,
                UpdatedDate = DateTime.UtcNow
            };
            await _unitOfWork.KbArticleVersions.AddAsync(version);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<KbArticleResponseDto>.Success(_mapper.Map<KbArticleResponseDto>(createdArticle), "Article created successfully");
        }

        public async Task<ApiResponse<KbArticleResponseDto>> UpdateArticleAsync(int id, UpdateKbArticleDto dto)
        {
            var currentUserId = _currentUserProvider.GetCurrentUserId();
            var currentUserRole = _currentUserProvider.GetCurrentUserRole();

            var article = await _unitOfWork.KbArticles.GetByIdAsync(id);
            if (article == null) return ApiResponse<KbArticleResponseDto>.Failure("Article not found.");

            // --- FIX 3 (L24): Prevent Agents from editing other's drafts or publishing ---
            if (currentUserRole == "SupportAgent")
            {
                if (article.AuthorUserId != currentUserId)
                    return ApiResponse<KbArticleResponseDto>.Failure("You can only edit your own draft articles.");

                if (dto.Status == KbArticleStatus.Published)
                    return ApiResponse<KbArticleResponseDto>.Failure("Support Agents cannot publish articles. Please save as Draft for Admin review.");
            }

            // Update fields
            article.Title = dto.Title;
            article.Content = dto.Content;
            article.CategoryId = dto.CategoryId;

            // Admin can publish it, Agent stays draft
            if (currentUserRole == "Admin" )
            {
                article.Status = dto.Status;
            }

            article.LastUpdatedByUserId = currentUserId; // Track who did this update

            // Create a new Version snapshot
            var currentVersions = await _unitOfWork.KbArticleVersions.FindAsync(v => v.KbArticleId == id);
            int nextVersionNumber = currentVersions.Any() ? currentVersions.Max(v => v.VersionNumber) + 1 : 1;

            var version = new KbArticleVersion
            {
                KbArticleId = article.Id,
                Title = article.Title,
                Content = article.Content,
                VersionNumber = nextVersionNumber,
                UpdatedByUserId = currentUserId,
                UpdatedDate = DateTime.UtcNow
            };

            await _unitOfWork.KbArticleVersions.AddAsync(version);
            await _unitOfWork.KbArticles.UpdateAsync(article);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<KbArticleResponseDto>.Success(_mapper.Map<KbArticleResponseDto>(article), "Article updated successfully.");
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
