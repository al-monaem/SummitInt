using BLL.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Models.Model;
using Models.RequestEntity;
using Models.ResponseEntity;
using Utility;
using static System.Net.Mime.MediaTypeNames;

namespace Summit_Interview.Controllers
{
    [Authorize(Roles = $"{AppRoles.GENERAL}, {AppRoles.ADMIN}")]
    public class ItemController: Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ItemManager _manager;
        private readonly UserManager<IdentityUser> _userManager;

        public ItemController(IWebHostEnvironment webHostEnvironment, ItemManager manager, UserManager<IdentityUser> userManager)
        {
            _webHostEnvironment = webHostEnvironment;
            _manager = manager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewData["Current"] = "Items";
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _manager.GetItem(id);
            ViewData["Current"] = "Items";
            if (item == null)
            {
                return NotFound();
            }

            var (similarItems, _, _, _) = await _manager.GetItems(new ItemPagination()
            {
                ItemCategory = item.Category.Name,
                Limit = 10,
                Page = 1,
            });

            return View(new ItemDetails()
            {
                Item = item,
                SimilarItems = similarItems,
            });
        }

        public async Task<IActionResult> Items([FromQuery]ItemPagination pagination)
        {
            var (items, status, message, _pagination) = await _manager.GetItems(pagination);

            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_ItemList",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new DataResponse<Item>()
                    {
                        List = items,
                        Pagination = _pagination,
                    }
                }
            };

            return result;
        }

        public async Task<IActionResult> GetItems([FromQuery]ItemPagination pagination)
        {
            var (items, status, message, _pagination) = await _manager.GetItems(pagination);
            return Json(new
            {
                Status = status,
                Message = message,
                Data = new
                {
                    List = items,
                    Pagniation = _pagination,
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(Item item, List<IFormFile> files)
        {
            if(ModelState.IsValid)
            {
                var wwwrootpath = _webHostEnvironment.WebRootPath;
                List<ItemImage> images = new List<ItemImage>();

                if (files.Count > 0)
                {
                    foreach(var file in files)
                    {
                        var filenameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                        string filename = filenameWithoutExt.Length > 20 ? filenameWithoutExt.Substring(0, 20) : filenameWithoutExt + "__" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string itemPath = Path.Combine(wwwrootpath, @"images\item", filename);
                        using(var fileStream = new FileStream(itemPath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        images.Add(new ItemImage()
                        {
                            ImageURL = @"images\item\" + filename
                        });
                    }

                    item.ItemImages = images;
                }

                var (status, message) = await _manager.CreateItem(item);

                if(status != 201)
                {
                    foreach (var file in images)
                    {
                        string imagePath = Path.Combine(wwwrootpath, file.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

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

        [HttpPost]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            if(file == null)
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

            System.IO.Directory.CreateDirectory(Path.Combine(wwwrootpath, @"files\item"));

            string itemPath = Path.Combine(wwwrootpath, @"files\item", filename);
            using (var fileStream = new FileStream(itemPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            string userId = _userManager.GetUserId(User);
            var (status, message) = await _manager.BulkUpload(itemPath, filename, userId);

            if(status != 201)
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

        [HttpPut]
        public async Task<IActionResult> Update(ItemUpdate item, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                var wwwrootpath = _webHostEnvironment.WebRootPath;
                item.ItemImages = new List<ItemImage>();
                if (files.Count > 0)
                {
                    List<ItemImage> images = new List<ItemImage>();
                    foreach (var file in files)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string itemPath = Path.Combine(wwwrootpath, @"images\item", filename);
                        using (var fileStream = new FileStream(itemPath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        images.Add(new ItemImage()
                        {
                            ImageURL = @"images\item\" + filename
                        });
                    }

                    item.ItemImages = images;
                }

                // Delete Image if needed
                var existingItem = await _manager.GetItem(item.Id);
                if(existingItem != null)
                {
                    foreach (var itemImage in existingItem.ItemImages)
                    {
                        if (!item.ExistingImages.Contains(itemImage.ImageURL))
                        {
                            string imagePath = Path.Combine(wwwrootpath, itemImage.ImageURL.TrimStart('\\'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                System.IO.File.Delete(imagePath);
                            }
                        }
                        else
                        {
                            item.ItemImages.Add(itemImage);
                        }
                    }
                }

                var (status, message) = await _manager.UpdateItem(item);

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
        public async Task<IActionResult> Delete(int id)
        {
            var (status, message, imageFiles) = await _manager.DeleteItem(id);

            if(status == 201)
            {
                var wwwrootpath = _webHostEnvironment.WebRootPath;
                foreach (var itemImage in imageFiles)
                {
                    string imagePath = Path.Combine(wwwrootpath, itemImage.TrimStart('\\'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            return Json(new
            {
                Status = status,
                Message = message,
            });
        }
    }
}
