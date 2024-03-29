﻿using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Services
{
    public class BTFileService : IBTFileService
    {
        private readonly string _defaultCompanyImageSrc = "/img/Team-bro.png";
        private readonly string _deafaultProjectImageSrc = "/img/Shared goals-pana.png";
        private readonly string _defaultUserImgSrc = "/img/Uploading-amico.png";
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int? imageType)
        {

            if ((fileData == null || fileData.Length == 0) && imageType != null)
            {

                switch (imageType)
                {
                    //numbers match defaultIamge in enums 
                    //BlogUser Image
                    case 1: return _defaultCompanyImageSrc;
                    //BlogPost Image
                    case 2: return _deafaultProjectImageSrc;
                    // Category Image
                    case 3: return _defaultUserImgSrc;
                }
            }


            try
            {
                string ImageBase64Data = Convert.ToBase64String(fileData!);
                return string.Format($"data:{extension};base64,{ImageBase64Data}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();

                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");
            return $"/img/contenttype/{ext}.png";
        }


        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }
}
