using Microsoft.AspNetCore.Mvc;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Threading;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления заказами администратором
    /// </summary>
    [Route("api/admin/orders")]
    [ApiController] 
    [TypeFilter(typeof(AdminEmailFilter))]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAdminOrderService _adminService;

        /// <summary>
        /// Создает новый экземпляр контроллера
        /// </summary>
        /// <param name="orderService">Сервис заказов</param>
        /// <param name="adminService">Сервис администрирования заказов</param>
        public AdminOrdersController(IOrderService orderService, IAdminOrderService adminService)
        {
            _orderService = orderService;
            _adminService = adminService;
        }

        /// <summary>
        /// Подтверждает заказ
        /// </summary>
        /// <param name="cartId">ID корзины</param>
        /// <param name="dto">Данные обновления заказа</param>
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
        /// Получает статистику по заказам
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статистика заказов</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<OrderStatsDto>> GetStats(CancellationToken cancellationToken)
        {
            return await _adminService.GetOrderStatsAsync(cancellationToken);
        }

        /// <summary>
        /// Получает заказ с комментарием
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
        /// Получает все заказы
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список заказов</returns>
        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
        {
            return Ok(await _orderService.GetAllOrdersAsync(cancellationToken));
        }

        /// <summary>
        /// Получает заказ по ID
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Заказ</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id, CancellationToken cancellationToken)
        {
            try
            {
                return Ok(await _orderService.GetOrderAsync(id, cancellationToken));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Обновляет статус заказа
        /// </summary>
        /// <param name="id">ID заказа</param>
        /// <param name="statusDto">Новый статус</param>
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
                return Ok(await _orderService.UpdateOrderStatusAsync(id, statusDto, cancellationToken));
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

        /// <summary>
        /// Обновляет заказ
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
                return Ok(await _orderService.UpdateOrderAsync(id, orderDto, cancellationToken));
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
