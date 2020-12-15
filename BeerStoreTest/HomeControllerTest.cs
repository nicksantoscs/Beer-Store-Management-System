using COMP2084BeerStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeerStoreTest
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void IndexWithLoggerReturnsResult()
        {
            // arrange
            LoggerFactory loggerFactory = new LoggerFactory();
            ILogger<HomeController> logger = new Logger<HomeController>(loggerFactory);
            HomeController controller = new HomeController(logger);

            // act
            var result = controller.Index();

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PrivacyReturnsResult()
        {
            // arrange
            LoggerFactory loggerFactory = new LoggerFactory();
            ILogger<HomeController> logger = new Logger<HomeController>(loggerFactory);
            HomeController controller = new HomeController(logger);

            // act
            var result = controller.Privacy();

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PrivacyLoadsPrivacyView()
        {
            // arrange
            LoggerFactory loggerFactory = new LoggerFactory();
            ILogger<HomeController> logger = new Logger<HomeController>(loggerFactory);
            HomeController controller = new HomeController(logger);

            // act - case return type as ViewResult so we can check the view name
            var result = (ViewResult)controller.Privacy();

            // assert
            Assert.AreEqual("Privacy", result.ViewName);
        }
    }
}
