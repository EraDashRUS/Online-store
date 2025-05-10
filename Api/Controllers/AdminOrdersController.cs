using Microsoft.AspNetCore.Mvc;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Threading;

namespace OnlineStore.Api.Controllers
{
    [Route("api/admin/orders")]
    [ApiController]
    [TypeFilter(typeof(AdminEmailFilter))]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAdminOrderService _adminService;

        public AdminOrdersController(IOrderService orderService, IAdminOrderService adminService)
        {
            _orderService = orderService;
            _adminService = adminService;
        }

        [HttpPut("{cartId}/approve")]
        public async Task<IActionResult> ApproveOrder(
        int cartId,
        [FromBody] AdminOrderUpdateDto dto,
        CancellationToken cancellationToken)
        {
            await _adminService.ApproveOrderAsync(cartId, dto.AdminComment, cancellationToken);
            return NoContent();
        }

        [HttpGet("stats")]
        public async Task<ActionResult<OrderStatsDto>> GetStats(CancellationToken cancellationToken)
        {
            return await _adminService.GetOrderStatsAsync(cancellationToken);
        }

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


        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
        {
            return Ok(await _orderService.GetAllOrdersAsync(cancellationToken));
        }

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
