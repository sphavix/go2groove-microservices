namespace Go2GrooveApi.Services
{
    public class FileUpload
    {
        public static async Task<string> UploadFile(IFormFile file)
        {
            var fileDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if(!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var filePath = Path.Combine(fileDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(stream);

            return fileName;
        }
    }
}
