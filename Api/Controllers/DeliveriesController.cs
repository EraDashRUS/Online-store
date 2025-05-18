using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления доставками
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр контроллера доставок
    /// </remarks>
    /// <param name="context">Контекст базы данных</param>
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получает список всех доставок
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список доставок</returns>
        /// <response code="200">Успешно возвращен список доставок</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Delivery>>> GetDeliveries(CancellationToken cancellationToken)
        {
            return await _context.Deliveries.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получает доставку по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор доставки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Данные доставки</returns>
        /// <response code="200">Доставка найдена</response>
        /// <response code="404">Доставка не найдена</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Delivery>> GetDelivery(int id, CancellationToken cancellationToken)
        {
            var delivery = await _context.Deliveries.FindAsync(new object[] { id }, cancellationToken);

            if (delivery == null)
            {
                return NotFound();
            }

            return delivery;
        }

        /// <summary>
        /// Обновляет данные доставки
        /// </summary>
        /// <param name="id">Идентификатор доставки</param>
        /// <param name="delivery">Обновленные данные доставки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Доставка успешно обновлена</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Доставка не найдена</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutDelivery(int id, Delivery delivery, CancellationToken cancellationToken)
        {
            if (id != delivery.Id)
            {
                return BadRequest();
            }

            _context.Entry(delivery).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeliveryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Создает новую доставку
        /// </summary>
        /// <param name="delivery">Данные новой доставки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Созданная доставка</returns>
        /// <response code="201">Доставка успешно создана</response>
        [HttpPost]
        public async Task<ActionResult<DeliveryResponseDto>> CreateDelivery(
    [FromBody] CreateDeliveryDto dto)
        {
            // Проверка существования заказа
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) return NotFound("Заказ не найден");

            var delivery = new Delivery
            {
                Status = dto.Status,
                DeliveryDate = dto.DeliveryDate,
                OrderId = dto.OrderId
            };

            _context.Deliveries.Add(delivery);

            // Обновляем заказ
            order.DeliveryId = delivery.Id;
            order.OrderDate = DateTime.UtcNow; // Устанавливаем текущую дату

            await _context.SaveChangesAsync();

            // Возвращаем DTO вместо модели
            return Ok(new DeliveryResponseDto
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
            });
        }

        /// <summary>
        /// Удаляет доставку
        /// </summary>
        /// <param name="id">Идентификатор доставки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Доставка успешно удалена</response>
        /// <response code="404">Доставка не найдена</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDelivery(int id, CancellationToken cancellationToken)
        {
            var delivery = await _context.Deliveries.FindAsync(new object[] { id }, cancellationToken);
            if (delivery == null)
            {
                return NotFound();
            }

            _context.Deliveries.Remove(delivery);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверяет существование доставки
        /// </summary>
        /// <param name="id">Идентификатор доставки</param>
        /// <returns>True если доставка существует, иначе False</returns>
        private bool DeliveryExists(int id)
        {
            return _context.Deliveries.Any(e => e.Id == id);
        }
    }
}
