using FUMiniTikiSystem.DAL;
using FUMiniTikiSystem.DAL.Models;

namespace FUMiniTikiSystem.BLL
{
    public class CategoryService
    {
        private readonly CategoryRepo categoryRepo;

        public CategoryService(FUMiniTikiSystem.DAL.CategoryRepo categoryRepo)
        {
            this.categoryRepo = categoryRepo;
        }

        public Category GetCategory(int id)
        {
            return categoryRepo.Get(id);
        }

        public List<Category> GetAllCategories()
        {
            return categoryRepo.GetAll();
        }

        public List<Category> SearchCategories(string keyword)
        {
            return categoryRepo.Search(keyword);
        }

        public void AddCategory(Category category)
        {
            categoryRepo.Add(category);
        }

        public void UpdateCategory(Category category)
        {
            categoryRepo.Update(category);
        }

        public void DeleteCategory(int id)
        {
            var category = categoryRepo.Get(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            category.IsActive = false; // Soft delete
            categoryRepo.Update(category);
        }

        //public void DeleteCategory(int id)
        //{
        //    categoryRepo.Delete(id);
        //}
    }
}
