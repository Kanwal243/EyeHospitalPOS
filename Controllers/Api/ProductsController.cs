using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EyeHospitalPOS.Services;
using EyeHospitalPOS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using EyeHospitalPOS.Interfaces;

namespace EyeHospitalPOS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly BarcodeService _barcodeService;

        public ProductsController(IProductService productService, BarcodeService barcodeService)
        {
            _productService = productService;
            _barcodeService = barcodeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<Product>>> GetActive()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            if (product == null) return BadRequest();

            try
            {
                var created = await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating product: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> Update(int id, [FromBody] Product product)
        {
            if (product == null || product.Id != id) return BadRequest();

            try
            {
                var updated = await _productService.UpdateProductAsync(product);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating product: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting product: {ex.Message}");
            }
        }

        [HttpGet("by-barcode/{barcode}")]
        public async Task<ActionResult<Product>> GetByBarcode(string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    return BadRequest("Barcode cannot be empty");
                }

                var product = await _productService.GetProductByBarcodeAsync(barcode);
                if (product == null)
                {
                    return NotFound($"No product found with barcode: {barcode}");
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving product: {ex.Message}");
            }
        }

        [HttpPost("validate-barcode")]
        public async Task<ActionResult<BarcodeValidationResult>> ValidateBarcode([FromBody] BarcodeValidationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Barcode))
                {
                    return BadRequest(new BarcodeValidationResult 
                    { 
                        IsValid = false, 
                        Message = "Barcode cannot be empty" 
                    });
                }

                var exists = await _productService.BarcodeExistsAsync(request.Barcode);
                var product = exists ? await _productService.GetProductByBarcodeAsync(request.Barcode) : null;

                return Ok(new BarcodeValidationResult
                {
                    IsValid = exists,
                    Message = exists ? "Barcode is valid" : "Barcode not found in system",
                    ProductId = product?.Id,
                    ProductName = product?.Name,
                    StockQuantity = product?.StockQuantity ?? 0,
                    SalePrice = product?.SalePrice ?? 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BarcodeValidationResult 
                { 
                    IsValid = false, 
                    Message = $"Error validating barcode: {ex.Message}" 
                });
            }
        }

        [HttpPost("search-by-barcode")]
        public async Task<ActionResult<List<Product>>> SearchByBarcodes([FromBody] BarcodeSearchRequest request)
        {
            try
            {
                if (request?.Barcodes == null || !request.Barcodes.Any())
                {
                    return BadRequest("Barcode list cannot be empty");
                }

                /*                var products = await _productService.GetProductsByBarcodesAsync(request.Barcodes);
                */
                var products = new List<Product>();
                foreach (var barcode in request.Barcodes)
                {
                    var product = await _productService.GetProductByBarcodeAsync(barcode);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                }
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching products: {ex.Message}");
            }
        }

        [HttpPost("validate-bulk-barcodes")]
        public async Task<ActionResult<BulkBarcodeValidationResult>> ValidateBulkBarcodes([FromBody] BulkBarcodeValidationRequest request)
        {
            try
            {
                if (request?.Barcodes == null || !request.Barcodes.Any())
                {
                    return BadRequest(new BulkBarcodeValidationResult { IsSuccess = false, Message = "Barcode list empty" });
                }

                var results = new BulkBarcodeValidationResult();
                var validCount = 0;
                var invalidCount = 0;

                foreach (var barcode in request.Barcodes)
                {
                    var exists = await _productService.BarcodeExistsAsync(barcode);

                    if (exists)
                    {
                        var product = await _productService.GetProductByBarcodeAsync(barcode);
                        if (product != null)
                        {
                            results.ValidBarcodes.Add(new BarcodeValidationResult
                            {
                                IsValid = true,
                                Barcode = barcode,
                                ProductId = product.Id,
                                ProductName = product.Name,
                                StockQuantity = product.StockQuantity,
                                SalePrice = product.SalePrice
                            });
                            validCount++;
                        }
                    }
                    else
                    {
                        results.InvalidBarcodes.Add(new BarcodeValidationResult
                        {
                            IsValid = false,
                            Barcode = barcode,
                            Message = "Barcode not found"
                        });
                        invalidCount++;
                    }
                }

                results.IsSuccess = validCount > 0;
                results.Message = $"Validation complete: {validCount} valid, {invalidCount} invalid";
                results.ValidCount = validCount;
                results.InvalidCount = invalidCount;

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BulkBarcodeValidationResult 
                { 
                    IsSuccess = false, 
                    Message = $"Error validating barcodes: {ex.Message}" 
                });
            }
        }

        [HttpPost("import-products")]
        public async Task<ActionResult<BulkImportResult>> ImportProducts([FromBody] BulkProductImportRequest request)
        {
            var result = new BulkImportResult();

            try
            {
                if (request?.Products == null || !request.Products.Any())
                {
                    result.IsSuccess = false;
                    result.Message = "No products provided for import";
                    return BadRequest(result);
                }

                foreach (var item in request.Products)
                {
                    try
                    {
                        // Check if barcode already exists
                        if (request.SkipDuplicates && await _productService.BarcodeExistsAsync(item.Barcode))
                        {
                            result.SkippedCount++;
                            continue;
                        }

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Barcode))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Product missing required fields: {item.Barcode}");
                            continue;
                        }

                        var product = new Product
                        {
                            Name = item.Name,
                            Barcode = item.Barcode,
                            CategoryId = item.CategoryId,
                            TypeId = item.TypeId,
                            SupplierId = item.SupplierId,
                            CostPrice = item.CostPrice,
                            SalePrice = item.SalePrice,
                            StockQuantity = item.StockQuantity,
                            ReorderLevel = item.ReorderLevel,
                            IsActive = true
                        };

                        var created = await _productService.CreateProductAsync(product);
                        result.ImportedProducts.Add(created);
                        result.ImportedCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Error importing product {item.Barcode}: {ex.Message}");
                    }
                }

                result.IsSuccess = result.ImportedCount > 0;
                result.Message = $"Import completed: {result.ImportedCount} imported, {result.SkippedCount} skipped, {result.FailedCount} failed";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Import failed: {ex.Message}";
                result.Errors.Add(ex.Message);
                return StatusCode(500, result);
            }
        }

        [HttpPost("generate-print-labels")]
        public async Task<ActionResult<BarcodePrintResult>> GeneratePrintLabels([FromBody] BarcodePrintRequest request)
        {
            var result = new BarcodePrintResult();

            try
            {
                if (request?.ProductIds == null || !request.ProductIds.Any())
                {
                    result.IsSuccess = false;
                    result.Message = "No products selected for printing";
                    return BadRequest(result);
                }

                // Fetch all products
                var products = new List<Product>();
                foreach (var productId in request.ProductIds)
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                }

                if (products.Count == 0)
                {
                    result.IsSuccess = false;
                    result.Message = "No valid products found for printing";
                    return NotFound(result);
                }

                // Generate print job
                result.PrintJobId = Guid.NewGuid().ToString();
                result.BarcodeCount = products.Count * request.Quantity;
                result.IsSuccess = true;
                result.Message = $"Print job created: {result.BarcodeCount} labels ready to print";
                result.PdfUrl = $"/api/products/print-labels/{result.PrintJobId}";

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error generating print labels: {ex.Message}";
                return StatusCode(500, result);
            }
        }

        [HttpPost("decode-barcode-image")]
        public async Task<ActionResult<BarcodeDecodeResponse>> DecodeBarcodeImage([FromBody] BarcodeImageRequest request)
        {
            var response = new BarcodeDecodeResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(request?.ImageData))
                {
                    response.Success = false;
                    response.ErrorMessage = "Image data is required";
                    return BadRequest(response);
                }

                // Decode barcode from image using ZXing.Net
                var decodeResult = _barcodeService.DecodeFromBase64(request.ImageData);

                if (decodeResult.Success && !string.IsNullOrEmpty(decodeResult.Barcode))
                {
                    response.Success = true;
                    response.Barcode = decodeResult.Barcode;
                    response.Format = decodeResult.Format;

                    // Look up product by barcode
                    var product = await _productService.GetProductByBarcodeAsync(decodeResult.Barcode);
                    response.Product = product;

                    return Ok(response);
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = decodeResult.ErrorMessage ?? "No barcode detected in image";
                    return Ok(response); // Return 200 with success=false so client can continue scanning
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error decoding barcode: {ex.Message}";
                return StatusCode(500, response);
            }
        }

        [HttpPost("camera-scan")]
        public async Task<ActionResult<CameraScanResponse>> ProcessCameraScan([FromBody] CameraScanResult scanResult)
        {
            var response = new CameraScanResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(scanResult?.Barcode))
                {
                    response.IsProcessed = false;
                    response.Message = "Invalid scan data";
                    response.Errors.Add("Barcode cannot be empty");
                    return BadRequest(response);
                }

                var product = await _productService.GetProductByBarcodeAsync(scanResult.Barcode);

                if (product == null)
                {
                    response.IsProcessed = false;
                    response.Message = $"Product not found for barcode: {scanResult.Barcode}";
                    response.Errors.Add("Barcode not in system");
                    return NotFound(response);
                }

                response.IsProcessed = true;
                response.Message = "Scan processed successfully";
                response.Product = product;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsProcessed = false;
                response.Message = $"Error processing camera scan: {ex.Message}";
                response.Errors.Add(ex.Message);
                return StatusCode(500, response);
            }
        }
    }
}
