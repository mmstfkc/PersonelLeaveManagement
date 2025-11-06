using Microsoft.AspNetCore.Mvc;
using PersonelLeaveManagement.Application.DTOs;
using PersonelLeaveManagement.Application.Interfaces;

namespace PersonelLeaveManagement.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class IzinTalebiController: ControllerBase
{
    private readonly IIzinTalebiService _service;

    public IzinTalebiController(IIzinTalebiService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(IzinTalebiDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, IzinTalebiDto dto)
    {
        var results = await _service.UpdateAsync(id, dto);
        return results ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
