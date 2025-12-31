// Optional: Add this endpoint to Controllers/Api/ProductsController.cs for server-side barcode lookup
// This provides a backend API for barcode scanning validation

/*
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
public async Task<ActionResult<bool>> ValidateBarcode([FromBody] string barcode)
{
    try
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return BadRequest("Barcode cannot be empty");
        }

        var exists = await _productService.BarcodeExistsAsync(barcode);
        return Ok(exists);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error validating barcode: {ex.Message}");
    }
}

[HttpPost("search-by-barcode")]
public async Task<ActionResult<List<Product>>> SearchByBarcode([FromBody] List<string> barcodes)
{
    try
    {
        if (barcodes == null || !barcodes.Any())
        {
            return BadRequest("Barcode list cannot be empty");
        }

        var products = await _productService.GetProductsByBarcodesAsync(barcodes);
        return Ok(products);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error searching products: {ex.Message}");
    }
}

// Add these methods to IProductService interface:
Task<Product> GetProductByBarcodeAsync(string barcode);
Task<bool> BarcodeExistsAsync(string barcode);
Task<List<Product>> GetProductsByBarcodesAsync(List<string> barcodes);

// Implement in ProductService:
public async Task<Product> GetProductByBarcodeAsync(string barcode)
{
    return await _context.Products
        .Include(p => p.Category)
        .Include(p => p.Type)
        .Include(p => p.Supplier)
        .FirstOrDefaultAsync(p => p.Barcode.ToLower() == barcode.ToLower());
}

public async Task<bool> BarcodeExistsAsync(string barcode)
{
    return await _context.Products
        .AnyAsync(p => p.Barcode.ToLower() == barcode.ToLower());
}

public async Task<List<Product>> GetProductsByBarcodesAsync(List<string> barcodes)
{
    var lowerBarcodes = barcodes.Select(b => b.ToLower()).ToList();
    return await _context.Products
        .Include(p => p.Category)
        .Include(p => p.Type)
        .Include(p => p.Supplier)
        .Where(p => lowerBarcodes.Contains(p.Barcode.ToLower()))
        .ToListAsync();
}
*/

// Then update ProductList.razor to use this endpoint instead of local search:
/*
private async Task OnBarcodeScanned(KeyboardEventArgs e)
{
    if (e.Key == "Enter" && !string.IsNullOrEmpty(scannedBarcode))
    {
        try
        {
            // Call backend API instead
            var response = await Http.GetAsync($"api/products/by-barcode/{Uri.EscapeDataString(scannedBarcode.Trim())}");
            
            if (response.IsSuccessStatusCode)
            {
                barcodeSearchResult = await response.Content.ReadAsAsync<Product>();
                barcodeSearchError = false;
            }
            else
            {
                barcodeSearchError = true;
                barcodeSearchResult = null;
            }
            
            scannedBarcode = string.Empty;
            await barcodeInputRef.FocusAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning barcode: {ex.Message}");
            barcodeSearchError = true;
        }
    }
}

// Add this to top of ProductList.razor
@inject HttpClient Http
*/
