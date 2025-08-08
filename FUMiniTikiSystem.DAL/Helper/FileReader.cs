using System.Text;
using SystemFile = System.IO;

namespace FUMiniTikiSystem.DAL.Helper
{
    public class FileReader : IFileReader
    {
        public string ReadFile(string filePath)
        {
            try
            {
                var isExist = SystemFile.File.Exists(filePath);
                if (!SystemFile.File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found. {filePath}", filePath);
                }

                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );
                using var readerStream = new StreamReader(fileStream, Encoding.UTF8);

                return readerStream.ReadToEnd();
            }
            catch (FileNotFoundException ex)
            {
                // Handle file not found exception specifically
                throw new Exception(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle permission-related issues
                throw new Exception($"Access to the file at {filePath} is denied.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while reading the file.", ex);
            }
        }
    }
}
