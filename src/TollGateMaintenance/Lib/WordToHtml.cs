using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using TollGateMaintenance.Models;
using Aspose.Words;

namespace TollGateMaintenance.Lib
{
    public static class WordToHtml
    {
        public static Report Parse(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + extension);
            File.WriteAllBytes(path, file.ReadAllBytes());
            var doc = new Document(path);
            File.Delete(path);
            var ret = new Report
            {
                FileName = file.FileName,
                FileBlob = file.ReadAllBytes(),
                Time = DateTime.Now,
                RawHtml = doc.ToString(SaveFormat.Html),
                RawText = doc.ToString(SaveFormat.Text)
            };
            return ret;
        }
    }
}
