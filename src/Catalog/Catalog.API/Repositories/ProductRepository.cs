using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Data.Interfaces;
using Catalog.API.Entitites;
using Catalog.API.Repositories.Interfaces;
using MongoDB.Driver;

namespace Catalog.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ICatalogContext _context;

        public ProductRepository(ICatalogContext context)
        {
            _context = context;
        }
        public async Task<Product> GetProductAsync(string id)
        {
            return await _context
                .Products
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Product>> GetProductByNameAsync(string name)
        {
            var filter = Builders<Product>.Filter.ElemMatch(p => p.Name, name);
            return await _context
                .Products
                .Find(filter)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductByCategoryAsync(string categoryName)
        {
            var filters = new List<FilterDefinition<Product>>();
            if (!categoryName.Equals(""))
                filters.Add(Builders<Product>.Filter.Eq(p=>p.Category, categoryName));
            return await _context
                .Products
                .Find(filters.FirstOrDefault())
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context
                .Products
                .Find(p => true)
                .ToListAsync();
        }

        public async Task CreateAsync(Product product)
        {
            await _context.Products.InsertOneAsync(product);
        }
        public async Task<bool> UpdateAsync(Product product)
        {
           var updateResult= await _context.Products
                .ReplaceOneAsync(filter: g => g.Id == product.Id, product);
           return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
           var deleteResult= await _context.Products.DeleteOneAsync(id);
           return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }

    }
}