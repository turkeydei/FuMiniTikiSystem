using FUMiniTikiSystem.BLL.DTOs;

namespace FUMiniTikiSystem.BLL
{
    public class ProductService
    {
        private readonly FUMiniTikiSystem.DAL.ProductRepo productRepo;

        public ProductService(FUMiniTikiSystem.DAL.ProductRepo productRepo)
        {
            this.productRepo = productRepo;
        }

        public FUMiniTikiSystem.DAL.Models.Product GetProduct(int id)
        {
            return productRepo.Get(id);
        }
        public List<FUMiniTikiSystem.DAL.Models.Product> GetAllProducts()
        {
            return productRepo.GetAll();
        }

        public PaginatedList<FUMiniTikiSystem.DAL.Models.Product> GetAllProducts(GetProductsRequest request)
        {
            var products = productRepo.GetAllProductsQueryable();
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                products = products.Where(p => p.Name.ToLower().Contains(request.SearchTerm.ToLower()));
            }
            if (request.CategoryID.HasValue)
            {
                products = products.Where(p => p.CategoryID == request.CategoryID.Value);
            }
            if (request.IsActive.HasValue)
            {
                products = products.Where(p => p.IsActive == request.IsActive.Value);
            }
            if (request.MinPrice.HasValue)
            {
                products = products.Where(p => p.Price >= request.MinPrice.Value);
            }
            if (request.MaxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= request.MaxPrice.Value);
            }

            products = request.OrderBy.ToLower() switch
            {
                "name" => products.OrderBy(b => b.Name),
                "cheap" => products.OrderBy(b => b.Price),
                "expensive" => products.OrderByDescending(b => b.Price),
                "trend" => products.OrderBy(b => b.OrderDetails.Count),
                "rating" => products.OrderBy(b => b.Reviews.Average(r => r.Rating)),
                _ => products
            };

            var totalCount = products.ToList().Count;
            var items = products
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedList<FUMiniTikiSystem.DAL.Models.Product>(items, totalCount, request.PageNumber, request.PageSize);
        }

        public void AddProduct(FUMiniTikiSystem.DAL.Models.Product product)
        {
            productRepo.Create(product);
        }

        public void UpdateProduct(FUMiniTikiSystem.DAL.Models.Product product)
        {
            productRepo.Update(product);
        }

        public void DeleteProduct(int id)
        {
            var product = productRepo.Get(id);
            if (product != null)
            {
                product.IsActive = false; // Soft delete
                productRepo.Update(product);
            }
        }

        //public void DeleteProduct(int id)
        //{
        //    var product = productRepo.Get(id);
        //    if (product != null)
        //    {
        //        productRepo.Remove(product);
        //    }
        //}
    }
}
