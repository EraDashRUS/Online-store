using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления доставками
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера доставок
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public DeliveriesController(ApplicationDbContext context)
        {
            _context = context;
        }

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
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Delivery>> PostDelivery(Delivery delivery, CancellationToken cancellationToken)
        {
            _context.Deliveries.Add(delivery);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction("GetDelivery", new { id = delivery.Id }, delivery);
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
