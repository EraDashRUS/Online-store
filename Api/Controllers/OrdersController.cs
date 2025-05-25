using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(
        ApplicationDbContext context,
        IOrderService orderService) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IOrderService _orderService = orderService;

        /// <summary>
        /// Получить список всех заказов (только для администратора)
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список заказов</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(CancellationToken cancellationToken)
        {
            return await _context.Orders.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получить заказ по идентификатору
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные заказа или 404</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> GetOrder(int id, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        /// <summary>
        /// Оформить заказ из корзины (только для администратора)
        /// </summary>
        /// <param name="cartId">ID корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные заказа или 404</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("cart/{cartId}/checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> Checkout(int cartId, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(cartId, cancellationToken);
                return Ok(order);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Обновить данные заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="order">Обновлённые данные заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>204, 400 или 404</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutOrder(int id, Order order, CancellationToken cancellationToken)
        {
            if (id != order.Id)
                return BadRequest();

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Создать новый заказ
        /// </summary>
        /// <param name="orderDto">Данные для создания заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданный заказ</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Order>> PostOrder(OrderCreateDto orderDto, CancellationToken cancellationToken)
        {
          
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product) 
                .FirstOrDefaultAsync(c => c.Id == orderDto.CartId, cancellationToken);

            if (cart == null)
                return BadRequest("Корзина не найдена");

            if (!cart.CartItems.Any())
                return BadRequest("Корзина пуста");

           
            decimal totalAmount = cart.CartItems.Sum(item =>
                item.Quantity * (item.Product?.Price ?? 0));

            
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                Status = orderDto.Status,
                TotalAmount = totalAmount,
                DeliveryAddress = orderDto.DeliveryAddress,
                UserId = orderDto.UserId,
                CartId = orderDto.CartId,
                Delivery = new Delivery
                {
                    Status = orderDto.DeliveryStatus,
                    DeliveryDate = orderDto.DeliveryDate
                },
                Payment = new Payment
                {
                    Status = orderDto.PaymentStatus,
                    Amount = totalAmount,
                    PaymentDate = DateTime.UtcNow
                }
            };

            _context.Orders.Add(order);

         
            cart.Status = "ConvertedToOrder";

            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        private static Order CreateOrderFromDto(OrderCreateDto orderDto)
        {
            return new Order
            {
                OrderDate = DateTime.UtcNow,
                Status = orderDto.Status,
                TotalAmount = orderDto.TotalAmount,
                DeliveryAddress = orderDto.DeliveryAddress,
                UserId = orderDto.UserId,
                CartId = orderDto.CartId,
                Delivery = new Delivery
                {
                    Status = orderDto.DeliveryStatus,
                    DeliveryDate = orderDto.DeliveryDate
                },
                Payment = new Payment
                {
                    Status = orderDto.PaymentStatus,
                    Amount = orderDto.TotalAmount,
                    PaymentDate = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Удалить заказ по идентификатору
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>204 или 404</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Проверить, существует ли заказ по ID
        /// </summary>
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}