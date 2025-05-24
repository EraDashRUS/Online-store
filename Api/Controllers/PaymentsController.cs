using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления платежами
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр контроллера платежей
    /// </remarks>
    /// <param name="context">Контекст базы данных</param>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получает список всех платежей
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список платежей</returns>
        /// <response code="200">Успешно возвращен список платежей</response>
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
        public async Task<ActionResult<Payment>> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            // Проверяем существование заказа
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) return NotFound("Заказ не найден");

            // Если для заказа уже есть платеж
            if (order.PaymentId != null)
            {
                var existingPayment = await _context.Payments.FindAsync(order.PaymentId);
                if (existingPayment != null)
                {
                    return Conflict("Платеж для этого заказа уже существует");
                }
            }

            var payment = new Payment
            {
                Status = dto.Status,
                Amount = dto.Amount,
                OrderId = dto.OrderId,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            try
            {
                await _context.SaveChangesAsync();

                
                order.PaymentId = payment.Id;
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Ошибка при сохранении платежа: {ex.Message}");
            }
        }

        /// <summary>
        /// Удаляет платеж
        /// </summary>
        /// <param name="id">Идентификатор платежа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Платеж успешно удален</response>
        /// <response code="404">Платеж не найден</response>
        [Authorize(Policy = "AdminOnly")]
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
