using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Payment;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления платежами.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получить список всех платежей (только для администратора).
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Список платежей.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments(CancellationToken cancellationToken)
        {
            return await _context.Payments.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получить платеж по идентификатору.
        /// </summary>
        /// <param name="id">ID платежа.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Платеж или 404.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Payment>> GetPayment(int id, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }

        /// <summary>
        /// Обновить данные платежа (только для администратора).
        /// </summary>
        /// <param name="id">ID платежа.</param>
        /// <param name="payment">Обновлённые данные платежа.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>204, 400 или 404.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPayment(int id, Payment payment, CancellationToken cancellationToken)
        {
            if (id != payment.Id)
                return BadRequest();

            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Создать новый платеж.
        /// </summary>
        /// <param name="dto">Данные нового платежа.</param>
        /// <returns>Созданный платеж или ошибка.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Payment>> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return NotFound("Заказ не найден");

            var conflictResult = await CheckOrderPaymentConflict(order);
            if (conflictResult != null)
                return conflictResult;

            var payment = CreatePaymentEntity(dto);

            _context.Payments.Add(payment);

            try
            {
                await _context.SaveChangesAsync();
                await LinkPaymentToOrder(order, payment);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Ошибка при сохранении платежа: {ex.Message}");
            }
        }

        private async Task<ActionResult?> CheckOrderPaymentConflict(Order order)
        {
            if (order.PaymentId != null)
            {
                var existingPayment = await _context.Payments.FindAsync(order.PaymentId);
                if (existingPayment != null)
                    return Conflict("Платеж для этого заказа уже существует");
            }
            return null;
        }

        private static Payment CreatePaymentEntity(CreatePaymentDto dto)
        {
            return new Payment
            {
                Status = dto.Status,
                Amount = dto.Amount,
                OrderId = dto.OrderId,
                PaymentDate = DateTime.UtcNow
            };
        }

        private async Task LinkPaymentToOrder(Order order, Payment payment)
        {
            order.PaymentId = payment.Id;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить платеж (только для администратора).
        /// </summary>
        /// <param name="id">ID платежа.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>204 или 404.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
            if (payment == null)
                return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверить существование платежа по ID.
        /// </summary>
        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
