using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    //[Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id) {
            //handle error section
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var message = await _repo.GetMessage(id);
            if( message == null )
                return NotFound();
            //end handle error section
            return Ok(message);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams) {
            //handle error section
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            //end handle error section
            messageParams.UserId = userId;
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageAndUserDataForReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalItems, messagesFromRepo.TotalPages);

            return Ok(messages);            
        }

        //[HttpGet("{recipientId}")] not work because it use Url same [HttpGet("{id}", Name = "GetMessage")]
        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId){
            //handle error section
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var messageFromRepo = await _repo.GetMessagesThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageAndUserDataForReturnDto>>(messageFromRepo);

            return Ok(messageThread);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationAndReturnDto messageForCreationDto) {

            var sender = await _repo.GetUser(userId, false); //declare to auto match id to object with object.id

            //handle error section
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;
            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId, false);

            if (recipient==null)
                return BadRequest("Could not find user");
            //end handle error section

            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add<Message>(message);
            if (await _repo.SaveAll()){
                var messageForReturnDto = _mapper.Map<MessageAndUserDataForReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageForReturnDto);
            }
            throw new Exception("Creating the message failed on save");
        }

        [HttpPost("{id}")] // don't using HttpDelete because some case is only mark delete
        public async Task<IActionResult> DeleteMessage(int id, int userId){
            //handle error section
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            //end handle error section
            var messageFromRepo = await _repo.GetMessage(id);

            if(messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;

            if(messageFromRepo.RecipientId == userId)
                messageFromRepo.RecipientDeleted = true;

            if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _repo.Delete(messageFromRepo);

            if(await _repo.SaveAll())
            return NoContent();

            throw new Exception("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id) {
            //handle error section
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            //end handle error section

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo.RecipientId != userId) // only RecipientId that had mark
                return Unauthorized();

            messageFromRepo.IsRead = true;
            messageFromRepo.ReadTime = DateTime.Now;

            await _repo.SaveAll();

            return NoContent();
        }
    }
}