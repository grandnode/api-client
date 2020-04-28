using Default;
using Microsoft.OData.Client;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OdataClientGrandNode
{
    class Program
    {
        public static string UserName = "admin@yourStore.com";
        public static string Password = "123456";
        public static string StoreUrl = "http://localhost:16592/";
        public static string TOKEN;
        private static Container container;
        static void Main(string[] args)
        {
            TOKEN = WebApiServices.GenerateToken().Result;
            container = new Container(new Uri(StoreUrl + "odata"));
            container.BuildingRequest += OnBuildingRequest;

            var product = WebApiServices.GetProduct().Result;
            WebApiServices.UpdatePrice(product.Id,876);
            WebApiServices.UpdateStock(product, "", 1200).Wait();

            //pictures
            byte[] binary = File.ReadAllBytes("./bill.jpg");
            var newPicture = WebApiServices.InsertPicture(binary).Result;

            ProductPictureDto newProductPicture = new ProductPictureDto();
            newProductPicture.PictureId = newPicture.Id;
            newProductPicture.MimeType = newPicture.MimeType;
            newProductPicture.SeoFilename = newPicture.SeoFilename;
            newProductPicture.AltAttribute = newPicture.AltAttribute;
            newProductPicture.TitleAttribute = newPicture.TitleAttribute;

            // WebApiServices.AddPictureToProduct(product, newProductPicture);

            //5d1613b4be3bff0a8444a7d7

            var updatedPicture = product.Pictures.FirstOrDefault();
            updatedPicture.DisplayOrder = 4;
            WebApiServices.UpdatePicture(product, updatedPicture).Wait();

            //WebApiServices.RemoveProductPicture(product, "5d1613b4be3bff0a8444a7d7").Wait();

            //product spec
            ProductSpecificationAttributeDto spec = new ProductSpecificationAttributeDto();
            spec.AllowFiltering = true;
            spec.DisplayOrder = 1;
            spec.ShowOnProductPage = true;
            spec.AttributeType = SpecificationAttributeType.Option;
            spec.SpecificationAttributeId = "5d107e372e8d1143384d957";
            spec.SpecificationAttributeOptionId = "5d107e372e8d1143384d9580";
            //WebApiServices.AddProductSpecification(product, spec).Wait();

            ProductSpecificationAttributeDto updateSpec = product.SpecificationAttribute.FirstOrDefault();
            updateSpec.AllowFiltering = false;
            updateSpec.DisplayOrder = 2;
            updateSpec.ShowOnProductPage = false;
            updateSpec.SpecificationAttributeId = "5d107e372e8d1143384d9581";
            updateSpec.SpecificationAttributeOptionId = "5d107e372e8d1143384d9583";

            //WebApiServices.UpdateProductSpecification(product, updateSpec).Wait();

            //WebApiServices.RemoveProductSpecification(product, "").Wait();

            //tier price
            ProductTierPriceDto tierPrice = new ProductTierPriceDto();
            tierPrice.Quantity = 50;
            tierPrice.Price = 500;
            tierPrice.StartDateTimeUtc = DateTime.UtcNow;
            tierPrice.EndDateTimeUtc = DateTime.UtcNow.AddDays(5);

           // WebApiServices.AddTierPricesToProduct(product, tierPrice).Wait();

            ProductTierPriceDto updatedTierPrice = product.TierPrices.FirstOrDefault();
            updatedTierPrice.Quantity = 80;
            updatedTierPrice.Price = 600;
            updatedTierPrice.StartDateTimeUtc = DateTime.UtcNow.AddDays(2);
            updatedTierPrice.EndDateTimeUtc = DateTime.UtcNow.AddDays(7);

            WebApiServices.UpdateTierPrices(product, updatedTierPrice).Wait();
            //WebApiServices.DeleteTierPrices(product,"5d161422be3bff0a8444a83a").Wait();

            //manufacturer

            ProductManufacturerDto manufacturer = new ProductManufacturerDto();
            manufacturer.IsFeaturedProduct = true;
            manufacturer.ManufacturerId = "5d107e2e2e8d1143384d9567";
            WebApiServices.AddManufacturer(product, manufacturer).Wait();

            ProductManufacturerDto updatedManufacturer = new ProductManufacturerDto();
            updatedManufacturer.IsFeaturedProduct = false;
            updatedManufacturer.ManufacturerId = "5d107e2e2e8d1143384d9567";
            WebApiServices.UpdateManufacturer(product, updatedManufacturer).Wait();

            //WebApiServices.DeleteManufacturer(product, "5d107e2e2e8d1143384d9567").Wait();

            // categories
            var categories = WebApiServices.GetCategories().Result.ToList();
            var category = categories.FirstOrDefault();

            //CategoryDto newCategory = new CategoryDto();
            //newCategory.Name = "New category";
            //newCategory.SeName = "new-category";
            //newCategory.Published = true;
            //newCategory.PageSize = 10;
            //newCategory.Id = "";
            //WebApiServices.InsertCategory(container, newCategory);

            WebApiServices.UpdateCategory("5d107df92e8d1143384d9536", "new title22");

            //var categoryToDelete = categories.LastOrDefault();
            //WebApiServices.DeleteCategory(container, categoryToDelete);

            //product category
            ProductCategoryDto updatedProductCategory = product.Categories.FirstOrDefault();
            updatedProductCategory.IsFeaturedProduct = true;

            WebApiServices.UpdateProductCategoryMethod(product.Id, updatedProductCategory).Wait();

            //users
            var customer = WebApiServices.GetCustomerByEmail(UserName);
            var role = WebApiServices.GetFirstCustomerRole(container);

            var address = new AddressDto();
            address.FirstName = "John";
            address.LastName = "Smith";
            address.Address1 = "Address 1";
            address.Email = "email@test.com";
            address.City = "New York";
            address.PhoneNumber = "111111111";
            address.ZipPostalCode = "33333";
            address.CreatedOnUtc = DateTime.Now;
            address.CountryId = "1";
           

            //WebApiServices.AssignAddressToCustomer(container, address, customer).Wait();
            var addressToDelete = customer.Addresses.LastOrDefault();

            WebApiServices.DeleteCustomerAddress(addressToDelete, customer).Wait();
        }
        private static void OnBuildingRequest(object sender, BuildingRequestEventArgs e)
        {
            e.Headers.Add("Authorization", "Bearer " + TOKEN);
        }
    }
}
