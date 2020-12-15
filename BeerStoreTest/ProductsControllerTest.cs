using COMP2084BeerStore.Controllers;
using COMP2084BeerStore.Data;
using COMP2084BeerStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeerStoreTest
{
    [TestClass]
    public class ProductsControllerTest
    {
        // create db reference that will point to our in-memory db
        private ApplicationDbContext _context;

        // create empty product list to hold mock product data
        List<Product> products = new List<Product>();

        // declare controller we are going to test
        ProductsController controller;

        [TestInitialize]
        // this method runs automatically before each unit test to streamline the arranging
        public void TestInitialize()
        {
            // instantiate in-memory db
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // create mock data inside the in-memory db
            var category = new Category { Id = 505, Name = "Some Category" };

            products.Add(new Product { Id = 87, ProductName = "Prod 1", Price = 8, Category = category });
            products.Add(new Product { Id = 92, ProductName = "Prod 0", Price = 9, Category = category });
            products.Add(new Product { Id = 95, ProductName = "Prod 3", Price = 10, Category = category });

            foreach (var p in products)
            {
                _context.Products.Add(p);
            }

            _context.SaveChanges();

            // instantiate the products controller and pass it the mock db object (dependency injection)
            controller = new ProductsController(_context);
        }

        [TestMethod]
        public void IndexViewLoads()
        {
            // no arrange needed as all setup done first in TestInitialize()
            // act, casting the Result property to a ViewResult
            var result = controller.Index();
            var viewResult = (ViewResult)result.Result;

            // assert
            Assert.AreEqual("Index", viewResult.ViewName);
        }

        [TestMethod]
        public void IndexReturnsProductData()
        {
            // act
            var result = controller.Index();
            var viewResult = (ViewResult)result.Result;
            // cast the result's data Model to a list of products so we can check it
            List<Product> model = (List<Product>)viewResult.Model;

            // assert
            CollectionAssert.AreEqual(products.OrderBy(p => p.ProductName).ToList(), model);
        }

        // Question 7
        [TestMethod]
        public void DeleteCorrectView()
        {
            var id = 1;
            var result = controller.Delete(id); // valid ID
            var viewResult = (ViewResult)result.Result;
            Assert.AreEqual("Delete", viewResult.ViewName);
        }

        // Question 8
        [TestMethod]
        public void DeleteConfirmedSuccess()
        {
            var id = 1;
            var result = controller.DeleteConfirmed(id); // valid ID
            var product = _context.Products.Find(id);
            Assert.AreEqual(product, null);
        }
    }
}
