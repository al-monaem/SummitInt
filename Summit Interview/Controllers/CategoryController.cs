using BLL.Manager;
using Microsoft.AspNetCore.Mvc;
using Models.Model;
using System.Collections.Generic;

namespace Summit_Interview.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryManager _manager;

        public CategoryController(CategoryManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] int page, [FromQuery]int limit)
        {
            var (list, status, message) = await this._manager.GetCategories(page, limit);
            return Json(new
            {
                Data = list,
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

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if(ModelState.IsValid)
            {
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
