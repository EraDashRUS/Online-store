﻿using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Delivery
{
    public class DeliveryResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int OrderId { get; set; }

        public OrderBriefDto Order { get; set; }
    }
}
