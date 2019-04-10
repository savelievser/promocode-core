using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoCodesWebApp.Models;
using PromoCodesWebApp.Services;

namespace PromoCodesWebApp.Controllers
{
    [Route("api/Promo")]
    [ApiController]
    public class PromoApiController : ControllerBase
    {
        private readonly PromoCodeService _promoCodeService;
        private readonly AntiFraudService _antiFraudService;

        private const int PageSize = 10;
        private const int MaxCreateCount = 100;

        public PromoApiController(PromoCodeService promoCodeService, AntiFraudService blackListService)
        {
            _promoCodeService = promoCodeService;
            _antiFraudService = blackListService;
        }

        // GET: api/Promo/list/1
        [HttpGet("list/{page}")]
        public async Task<PageResult<PromoCode>> GetPage(int page)
        {
            return await _promoCodeService.GetAsync(PageSize * (page - 1), PageSize);
        }

        // GET: api/Promo/a1b1c1d1
        [HttpGet("{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PromoCode>> Get(string key)
        {
            // if api will be used behind facade, we should pass userId as parameter
            var userId = Request.Host.Value;
            if (!(await _antiFraudService.Accept(userId)))
                return StatusCode(StatusCodes.Status429TooManyRequests);

            var code = await _promoCodeService.GetAsync(key);
            if (code == null)
            {
                if (await _antiFraudService.HandleMiss(userId))
                    return NotFound();
                else
                    return StatusCode(StatusCodes.Status429TooManyRequests);
            }
            return code;
        }

        // POST: api/Promo/1
        [HttpPost("{count}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<string>>> Post(int count)
        {
            if (count > MaxCreateCount)
                return BadRequest($"{nameof(count)} parameter exceeded the allowed max value: {MaxCreateCount}");

            var result = await _promoCodeService.GenerateAsync(count);
            return Ok(result);
        }

        // PUT: api/Promo/a1b1c1d1
        [HttpPut("{key}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(string key, PromoCode value)
        {
            try
            {
                await _promoCodeService.UpdateAsync(key, value);
            }
            catch (PromoCodeValidationException e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        // DELETE: api/PromoWithActions/a1b1c1d1
        [HttpDelete("{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task Delete(string key)
        {
            await _promoCodeService.DeleteAsync(key);
        }
    }
}
