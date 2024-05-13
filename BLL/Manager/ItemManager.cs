using Azure;
using DAL.Repositories;
using Models.Model;
using Models.RequestEntity;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Manager
{
    public class ItemManager
    {
        private readonly ItemRepo _repo;
        public ItemManager(ItemRepo repo)
        {
            _repo = repo;
        }

        public async Task<(List<Item>, int, string, ItemPagination)> GetItems(ItemPagination pagination)
        {
            try
            {
                var (items, _pagination) = await _repo.GetItems(pagination);
                return (items, 200, "Item fetch succeded", _pagination);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return (new List<Item>(), 500, "Something Went Wrong", pagination);
            }
        }

        public async Task<(int, string)> BulkUpload(string filepath)
        {
            try
            {
                if (File.Exists(filepath))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    ExcelPackage package = new ExcelPackage(new FileInfo(filepath));
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Assuming data is in the first sheet

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    var items = new List<Item>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var item = new Item()
                        {
                            Id = Convert.ToInt32(worksheet.Cells[row, 1]?.Value?.ToString() ?? "0"),
                            ItemName = worksheet.Cells[row, 2]?.Value?.ToString() ?? null,
                            ItemUnit = worksheet.Cells[row, 3]?.Value?.ToString() ?? null,
                            ItemQuantity = worksheet.Cells[row, 4]?.Value?.ToString() ?? null,
                            CategoryId = Convert.ToInt32(worksheet.Cells[row, 5]?.Value?.ToString() ?? "0"),
                        };

                        if (item.Id == 0)
                            return (400, $"Id column value invalid at row {row}");
                        if(String.IsNullOrEmpty(item.ItemName))
                            return (400, $"ItemName column value cannot be empty at row {row}");
                        if(String.IsNullOrEmpty(item.ItemUnit))
                            return (400, $"ItemUnit column value cannot be empty at row {row}");
                        if(String.IsNullOrEmpty(item.ItemQuantity))
                            return (400, $"ItemQuantity column value cannot be empty at row {row}");
                        if (item.CategoryId == 0)
                            return (400, $"CategoryId column value invalid at row {row}");

                        items.Add(item);
                    }

                    package.Dispose();

                    var status = await _repo.BulkUpload(items);

                    return (status, "File uploaded successfully");
                }
                else
                {
                    return (400, "Could not upload file");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Something Went Wrong");
            }
        }

        public async Task<Item?> GetItem(int itemId)
        {
            try
            {
                return await _repo.GetItem(itemId);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public async Task<(int, string)> CreateItem(Item item)
        {
            try
            {
                var created = await _repo.CreateItem(item);
                return (created > 0 ? 201 : 400, created > 0 ? "Item created successfully" : "Could not create item");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Something Went Wrong");
            }
        }

        public async Task<(int, string)> UpdateItem(Item item)
        {
            try
            {
                var updated = await _repo.UpdateItem(item);
                return (updated > 0 ? 201 : 400, updated > 0 ? "Item updated successfully" : "Could not update item");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Something Went Wrong");
            }
        }

        public async Task<(int, string, List<string>)> DeleteItem(int id)
        {
            try
            {
                var (deleted, imageFiles) = await _repo.DeleteItem(id);
                return (deleted > 0 ? 201 : 400, deleted > 0 ? "Item deleted successfully" : "Could not delete item", imageFiles);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Something Went Wrong", new List<string>());
            }
        }
    }
}
