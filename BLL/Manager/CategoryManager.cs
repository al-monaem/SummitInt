using DAL.Repositories;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Manager
{
    public class CategoryManager
    {
        private readonly CategoryRepo _repo;
        public CategoryManager(CategoryRepo repo)
        {
            _repo = repo;
        }

        public async Task<(List<Category>, int, string)> GetCategories(int page, int limit)
        {
            try
            {
                var categories = await this._repo.GetCategories(page, limit);
                return (
                    categories,
                    200,
                    "Data fetching succeded"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (new List<Category>(), 500, "Something Went Wrong");
            }
        }

        public async Task<(List<Category>, int, string)> GetCategory(string name)
        {
            try
            {
                var categories = await this._repo.GetCategory(name);
                return (
                    categories,
                    200,
                    "Data fetching succeded"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (new List<Category>(), 500, "Something Went Wrong");
            }
        }

        public async Task<(int, string)> CreateCategory(Category category)
        {
            try
            {
                var created = await this._repo.CreateCategory(category);
                return (created == 1 ? 201 : 400, created == 1 ? "Category created successfully" : "Nothing has changed");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Could not update category");
            }
        }

        public async Task<(int, string)> DeleteCategory(int categoryId)
        {
            try
            {
                await this._repo.DeleteCategory(categoryId);
                return (201, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Could not delete category");
            }
        }

        public async Task<(int, string)> UpdateCategory(Category category)
        {
            try
            {
                var updated = await this._repo.UpdateCategory(category);
                return (updated == 1 ? 201 : 400, updated == 1 ? "Category updated successfully" : "No new changes made");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Could not delete category");
            }
        }
    }
}
