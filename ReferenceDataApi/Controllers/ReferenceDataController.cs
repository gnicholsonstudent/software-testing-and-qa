using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReferenceDataApi.Contracts;
using ReferenceDataApi.Services;

namespace ReferenceDataApi.Controllers;

[ApiController]
[Route("{referenceDataType}")]
public sealed class ReferenceDataController : ControllerBase
{
    private readonly IReferenceDataService _service;

    public ReferenceDataController(IReferenceDataService service)
    {
        _service = service;
    }

    // READ (Reader or Admin)
    // GET /{referenceDataType}/{referenceDataId}
    [HttpGet("{referenceDataId:int}")]
    [Authorize(Policy = "ReaderOrAdmin")]
    [ProducesResponseType(typeof(ReferenceDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ReferenceDataDto> Get(
        [FromRoute] string referenceDataType,
        [FromRoute] int referenceDataId)
    {
        var result = _service.Get(referenceDataType, referenceDataId);
        return result is null ? NotFound() : Ok(result);
    }

    // CREATE (Admin only)
    // POST /{referenceDataType}
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ReferenceDataDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<ReferenceDataDto> Create(
        [FromRoute] string referenceDataType,
        [FromBody] CreateReferenceDataRequest request)
    {
        try
        {
            var created = _service.Create(referenceDataType, request);
            return CreatedAtAction(
                nameof(Get),
                new { referenceDataType, referenceDataId = created.Id },
                created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // UPDATE (Admin only)
    // PUT /{referenceDataType}/{referenceDataId}
    [HttpPut("{referenceDataId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ReferenceDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ReferenceDataDto> Update(
        [FromRoute] string referenceDataType,
        [FromRoute] int referenceDataId,
        [FromBody] UpdateReferenceDataRequest request)
    {
        try
        {
            var updated = _service.Update(referenceDataType, referenceDataId, request);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE (Admin only)
    // DELETE /{referenceDataType}/{referenceDataId}
    [HttpDelete("{referenceDataId:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(
        [FromRoute] string referenceDataType,
        [FromRoute] int referenceDataId)
    {
        var deleted = _service.Delete(referenceDataType, referenceDataId);
        return deleted ? NoContent() : NotFound();
    }
}