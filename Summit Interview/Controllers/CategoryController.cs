using BLL.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Model;
using Models.RequestEntity;
using System.Collections.Generic;
using System.Security.Claims;
using Utility;

namespace Summit_Interview.Controllers
{
    [Authorize(Roles = $"{AppRoles.GENERAL}, {AppRoles.ADMIN}")]
    public class CategoryController : Controller
    {
        private readonly CategoryManager _manager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public CategoryController(CategoryManager manager, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> user)
        {
            _manager = manager;
            _webHostEnvironment = webHostEnvironment;
            _userManager = user;
        }

        public IActionResult Index()
        {
            ViewData["Current"] = "Category";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(CategoryPagination categoryPagination)
        {
            var (list, pagination, status, message) = await this._manager.GetCategories(categoryPagination);
            return Json(new
            {
                Data = new
                {
                    List = list,
                    Pagination = pagination,
                },
                Status = status,
                Message = message,
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory([FromQuery]string name)
        {
            var (list, status, message) = await this._manager.GetCategory(name);
            return Json(new
            {
                Data = list,
                Status = status,
                Message = message,
            });
        }

        [Authorize(Roles = AppRoles.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if(ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                category.UpdatedBy = userId;
                category.CreatedBy = userId;
                var (status, message) = await this._manager.CreateCategory(category);
                return Json(new
                {
                    Status = status,
                    Message = message,
                });
            }

            return Json(new
            {
                Status = 410,
                Message = ModelState,
            });
        }

        [Authorize(Roles = AppRoles.ADMIN)]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            if (file == null)
            {
                return Json(new
                {
                    Status = 400,
                    Message = "Invalid File"
                });
            }

            var wwwrootpath = _webHostEnvironment.WebRootPath;
            var filenameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            string filename = filenameWithoutExt.Length > 20 ? filenameWithoutExt.Substring(0, 20) : filenameWithoutExt + "__" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            System.IO.Directory.CreateDirectory(Path.Combine(wwwrootpath, @"files\category"));

            string itemPath = Path.Combine(wwwrootpath, @"files\category", filename);
            using (var fileStream = new FileStream(itemPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            string userId = _userManager.GetUserId(User);
            var (status, message) = await _manager.BulkUpload(itemPath, filename, userId);

            if (status != 201)
            {
                if (System.IO.File.Exists(itemPath))
                {
                    System.IO.File.Delete(itemPath);
                }
            }

            return Json(new
            {
                Status = status,
                Message = message,
            });
        }

        [Authorize(Roles = AppRoles.ADMIN)]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var (status, message) = await this._manager.DeleteCategory(id);
            return Json(new
            {
                Status = status,
                Message = message,
            });
        }

        [Authorize(Roles = AppRoles.ADMIN)]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                var (status, message) = await this._manager.UpdateCategory(category);
                return Json(new
                {
                    Status = status,
                    Message = message,
                });
            }

            return Json(new
            {
                Status = 410,
                Message = ModelState,
            });
        }
    }
}
