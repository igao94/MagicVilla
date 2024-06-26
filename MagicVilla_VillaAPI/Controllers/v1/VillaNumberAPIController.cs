﻿using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ResponseCache(CacheProfileName = "Default30")]
    public class VillaNumberAPIController : ControllerBase
    {
        protected APIResponse _apiResponse;
        private readonly IVillaNumberRepository _villaNumberRepository;
        private readonly IVillaRepository _villRepository;
        private readonly IMapper _mapper;

        public VillaNumberAPIController(IVillaNumberRepository villaNumberRepository,
            IVillaRepository villaRepository,
            IMapper mapper)
        {
            _villaNumberRepository = villaNumberRepository ?? throw new ArgumentNullException(nameof(villaNumberRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _villRepository = villaRepository ?? throw new ArgumentNullException(nameof(villaRepository));
            _apiResponse = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {
            try
            {
                IEnumerable<VillaNumber> villaNumbers = await _villaNumberRepository.GetAllAsync(
                    includeProperties: "Villa");
                _apiResponse.Result = _mapper.Map<IEnumerable<VillaNumberDto>>(villaNumbers);
                _apiResponse.StatusCode = HttpStatusCode.OK;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.Message };
            }

            return Ok(_apiResponse);
        }

        [HttpGet("{id}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid Villa Number.");
                    return BadRequest(_apiResponse);
                }

                var villaNumber = await _villaNumberRepository.GetAsync(v => v.VillaNo == id,
                    includeProperties: "Villa");

                if (villaNumber == null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid Villa Number.");
                    return BadRequest(_apiResponse);
                }

                _apiResponse.Result = _mapper.Map<VillaNumberDto>(villaNumber);
                _apiResponse.StatusCode = HttpStatusCode.OK;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.Message };
            }

            return _apiResponse;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDto villaNumberCreateDto)
        {
            try
            {
                if (await _villaNumberRepository.GetAsync(v => v.VillaNo == villaNumberCreateDto.VillaNo) != null)
                {
                    ModelState.AddModelError("Custom Error", "Villa number already exists.");
                    return BadRequest(ModelState);
                }

                if (await _villRepository.GetAsync(v => v.Id == villaNumberCreateDto.VillaId) == null)
                {
                    ModelState.AddModelError("Custom Error", "Villa doesn't exists.");
                    return BadRequest(ModelState);
                }

                if (villaNumberCreateDto == null)
                {
                    _apiResponse.ErrorMessages.Add("Please add Villa Number.");
                    return NotFound();
                }

                var villaNumber = _mapper.Map<VillaNumber>(villaNumberCreateDto);

                await _villaNumberRepository.CreateAsync(villaNumber);

                _apiResponse.Result = _mapper.Map<VillaNumberDto>(villaNumber);
                _apiResponse.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute(nameof(GetVillaNumber), new { id = villaNumber.VillaNo }, _apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.Message };
            }

            return _apiResponse;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid Villa Number.");
                    return BadRequest(_apiResponse);
                }

                var villaNumber = await _villaNumberRepository.GetAsync(v => v.VillaNo == id);

                if (villaNumber == null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid Villa Number.");
                    return NotFound(_apiResponse);
                }

                await _villaNumberRepository.RemoveAsync(villaNumber);

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.Message };
            }

            return _apiResponse;
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber([FromBody] VillaNumberUpdateDto villaNumberUpdateDto,
            int id)
        {
            try
            {
                if (villaNumberUpdateDto == null || id != villaNumberUpdateDto.VillaNo)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add("Invalid Villa Number.");
                    return BadRequest(_apiResponse);
                }

                if (await _villRepository.GetAsync(v => v.Id == villaNumberUpdateDto.VillaId) == null)
                {
                    ModelState.AddModelError("Custom Error", "Villa doesn't exists.");
                    return BadRequest(ModelState);
                }

                var villaNumber = _mapper.Map<VillaNumber>(villaNumberUpdateDto);

                await _villaNumberRepository.UpdateAsync(villaNumber);

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.Result = _mapper.Map<VillaNumberDto>(villaNumber);
                _apiResponse.IsSuccess = true;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string> { ex.Message };
            }

            return _apiResponse;
        }
    }
}
