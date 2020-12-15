using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMP2084BeerStore.Data;
using COMP2084BeerStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// stripe payment references
using Stripe;
using System.Configuration;  // this is to read the Stripe API keys from appsettings.json
using Microsoft.Extensions.Configuration;

namespace COMP2084BeerStore.Controllers
{
    public class StoreController : Controller
    {
        // db connection
        private readonly ApplicationDbContext _context;
        IConfiguration _iconfiguration;

        // constructor that instantiates an instance of our db connection
        public StoreController(ApplicationDbContext context, IConfiguration iconfiguration)
        {
            _context = context;
            _iconfiguration = iconfiguration;
        }

        public IActionResult Index()
        {
            //// use our mock Category model to create and display 10 categories
            //// first, create an object to hold a list of categories
            //var categories = new List<Category>();

            //for (var i = 1; i <= 10; i++)
            //{
            //    categories.Add(new Category { Id = i, Name = "Category " + i.ToString() });
            //}

            // use the Categories DbSet in ApplicationDbContext to query the db for the list of Categories
            var categories = _context.Categories.OrderBy(c => c.Name).ToList();

            // modify the return View statement so that it now accepts a list of categories to pass to the view for display
            return View(categories);
        }

        // /Store/Browse/6
        public IActionResult Browse(int id)
        {
            // query Products for the selected Category
            var products = _context.Products.Where(p => p.CategoryId == id).OrderBy(p => p.ProductName).ToList();

            // get Name of selected Category.  Find() only filters on key fields
            ViewBag.category = _context.Categories.Find(id).Name.ToString();
            return View(products);
        }

        // /Store/AddCategory
        public IActionResult AddCategory()
        {
            // load a form to capture a new category object from the user
            return View();
        }

        // /Store/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int ProductId, int Quantity)
        {
            // query the db for the product price
            var price = _context.Products.Find(ProductId).Price;

            // get current Date & Time using built in .net function
            var currentDateTime = DateTime.Now;

            // CustomerId variable
            var CustomerId = GetCustomerId();

            // create and save a new Cart object
            var cart = new Cart
            {
                ProductId = ProductId,
                Quantity = Quantity,
                Price = price,
                DateCreated = currentDateTime,
                CustomerId = CustomerId
            };

            _context.Carts.Add(cart);
            _context.SaveChanges();

            // redirect to the Cart view
            return RedirectToAction("Cart");
        }

        private string GetCustomerId()
        {
            // check the session for an existing CustomerId
            if (HttpContext.Session.GetString("CustomerId") == null) {
                // if we don't already have an existing CustomerId in the session, check if customer is logged in
                var CustomerId = "";

                // if customer is logged in, use their email as the CustomerId
                if (User.Identity.IsAuthenticated)
                {
                    CustomerId = User.Identity.Name;
                }
                // if the customer is anonymous, use Guid to create a new identifier
                else
                {
                    CustomerId = Guid.NewGuid().ToString();
                } 

                // now store the CustomerId in a session variable
                HttpContext.Session.SetString("CustomerId", CustomerId);
            }

            // return the Session variable
            return HttpContext.Session.GetString("CustomerId");
        }

        // /Store/Cart
        public IActionResult Cart()
        {
            // fetch current cart for display
            var CustomerId = "";

            // in case user comes to cart page before adding anything, identify them first
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                CustomerId = GetCustomerId();
            }
            else
            {
                CustomerId = HttpContext.Session.GetString("CustomerId");
            }

            // query the db for this customer
            var cartItems = _context.Carts.Include(c => c.Product).Where(c => c.CustomerId == CustomerId).ToList();

            // pass the data to the view for display
            return View(cartItems);
        }

        [Authorize]
        // GET: /Store/Checkout
        public IActionResult Checkout()
        {
            return View();
        }

        // POST: /Store/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout([Bind("Address,City,Province,PostalCode")] Models.Order order)
        {
            // populate the 3 automatic order properties
            order.OrderDate = DateTime.Now;
            order.CustomerId = User.Identity.Name;

            // calc order total based on the current cart
            var cartCustomer = HttpContext.Session.GetString("CustomerId");
            var cartItems = _context.Carts.Where(c => c.CustomerId == cartCustomer);
            var orderTotal = (from c in cartItems
                              select c.Quantity * c.Price).Sum();
            order.Total = orderTotal;

            // use SessionsExtension object to store the order object in a session variable
            HttpContext.Session.SetObject("Order", order);

            // redirect to payment page
            return RedirectToAction("Payment");
        }

        // GET: /Cart/Payment
        [Authorize]
        public IActionResult Payment()
        {
            var order = HttpContext.Session.GetObject<Models.Order>("Order");
            // stripe charge amount must be in cents, not dollars
            ViewBag.Total = order.Total * 100;  

            // use iconfiguration to read key from appsettings
            ViewBag.PublishableKey = _iconfiguration["Stripe:PublishableKey"]; 

            return View();
        }

        // POST: /Cart/Payment
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Payment(string stripeToken)
        {
            // retriever order from session
            var order = HttpContext.Session.GetObject<Models.Order>("Order");

            // 1. create Stripe customer
            var customerService = new Stripe.CustomerService();
            var charges = new Stripe.ChargeService();

            StripeConfiguration.ApiKey = _iconfiguration["Stripe:SecretKey"];
            Stripe.Customer customer = customerService.Create(new Stripe.CustomerCreateOptions
            {
                Source = stripeToken,
                Email = User.Identity.Name
            });

            // 2. create Stripe charge
            var charge = charges.Create(new Stripe.ChargeCreateOptions
            {
                Amount = Convert.ToInt32(order.Total * 100),
                Description = "COMP2084 Beer Store Purchase",
                Currency = "cad",
                Customer = customer.Id
            });

            // 3. save a new order to our db
            _context.Orders.Add(order);
            _context.SaveChanges();

            // 4. save the cart items as new OrderDetails to our db
            var cartItems = _context.Carts.Where(c => c.CustomerId == HttpContext.Session.GetString("CartUsername"));
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Cost = item.Price
                };

                _context.OrderDetails.Add(orderDetail);
            }

            _context.SaveChanges();

            // 5. delete the cart items from this order 
            foreach (var item in cartItems)
            {
                _context.Carts.Remove(item);
            }

            _context.SaveChanges();

            // 6. load an order confirmation page
            return RedirectToAction("Details", "Orders", new { @id = order.Id });
        }

    }
}
