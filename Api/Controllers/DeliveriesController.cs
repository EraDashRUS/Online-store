using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Delivery;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// API для управления доставками (создание, просмотр, обновление, удаление).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получить список всех доставок (только для администратора).
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Список доставок.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Delivery>>> GetDeliveries(CancellationToken cancellationToken)
        {
            return await _context.Deliveries.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получить доставку по идентификатору.
        /// </summary>
        /// <param name="id">ID доставки.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Доставка или 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Delivery>> GetDelivery(int id, CancellationToken cancellationToken)
        {
            var delivery = await _context.Deliveries.FindAsync(new object[] { id }, cancellationToken);
            if (delivery == null)
                return NotFound();
            return delivery;
        }

        /// <summary>
        /// Обновить данные доставки.
        /// </summary>
        /// <param name="id">ID доставки.</param>
        /// <param name="delivery">Обновленные данные доставки.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>204, 400 или 404.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutDelivery(int id, Delivery delivery, CancellationToken cancellationToken)
        {
            if (id != delivery.Id)
                return BadRequest();

            _context.Entry(delivery).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeliveryExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Создать новую доставку и связать с заказом.
        /// </summary>
        /// <param name="dto">Данные новой доставки.</param>
        /// <returns>Созданная доставка (DTO) или 404, если заказ не найден.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeliveryResponseDto>> CreateDelivery([FromBody] CreateDeliveryDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return NotFound("Заказ не найден");

            var delivery = CreateDeliveryEntity(dto);
            _context.Deliveries.Add(delivery);

            UpdateOrderForDelivery(order, delivery);

            await _context.SaveChangesAsync();

            return Ok(MapToDeliveryResponseDto(delivery, order));
        }

        private static Delivery CreateDeliveryEntity(CreateDeliveryDto dto)
        {
            return new Delivery
            {
                Status = dto.Status,
                DeliveryDate = dto.DeliveryDate,
                OrderId = dto.OrderId
            };
        }

        private static void UpdateOrderForDelivery(Order order, Delivery delivery)
        {
            order.DeliveryId = delivery.Id;
            order.OrderDate = DateTime.UtcNow;
        }

        private static DeliveryResponseDto MapToDeliveryResponseDto(Delivery delivery, Order order)
        {
            return new DeliveryResponseDto
            {
                Id = delivery.Id,
                Status = delivery.Status,
                DeliveryDate = delivery.DeliveryDate,
                OrderId = delivery.OrderId,
                Order = new OrderBriefDto
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount
                }
            };
        }

        /// <summary>
        /// Удалить доставку по идентификатору.
        /// </summary>
        /// <param name="id">ID доставки.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>204 или 404.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDelivery(int id, CancellationToken cancellationToken)
        {
            var delivery = await _context.Deliveries.FindAsync(new object[] { id }, cancellationToken);
            if (delivery == null)
                return NotFound();

            _context.Deliveries.Remove(delivery);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверить существование доставки по ID.
        /// </summary>
        private bool DeliveryExists(int id)
        {
            return _context.Deliveries.Any(e => e.Id == id);
        }
    }
}
