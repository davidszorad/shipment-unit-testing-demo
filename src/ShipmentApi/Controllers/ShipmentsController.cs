using Microsoft.AspNetCore.Mvc;
using ShipmentApi.Domain;
using ShipmentApi.Domain.Exceptions;
using ShipmentApi.Services;

namespace ShipmentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShipmentsController(ShipmentService shipmentService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Shipment>> BookAsync(BookShipmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var shipment = await shipmentService.BookAsync(request, cancellationToken);
            return Ok(shipment);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (LocationNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (LocationInactiveException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ColdChainNotSupportedException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
