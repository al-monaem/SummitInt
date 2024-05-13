using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class CategoryRepo
    {
        private readonly SummitDbContext _db;
        public CategoryRepo(SummitDbContext db)
        {
            _db = db;
        }

        public async Task<List<Category>> GetCategories(int page, int limit)
        {
            return await this._db.Categories.Skip((page-1) * limit).Take(limit).ToListAsync();
        }

        public async Task<List<Category>> GetCategory(string name)
        {
            return await this._db.Categories.Where(cat => EF.Functions.Like(cat.Name, $"%{name}%")).ToListAsync();
        }

        public async Task<int> CreateCategory(Category category)
        {
            if(category == null)
            {
                throw new Exception("Could not update category");
            }

            await _db.Categories.AddAsync(category);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> DeleteCategory(int categoryId)
        {
            if(categoryId == 0)
            {
                throw new Exception("Could not delete category");
            }
            var category = await _db.Categories.FindAsync(categoryId);
            if(category == null)
            {
                throw new Exception("Could not delete category");
            }

            _db.Categories.Remove(category);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> UpdateCategory(Category category)
        {
            if(category == null)
            {
                throw new Exception("Could not update category");
            }

            var _category = await _db.Categories.FindAsync(category.Id);
            if(_category == null)
            {
                throw new Exception("Could not update category");
            }

            _category.Name = category.Name;
            _category.UpdatedAt = DateTime.Now;

            return await _db.SaveChangesAsync();
        }
    }
}
