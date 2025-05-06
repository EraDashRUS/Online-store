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
    /// Контроллер для управления заказами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера заказов
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает список всех заказов
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список заказов</returns>
        /// <response code="200">Успешно возвращен список заказов</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(CancellationToken cancellationToken)
        {
            return await _context.Orders.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получает заказ по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Данные заказа</returns>
        /// <response code="200">Заказ найден</response>
        /// <response code="404">Заказ не найден</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> GetOrder(int id, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        /// <summary>
        /// Обновляет данные заказа
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <param name="order">Обновленные данные заказа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Заказ успешно обновлен</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Заказ не найден</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutOrder(int id, Order order, CancellationToken cancellationToken)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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
        /// Создает новый заказ
        /// </summary>
        /// <param name="order">Данные нового заказа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Созданный заказ</returns>
        /// <response code="201">Заказ успешно создан</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Order>> PostOrder(Order order, CancellationToken cancellationToken)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        /// <summary>
        /// Удаляет заказ
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Заказ успешно удален</response>
        /// <response code="404">Заказ не найден</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверяет существование заказа
        /// </summary>
        /// <param name="id">Идентификатор заказа</param>
        /// <returns>True если заказ существует, иначе False</returns>
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
