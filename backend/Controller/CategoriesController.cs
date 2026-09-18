using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.DTO.Categories;
using Resturant_Backend.Helpers.PhotosHandle;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Models;
using Resturant_Backend.Roles;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _unitOfWork.CategoreisRepo.GetAllAsync();
            var res = _mapper.Map<List<GetCategoriesDto>>(categories);
            return this.Success(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _unitOfWork.CategoreisRepo.GetAsync(id);
            Ensure.NotNull(category, "Category Not Found");
            var res = _mapper.Map<GetCategoriesDto>(category);
            return this.Success(res);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager}")]
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromForm] AddCategoriesDto category)
        {
            var categoryToAdd = _mapper.Map<Category>(category);

            // update the image if it exists
            if(category.ImageUrl != null && category.ImageUrl.Length > 0)
            {
                var uploadResult = await _photoService.AddPhotoAsync(category.ImageUrl);
                Ensure.Check(uploadResult.Error == null, uploadResult.Error?.Message ?? "Image upload failed");

                categoryToAdd.ImageUrl = uploadResult.SecureUrl.ToString();
                categoryToAdd.ImagePublicId = uploadResult.PublicId;
            }

            var Cat = await _unitOfWork.CategoreisRepo.AddAsync(categoryToAdd);
            Ensure.NotNull(Cat, "Cant Add This Category Please Try Again");

            await _unitOfWork.SaveChangesAsync();

            var catToShow = _mapper.Map<GetCategoriesDto>(Cat);
            return this.Success(catToShow);
        }

        [Authorize(Roles = $"{Role.Admin},{Role.Manager}")]
        [HttpPut("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromForm] EditCategoriesDto editCategoriesDto)
        {
            var categoryToEdit = await _unitOfWork.CategoreisRepo.GetAsync(id);
            Ensure.NotNull(categoryToEdit, "Category Not Found");


            _mapper.Map(editCategoriesDto, categoryToEdit);

            var imageFile = editCategoriesDto.ImageUrl;

            if(imageFile != null)
            {
                //delete the old image from cloudinary if it exists
                if(!string.IsNullOrEmpty(categoryToEdit.ImagePublicId))
                {
                    await _photoService.DeletePhotoAsync(categoryToEdit.ImagePublicId);
                }

                if(imageFile.Length > 0)
                {
                    // upload the new image to cloudinary
                    var uploadResult = await _photoService.AddPhotoAsync(imageFile);
                    Ensure.Check(uploadResult.Error == null, uploadResult.Error?.Message ?? "Image upload failed");

                    categoryToEdit.ImageUrl = uploadResult.SecureUrl.ToString();
                    categoryToEdit.ImagePublicId = uploadResult.PublicId;
                }
                else
                {
                    // delete the image if the new image file is empty (user wants to remove the image)
                    categoryToEdit.ImageUrl = null;
                    categoryToEdit.ImagePublicId = null;
                }
            }
            // the image remains unchanged if the new image file is null (user didn't provide a new image)

            await _unitOfWork.SaveChangesAsync();
            var catToShow = _mapper.Map<GetCategoriesDto>(categoryToEdit);

            return this.Success(catToShow);
        }

        [Authorize(Roles = $"{Role.Admin}")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // check if the category exists
            var categoryToDelete = await _unitOfWork.CategoreisRepo.GetAsync(id);
            Ensure.NotNull(categoryToDelete, "Category Not Found");

            // delete the image from cloudinary if it exists
            if(!string.IsNullOrEmpty(categoryToDelete.ImagePublicId))
            {
                await _photoService.DeletePhotoAsync(categoryToDelete.ImagePublicId);
            }

            await _unitOfWork.CategoreisRepo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return this.SuccessMessage("Category deleted successfully.");
        }
    }
}