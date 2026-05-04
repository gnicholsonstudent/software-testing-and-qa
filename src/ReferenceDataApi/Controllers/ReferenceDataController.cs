using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReferenceDataApi.Dtos;
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

    // READ (Reader + Admin)
    // GET /{referenceDataType}/{referenceDataId}
    [HttpGet("{referenceDataId:int}")]
    [Authorize(Roles = "Admin,Reader")]
    public ActionResult<ReferenceDataResponse> GetById(
        [FromRoute] string referenceDataType,
        [FromRoute] int referenceDataId)
    {
        var result = _service.Get(referenceDataType, referenceDataId);
        return result is null ? NotFound() : Ok(result);
    }

    // CREATE (Admin only)
    // POST /{referenceDataType}
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<ReferenceDataResponse> Create(
        [FromRoute] string referenceDataType,
        [FromBody] CreateReferenceDataRequest request)
    {
        try
        {
            var created = _service.Create(referenceDataType, request);
            return CreatedAtAction(nameof(GetById),
                new { referenceDataType, referenceDataId = created.Id },
                created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // UPDATE (Admin only)
    // PUT /{referenceDataType}/{referenceDataId}
    [HttpPut("{referenceDataId:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(
        [FromRoute] string referenceDataType,
        [FromRoute] int referenceDataId,
        [FromBody] UpdateReferenceDataRequest request)
    {
        var ok = _service.Update(referenceDataType, referenceDataId, request, out var error);
        if (error is not null) return BadRequest(new { error });

        return ok ? NoContent() : NotFound();
    }

    // DELETE (Admin only)
    // DELETE /{referenceDataType}/{referenceDataId}
    [HttpDelete("{referenceDataId:int}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(
        [FromRoute] string referenceDataType,
        [FromRoute] int referenceDataId)
    {
        return _service.Delete(referenceDataType, referenceDataId)
            ? NoContent()
            : NotFound();
    }
}