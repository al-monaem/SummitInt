using BLL.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Models.Model;
using Models.RequestEntity;
using Models.ResponseEntity;
using static System.Net.Mime.MediaTypeNames;

namespace Summit_Interview.Controllers
{
    public class ItemController: Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ItemManager _manager;
        public ItemController(IWebHostEnvironment webHostEnvironment, ItemManager manager)
        {
            _webHostEnvironment = webHostEnvironment;
            _manager = manager;
        }

        public IActionResult Index()
        {
            var UnitTypes = Utility.Utility.UnitTypes;
            ViewBag.UnitTypes = UnitTypes;

            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _manager.GetItem(id);
            var UnitTypes = Utility.Utility.UnitTypes;
            ViewBag.UnitTypes = UnitTypes;

            if (item == null)
            {
                return NotFound();
            }
            return View(item);
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
                        Pagination = pagination,
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
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
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
            string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            System.IO.Directory.CreateDirectory(Path.Combine(wwwrootpath, @"files\item"));

            string itemPath = Path.Combine(wwwrootpath, @"files\item", filename);
            using (var fileStream = new FileStream(itemPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            var (status, message) = await _manager.BulkUpload(itemPath);

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
