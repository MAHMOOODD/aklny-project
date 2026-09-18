using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.Products;
using Resturant_Backend.Helpers.Filter;
using Resturant_Backend.Helpers.Pagination;
using Resturant_Backend.Helpers.PhotosHandle;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public ProductController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] Filters filter)
        {
            var vaildFilter = new PaginationFilter(filter.Pagination.PageNumber, filter.Pagination.PageSize);
            var (products, ProductsCount) = await _unitOfWork.ProductsRepo.GetAllAsync(filter);


            var P = _mapper.Map<List<GetAllProductDto>>(products);
            var res = new PagedResponse<GetAllProductDto>(P, vaildFilter.PageNumber, vaildFilter.PageSize, ProductsCount);
            return this.Success(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _unitOfWork.ProductsRepo.GetAsync(id);
            Ensure.NotNull(product, "Product Not Found");

            var res = _mapper.Map<GetProductDto>(product);
            return this.Success(res);
        }

        [HttpGet("GetProductByName")]
        public async Task<IActionResult> GetProductByName(string name, [FromQuery] Filters filters)
        {
            var (products, totalCount) = await _unitOfWork.ProductsRepo.GetProductsByName(name, filters);
            Ensure.NotNull(products, "Products Not Found");
            var ress = _mapper.Map<List<GetAllProductDto>>(products);
            var res = new PagedResponse<GetAllProductDto>(ress, filters.Pagination.PageNumber, filters.Pagination.PageSize, totalCount);
            return this.Success(res);
        }

        [HttpGet("GetProductByCategory")]
        public async Task<IActionResult> GetProductByCategory(string categoryName, [FromQuery] Filters filters)
        {
            var (products, totalCount) = await _unitOfWork.ProductsRepo.GetProductsByCategory(categoryName, filters);
            Ensure.NotNull(products, "Products Not Found");
            var ress = _mapper.Map<List<GetAllProductDto>>(products);
            var res = new PagedResponse<GetAllProductDto>(ress, filters.Pagination.PageNumber, filters.Pagination.PageSize, totalCount);
            return this.Success(res);
        }

        [HttpGet("GetProductByCategoryId")]
        public async Task<IActionResult> GetProductByCategory(int categoryId, [FromQuery] Filters filters)
        {
            var (products, totalCount) = await _unitOfWork.ProductsRepo.GetProductsByCategory(categoryId, filters);
            Ensure.NotNull(products, "Products Not Found");
            var ress = _mapper.Map<List<GetAllProductDto>>(products);
            var res = new PagedResponse<GetAllProductDto>(ress, filters.Pagination.PageNumber, filters.Pagination.PageSize, totalCount);
            return this.Success(res);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager}")]
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromForm] AddProductDto productDto)
        {
            var catExist = await _unitOfWork.CategoreisRepo.GetAsync(productDto.CategoryId);
            Ensure.NotNull(catExist, "Category Not Found");

            var productToAdd = _mapper.Map<Product>(productDto);

            // upload the image if it exists
            if(productDto.ImageUrl != null && productDto.ImageUrl.Length > 0)
            {
                var uploadResult = await _photoService.AddPhotoAsync(productDto.ImageUrl);
                Ensure.Check(uploadResult.Error == null, uploadResult.Error?.Message ?? "Image upload failed");

                productToAdd.ImageUrl = uploadResult.SecureUrl.ToString();
                productToAdd.ImagePublicId = uploadResult.PublicId;
            }

            var Pro = await _unitOfWork.ProductsRepo.AddAsync(productToAdd);
            Ensure.NotNull(Pro, "Cant Add This Product Please Try Again ");

            await _unitOfWork.SaveChangesAsync();

            var proToShow = _mapper.Map<GetProductDto>(Pro);
            return this.Success(proToShow);
        }
        [Authorize(Roles = $"{Role.Admin},{Role.Manager}")]
        [HttpPut("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromForm] EditProductDto editProductsDto)
        {
            var proToEdit = await _unitOfWork.ProductsRepo.GetAsync(id);
            Ensure.NotNull(proToEdit, "Product Not Found");

            _mapper.Map(editProductsDto, proToEdit);

            var imageFile = editProductsDto.ImageUrl;


            if(imageFile != null)
            {
                if(!string.IsNullOrEmpty(proToEdit.ImagePublicId))
                {
                    await _photoService.DeletePhotoAsync(proToEdit.ImagePublicId);
                }

                if(imageFile.Length > 0)
                {
                    var uploadResult = await _photoService.AddPhotoAsync(imageFile);
                    Ensure.Check(uploadResult.Error == null, uploadResult.Error?.Message ?? "Image upload failed");

                    proToEdit.ImageUrl = uploadResult.SecureUrl.ToString();
                    proToEdit.ImagePublicId = uploadResult.PublicId;
                }
                else
                {
                    proToEdit.ImageUrl = null;
                    proToEdit.ImagePublicId = null;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var proToShow = _mapper.Map<GetProductDto>(proToEdit);
            return this.Success(proToShow);
        }
        [Authorize(Roles = $"{Role.Admin}")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var productToDelete = await _unitOfWork.ProductsRepo.GetAsync(id);
            Ensure.NotNull(productToDelete, "Product Not Found");

            if(!string.IsNullOrEmpty(productToDelete.ImagePublicId))
            {
                await _photoService.DeletePhotoAsync(productToDelete.ImagePublicId);
            }

            await _unitOfWork.ProductsRepo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return this.SuccessMessage("Product Deleted Successfully");
        }
    }
}