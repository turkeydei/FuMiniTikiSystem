using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FUMiniTikiSystem.DAL.Helper
{
    public class JsonDataSeeder<T, TContext>
        where T : class
        where TContext : DbContext
    {
        private readonly IFileReader _fileReader;
        private string _absoluteFilePathJson = default!;
        private readonly TContext _dbContext;

        public JsonDataSeeder(IFileReader fileReader, TContext dbContext)
        {
            _fileReader = fileReader;
            _dbContext = dbContext;
        }

        public JsonDataSeeder<T, TContext> AddRelativeFilePath(
            string basePath,
            string relativeFilePath
        )
        {
            _absoluteFilePathJson = Path.Combine(basePath, relativeFilePath);
            return this;
        }

        private IEnumerable<T> ParseJsonToObject()
        {
            try
            {
                var json = _fileReader.ReadFile(_absoluteFilePathJson);
                var settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                };

                var data =
                    JsonConvert.DeserializeObject<IEnumerable<T>>(json, settings)
                    ?? Enumerable.Empty<T>();
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Seed()
        {
            try
            {
                // check file exists first
                if (string.IsNullOrEmpty(_absoluteFilePathJson))
                    throw new FileNotFoundException("Does not have file");

                // Seed data based on entity
                if (_dbContext.Database.CanConnect())
                {
                    if (!_dbContext.Set<T>().Any())
                    {
                        _dbContext.Set<T>().AddRange(ParseJsonToObject());
                        _dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while seeding data: {ex.Message}", ex);
            }
        }
    }
}
