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
    public class CategoryManager
    {
        private readonly CategoryRepo _repo;
        public CategoryManager(CategoryRepo repo)
        {
            _repo = repo;
        }

        public async Task<(List<Category>, CategoryPagination, int, string)> GetCategories(CategoryPagination categoryPagination)
        {
            try
            {
                var (categories, pagination) = await this._repo.GetCategories(categoryPagination);
                return (
                    categories,
                    pagination,
                    200,
                    "Data fetching succeded"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (new List<Category>(), categoryPagination, 500, "Something Went Wrong");
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

        public async Task<(int, string)> BulkUpload(string filepath, string filename, string userId)
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

                    var categories = new List<Category>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var category = new Category()
                        {
                            Id = Convert.ToInt32(worksheet.Cells[row, 1]?.Value?.ToString() ?? "0"),
                            Name = worksheet.Cells[row, 2]?.Value?.ToString() ?? "",
                        };

                        if (category.Id == 0)
                            return (400, $"Id column value invalid at row {row}");
                        if (String.IsNullOrEmpty(category.Name))
                            return (400, $"ItemName column value cannot be empty at row {row}");

                        categories.Add(category);
                    }

                    package.Dispose();

                    var status = await _repo.BulkUpload(categories, filename, userId);

                    return (status, "File uploaded successfully");
                }
                else
                {
                    return (400, "Could not upload file");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (500, "Something Went Wrong");
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
