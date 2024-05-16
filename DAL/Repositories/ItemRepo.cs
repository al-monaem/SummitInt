using DAL.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models.Model;
using Models.RequestEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DAL.Repositories
{
    public class ItemRepo
    {
        private readonly SummitDbContext _db;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ItemRepo> _logger;

        public ItemRepo(SummitDbContext db, IServiceScopeFactory serviceScopeFactory, ILogger<ItemRepo> logger)
        {
            _db = db;
            _serviceScopeFactory = serviceScopeFactory;
            this._logger = logger;
        }

        public async Task<(List<Item>, ItemPagination)> GetItems(ItemPagination pagination)
        {
            var page = pagination.Page;
            var limit = pagination.Limit;

            var query = _db.Items.Include(i => i.Category).Include(i => i.ItemImages).AsQueryable();

            if(!String.IsNullOrEmpty(pagination.ItemName))
            {
                query = query.Where(i => EF.Functions.Like(i.ItemName, $"%{pagination.ItemName}%"));
            }
            if(!String.IsNullOrEmpty(pagination.ItemCategory))
            {
                query = query.Where(i => EF.Functions.Like(i.Category.Name, $"%{pagination.ItemCategory}%"));
            }

            pagination.TotalItem = query.Count();
            pagination.TotalPage = (int)Math.Ceiling((decimal)pagination.TotalItem / (decimal)limit);

            return (await query.Skip((page - 1) * limit).Take(limit).ToListAsync(), pagination);
        }

        public async Task<int> CreateItem(Item item)
        {
            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;
            await _db.Items.AddAsync(item);
            return await _db.SaveChangesAsync();
        }

        public async Task<Item?> GetItem(int itemId)
        {
            return await _db.Items
                .Include(item => item.ItemImages)
                .Include(item => item.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == itemId);
        }

        public async Task<int> UpdateItem(Item item)
        {
            item.UpdatedAt = DateTime.Now;

            var imageUrls = item.ItemImages.Select(i => i.ImageURL).ToList();

            var delete = await _db.ItemImages.Where(e => e.ItemId == item.Id).Where(e => !imageUrls.Contains(e.ImageURL)).ToListAsync();

            _db.ItemImages.RemoveRange(delete);
            _db.Items.Update(item);

            return await _db.SaveChangesAsync();
        }

        public async Task<(int, List<string>)> DeleteItem(int id)
        {
            var item = await _db.Items.Include(i => i.ItemImages).FirstOrDefaultAsync(i => i.Id == id) ?? throw new Exception("Could not delete item");
            var imageFiles = item.ItemImages.Select(i => i.ImageURL).ToList();
            _db.Items.Remove(item);

            return (await _db.SaveChangesAsync(), imageFiles);
        }

        public async Task<int> BulkUpload(List<Item> items, string filename, string userid)
        {
            var dataTable = CreateDataTable(items);

            var parameter = new SqlParameter("@Item", SqlDbType.Structured);
            parameter.Value = dataTable;
            parameter.TypeName = "[dbo].[ItemType]";

            var fileUpload = new FileUploadTracker()
            {
                Filename = filename,
                UploadedBy = userid,
                FileType = (byte)UploadFileType.Item,
                ReadStatus = (byte)ReadStatus.UnRead,
                UploadStatus = (byte)UploadStatus.Uploading
            };

            await _db.FileUploads.AddAsync(fileUpload);
            await _db.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                await using (var scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<SummitDbContext>();
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync($"EXEC [dbo].[InsertMultipleItems] @Item", parameter);
                        fileUpload.UploadStatus = (byte)UploadStatus.Completed;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        fileUpload.UploadStatus = (byte)UploadStatus.Failed;
                        fileUpload.Error = ex.Message;
                    }
                    finally
                    {
                        db.FileUploads.Update(fileUpload);
                        await db.SaveChangesAsync();
                        Debug.WriteLine("======================================");
                        Debug.WriteLine(fileUpload.Filename);
                        Debug.WriteLine(fileUpload.UploadStatus);
                        Debug.WriteLine("======================================");
                    }
                }
            });

            return 201;
        }

        private static DataTable CreateDataTable(List<Item> items)
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("ItemUnit", typeof(string));
            table.Columns.Add("ItemQuantity", typeof(string));
            table.Columns.Add("CategoryId", typeof(int));
            table.Columns.Add("ItemDescription", typeof(string));
            table.Columns.Add("CreatedAt", typeof(DateTime));
            table.Columns.Add("UpdatedAt", typeof(DateTime));

            foreach (var item in items)
            {
                table.Rows.Add(item.Id, item.ItemName, item.ItemUnit, item.ItemQuantity, item.CategoryId, item.ItemDescription, DateTime.Now, DateTime.Now);
            }
            return table;
        }
    }
}
