using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
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
        /// Получает список всех заказов
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(CancellationToken cancellationToken)
        {
            return await _context.Orders.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получает заказ по идентификатору
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> GetOrder(int id, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
            return order == null ? NotFound() : Ok(order);
        }

        /// <summary>
        /// Оформляет заказ из корзины
        /// </summary>
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
        /// Обновляет данные заказа
        /// </summary>
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
        /// Создает новый заказ
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Order>> PostOrder(OrderCreateDto orderDto, CancellationToken cancellationToken)
        {
            var order = new Order
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

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        /// <summary>
        /// Удаляет заказ
        /// </summary>
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

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}