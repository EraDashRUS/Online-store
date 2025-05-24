using Microsoft.AspNetCore.Mvc;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Storage.Data;
using Microsoft.AspNetCore.Authorization;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Admin;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// API для администрирования заказов: просмотр, подтверждение, обновление статуса и редактирование заказов.
    /// </summary>
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAdminOrderService _adminService;
        private readonly IAdminCommentService _adminCommentService;
        private readonly ApplicationDbContext _context;

        public AdminOrdersController(
            IOrderService orderService,
            IAdminOrderService adminService,
            ApplicationDbContext context,
            IAdminCommentService adminCommentService)
        {
            _orderService = orderService;
            _adminService = adminService;
            _context = context;
            _adminCommentService = adminCommentService;
        }

        /// <summary>
        /// Подтвердить заказ администратором.
        /// </summary>
        /// <param name="cartId">ID корзины</param>
        /// <param name="dto">Данные для подтверждения заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>204 No Content</returns>
        [HttpPut("{cartId}/approve")]
        public async Task<IActionResult> ApproveOrder(
            int cartId,
            [FromBody] AdminOrderUpdateDto dto,
            CancellationToken cancellationToken)
        {
            await _adminService.ApproveOrderAsync(cartId, dto.AdminComment, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Получить статистику по заказам.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статистика заказов</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<OrderStatsDto>> GetStats(CancellationToken cancellationToken)
        {
            return await _adminService.GetOrderStatsAsync(cancellationToken);
        }

        /// <summary>
        /// Получить заказ с комментарием по ID.
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Заказ с комментарием</returns>
        [HttpGet("{id}/with-comment")]
        public async Task<ActionResult<OrderWithCommentDto>> GetOrderWithComment(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _orderService.GetOrderWithCommentAsync(id, cancellationToken);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Получить список всех заказов.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список заказов</returns>
        [HttpGet]
        public async Task<ActionResult<List<AdminOrderDto>>> GetAllAdminOrders(CancellationToken cancellationToken)
        {
            var orders = await GetAdminOrdersListAsync(cancellationToken);
            return Ok(orders);
        }

        private async Task<List<AdminOrderDto>> GetAdminOrdersListAsync(CancellationToken cancellationToken)
        {
            return await _context.Carts
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Where(c => c.Status != null)
                .Select(c => new AdminOrderDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserEmail = c.User.Email,
                    Status = c.Status!,
                    TotalAmount = c.CartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                    Items = c.CartItems.Select(ci => new OrderItemDto
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Product.Price
                    }).ToList()
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получить заказ по ID.
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные заказа</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminOrderDto>> GetAdminOrder(int id, CancellationToken cancellationToken)
        {
            try
            {
                return await _orderService.GetAdminOrderAsync(id, cancellationToken);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Обновить статус заказа.
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="statusDto">Новый статус и комментарий</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный заказ</returns>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(
            int id,
            [FromBody] OrderStatusUpdateDto statusDto,
            CancellationToken cancellationToken)
        {
            try
            {
                await AddAdminCommentIfNeeded(id, statusDto.AdminComment);
                var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, statusDto, cancellationToken);
                return Ok(updatedOrder);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private Task AddAdminCommentIfNeeded(int orderId, string? comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                _adminCommentService.AddComment(orderId, comment);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обновить заказ полностью.
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="orderDto">Данные заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный заказ</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDto>> UpdateOrder(
            int id,
            [FromBody] OrderDto orderDto,
            CancellationToken cancellationToken)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, orderDto, cancellationToken);
                return Ok(updatedOrder);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
