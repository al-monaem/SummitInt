using DAL.Repositories;
using Microsoft.Extensions.Logging;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Manager
{
    public class NotificationManager
    {
        private readonly NotificationRepo _notificationRepo;
        private readonly ILogger<NotificationManager> _logger;

        public NotificationManager(NotificationRepo repo, ILogger<NotificationManager> logger)
        {
            _notificationRepo = repo;
            _logger = logger;
        }

        public async Task<List<FileUploadTracker>> GetFileUploadsNotifications(string userId)
        {
            try
            {
                return await _notificationRepo.GetFileUploadsNotifications(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<FileUploadTracker>();
            }
        }
    }
}
