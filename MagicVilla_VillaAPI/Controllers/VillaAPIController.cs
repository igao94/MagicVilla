using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        protected APIResponse _apiResponse;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository villaRepository, IMapper mapper)
        {
            _villaRepository = villaRepository ?? throw new ArgumentNullException(nameof(villaRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apiResponse = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                IEnumerable<Villa> villas = await _villaRepository.GetAllAsync();
                _apiResponse.Result = _mapper.Map<IEnumerable<VillaDto>>(villas);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
            }

            return _apiResponse;
        }

        [HttpGet("{id}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                var villa = await _villaRepository.GetAsync(v => v.Id == id);

                if (villa == null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_apiResponse);
                }

                _apiResponse.Result = _mapper.Map<VillaDto>(villa);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
            }

            return _apiResponse;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDto villaCreateDto)
        {
            try
            {
                if (await _villaRepository.GetAsync(v => v.Name == villaCreateDto.Name) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa already exists!");
                    return BadRequest(ModelState);
                }

                if (villaCreateDto == null) return BadRequest(villaCreateDto);

                var villa = _mapper.Map<Villa>(villaCreateDto);

                await _villaRepository.CreateAsync(villa);

                _apiResponse.Result = _mapper.Map<VillaDto>(villa);
                _apiResponse.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute(nameof(GetVilla), new { villa.Id }, _apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
            }

            return _apiResponse;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                var villa = await _villaRepository.GetAsync(v => v.Id == id);

                if (villa == null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_apiResponse);
                }

                await _villaRepository.RemoveAsync(villa);

                _apiResponse.Result = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
            }

            return _apiResponse;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> UpdateVila(int id, [FromBody] VillaUpdateDto villaUpdateDto)
        {
            try
            {
                if (await _villaRepository.GetAsync(v => v.Name == villaUpdateDto.Name) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa already exists!");
                    return BadRequest(ModelState);
                }

                if (villaUpdateDto == null || id != villaUpdateDto.Id) 
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

                var villa = _mapper.Map<Villa>(villaUpdateDto);

                await _villaRepository.UpdateAsync(villa);

                _apiResponse.Result = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.Message };
            }

            return _apiResponse;
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDto)
        {
            if (patchDto == null || id == 0) return BadRequest();

            var villa = await _villaRepository.GetAsync(v => v.Id == id, tracked: false);

            if (villa == null) return NotFound();

            var villaUpdateDto = _mapper.Map<VillaUpdateDto>(villa);

            patchDto.ApplyTo(villaUpdateDto, ModelState);

            var villaModel = _mapper.Map<Villa>(villaUpdateDto);

            await _villaRepository.UpdateAsync(villaModel);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return NoContent();
        }
    }
}