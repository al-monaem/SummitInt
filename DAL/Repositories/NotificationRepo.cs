using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DAL.Repositories
{
    public class NotificationRepo
    {
        private readonly SummitDbContext _db;

        public NotificationRepo(SummitDbContext db)
        {
            _db = db;
        }

        public async Task<List<FileUploadTracker>> GetFileUploadsNotifications(string userid)
        {
            return await _db.FileUploads
                .Where(file => file.ReadStatus == (byte)ReadStatus.UnRead)
                .Where(file => file.UploadedBy.Equals(userid))
                .ToListAsync();
        }

    }
}
