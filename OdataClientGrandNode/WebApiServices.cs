using Default;
using Microsoft.OData.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OdataClientGrandNode
{
    public class WebApiServices
    {
        private static string Token = GenerateToken().Result;
        private static Default.Container container = InitContainer(Token, Program.StoreUrl);

        public static void onBuildingRequest(object sender, Microsoft.OData.Client.BuildingRequestEventArgs e)
        {
            e.Headers.Add("Authorization", "Bearer " + Token);
        }

        public static Container InitContainer(string token, string storeurl)
        {
            Token = token;
            Program.StoreUrl = storeurl;
            container = new Default.Container(new Uri(Program.StoreUrl + "/odata/"));
            container.BuildingRequest += onBuildingRequest;
            container.MergeOption = MergeOption.AppendOnly;
            return container;
        }

        public static async Task<IEnumerable<CategoryDto>> GetCategories()
        {
            var list = await container.Category.ExecuteAsync();
            return list;
        }
        public static void InsertCategory(CategoryDto model)
        {
            container.AddToCategory(model);
            container.SaveChangesAsync().Wait();
        }

        public static void DeleteCategory(CategoryDto model)
        {
            container.DeleteObject(model);
            container.SaveChangesAsync().Wait();
        }
        public static void UpdateCategory( string oldName,string newName)
        {
            var test = new DataServiceCollection<CategoryDto>(container.Category.ExecuteAsync().Result);
            test.Where(x=>x.Id == oldName).FirstOrDefault().Name = newName;
            container.SaveChangesAsync(SaveChangesOptions.PostOnlySetProperties).Wait();
        }

        public static async Task UpdateProductCategoryMethod( string productId, ProductCategoryDto model)
        {
            var products = container.Product.ExecuteAsync().Result;
            var product = products.Where(x => x.Id == productId).FirstOrDefault();

            await product.UpdateProductCategory(model.CategoryId,model.IsFeaturedProduct).GetValueAsync();
            
        }
        public static void UpdatePrice(string productId, decimal price)
        {
            var products = container.Product.ExecuteAsync().Result;
            var product = products.Where(x => x.Id == productId).FirstOrDefault();
            product.Price = price;
            container.UpdateObject(product);
            container.SaveChangesAsync().Wait();
        }
        public static async Task<ProductDto> GetProduct()
        {
            var products = await container.Product.ExecuteAsync();
            return products.FirstOrDefault();
        }

        public static PictureDto GetPicture(string id)
        {
            return container.Picture.AddQueryOption("key", id).ExecuteAsync().Result.ToList().FirstOrDefault();
        }
        public static async Task<PictureDto> InsertPicture(byte[] binary)
        {
            var picture = new PictureDto();
            picture.PictureBinary = binary;
            picture.MimeType = "image/jpeg";
            picture.SeoFilename = "";
            picture.IsNew = true;
            picture.Id = "";
            container.AddToPicture(picture);
            container.SaveChangesAsync().Wait();
            return picture;
        }
        public static async Task UpdatePicture(ProductDto product,ProductPictureDto model)
        {
           await product.UpdateProductPicture(model.PictureId, model.MimeType, model.SeoFilename, model.AltAttribute, model.DisplayOrder, model.TitleAttribute).GetValueAsync();
        }
        public static async Task RemoveProductPicture(ProductDto product,string id)
        {
            product.DeleteProductPicture(id);
        }
        public static async Task UpdateStock(ProductDto product, string warehouseId, int stock)
        {
            await product.UpdateStock(warehouseId,stock).GetValueAsync();
        }
        public static void AddPictureToProduct(ProductDto product,ProductPictureDto PPD)
        {
            product.CreateProductPicture(PPD.PictureId,PPD.MimeType,PPD.SeoFilename,PPD.AltAttribute,PPD.DisplayOrder,PPD.TitleAttribute).GetValueAsync().Wait();
        }

        public static async Task AddProductSpecification(ProductDto product, ProductSpecificationAttributeDto specification)
        {
            await product.CreateProductSpecification("", specification.DisplayOrder, specification.CustomValue, specification.AttributeType, specification.AllowFiltering,
                                               specification.SpecificationAttributeId, specification.ShowOnProductPage, specification.SpecificationAttributeOptionId).GetValueAsync();
        }
        public static async Task UpdateProductSpecification(ProductDto product, ProductSpecificationAttributeDto specification)
        {
            await product.UpdateProductSpecification(specification.Id, specification.DisplayOrder, specification.CustomValue, specification.AttributeType, specification.AllowFiltering,
                                               specification.SpecificationAttributeId, specification.ShowOnProductPage, specification.SpecificationAttributeOptionId).GetValueAsync();
        }
        public static async Task RemoveProductSpecification(ProductDto product, string id)
        {
            await product.DeleteProductSpecification(id).GetValueAsync();
        }

        public static async Task AddTierPricesToProduct(ProductDto product, ProductTierPriceDto tierPrice)
        {
            await product.CreateProductTierPrice(tierPrice.Quantity, tierPrice.Price, "", "", tierPrice.StartDateTimeUtc, tierPrice.EndDateTimeUtc).GetValueAsync();
        }
        public static async Task UpdateTierPrices(ProductDto product, ProductTierPriceDto tierPrice)
        {
            await product.UpdateProductTierPrice(tierPrice.Id, 400, 40, "", "", tierPrice.StartDateTimeUtc, tierPrice.EndDateTimeUtc).GetValueAsync();
        }
        public static async Task DeleteTierPrices(ProductDto product, string id)
        {
            await product.DeleteProductTierPrice(id).GetValueAsync();
        }

        public static async Task AddManufacturer(ProductDto product, ProductManufacturerDto model)
        {
            await product.CreateProductManufacturer(model.ManufacturerId, model.IsFeaturedProduct).GetValueAsync();
        }
        public static async Task UpdateManufacturer(ProductDto product, ProductManufacturerDto model)
        {
            await product.UpdateProductManufacturer(model.ManufacturerId, model.IsFeaturedProduct).GetValueAsync();
        }
        public static async Task DeleteManufacturer(ProductDto product, string id)
        {
            await product.DeleteProductManufacturer(id).GetValueAsync();
        }

        public static CustomerDto GetCustomerByEmail( string email)
        {
            return container.Customer.AddQueryOption("key", email).ExecuteAsync().Result.ToList().FirstOrDefault();
        }
        public static CustomerRoleDto GetFirstCustomerRole(Container container)
        {
            return container.CustomerRole.ExecuteAsync().Result.FirstOrDefault();
        }

        public static async Task AssignAddressToCustomer(AddressDto address, CustomerDto customer)
        {
            await customer.AddAddress("", address.City, address.Email,address.Company, address.Address1, address.Address2, address.LastName,
                                address.CountryId, address.FaxNumber, address.FirstName, address.VatNumber, address.PhoneNumber,
                                address.CustomAttributes, address.CreatedOnUtc, address.ZipPostalCode, address.StateProvinceId).GetValueAsync();
           
        }
        public static async Task DeleteCustomerAddress(AddressDto address, CustomerDto customer)
        {
            await customer.DeleteAddress(address.Id).GetValueAsync();
        }
 
        public static async Task<string> GenerateToken()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Program.StoreUrl);
            var credentials = new GenerateTokenModel();
            credentials.Email = Program.UserName;
            credentials.Password = Base64Encode(Program.Password);

            var serializedJson = JsonConvert.SerializeObject(credentials);
            var httpContent = new StringContent(serializedJson.ToString(), Encoding.UTF8, "application/json");
            var result = await client.PostAsync("api/token/create", httpContent);
            return result.Content.ReadAsStringAsync().Result;
        }
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
    public class GenerateTokenModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
