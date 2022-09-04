using NovaBugTracker.Services.Interfaces;

namespace NovaBugTracker.Services
{
    public class BTFileService : IBTFileService
    {
        private readonly string _defaultCompanyImageSrc = "/img/Team-bro.png";
        private readonly string _deafaultProjectImageSrc = "/img/Shared goals-pana.png";
        private readonly string _defaultUserImgSrc = "/img/Uploading-amico.png";
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
    }
}
