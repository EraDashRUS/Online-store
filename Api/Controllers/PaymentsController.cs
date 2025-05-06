using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models.OnlineStore.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления платежами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера платежей
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает список всех платежей
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список платежей</returns>
        /// <response code="200">Успешно возвращен список платежей</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments(CancellationToken cancellationToken)
        {
            return await _context.Payments.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получает платеж по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор платежа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Данные платежа</returns>
        /// <response code="200">Платеж найден</response>
        /// <response code="404">Платеж не найден</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Payment>> GetPayment(int id, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        /// <summary>
        /// Обновляет данные платежа
        /// </summary>
        /// <param name="id">Идентификатор платежа</param>
        /// <param name="payment">Обновленные данные платежа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Платеж успешно обновлен</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Платеж не найден</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPayment(int id, Payment payment, CancellationToken cancellationToken)
        {
            if (id != payment.Id)
            {
                return BadRequest();
            }

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
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
        /// Создает новый платеж
        /// </summary>
        /// <param name="payment">Данные нового платежа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Созданный платеж</returns>
        /// <response code="201">Платеж успешно создан</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Payment>> PostPayment(Payment payment, CancellationToken cancellationToken)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction("GetPayment", new { id = payment.Id }, payment);
        }

        /// <summary>
        /// Удаляет платеж
        /// </summary>
        /// <param name="id">Идентификатор платежа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Платеж успешно удален</response>
        /// <response code="404">Платеж не найден</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверяет существование платежа
        /// </summary>
        /// <param name="id">Идентификатор платежа</param>
        /// <returns>True если платеж существует, иначе False</returns>
        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
