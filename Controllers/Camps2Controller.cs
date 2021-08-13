using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps")]
    [ApiController]
    [ApiVersion("2.0")]
    
    public class Camps2Controller : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public Camps2Controller(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }
        [HttpGet]
        public async Task<ActionResult> GetCamps(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);
                var result = new
                {
                    Count = results.Count(),
                    Results = _mapper.Map<Campmodel[]>(results)
                };
            
                return Ok(result);
               
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        
       
        [HttpGet("{moniker}")]
        
        public async Task<ActionResult<Campmodel>> GetByMoniker(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);
                if (result == null) return NotFound();
                return _mapper.Map<Campmodel>(result);
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<Campmodel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any()) return NotFound();
                return _mapper.Map <Campmodel[]>(results);

            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
        [HttpPost]
        public async Task<ActionResult<Campmodel>> PostCamp([FromBody] Campmodel model)
        {
            try
            {
                var existing = _repository.GetCampAsync(model.Moniker);
                if(existing != null)
                {
                    return BadRequest("Moniker in use");
                }
                var location = _linkGenerator.GetPathByAction("GetCamps", "Camps",
                    new { moniker = model.Moniker});
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use Current Moniker");
                }
                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);
                if(await _repository.SaveChangesAsync())
                {
                    return Created("", _mapper.Map<Campmodel>(camp));
                }
                
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<Campmodel>> PutCamp(string moniker, Campmodel model)
        {
            try
            {
                var oldCamp = _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound($"Could not find camp with moniker {moniker}");

                await _mapper.Map(model, oldCamp);
                if(await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<Campmodel>(oldCamp);
                }
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> DeleteCamp(string moniker)
        {
            try
            {
                var oldCamp = _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound();

                _repository.Delete(oldCamp);
                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            return BadRequest();
        }


    }
}
