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
    public class TalksController : ControllerBase
    {
        [Route("api/camps/{moniker}/talks")]
        [ApiController]
        public class CampsController : ControllerBase
        {
            private readonly LinkGenerator _linkGenerator;
            private readonly ICampRepository _repository;
            private readonly IMapper _mapper;

            public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
            {
                _repository = repository;
                _mapper = mapper;
                _linkGenerator = linkGenerator;
            }

            [HttpGet]
            public async Task<ActionResult<TalkModel[]>> GetTalks(string moniker)
            {
                try
                {
                    var talks = await _repository.GetTalksByMonikerAsync(moniker);
                    return _mapper.Map<TalkModel[]>(talks);
                }
                catch (Exception)
                {

                    return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
                }
            }
            [HttpGet("{id:int}")]
            public async Task<ActionResult<TalkModel[]>> GetTalk(string moniker, int id)
            {
                try
                {
                    var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                    return _mapper.Map<TalkModel[]>(talk);
                }
                catch (Exception)
                {

                    return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
                }
            }

            [HttpGet("{moniker}")]
            [MapToApiVersion("1.1")]
            public async Task<ActionResult<Campmodel>> GetByMoniker11(string moniker)
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
                    return _mapper.Map<Campmodel[]>(results);

                }
                catch (Exception)
                {

                    return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
                }
            }
            [HttpPost]
            public async Task<ActionResult<Campmodel>> PostTalk(string moniker, TalkModel model)
            {
                try
                {

                    var camp = await _repository.GetCampAsync(moniker);
                    if (camp == null) return BadRequest("Camp does not exit");
                    var talk = _mapper.Map<Talk>(model);
                    talk.Camp = camp;

                    if (model.Speaker == null) return BadRequest("Speaker ID is Required");
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker == null) return BadRequest("Speaker could not be found");
                    _repository.Add(talk);
                    
                    if (await _repository.SaveChangesAsync())
                    {
                        var url = _linkGenerator.GetPathByAction(HttpContext, "GetTalks",
                        values: new { moniker, id=talk.TalkId });
                        return Created(url, _mapper.Map<TalkModel>(talk));
                    }

                    else
                    {
                        return BadRequest("Failed to save new Talk");
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
                    if (await _repository.SaveChangesAsync())
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
}

