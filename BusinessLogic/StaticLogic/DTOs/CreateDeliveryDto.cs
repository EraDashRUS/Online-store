using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs;

public class CreateDeliveryDto
{
[Required]
public string Status { get; set; }

[Required]
public DateTime DeliveryDate { get; set; }

[Required]
public int OrderId { get; set; }
}