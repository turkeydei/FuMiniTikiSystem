using FUMiniTikiSystem.DAL.Base;
using FUMiniTikiSystem.DAL.DBContext;
using FUMiniTikiSystem.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FUMiniTikiSystem.DAL
{
    public class CategoryRepo : GenericRepository<Category>
    {
        public CategoryRepo(FUMiniTikiSystemDbContext context) : base(context)
        {
        }

        public Category Get(int id)
        {
            return context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryID == id);
        }

        public List<Category> GetAll()
        {
            return context.Categories
                .Include(c => c.Products)
                .ToList();
        }

        public List<Category> Search(string keyword)
        {
            return context.Categories
                .Include(c => c.Products)
                .Where(c =>
                    c.Name.Contains(keyword) ||
                    c.Description.Contains(keyword))
                .ToList();
        }

        public void Add(Category category)
        {
            context.Categories.Add(category);
            context.SaveChanges();
        }

        public void Update(Category category)
        {
            var local = context.Categories.Local
                .FirstOrDefault(c => c.CategoryID == category.CategoryID);

            if (local != null)
            {
                context.Entry(local).State = EntityState.Detached;
            }

            context.Categories.Update(category);
            context.SaveChanges();
        }



        public void Delete(int id)
        {
            var tracked = context.ChangeTracker.Entries<Category>()
                                 .FirstOrDefault(e => e.Entity.CategoryID == id)?.Entity;

            if (tracked != null)
            {
                context.Categories.Remove(tracked);
            }
            else
            {
                var category = new Category { CategoryID = id };
                context.Entry(category).State = EntityState.Deleted;
            }

            context.SaveChanges();
        }

    }
}
