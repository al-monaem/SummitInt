using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models.Model;
using Models.RequestEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DAL.Repositories
{
    public class CategoryRepo
    {
        private readonly SummitDbContext _db;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CategoryRepo> _logger;

        public CategoryRepo(SummitDbContext db, IServiceScopeFactory serviceScopeFactory, ILogger<CategoryRepo> logger)
        {
            _db = db;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task<(List<Category>, CategoryPagination)> GetCategories(CategoryPagination pagination)
        {
            var query = _db.Categories.Include(cat => cat.UpdatedByUser).AsQueryable();

            if(!String.IsNullOrEmpty(pagination.CategoryName))
            {
                query = query.Where(cat => EF.Functions.Like(cat.Name ,$"%{pagination.CategoryName}%"));
            }

            pagination.TotalItem = await query.CountAsync();
            pagination.TotalPage = (int)Math.Ceiling((decimal)pagination.TotalItem / pagination.Limit);

            return (await query.Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit).ToListAsync(), pagination);
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

        public async Task<int> BulkUpload(List<Category> categories, string filename, string userId)
        {
            var dataTable = CreateDataTable(categories, userId);

            var parameter = new SqlParameter("@Category", SqlDbType.Structured);
            parameter.Value = dataTable;
            parameter.TypeName = "[dbo].[CategoryType]"; // My Table valued user defined type

            var fileUpload = new FileUploadTracker()
            {
                Filename = filename,
                UploadedBy = userId,
                FileType = (byte)UploadFileType.Item,
                ReadStatus = (byte)ReadStatus.UnRead,
                UploadStatus = (byte)UploadStatus.Uploading
            };

            await _db.FileUploads.AddAsync(fileUpload);
            await _db.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<SummitDbContext>();
                try
                {
                    await db.Database.ExecuteSqlRawAsync($"EXEC [dbo].[InsertMultipleCategory] @Category", parameter);
                    fileUpload.UploadStatus = (byte)UploadStatus.Completed;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    fileUpload.Error = ex.Message;
                    fileUpload.UploadStatus = (byte)UploadStatus.Failed;
                }
                finally
                {
                    db.FileUploads.Update(fileUpload);
                    await db.SaveChangesAsync();
                }
            });

            return 201;
        }

        private static DataTable CreateDataTable(List<Category> categories, string userId)
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("CreatedBy", typeof(string));
            table.Columns.Add("UpdatedBy", typeof(string));
            table.Columns.Add("CreatedAt", typeof(DateTime));
            table.Columns.Add("UpdatedAt", typeof(DateTime));

            foreach (var category in categories)
            {
                table.Rows.Add(category.Id, category.Name, userId, userId, DateTime.Now, DateTime.Now);
            }
            return table;
        }
    }
}
