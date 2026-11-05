using AutoMapper;
using back_end.DTOs;
using back_end.DTOs.Categories.Requests;
using back_end.DTOs.Categories.Responses;
using back_end.Entities;
using back_end.Exceptions;
using back_end.Records;
using back_end.Repositories;

namespace back_end.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<object?>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            string name = request.Name.Trim();
            await ValidateParentAsync(request.ParentId, null);
            if (await _categoryRepository.CategoryNameExistsAsync(name, request.ParentId))
                throw new BusinessException(ErrorRecord.CategoryExists);

            CategoryEntity category = _mapper.Map<CategoryEntity>(request);
            category.Name = name;
            category.CreatedAt = DateTime.UtcNow;
            if (!await _categoryRepository.AddCategoryAsync(category))
                throw new BusinessException(ErrorRecord.Failed);

            return ApiResponse<object?>.Response(
                MessageRecord.CreatedSuccessfully,
                data: _mapper.Map<CategoryResponse>(category));
        }

        public async Task<ApiResponse<object?>> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            if (request.CategoryId == Guid.Empty)
                throw new BusinessException(ErrorRecord.RequestInvalid);
            CategoryEntity? category = await _categoryRepository.GetCategoryByIdAsync(request.CategoryId);
            if (category == null || category.IsDeleted)
                throw new BusinessException(ErrorRecord.NotFound);

            string name = request.Name.Trim();
            await ValidateParentAsync(request.ParentId, category.Id);
            if (await _categoryRepository.CategoryNameExistsAsync(name, request.ParentId, category.Id))
                throw new BusinessException(ErrorRecord.CategoryExists);

            _mapper.Map(request, category);
            category.Name = name;
            category.UpdatedAt = DateTime.UtcNow;
            if (!await _categoryRepository.UpdateCategoryAsync(category))
                throw new BusinessException(ErrorRecord.Failed);

            return ApiResponse<object?>.Response(MessageRecord.Success, data: _mapper.Map<CategoryResponse>(category));
        }

        public async Task<ApiResponse<object?>> SoftDeleteCategoryAsync(Guid categoryId)
        {
            CategoryEntity? category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null || category.IsDeleted)
                throw new BusinessException(ErrorRecord.NotFound);
            if (!await _categoryRepository.SoftDeleteCategoryAsync(categoryId))
                throw new BusinessException(ErrorRecord.Failed);
            return ApiResponse<object?>.Response(MessageRecord.Success);
        }

        public async Task<ApiResponse<object?>> RestoreCategoryAsync(Guid categoryId)
        {
            CategoryEntity? category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null || !category.IsDeleted)
                throw new BusinessException(ErrorRecord.NotFound);
            await ValidateParentAsync(category.ParentId, category.Id);
            if (await _categoryRepository.CategoryNameExistsAsync(category.Name, category.ParentId, category.Id))
                throw new BusinessException(ErrorRecord.CategoryExists);
            if (!await _categoryRepository.RestoreCategoryAsync(categoryId))
                throw new BusinessException(ErrorRecord.Failed);
            return ApiResponse<object?>.Response(MessageRecord.Success, data: _mapper.Map<CategoryResponse>(category));
        }

        public async Task<ApiResponse<object?>> GetCategoryByIdAsync(Guid categoryId)
        {
            CategoryEntity? category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null || category.IsDeleted)
                throw new BusinessException(ErrorRecord.NotFound);
            return ApiResponse<object?>.Response(MessageRecord.Success, data: _mapper.Map<CategoryResponse>(category));
        }

        public async Task<ApiResponse<object?>> GetListCategoryAsync(
            int pageNumber, int pageSize, bool isDeleted = false)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                throw new BusinessException(ErrorRecord.RequestInvalid);

            var result = await _categoryRepository.GetCategoriesAsync(pageNumber, pageSize, isDeleted);
            if (result.TotalItems == 0 || result.Categories.Count == 0)
                throw new BusinessException(ErrorRecord.NotFound);

            PaginationResponse<CategoryResponse> response = new()
            {
                Items = _mapper.Map<List<CategoryResponse>>(result.Categories),
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = result.TotalItems,
                TotalPages = (int)Math.Ceiling((double)result.TotalItems / pageSize)
            };
            return ApiResponse<object?>.Response(MessageRecord.Success, data: response);
        }

        private async Task ValidateParentAsync(Guid? parentId, Guid? categoryId)
        {
            if (parentId == null) return;
            if (categoryId == parentId || parentId == Guid.Empty)
                throw new BusinessException(ErrorRecord.RequestInvalid);

            CategoryEntity? parent = await _categoryRepository.GetCategoryByIdAsync(parentId.Value);
            if (parent == null || parent.IsDeleted || !parent.IsActive)
                throw new BusinessException(ErrorRecord.NotFound);

            // Walking ancestors prevents assigning a category beneath one of its descendants.
            HashSet<Guid> visited = new() { parent.Id };
            while (parent.ParentId.HasValue)
            {
                if (categoryId == parent.ParentId || !visited.Add(parent.ParentId.Value))
                    throw new BusinessException(ErrorRecord.RequestInvalid);
                parent = await _categoryRepository.GetCategoryByIdAsync(parent.ParentId.Value);
                if (parent == null || parent.IsDeleted)
                    break;
            }
        }
    }
}
