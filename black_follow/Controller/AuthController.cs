using black_follow.Data;
using black_follow.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace black_follow.Controller;

[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;

    [HttpPost]
    public IActionResult Login()
    {
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var order =  await _context.Orders.AddAsync(new Order()
            {
                Name = "hello"
            }
        );
        _context.SaveChanges();
        return Ok(order.Entity);
    }
    
    [HttpGet("get_orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _context.Orders.ToListAsync();
        return Ok(orders);
    }
    
    public AuthController(DataContext context)
    {
        _context = context;
    }
}